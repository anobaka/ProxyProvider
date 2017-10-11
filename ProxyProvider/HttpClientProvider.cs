using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProxyProvider.Infrastructures;
using ProxyProvider.Models;

namespace ProxyProvider
{
    public class HttpClientProvider
    {
        private readonly string _proxyDbConnectionString;

        public HttpClientProvider()
        {
        }

        public HttpClientProvider(string proxyDbConnectionString)
        {
            _proxyDbConnectionString = proxyDbConnectionString;
        }

        public virtual async Task<HttpClient> GetClient(string purpose)
        {
            if (string.IsNullOrEmpty(_proxyDbConnectionString))
            {
                return new HttpClient();
            }
            var db = new ProxyProviderDbContext(new DbContextOptionsBuilder<ProxyProviderDbContext>()
                .UseMySql(_proxyDbConnectionString).Options);
            var options = await db.PurposeClientOptionses.FirstOrDefaultAsync(t => t.Purpose == purpose);
            HttpClient client;
            if (options != null)
            {
                if (options.UseProxy)
                {
                    var now = DateTime.Now;
                    var proxyCheckSoonestDt = now.AddDays(-2);
                    var proxy = (await db.Proxies.Where(t => t.Alive && t.LastCheckDt > proxyCheckSoonestDt)
                            .Join(db.ProxyUsages.Where(a => a.Purpose == purpose), t => t.Id, t => t.ProxyId,
                                (proxy1, proxyUsage) => new {Proxy = proxy1, ProxyUsage = proxyUsage})
                            .Where(t => t.ProxyUsage == null || t.ProxyUsage.LockReleaseDt < now).FirstOrDefaultAsync())
                        ?.Proxy;
                    client = new HttpClient(new HttpClientHandler
                    {
                        Proxy = proxy == null
                            ? null
                            : new CoreWebProxy(new Uri($"{proxy.Schema.ToLower()}://{proxy.Ip}:{proxy.Port}")),
                        AllowAutoRedirect = true,
                        MaxAutomaticRedirections = 10
                    });
                }
                else
                {
                    client = new HttpClient();
                }
                if (options.DefaultHeaders?.Any() == true)
                {
                    foreach (var header in options.DefaultHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            }
            else
            {
                client = new HttpClient();
            }
            return client;
        }
    }
}