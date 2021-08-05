using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Model
{
    public class PlayTrackByIdRequest
    {
        public string[] uris { get; set; }

        public PlayTrackByIdRequest(string trackId)
        {
            uris = new string[] { $"spotify:track:{trackId}" };
        }

        //!!!constructor not functional. Only pseudo code. Needs to be implement correctly!!!
        public PlayTrackByIdRequest(List<string> trackIds)
        {
            foreach (var item in trackIds)
            {
                uris = new string[] { $"spotify:track:{trackIds}" };
            } 
        }
    }
}
