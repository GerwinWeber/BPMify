using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Model
{
    public class PkceData
    {
        public string Verifier { get; set; }
        public string Challenge { get; set; }
        public string RefreshToken { get; set; }

    }
}
