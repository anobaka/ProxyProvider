using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsQuery;
using Microsoft.Extensions.Logging;
using ProxyProvider.Abstractions.Models;
using ProxyProvider.SampleCollectors.Infrastructures;

namespace ProxyProvider.SampleCollectors
{
    public class KuaiDaiLiProxyCollector : AbstractProxyCollector
    {
        protected KuaiDaiLiProxyCollector(ProxyCollectorOptions options, ILoggerFactory loggerFactory) : base(options, loggerFactory)
        {
        }

        protected override async Task<List<Proxy>> GetProxies(CQ cq)
        {
            return cq["#list table tbody>tr"].Select(t =>
            {
                var properties = t.ChildElements.ToDictionary(a => a.GetAttribute("data-title"),
                    a => a.InnerText, StringComparer.OrdinalIgnoreCase);
                var proxy = new Proxy
                {
                    Ip = properties["IP"],
                    Port = properties["Port"],
                    UpdateDt = DateTime.Parse(properties["最后验证时间"]),
                    Alive = true,
                    Speed = (int) (double.Parse(Regex.Match(properties["响应速度"], "(\\d+\\.)?(\\d+)").Value) *
                                   1000),
                    Source = GetType().Name,
                    Schema = properties["类型"],
                    Type = ProxyType.HighAnonymous,
                };
                return proxy;
            }).ToList();
        }

        
    }
}