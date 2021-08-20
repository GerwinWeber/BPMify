using BPMify_Server.Model.CurrentUserPlaylistsResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BPMify_Server.Services.IServices
{
    public interface ISpotifyApiResponseHandler
    {
        public bool IsRequestSuccessfull(HttpResponseMessage response);

        //public event EventHandler<EventArgs> AccessTokenExpired;
    }
}
