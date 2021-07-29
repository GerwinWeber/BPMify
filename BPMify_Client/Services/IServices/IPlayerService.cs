using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Services.IServices
{
    public interface IPlayerService
    {
        public void InitializePlayer(string token);
        public Task TransferPlayback(string deviceId);
    }
}
