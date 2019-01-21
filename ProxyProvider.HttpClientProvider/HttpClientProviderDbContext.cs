using Microsoft.EntityFrameworkCore;
using ProxyProvider.HttpClientProvider.Models;

namespace ProxyProvider.HttpClientProvider
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpClientProviderDbContext : DbContext
    {
        public DbSet<HttpClientOptions> HttpClientOptions { get; set; }

        protected HttpClientProviderDbContext()
        {
        }

        public HttpClientProviderDbContext(DbContextOptions<HttpClientProviderDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
