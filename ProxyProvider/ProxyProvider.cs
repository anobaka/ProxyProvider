using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProxyProvider.Models;

namespace ProxyProvider
{
    public class ProxyProvider
    {
        private readonly string _dbConnectionString;

        /// <summary>
        /// Customize mechanism to get <see cref="ProxyProviderDbContext"/>
        /// </summary>
        public ProxyProvider()
        {
        }

        /// <summary>
        /// Get <see cref="ProxyProviderDbContext"/> by default mechanism.
        /// </summary>
        /// <param name="dbConnectionString"></param>
        public ProxyProvider(string dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
        }

        protected virtual Task<ProxyProviderDbContext> GetDbContext() => Task.FromResult(new ProxyProviderDbContext(
            new DbContextOptionsBuilder<ProxyProviderDbContext>().UseMySql(_dbConnectionString).Options));

        public async Task AddOrUpdate(List<Proxy> proxies)
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
        }

        public async Task<Proxy> Get(string purpose, DateTime? collectedDtAfter = null)
        {
            var db = await GetDbContext();
            var now = DateTime.Now;
            var proxy = (await db.Proxies.Where(t =>
                        t.Alive && (!collectedDtAfter.HasValue || t.UpdateDt > collectedDtAfter.Value))
                    .Join(db.ProxyUsages.Where(a => a.Purpose == purpose), t => t.Id, t => t.ProxyId,
                        (proxy1, proxyUsage) => new {Proxy = proxy1, ProxyUsage = proxyUsage})
                    .Where(t => t.ProxyUsage == null || t.ProxyUsage.LockReleaseDt < now).FirstOrDefaultAsync())
                ?.Proxy;
            return proxy;
        }
    }
}