using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProxyProvider.HttpClientProvider
{
    public interface IHttpClientProvider
    {
        Task<HttpClient> GetClient(string purpose = null);
    }
}
