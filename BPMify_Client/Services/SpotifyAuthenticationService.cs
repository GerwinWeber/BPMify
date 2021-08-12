using Blazored.LocalStorage;
using BPMify_Client.Helpers;
using BPMify_Client.Model;
using BPMify_Client.Services.IServices;
using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace BPMify_Client.Services
{
    public class SpotifyAuthenticationService : ISpotifyAuthenticationService
    {
        private PkceData _pkceData;//= new PkceData() { Challenge = "", RefreshToken ="", Verifier = ""};
        private string _authenticationState = SD.AuthState_Initialized;
        private HttpResponseMessage _response;
        private string _responseContent;
        private AccessTokenResponse _accessTokenRespond;

        private Uri _currentUri;
        private NavigationManager _navManager;
        
        private const string _clientId = "c5d79ae00f804be181a0551bda35a318";
        private string _spotifyScopes = @"user-read-currently-playing%20user-read-playback-state%20user-modify-playback-state%20streaming%20user-read-email%20user-read-private%20playlist-read-private";
        private string _code = "";
        private string _codeVerifier = "";
        private string _codeChallenge = "";
        private string SpotifyAuthUrl = "";
        private Uri _redirectUri = new Uri("https://localhost:44352/");


        private IPlayerService _player;
        private ILocalStorageService _localStorage;
        private IJSRuntime _js;
        private IHttpClientFactory _clientFactory;
        private SpotifyApiResponseHandler _responseHandler;
        private PlayerStateManager _stateManager;
        private JsonSerializerOptions _jsonSerializerOptions;
        private static Timer _refreshTokenTimer;


        public SpotifyAuthenticationService([FromServices] ILocalStorageService localStorage,[FromServices] IPlayerService player,[FromServices] IJSRuntime js, NavigationManager navManager,
            [FromServices] PlayerStateManager stateManager,[FromServices] SpotifyApiResponseHandler responseHandler, IHttpClientFactory clientFactory)
        {
            _localStorage = localStorage;
            _player = player;
            _js = js;
            _navManager = navManager;
            _clientFactory = clientFactory;
            _stateManager = stateManager;
            _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            _responseHandler = responseHandler;
            //_responseHandler.AccessTokenExpired +=  async (sender, e) => await StartNewAuthentication(sender, e);//this line is the reason to use the class instead of the interface
        }

        //public async Task StartNewAuthentication(object sender, EventArgs e)
        //{
        //    await CheckAuthenticationState();
        //}

        public async Task StartNewAuthentication(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Automatically request new Access Token");
            await CheckAuthenticationState();
        }


        public async Task CheckAuthenticationState()
        {
            try
            {
                _pkceData = await _localStorage.GetItemAsync<PkceData>(SD.Local_PkceData);
                Console.WriteLine("Refreshtoken: " + _pkceData.RefreshToken);
                if (!string.IsNullOrEmpty(_pkceData.RefreshToken))
                {
                    //RefreshToken found in local storage
                    _authenticationState = SD.AuthState_RefreshTokenStored;
                    Console.WriteLine("Refreshtoken: " + _pkceData.RefreshToken);
                    _stateManager.ValidTokenAvailable = true;
                    await RequestAccessTokenWithRefreshToken();
                    return;
                }
            }
            catch (Exception)
            {
                //Console.WriteLine(e.Message);
                Console.WriteLine("no RefreshToken found in local storage");
            }
            if (!_stateManager.ValidTokenAvailable)
            {
                //no Refresh token stored in local storage
                Console.WriteLine("Check for code");
                await CheckForRecievedCode();
            }
        }

        public async Task CheckForRecievedCode()
        {
            _currentUri = new Uri(_navManager.Uri);
            //Console.WriteLine(_currentUri.Query);

            if (String.IsNullOrEmpty(_currentUri.Query) || !_currentUri.Query.Contains("?code="))
            {
                //no code for requesting Accesstoken available
                _authenticationState = SD.PlayerState_PlayerNotInitialized;
                Console.WriteLine("no code for requesting Accesstoken available");
                _stateManager.TryToAuthenticate = false;
            }
            else
            {
                _authenticationState = SD.AuthState_ReceivedCode;
                Console.WriteLine(_authenticationState);
                try
                {
                    await GetLocalStorageData();
                    await RequestAccessTokenWithCode();
                }
                catch (Exception)
                {
                    //code not valid
                    Console.WriteLine("code not valid");
                    _stateManager.TryToAuthenticate = false;
                    _authenticationState = SD.PlayerState_PlayerNotInitialized;
                }
            }
        }

        public HttpRequestMessage BuildTokenRequest()
        {
            //http://ronaldrosiernet.azurewebsites.net/Blog/2013/12/07/posting_urlencoded_key_values_with_httpclient
            //http.BaseAddress = new Uri("https://accounts.spotify.com");
            return new HttpRequestMessage(HttpMethod.Post, "/api/token");
        }

        public List<KeyValuePair<string, string>> BuildRequestContentWithAccessToken()
        {
            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("client_id", _clientId));
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            keyValues.Add(new KeyValuePair<string, string>("code", _code));
            keyValues.Add(new KeyValuePair<string, string>("redirect_uri", _redirectUri.ToString()));
            keyValues.Add(new KeyValuePair<string, string>("code_verifier", _pkceData.Verifier));
            return keyValues;
        }

        public List<KeyValuePair<string, string>> BuildRequestContentWithRefreshToken()
        {
            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            keyValues.Add(new KeyValuePair<string, string>("refresh_token", _pkceData.RefreshToken));
            keyValues.Add(new KeyValuePair<string, string>("client_id", _clientId));
            return keyValues;
        }

        public async Task RequestAccessTokenWithRefreshToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/token");
            var keyValues = BuildRequestContentWithRefreshToken();
            request.Content = new FormUrlEncodedContent(keyValues);
            Console.WriteLine("Send token request");
            var httpClient = _clientFactory.CreateClient(SD.HttpClient_SpotifyAuthenticationClient);
            _response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", request.Content);
            if (_responseHandler.IsRequestSuccessfull(_response))
            {
                await HandleTokenRequestResponse();
            }
        }

        public async Task RequestAccessTokenWithCode()
        {
            _code = _currentUri.Query.Substring(6);//localhost:44359/?code=<code>
            Console.WriteLine($"_code: {_currentUri.Query}; _verifier: {_codeVerifier} ");

            var request = BuildTokenRequest();
            var keyValues = BuildRequestContentWithAccessToken();
            request.Content = new FormUrlEncodedContent(keyValues);
            Console.WriteLine($"Send token request with code: {_code}");
            var httpClient = _clientFactory.CreateClient(SD.HttpClient_SpotifyAuthenticationClient);
            _response = await httpClient.SendAsync(request);
            if (_responseHandler.IsRequestSuccessfull(_response))
            {
                await HandleTokenRequestResponse();
            }
            else//no AccessToken received
            {
                Console.WriteLine("no AccessToken received");
            }
        }

        public async Task HandleTokenRequestResponse()
        {
            StopAndDisposeRefreshTokenTimer();
            SetRefreshTokenTimer();
            _responseContent = await _response.Content.ReadAsStringAsync();
            _accessTokenRespond = JsonSerializer.Deserialize<AccessTokenResponse>(_responseContent, _jsonSerializerOptions);

            Console.WriteLine("AccessToken: " + _accessTokenRespond.Access_token);
            Console.WriteLine("RefreshToken: " + _accessTokenRespond.Refresh_token);
            _authenticationState = SD.AuthState_ReceivedAccessToken;
            await _player.PassTokenToPlayer(_accessTokenRespond.Access_token);

            //store refresh_token in local storage
            _pkceData.RefreshToken = _accessTokenRespond.Refresh_token;
            await _localStorage.SetItemAsync<PkceData>(SD.Local_PkceData, _pkceData);
        }

        public async Task NavigateToSpotifyLogin()
        {
            //_verfier and _challenge must be storaged locally
            await GenerateCodeAndChallenge();
            SpotifyAuthUrl = "https://accounts.spotify.com/en/authorize?client_id=" + _clientId + $"&response_type=code&redirect_uri={_redirectUri}&scope={_spotifyScopes}&code_challenge={_codeChallenge}&code_challenge_method=S256";
            _navManager.NavigateTo(SpotifyAuthUrl);

        }

        public async Task GenerateCodeAndChallenge()
        {
            //https://stackoverflow.com/questions/26353710/how-to-achieve-base64-url-safe-encoding-in-c
            //https://www.scottbrady91.com/OpenID-Connect/ASPNET-Core-using-Proof-Key-for-Code-Exchange-PKCE
            //needed @using-Statements: @using System.Security.Cryptography @using System.Textusing @using Microsoft.IdentityModel.Tokensusing @using IdentityModel

            // generates code_verifier
            _codeVerifier = CryptoRandom.CreateUniqueId(32);

            char[] padding = { '=' };

            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(_codeVerifier));
                _codeChallenge = Base64UrlEncoder.Encode(challengeBytes);
            }
            _pkceData = new PkceData() { Challenge = _codeChallenge, Verifier = _codeVerifier, RefreshToken = "" };

            await _localStorage.SetItemAsync<PkceData>(SD.Local_PkceData, _pkceData);
        }

        public async Task GetLocalStorageData()
        {
            try
            {
                _pkceData = await _localStorage.GetItemAsync<PkceData>(SD.Local_PkceData);
            }
            catch (Exception)
            {
                Console.WriteLine("No PKCE-Data in local storage available");
            }
            
        }

        #region RequestNewAccessTokenBeforeItExpires
        private void SetRefreshTokenTimer()
        {
            //Timer für Token refresh setzen
            _refreshTokenTimer = new Timer(3300000);//3.300.000ms = 55min
            _refreshTokenTimer.Elapsed += async (sender, e) => await StartNewAuthentication(sender, e);
            _refreshTokenTimer.AutoReset = false;
            _refreshTokenTimer.Enabled = true;
        }

        private void StopAndDisposeRefreshTokenTimer()
        {
            if (_refreshTokenTimer != null)
            {
                _refreshTokenTimer.Stop();
                _refreshTokenTimer.Dispose();
            }
        }
        #endregion
    }
}
