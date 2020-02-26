using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Redpill.HttpClientExtensions {
    public class DefaultClientHandler : HttpClientHandler {
#if NETCOREAPP3_1
        public DefaultClientHandler () => AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
#else
        public DefaultClientHandler () => AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
#endif
    }
}