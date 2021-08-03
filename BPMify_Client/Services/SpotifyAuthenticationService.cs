﻿using Blazored.LocalStorage;
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
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        public ILocalStorageService _localStorage;
        public IJSRuntime _js;
        public IHttpClientFactory _clientFactory { get; set; }

        public SpotifyAuthenticationService([FromServices] ILocalStorageService localStorage,[FromServices] IPlayerService player,[FromServices] IJSRuntime js, NavigationManager navManager, IHttpClientFactory clientFactory)
        {
            _localStorage = localStorage;
            _player = player;
            _js = js;
            _navManager = navManager;
            _clientFactory = clientFactory;
        }

        public async Task CheckAuthenticationState()
        {
            try
            {
                _pkceData = await _localStorage.GetItemAsync<PkceData>(SD.Local_PkceData);
                Console.WriteLine("Refreshtoken: " + _pkceData.RefreshToken);
                if (!string.IsNullOrEmpty(_pkceData.RefreshToken))
                {
                    //RefreshToken found
                    _authenticationState = SD.AuthState_RefreshTokenStored;
                    Console.WriteLine("Refreshtoken: " + _pkceData.RefreshToken);
                    await RequestAccessTokenWithRefreshToken();
                }
                else
                {
                    //no RefreshToken found
                    await CheckForRecievedCode();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task CheckForRecievedCode()
        {
            _currentUri = new Uri(_navManager.Uri);
            Console.WriteLine(_currentUri.Query);

            if (String.IsNullOrEmpty(_currentUri.Query) || !_currentUri.Query.Contains("?code="))
            {
                _authenticationState = SD.AuthState_Initialized;
            }
            else
            {
                _authenticationState = SD.AuthState_ReceivedCode;
                await GetLocalStorageData();
                await RequestAccessTokenWithCode();
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
            _response = await httpClient.SendAsync(request);
            await HandleTokenRequestResponse();
        }

        public async Task RequestAccessTokenWithCode()
        {
            _code = _currentUri.Query.Substring(6);//localhost:44359/?code=<code>
            Console.WriteLine($"_code: {_currentUri.Query}; _verifier: {_codeVerifier} ");

            var request = BuildTokenRequest();
            var keyValues = BuildRequestContentWithAccessToken();
            request.Content = new FormUrlEncodedContent(keyValues);
            Console.WriteLine("Send token request with code");
            var httpClient = _clientFactory.CreateClient(SD.HttpClient_SpotifyAuthenticationClient);
            _response = await httpClient.SendAsync(request);
            await HandleTokenRequestResponse();
        }

        public async Task HandleTokenRequestResponse()
        {
            if (_response.IsSuccessStatusCode)//AccessToken received
            {
                _responseContent = await _response.Content.ReadAsStringAsync();
                _accessTokenRespond = System.Text.Json.JsonSerializer.Deserialize<AccessTokenResponse>(_responseContent);

                Console.WriteLine("AccessToken: " + _accessTokenRespond.access_token);
                Console.WriteLine("RefreshToken: " + _accessTokenRespond.refresh_token);
                _authenticationState = SD.AuthState_ReceivedAccessToken;
                Console.WriteLine("Reading response string ended");
                await _player.InitializePlayer(_accessTokenRespond.access_token);

                //store refresh_token in local storage
                _pkceData.RefreshToken = _accessTokenRespond.refresh_token;
                await _localStorage.SetItemAsync<PkceData>(SD.Local_PkceData, _pkceData);
            }
            else//no AccessToken received
            {
                Console.WriteLine("no AccessToken received");
            }
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
            //following three lines create an instanciating error
            //_pkceData.Challenge = _codeChallenge;
            //_pkceData.Verifier = _codeVerifier;
            //_pkceData.RefreshToken = "";

            await _localStorage.SetItemAsync<PkceData>(SD.Local_PkceData, _pkceData);

        }

        public async Task GetLocalStorageData()
        {
            _pkceData = await _localStorage.GetItemAsync<PkceData>(Helpers.SD.Local_PkceData);
        }
    }
}