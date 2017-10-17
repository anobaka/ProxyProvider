using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ProxyProvider.Models
{
    /// <summary>
    /// TODO: This class is used for generating migrations quickly, and shouldn't be included by design.
    /// </summary>
    public class DefaultProxyProviderDbContext : DbContext
    {
        public DbSet<Proxy> Proxies { get; set; }
        public DbSet<ProxyUsage> ProxyUsages { get; set; }
        public DbSet<PurposeHttpClientOptions> PurposeClientOptionses { get; set; }

        protected DefaultProxyProviderDbContext()
        {
        }

        public DefaultProxyProviderDbContext(DbContextOptions<DefaultProxyProviderDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proxy>(t =>
            {
                t.HasIndex(a => new {a.Ip, a.Port}).IsUnique();
            });
            modelBuilder.Entity<ProxyUsage>(t =>
            {
                t.HasIndex(a => new {a.ProxyId, a.Purpose}).IsUnique();
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
