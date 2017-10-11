using System;
using System.Net;

namespace ProxyProvider.Infrastructures
{
    public class CoreWebProxy : IWebProxy
    {
        public readonly Uri Uri;
        private readonly bool _bypass;

        public CoreWebProxy(Uri uri, ICredentials credentials = null, bool bypass = false)
        {
            Uri = uri;
            _bypass = bypass;
            Credentials = credentials;
        }

        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination) => Uri;

        public bool IsBypassed(Uri host) => _bypass;

        public override int GetHashCode()
        {
            if (Uri == null)
            {
                return -1;
            }

            return Uri.GetHashCode();
        }
    }
}
