using Microsoft.EntityFrameworkCore;
using ProxyProvider.Abstractions.Models;

namespace ProxyProvider.Abstractions
{
    /// <summary>
    /// </summary>
    public class ProxyProviderDbContext : DbContext
    {
        public DbSet<Proxy> Proxies { get; set; }

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
            base.OnModelCreating(modelBuilder);
        }
    }
}
