using System.Threading.Tasks;

namespace ProxyProvider.Collectors
{
    public interface IProxyCollector
    {
        Task GetProxies();
    }
}
