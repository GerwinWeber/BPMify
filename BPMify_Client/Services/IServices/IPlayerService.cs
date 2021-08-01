using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BPMify_Client.Services.IServices
{
    public interface IPlayerService
    {
        public Task InitializePlayer(string token, IJSRuntime js);
        public Task TransferPlayback(string deviceId);
    }
}
