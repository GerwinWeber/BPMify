
using System.Collections.Generic;

namespace BPMify_Server.Model
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
