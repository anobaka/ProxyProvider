using System.Threading.Tasks;

namespace ProxyProvider.Abstractions.Collectors
{
    public interface IProxyCollector
    {
        Task GetProxies();
    }
}
