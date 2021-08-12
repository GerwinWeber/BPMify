using BPMify_Client.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BPMify_Client.Services
{
    public class SpotifyApiResponseHandler : ISpotifyApiResponseHandler
    {
        //public event EventHandler<EventArgs> AccessTokenExpired;

        public bool IsRequestSuccessfull(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    //200
                    Console.WriteLine("HttpStatusCode: 200");
                    return true;
                case System.Net.HttpStatusCode.Created:
                    //201
                    Console.WriteLine("HttpStatusCode: 201");
                    break;
                case System.Net.HttpStatusCode.NoContent:
                    //204
                    Console.WriteLine("HttpStatusCode: 204");
                    //e.g. PlayerService.TransferPlayback()
                    return true;
                case System.Net.HttpStatusCode.BadRequest:
                    //400
                    Console.WriteLine("HttpStatusCode: 400");
                    return false;
                case System.Net.HttpStatusCode.Unauthorized:
                    //401
                    Console.WriteLine("HttpStatusCode: 401");
                    //AccessTokenExpired?.Invoke(this, EventArgs.Empty);
                    return false;
                case System.Net.HttpStatusCode.PaymentRequired:
                    //402
                    Console.WriteLine("HttpStatusCode: 402");
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    //403
                    Console.WriteLine("HttpStatusCode: 403");
                    break;
                case System.Net.HttpStatusCode.NotFound:
                    //404
                    Console.WriteLine("HttpStatusCode: 404");
                    return false;
                default:
                    break;
            }
            return false;
        }
    }
}
