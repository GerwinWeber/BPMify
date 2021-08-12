using BPMify_Client.Services.IServices;
using Microsoft.JSInterop;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using BPMify_Client.Helpers;
using Microsoft.AspNetCore.Mvc;
using BPMify_Client.Model.CurrentUserPlaylistsResponse;
using BPMify_Client.Model;

namespace BPMify_Client.Services
{
    public class PlayerService : IPlayerService
    {
        private string _token;
        private IJSRuntime _js;
        private string _deviceId = "";
        private string _lastPlaylistId = "";

        private List<Item> _allUserPlaylists;
        private List<Model.PlaylistResponse.Track> _allplaylistTracks;
        public IHttpClientFactory _clientFactory { get; set; }
        private SpotifyApiResponseHandler _responseHandler;

        public PlayerStateManager _stateManager { get; set; }


        public PlayerService(IHttpClientFactory clientFactory,[FromServices] IJSRuntime js, [FromServices] PlayerStateManager stateManager, [FromServices] SpotifyApiResponseHandler responseHandler)
        {
            _clientFactory = clientFactory;//Service is defined in Programm.cs in line -> builder.Services.AddHttpClient<PlayerService>("ApiClient",...
            _js = js;
            _stateManager = stateManager;
            _responseHandler = responseHandler;
        }

        

        public async Task PassTokenToPlayer(string token)
        {
            _token = token;
            if (_stateManager.PlayerState == SD.PlayerState_PlayerReady)
            {
                await _js.InvokeVoidAsync("RefreshToken", _token); // Refresh token of WEB SDK Player
            }
            else
            {
                await _js.InvokeVoidAsync("InitializePlayer", _token);// WEB SDK Player initialisieren
            }
            //_playerstate = SD.PlayerState_InitializePlayer;
        }


        public async Task TransferPlayback(string deviceId)
        {
            _deviceId = deviceId;
            var httpClient = _clientFactory.CreateClient(SD.HttpClient_SpotifyApiClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var uri = new Uri($"https://api.spotify.com/v1/me/player");
            var requestBody = new TransferPlaybackRequest(deviceId);
            var response = await httpClient.PutAsJsonAsync<TransferPlaybackRequest>(uri, requestBody);
            if (_responseHandler.IsRequestSuccessfull(response))
            {
                _stateManager.LocalPlayerIsReady = true;
                _stateManager.TryToAuthenticate = false;
                Console.WriteLine("Took control");
            }
            else
            {
                Console.WriteLine("The request failed with " + response.StatusCode);
            }
                       
        }

        

        public async Task<List<Item>> GetCurrentUsersPlaylists()
        {
            if (_allUserPlaylists == null)
            {
                _allUserPlaylists = new List<Item>();
                Console.WriteLine("Send reqeust for get Playlists of the current user");
                int counter = 0;
                int amountOfItems = 50;
                while (amountOfItems == 50)
                {
                    var httpClient = _clientFactory.CreateClient(SD.HttpClient_SpotifyApiClient);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                    var baseUri = new Uri("https://api.spotify.com/v1/me/playlists");
                    var uri = new Uri(baseUri, $"?limit=50&offset={counter}");
                    var response = await httpClient.GetFromJsonAsync<CurrentUserPlaylistsResponse>(uri);
                    foreach (var item in response.items)
                    {
                        Console.WriteLine($"Name: {item.name} Id: {item.id}");
                        _allUserPlaylists.Add(item);
                        counter += 1;
                    }
                    amountOfItems = response.items.Count<Item>();

                }
                Console.WriteLine("Got all playlists");
                return _allUserPlaylists;
            }
            return _allUserPlaylists;
        }

        public async Task<List<Model.PlaylistResponse.Track>> GetPlaylistItems(string playlistId)
        {
            if (_lastPlaylistId != playlistId)
            {
                _allplaylistTracks = new List<Model.PlaylistResponse.Track>();
                Console.WriteLine($"Send reqeust for get all items of the playlist: {playlistId}");

                var httpClient = _clientFactory.CreateClient(SD.HttpClient_SpotifyApiClient);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var baseUri = new Uri($"https://api.spotify.com/v1/playlists/{playlistId}");
                var uri = new Uri(baseUri, "?market=De");
                var response = await httpClient.GetFromJsonAsync<Model.PlaylistResponse.PlaylistResponse>(uri);
                foreach (var item in response.tracks.items)
                {
                    Console.WriteLine($"Name: {item.track.name} Id: {item.track.id}");
                    _allplaylistTracks.Add(item.track);
                }
                Console.WriteLine("Got all playlist items");
                _lastPlaylistId = playlistId;
                return _allplaylistTracks;

            }
            return _allplaylistTracks;
        }

        

        public async Task PlayTrackById(string trackId)
        {
            Console.WriteLine($"Send reqeust for play songs by its Id: {trackId}");
            //https://stackoverflow.com/questions/48532890/spotify-web-api-play-endpoint-play-track
            var httpClient = _clientFactory.CreateClient(SD.HttpClient_SpotifyApiClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var baseUri = new Uri($"https://api.spotify.com/v1/me/player/play");
            var uri = new Uri(baseUri, $"?device_id={_deviceId}");
            var requestBody = new PlayTrackByIdRequest(trackId);
            var response = await httpClient.PutAsJsonAsync<PlayTrackByIdRequest>(uri, requestBody);

        }

        public async Task Pause()
        {
            await _js.InvokeVoidAsync("Pause");
        }

        public async Task Resume()
        {
            await _js.InvokeVoidAsync("Resume");
        }



    }

    
}
