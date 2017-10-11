using System.Threading.Tasks;

namespace ProxyProvider.Collectors.Infrastructures
{
    public interface IProxyCollector
    {
        Task GetProxies();
    }
}
