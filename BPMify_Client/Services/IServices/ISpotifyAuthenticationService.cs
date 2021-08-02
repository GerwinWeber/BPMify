using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BPMify_Client.Services.IServices
{
    public interface ISpotifyAuthenticationService
    {
        public Task CheckAuthenticationState();
        public Task CheckForRecievedCode();
        public HttpRequestMessage BuildTokenRequest();
        public List<KeyValuePair<string, string>> BuildRequestContentWithAccessToken();
        public List<KeyValuePair<string, string>> BuildRequestContentWithRefreshToken();
        public Task RequestAccessTokenWithRefreshToken();
        public Task RequestAccessTokenWithCode();
        public Task HandleTokenRequestResponse();
        public Task NavigateToSpotifyLogin();
        public Task GenerateCodeAndChallenge();
        public Task GetLocalStorageData();

    }
}
