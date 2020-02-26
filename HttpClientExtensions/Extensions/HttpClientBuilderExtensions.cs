using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Redpill.HttpClientExtensions.DelegatingHandlers;

namespace Redpill.HttpClientExtensions.Extensions {
    public static class HttpClientBuilderExtensions {
        public static IHttpClientBuilder AddCompression (this IHttpClientBuilder builder) {
            return builder.ConfigurePrimaryHttpMessageHandler (() => new DefaultClientHandler ());
        }

        public static IHttpClientBuilder AddHttp2Support (this IHttpClientBuilder builder) {
            return builder.AddHttpMessageHandler<Http2DelegatingHandler> ();
        }

        public static IHttpClientBuilder OptimizeClient (this IHttpClientBuilder builder) {
            return builder.AddCompression ()
                .AddHttp2Support ();
        }

        public static IHttpClientBuilder AddOptimizedHttpClient<TClient, TImplementation>(
            this IServiceCollection services,
            Action<HttpClient> configureClient = null
            )
            where TClient : class
            where TImplementation : class, TClient =>
            services
                .AddHttpClient<TClient, TImplementation>()
                .ConfigureHttpClient((sp, options) =>
                {
                    options.DefaultRequestVersion = new Version(2, 0);
                    configureClient?.Invoke(options);
                })
                .ConfigurePrimaryHttpMessageHandler(x => new DefaultClientHandler());

        public static IHttpClientBuilder AddOptimizedHttpClient<TClient>(
            this IServiceCollection services,
            Action<HttpClient> configureClient = null
            )
            where TClient : class =>
            services
                .AddHttpClient<TClient>()
                .ConfigureHttpClient((sp, options) =>
                {
                    options.DefaultRequestVersion = new Version(2, 0);
                    configureClient?.Invoke(options);
                })
                .ConfigurePrimaryHttpMessageHandler(x => new DefaultClientHandler());
    }
}