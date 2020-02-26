using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redpill.HttpClientExtensions.DelegatingHandlers {
    /// <summary>
    /// Enables HttpClient to use Http/2 for connection by default
    /// </summary>
    public class Http2DelegatingHandler : DelegatingHandler {
        protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken) {
            // only attempt http/2 for HTTPS connections
            if (request.RequestUri.Scheme == Uri.UriSchemeHttps) {
                request.Version = new Version (2, 0);
            }

            return base.SendAsync (request, cancellationToken);
        }
    }
}