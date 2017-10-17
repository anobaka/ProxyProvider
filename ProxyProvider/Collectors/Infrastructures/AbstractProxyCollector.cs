using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using CsQuery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProxyProvider.Models;

namespace ProxyProvider.Collectors.Infrastructures
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
        protected virtual async Task<DefaultProxyProviderDbContext> GetDbContext() => new DefaultProxyProviderDbContext(
            new DbContextOptionsBuilder<DefaultProxyProviderDbContext>().UseMySql(_options.ConnectionString).Options);

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
                            proxies.RemoveAll(t => (now - t.LastCheckDt).Days > 1);
                            Logger.LogInformation(EventId, $"[Page-{page}]Got proxies: {proxies.Count}");
                            if (proxies.Any())
                            {
                                var newProxyUniqueKeys = new HashSet<string>();
                                var uniqueProxies = new List<Proxy>();
                                foreach (var proxy in proxies)
                                {
                                    var key = $"{proxy.Ip}:{proxy.Port}";
                                    if (!newProxyUniqueKeys.Contains(key))
                                    {
                                        uniqueProxies.Add(proxy);
                                        newProxyUniqueKeys.Add(key);
                                    }
                                }

                                var db = await GetDbContext();
                                var dbExistedProxies = await db.Proxies.AsNoTracking()
                                    .Where(t => uniqueProxies.Any(a => a.Port == t.Port && a.Ip == t.Ip))
                                    .ToDictionaryAsync(t => $"{t.Ip}:{t.Port}", t => t);
                                var newProxies = uniqueProxies
                                    .Where(t => !dbExistedProxies.ContainsKey($"{t.Ip}:{t.Port}")).ToList();
                                var tobeUpdatedProxies = uniqueProxies.Where(t => !newProxies.Contains(t)).ToList();
                                foreach (var tbProxies in tobeUpdatedProxies)
                                {
                                    tbProxies.Id = dbExistedProxies[$"{tbProxies.Ip}:{tbProxies.Port}"].Id;
                                }
                                db.Proxies.AddRange(newProxies);
                                foreach (var updProxy in tobeUpdatedProxies)
                                {
                                    db.Attach(updProxy).State = EntityState.Modified;
                                }
                                await db.SaveChangesAsync();
                                Logger.LogInformation(EventId,
                                    $"[Page-{page}]Added proxies: {newProxies.Count}, updated proxies: {tobeUpdatedProxies.Count}");
                                page++;
                                continue;
                            }
                        }
                        Logger.LogInformation(EventId, $"[Page-{page}]None proxies to be saved, job finished");
                        break;
                    }
                    catch (Exception e)
                    {
                        error++;
                        Logger.LogError(EventId, $"[Page-{page}]{e.Message}{Environment.NewLine}{e.StackTrace}");
                    }
                    if (error > 10)
                    {
                        Logger.LogError(EventId, $"[Page-{page}]Error count is over 10, giving up");
                        break;
                    }
                }
            }
        }
    }
}