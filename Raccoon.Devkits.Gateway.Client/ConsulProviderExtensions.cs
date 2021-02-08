using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.Devkits.Gateway.Client
{
    public static class ConsulProviderExtensions
    {
        public static IApplicationBuilder UseConsulAsServiceProvider(this IApplicationBuilder app, string serviceName,Uri? consulUri=null)
        {
            IHostApplicationLifetime appLifeTime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            appLifeTime.ApplicationStarted.Register(() => app.RegisterToConsulAsync(serviceName, consulUri).Wait());
            appLifeTime.ApplicationStopped.Register(() => app.DeregisterFromConsulAsync(serviceName, consulUri).Wait());
            return app;
        }
        public static IEnumerable<Uri> GetAddresses(this IApplicationBuilder app)=> app.ServerFeatures
                .Get<IServerAddressesFeature>().Addresses
                .Select(address => new Uri(address));
        public static async Task<IApplicationBuilder> RegisterToConsulAsync(this IApplicationBuilder app,
            string serviceName,
             Uri? consulUri=null)
        {
            if (consulUri is null) { consulUri = new Uri("http://localhost:8500"); }
            using ConsulClient consulClient = new(options=>options.Address=consulUri);
            IEnumerable<Task> registerTasks = app.GetAddresses().Select(address =>
                consulClient.Agent.ServiceRegister(new()
                {
                    ID=$"{serviceName}:{address}",
                    Name = serviceName,
                    Address = address.Host,
                    Port = address.Port
                }));
            await Task.WhenAll(registerTasks);
            return app;
        }

        public static async Task<IApplicationBuilder> DeregisterFromConsulAsync(this IApplicationBuilder app,           
            string serviceName,
            Uri? consulUri=null)
        {
            if (consulUri is null) { consulUri = new Uri("http://localhost:8500"); }
            using ConsulClient consulClient = new(options => options.Address = consulUri);
            IEnumerable<Task> deregisterTasks = app.GetAddresses()
                .Select(address => consulClient.Agent.ServiceDeregister($"{serviceName}:{address}"));
            await Task.WhenAll(deregisterTasks);
            return app;
        }
    }
}
