using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsQuery;
using Microsoft.Extensions.Logging;
using ProxyProvider.Models;
using ProxyProvider.SampleCollectors.Infrastructures;

namespace ProxyProvider.SampleCollectors
{
    public class XiCiDaiLiProxyCollector : AbstractProxyCollector
    {
        protected XiCiDaiLiProxyCollector(ProxyCollectorOptions options, ILoggerFactory loggerFactory) : base(options, loggerFactory)
        {
        }

        protected override async Task<List<Proxy>> GetProxies(CQ cq)
        {
            var trs = cq["#ip_list>tbody>tr"];
            var proxies = trs.Skip(1).Select(t =>
                {
                    try
                    {
                        var tds = t.Cq().Children("td");
                        var p = new Proxy
                        {
                            Alive = true,
                            CreateDt = DateTime.Now,
                            Ip = tds[1].InnerText,
                            Port = tds[2].InnerText,
                            Type = tds[4].InnerText == "高匿" ? ProxyType.HighAnonymous : 0,
                            Schema = tds[5].InnerText,
                            Speed = (int) (double.Parse(Regex
                                               .Match(tds[6].Cq().Children()[0].GetAttribute("title"),
                                                   "(\\d+\\.)?(\\d+)")
                                               .Value) * 1000),
                            ConnectionSpeed =
                                (int) (double.Parse(
                                           Regex.Match(tds[7].Cq().Children()[0].GetAttribute("title"),
                                                   "(\\d+\\.)?(\\d+)")
                                               .Value) *
                                       1000),
                            UpdateDt =
                                DateTime.ParseExact(tds[9].InnerText, "yy-MM-dd HH:mm", DateTimeFormatInfo.CurrentInfo)
                        };
                        return p;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(EventId,
                            $"Error occured during parsing response: {e.Message}, stack trace: {e.StackTrace}");
                        return null;
                    }
                }
            ).ToList();
            return proxies;
        }
    }
}