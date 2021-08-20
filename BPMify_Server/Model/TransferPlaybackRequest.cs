using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Server.Model
{
    public class TransferPlaybackRequest
    {
        public string[] device_ids { get; set; }

        public TransferPlaybackRequest(string deviceId)
        {
            device_ids = new string[] { deviceId };
        }
    }
}
