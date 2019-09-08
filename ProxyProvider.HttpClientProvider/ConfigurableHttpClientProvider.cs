using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProxyProvider.Abstractions;
using ProxyProvider.HttpClientProvider.Models;
using ProxyProvider.HttpClientProvider.Models.RequestModels;

namespace ProxyProvider.HttpClientProvider
{
    /// <summary>
    /// Default provider for common usage.
    /// </summary>
    public class ConfigurableHttpClientProvider : IHttpClientProvider
    {
        private readonly string _dbConnectionString;
        private readonly IProxyProvider _proxyProvider;

        /// <summary>
        /// Customize mechanism to get <see cref="HttpClientProviderDbContext"/>
        /// </summary>
        public ConfigurableHttpClientProvider(IProxyProvider proxyProvider)
        {
            _proxyProvider = proxyProvider;
        }

        /// <summary>
        /// Get <see cref="HttpClientProviderDbContext"/> by default mechanism.
        /// </summary>
        /// <param name="dbConnectionString"></param>
        /// <param name="proxyProvider"></param>
        public ConfigurableHttpClientProvider(string dbConnectionString, IProxyProvider proxyProvider)
        {
            _dbConnectionString = dbConnectionString;
            _proxyProvider = proxyProvider;
        }

        /// <summary>
        /// Change the provider if needed
        /// </summary>
        /// <returns></returns>
        protected virtual HttpClientProviderDbContext GetDbContext() => new HttpClientProviderDbContext(
            new DbContextOptionsBuilder<HttpClientProviderDbContext>()
                .UseSqlServer(_dbConnectionString).Options);

        public async Task AddOrUpdatePurposeHttpClientOptions(HttpClientOptionsAddOrUpdateRequestModel model)
        {
            var ctx = GetDbContext();
            var existedOptions =
                await ctx.HttpClientOptions.FirstOrDefaultAsync(t => t.Purpose == model.Purpose);
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
                ctx.HttpClientOptions.Add(existedOptions);
            }
            await ctx.SaveChangesAsync();
        }

        public virtual async Task<HttpClient> GetClient(string purpose)
        {
            if (string.IsNullOrEmpty(_dbConnectionString))
            {
                return new HttpClient();
            }

            var db = GetDbContext();
            var options = await db.HttpClientOptions.FirstOrDefaultAsync(t => t.Purpose == purpose);
            HttpClient client;
            if (options != null)
            {
                if (options.UseProxy)
                {
                    var proxy = await _proxyProvider.Use(purpose);
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