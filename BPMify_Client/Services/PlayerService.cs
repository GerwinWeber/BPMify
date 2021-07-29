using BPMify_Client.Services.IServices;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BPMify_Client.Services
{
    public class PlayerService : IPlayerService
    {
        public static SpotifyClient SpotifyClient;//have to be static because of "GetPlaybackControlAsyncInvokeable()"
        public static string _bpmImfyDeviceId { get; set; }
        static PlayerTransferPlaybackRequest request;


        public void InitializePlayer(string token)
        {
            SpotifyClient = new SpotifyClient(token);
        }

        public async Task TransferPlayback(string deviceId)
        {
            _bpmImfyDeviceId = deviceId;
            Console.WriteLine("DeviceID: " + deviceId);
            PlayerTransferPlaybackRequest request = new PlayerTransferPlaybackRequest(new List<string>() { deviceId });
            await SpotifyClient.Player.TransferPlayback(request);
            Console.WriteLine("Took control");
        }
    }
}
