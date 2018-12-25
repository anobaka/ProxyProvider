using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProxyProvider.HttpClientProvider
{
    public class StaticHttpClientProvider : IHttpClientProvider
    {
        private readonly HttpClient _client;

        public StaticHttpClientProvider() : this(null)
        {
        }

        public StaticHttpClientProvider(HttpClient client)
        {
            _client = client ?? new HttpClient();
        }


        public Task<HttpClient> GetClient(string purpose = null)
        {
            return Task.FromResult(_client);
        }
    }
}
