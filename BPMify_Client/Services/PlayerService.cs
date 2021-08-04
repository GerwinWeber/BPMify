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

namespace BPMify_Client.Services
{
    public class PlayerService : IPlayerService
    {
        private string _token;
        private IJSRuntime _js;
        private HttpResponseMessage _response;
        public bool IsPlaying = false;
        private List<Item> _allUserPlaylists = new List<Item>();
        public IHttpClientFactory _clientFactory { get; set; }


        public PlayerService(IHttpClientFactory clientFactory,[FromServices] IJSRuntime js)
        {
            _clientFactory = clientFactory;//Service is defined in Programm.cs in line -> builder.Services.AddHttpClient<PlayerService>("ApiClient",...
            _js = js;
        }

        

        public async Task InitializePlayer(string token)
        {
            _token = token;
            await _js.InvokeVoidAsync("InitializePlayer", _token);// WEB SDK Player initialisieren
            //_playerstate = SD.PlayerState_InitializePlayer;
        }



        public async Task TransferPlayback(string deviceId)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/v1/me/player");
            request.Content = new StringContent("{" + $"\"device_ids\": [\"" + deviceId + $"\"" + "]}");//find a better solution to format into -> {device_ids: ["74ASZWbe4lXaubB36ztrGX"]}
            Console.WriteLine("Send reqeust for transfer Playback ");
            var httpClient = _clientFactory.CreateClient(SD.HttpClient_SpotifyApiClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await httpClient.SendAsync(request);
            Console.WriteLine("Took control");            
        }

        public async Task Pause()
        {
            await _js.InvokeVoidAsync("Pause");
            IsPlaying = false;
        }

        public async Task Resume()
        {
            await _js.InvokeVoidAsync("Resume");
            IsPlaying = true;
        }

        public bool PlayStatus()
        {
            return IsPlaying;
        }

        public async Task<List<Item>> GetCurrentUsersPlaylists()
        {
            Console.WriteLine("Send reqeust for get Playlists of the current user");
            int counter = 0;
            int amountOfItems = 50;
            while(amountOfItems == 50)
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

    }
}
