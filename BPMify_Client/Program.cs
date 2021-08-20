using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using BPMify_Client.Services.IServices;
using BPMify_Client.Services;
using BPMify_Client.Helpers;

namespace BPMify_Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<IPlayerService, PlayerService>();
            builder.Services.AddScoped<ISpotifyAuthenticationService, SpotifyAuthenticationService>();//the components only uses the interface to call the functions
            builder.Services.AddScoped<ISpotifyApiResponseHandler, SpotifyApiResponseHandler>();
            builder.Services.AddScoped<PlayerStateManager>();//all the components use the same instance of this class in the whole project
            builder.Services.AddHttpClient<PlayerService>(SD.HttpClient_SpotifyApiClient, client =>
            {
                client.BaseAddress = new Uri("https://api.spotify.com");
            });
            builder.Services.AddHttpClient<SpotifyAuthenticationService>(SD.HttpClient_SpotifyAuthenticationClient, client =>
            {
                client.BaseAddress = new Uri("https://accounts.spotify.com");
            });

            await builder.Build().RunAsync();
        }
    }
}
