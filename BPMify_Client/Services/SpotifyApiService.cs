using BPMify_Client.Models;
using BPMify_Client.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace BPMify_Client.Services
{
    public class SpotifyApiService //: ISpotifyApiService
    {
        private HttpClient _client;
        public SpotifyApiService(HttpClient client)
        {
            _client = client;
        }
        
        
    }
}
