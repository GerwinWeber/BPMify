using Blazored.LocalStorage;
using BPMify_Server.Data;
using BPMify_Server.Helpers;
using BPMify_Server.Services;
using BPMify_Server.Services.IServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace BPMify_Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(SD.HttpClient_BaseAddress) });
            services.AddBlazoredLocalStorage();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<ISpotifyAuthenticationService, SpotifyAuthenticationService>();//the components only uses the interface to call the functions
            services.AddScoped<ISpotifyApiResponseHandler, SpotifyApiResponseHandler>();
            services.AddScoped<PlayerStateManager>();//all the components use the same instance of this class in the whole project
            services.AddHttpClient<PlayerService>(SD.HttpClient_SpotifyApiClient, client =>
            {
                client.BaseAddress = new Uri("https://api.spotify.com");
            });
            services.AddHttpClient<SpotifyAuthenticationService>(SD.HttpClient_SpotifyAuthenticationClient, client =>
            {
                client.BaseAddress = new Uri("https://accounts.spotify.com");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
