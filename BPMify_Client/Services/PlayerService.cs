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

namespace BPMify_Client.Services
{
    public class PlayerService : IPlayerService
    {
        private string _token;
        private IJSRuntime _js;
        private HttpResponseMessage _response;
        private static string _bpmImfyDeviceId;
        public static string _playerState { get; set; } = SD.PlayerState_Unitialized;
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
        }



        public async Task TransferPlayback()
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/v1/me/player");
            request.Content = new StringContent("{" + $"\"device_ids\": [\"" + _bpmImfyDeviceId + $"\"" + "]}");//find a better solution to format into -> {device_ids: ["74ASZWbe4lXaubB36ztrGX"]}
            Console.WriteLine("Send reqeust for transfer Playback ");
            var httpClient = _clientFactory.CreateClient(SD.HttpClient_SpotifyApiClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            _response = await httpClient.SendAsync(request);
            Console.WriteLine("Took control");            
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
