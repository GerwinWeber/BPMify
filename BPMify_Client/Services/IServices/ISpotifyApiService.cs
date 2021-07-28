using BPMify_Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Services.IServices
{
    public interface ISpotifyApiService
    {
        Task<DeviceResponse> GetAvailableDevices();
    }
}
