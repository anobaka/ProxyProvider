using System.Collections.Generic;

namespace ProxyProvider.Collectors.Infrastructures
{
    public class ProxyCollectorOptions
    {
        public string ConnectionString { get; set; }
        public Dictionary<string, int> UrlTemplateAndStartPages { get; set; }
        public int Interval { get; set; }
    }
}
