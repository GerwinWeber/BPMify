using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Models
{
    public class DeviceResponse
    {
        public List<Device> Devices { get; set; }
    }

    public class Device
    {
        public string id { get; set; }
        public bool is_active { get; set; }
        public bool is_private_session { get; set; }
        public string name { get; set; }
        public int volume { get; set; }
    }
}
