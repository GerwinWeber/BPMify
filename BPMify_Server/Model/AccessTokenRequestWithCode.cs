using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Server.Model
{
    public class AccessTokenRequestWithCode
    {
        public string Client_Id { get; set; }
        public string Grant_Type { get; set; }
        public string Code { get; set; }
        public string Redirect_Uri { get; set; }
        public string Code_Verifier { get; set; }

        public AccessTokenRequestWithCode(string clientId, string code, string redirectUri, string codeVerifier)
        {
            Client_Id = clientId;
            Grant_Type = "authorization_code";
            Code = code;
            Redirect_Uri = redirectUri;
            Code_Verifier = codeVerifier;
        }
    }
}
