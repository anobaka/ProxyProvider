using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProxyProvider.Abstractions;
using ProxyProvider.Abstractions.Models;

namespace ProxyProvider
{
    /// <summary>
    /// Default implementation
    /// </summary>
    public class ProxyProvider : IProxyProvider
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

        protected virtual ProxyProviderDbContext GetDbContext() => new ProxyProviderDbContext(
            new DbContextOptionsBuilder<ProxyProviderDbContext>().UseMySql(_dbConnectionString).Options);

        public async Task AddOrUpdate(IEnumerable<Proxy> proxies)
        {
            var uniqueProxies = proxies.GroupBy(t => t.UniqueKey).Select(t => t.FirstOrDefault()).ToList();

            var db = GetDbContext();
            var keys = uniqueProxies.Select(t => t.UniqueKey).ToList();
            var dbExistedProxies = await db.Proxies.AsNoTracking().Where(t => keys.Contains(t.UniqueKey))
                .ToDictionaryAsync(t => t.UniqueKey, t => t);
            var existedKeys = dbExistedProxies.Keys.ToList();
            var newProxies = uniqueProxies.Where(t => !existedKeys.Contains(t.UniqueKey)).ToList();
            db.Proxies.AddRange(newProxies);
            var tobeUpdatedProxies = uniqueProxies.Where(t => !newProxies.Contains(t)).ToList();
            tobeUpdatedProxies.ForEach(t => { t.Id = dbExistedProxies[t.UniqueKey].Id; });
            foreach (var updProxy in tobeUpdatedProxies)
            {
                db.Attach(updProxy).State = EntityState.Modified;
            }

            await db.SaveChangesAsync();
        }

        public async Task<Proxy> Use(string purpose)
        {
            var db = GetDbContext();
            var now = DateTime.Now;
            //todo: this is temporary mechanism that will be moved out in the future.
            var validCollectDt = now.AddDays(-2);
            var proxy = (await db.Proxies.Where(t => t.Alive && t.UpdateDt > validCollectDt)
                    .Join(db.ProxyUsages.Where(a => a.Purpose == purpose), t => t.Id, t => t.ProxyId,
                        (proxy1, proxyUsage) => new {Proxy = proxy1, ProxyUsage = proxyUsage})
                    .Where(t => t.ProxyUsage == null || t.ProxyUsage.LockReleaseDt < now).FirstOrDefaultAsync())
                ?.Proxy;
            return proxy;
        }
    }
}