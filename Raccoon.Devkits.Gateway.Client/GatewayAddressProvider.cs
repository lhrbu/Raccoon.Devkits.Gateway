using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.Devkits.Gateway.Client
{
    public class GatewayAddressProvider
    {
        public async ValueTask<Uri> GetAsync(string gatewayServiceName=nameof(Gateway), Uri? consulUri=null)
        {
            if(consulUri is null) { consulUri = new Uri("http://localhost:8500"); }
            using ConsulClient consulClient = new(options => options.Address = consulUri);
            var res = await consulClient.Agent.Services();
            if(res.Response.TryGetValue(gatewayServiceName,out AgentService? service) && service is not null)
            {
                return new Uri(service.Address);
            }
            else { throw new InvalidOperationException("Gateway service is not regeistered!"); }

        }
    }
}
