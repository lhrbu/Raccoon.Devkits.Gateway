using System;
using System.Threading.Tasks;

namespace Raccoon.Devkits.Gateway.Client
{
    public interface IGatewayAddressProvider
    {
        ValueTask<Uri> GetAsync(string gatewayServiceName = "Gateway", Uri? consulUri = null);
    }
}