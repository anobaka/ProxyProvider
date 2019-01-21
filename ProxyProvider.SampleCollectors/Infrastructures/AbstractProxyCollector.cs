using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsQuery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProxyProvider.Abstractions.Collectors;
using ProxyProvider.Abstractions.Models;

namespace ProxyProvider.SampleCollectors.Infrastructures
{
    public abstract class AbstractProxyCollector : IProxyCollector
    {
        private readonly ProxyCollectorOptions _options;
        protected ILogger Logger;
        protected EventId EventId;

        protected AbstractProxyCollector(ProxyCollectorOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            Logger = loggerFactory.CreateLogger(GetType());
            EventId = new EventId(0, GetType().Name);
        }

        /// <summary>
        /// Change the provider if needed
        /// </summary>
        /// <returns></returns>
        protected virtual Task<ProxyProvider> GetProxyProvider() =>
            Task.FromResult(new ProxyProvider(_options.ConnectionString));

        protected abstract Task<List<Proxy>> GetProxies(CQ cq);

        public virtual async Task GetProxies()
        {
            Logger.LogInformation(EventId, $"Getting proxies job starts.");
            foreach (var urlTemplateAndPage in _options.UrlTemplateAndStartPages)
            {
                var page = urlTemplateAndPage.Value;
                var error = 0;
                while (true)
                {
                    await Task.Delay(_options.Interval);
                    var client = new HttpClient
                    {
                        DefaultRequestHeaders =
                        {
                            {
                                "User-Agent",
                                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36"
                            }
                        }
                    };
                    try
                    {
                        var url = string.Format(urlTemplateAndPage.Key, page);
                        var cq = new CQ(await client.GetStringAsync(url));
                        Logger.LogInformation(EventId, $"[Page-{page}]Getting proxies: {url}");
                        var proxies = await GetProxies(cq);
                        if (proxies?.Any() == true)
                        {
                            var now = DateTime.Now;
                            proxies.RemoveAll(t => (now - t.UpdateDt).Days > 1);
                            Logger.LogInformation(EventId, $"[Page-{page}]Got proxies: {proxies.Count}");
                            if (proxies.Any())
                            {
                                var pp = await GetProxyProvider();
                                await pp.AddOrUpdate(proxies);
                                Logger.LogInformation(EventId,
                                    $"[Page-{page}]Complete");
                                page++;
                                continue;
                            }
                        }
                        Logger.LogInformation(EventId, $"[Page-{page}]None proxies found, job complete");
                        break;
                    }
                    catch (Exception e)
                    {
                        error++;
                        Logger.LogError(EventId, $"[Page-{page}]{e.Message}{Environment.NewLine}{e.StackTrace}");
                    }
                    if (error > 10)
                    {
                        Logger.LogError(EventId, $"[Page-{page}]Error occured more than 10 times, giving up");
                        break;
                    }
                }
            }
        }
    }
}