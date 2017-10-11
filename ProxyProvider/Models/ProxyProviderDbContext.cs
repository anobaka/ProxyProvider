using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ProxyProvider.Models
{
    public class ProxyProviderDbContext : DbContext
    {
        public DbSet<Proxy> Proxies { get; set; }
        public DbSet<ProxyUsage> ProxyUsages { get; set; }
        public DbSet<PurposeHttpClientOptions> PurposeClientOptionses { get; set; }

        public ProxyProviderDbContext()
        {
        }

        public ProxyProviderDbContext(DbContextOptions<ProxyProviderDbContext> options) : base(options)
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
