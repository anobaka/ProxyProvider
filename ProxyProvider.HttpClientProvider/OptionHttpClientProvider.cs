using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProxyProvider.HttpClientProvider.Models;
using ProxyProvider.HttpClientProvider.Models.RequestModels;

namespace ProxyProvider.HttpClientProvider
{
    /// <summary>
    /// Default provider for common usage.
    /// </summary>
    public class OptionsHttpClientProvider : IHttpClientProvider
    {
        private readonly string _proxyHttpClientProviderDbConnectionString;

        /// <summary>
        /// Customize mechanism to get <see cref="HttpClientProviderDbContext"/>
        /// </summary>
        public OptionsHttpClientProvider()
        {
        }

        /// <summary>
        /// Get <see cref="HttpClientProviderDbContext"/> by default mechanism.
        /// </summary>
        /// <param name="proxyHttpClientProviderDbConnectionString"></param>
        public OptionsHttpClientProvider(string proxyHttpClientProviderDbConnectionString)
        {
            _proxyHttpClientProviderDbConnectionString = proxyHttpClientProviderDbConnectionString;
        }

        /// <summary>
        /// Change the provider if needed
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<HttpClientProviderDbContext> GetDbContext() => new HttpClientProviderDbContext(
            new DbContextOptionsBuilder<HttpClientProviderDbContext>()
                .UseMySql(_proxyHttpClientProviderDbConnectionString).Options);

        protected virtual async Task<ProxyProvider> GetProxyProvider() =>
            new ProxyProvider(_proxyHttpClientProviderDbConnectionString);

        public async Task AddOrUpdatePurposeHttpClientOptions(HttpClientOptionsAddOrUpdateRequestModel model)
        {
            var ctx = await GetDbContext();
            var existedOptions =
                await ctx.HttpClientOptionses.FirstOrDefaultAsync(t => t.Purpose == model.Purpose);
            if (existedOptions != null)
            {
                existedOptions.DefaultHeaders = model.DefaultHeaders;
                existedOptions.UseProxy = model.UseProxy;
                existedOptions.ProxyLockLifetime = model.ProxyLockLifetime;
            }
            else
            {
                existedOptions = new HttpClientOptions
                {
                    DefaultHeaders = model.DefaultHeaders,
                    UseProxy = model.UseProxy,
                    ProxyLockLifetime = model.ProxyLockLifetime,
                    Purpose = model.Purpose
                };
                ctx.HttpClientOptionses.Add(existedOptions);
            }
            await ctx.SaveChangesAsync();
        }

        public virtual async Task<HttpClient> GetClient(string purpose)
        {
            if (string.IsNullOrEmpty(_proxyHttpClientProviderDbConnectionString))
            {
                return new HttpClient();
            }

            var db = await GetDbContext();
            var options = await db.HttpClientOptionses.FirstOrDefaultAsync(t => t.Purpose == purpose);
            HttpClient client;
            if (options != null)
            {
                if (options.UseProxy)
                {
                    var proxyProvider = await GetProxyProvider();
                    var proxy = await proxyProvider.Get(purpose, DateTime.Now.AddDays(-2));
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