using Microsoft.EntityFrameworkCore;
using ProxyProvider.Abstractions.Models;

namespace ProxyProvider
{
    /// <summary>
    /// </summary>
    public class ProxyProviderDbContext : DbContext
    {
        public DbSet<Proxy> Proxies { get; set; }
        public DbSet<ProxyUsage> ProxyUsages { get; set; }

        public ProxyProviderDbContext()
        {
        }

        public ProxyProviderDbContext(DbContextOptions<ProxyProviderDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proxy>(t => { t.HasIndex(a => a.UniqueKey).IsUnique(); });
            modelBuilder.Entity<ProxyUsage>(t => { t.HasIndex(a => new {a.ProxyId, a.Purpose}).IsUnique(); });
            base.OnModelCreating(modelBuilder);
        }
    }
}