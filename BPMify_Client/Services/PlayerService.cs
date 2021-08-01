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

namespace BPMify_Client.Services
{
    public class PlayerService : IPlayerService
    {
        private string _token;
        private IJSRuntime _js;
        private HttpClient _apiClient = new HttpClient();
        private HttpResponseMessage _response;

        //public static SpotifyClient SpotifyClient;//have to be static because of "GetPlaybackControlAsyncInvokeable()"
        //public static string _bpmImfyDeviceId { get; set; }
        //ApiClient is used for making requests to the Spotify API
        //private IHttpClientFactory _clientFactory;

        public async Task InitializePlayer(string token, IJSRuntime js)
        {
            //SpotifyClient = new SpotifyClient(token);
            _token = token;
            //_clientFactory = clientFactory;
            _apiClient.BaseAddress = new Uri("https://api.spotify.com");
            _js = js;
            await _js.InvokeVoidAsync("InitializePlayer", _token);// WEB SDK Player initialisieren
        }

        public async Task TransferPlayback(string deviceId)
        {
            //OLD
            //_bpmImfyDeviceId = deviceId;
            //Console.WriteLine("DeviceID: " + deviceId);
            //PlayerTransferPlaybackRequest request = new PlayerTransferPlaybackRequest(new List<string>() { deviceId });
            //await SpotifyClient.Player.TransferPlayback(request);
            //Console.WriteLine("Took control");

            //New
            var request = new HttpRequestMessage(HttpMethod.Put, "/v1/me/player");
            request.Content = new StringContent("{" + $"\"device_ids\": [\"" + deviceId + $"\"" + "]}");
            //find a better solution to format into -> {device_ids: ["74ASZWbe4lXaubB36ztrGX"]}
            //it need to be a list of string altough only one string is passed
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            Console.WriteLine("Send reqeust for transfer Playback ");
            _response = await _apiClient.SendAsync(request);
            _apiClient.Dispose();//not sure how to dispose correctly
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
