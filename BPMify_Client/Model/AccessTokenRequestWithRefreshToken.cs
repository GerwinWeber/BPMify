using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Model
{
    public class AccessTokenRequestWithRefreshToken
    {
        public string Grant_Type { get; set; }
        public string Refresh_Token { get; set; }
        public string Client_Id { get; set; }

        public AccessTokenRequestWithRefreshToken(string refreshToken, string clientId)
        {
            Grant_Type = "refresh_token";
            Refresh_Token = refreshToken;
            Client_Id = clientId;
        }
    }
}
