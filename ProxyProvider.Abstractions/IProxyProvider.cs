using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProxyProvider.Abstractions.Models;

namespace ProxyProvider.Abstractions
{
    public interface IProxyProvider
    {
        Task AddOrUpdate(IEnumerable<Proxy> proxies);
        Task<Proxy> Use(string purpose);
    }
}