using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Helpers
{
    public static class SD
    {
        public const string Local_PkceData = "PkceData";

        public const string AuthState_Initialized = "Initialized";
        public const string AuthState_ReceivedCode = "ReceivedCode";
        public const string AuthState_ReceivedToken = "ReceivedToken";
    }
}
