using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Redpill.JsonClient.Exceptions;

namespace Redpill.JsonClient {
    /// <summary>
    /// HttpClient extensions for working with Json objects
    /// </summary>
    public static class JsonClientExtensions {
        private static readonly JsonSerializerOptions _defaultJsonOptions = new JsonSerializerOptions ();

        public static Task<TResult> GetJsonAsync<TResult> (this HttpClient client, string path, CancellationToken cancellationToken = default) {
            return client.GetJsonAsync<TResult> (path, _defaultJsonOptions, cancellationToken);
        }

        public static async Task<TResult> GetJsonAsync<TResult> (this HttpClient client, string path, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken = default) {
            using var request = new HttpRequestMessage (HttpMethod.Get, path);
            request.Headers.Clear ();
            request.Headers.Accept.Add (new MediaTypeWithQualityHeaderValue ("application/json"));

            using var response = await client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            await response.ValidateSuccessfulStatusCode ().ConfigureAwait (false);

            return await response.DeserializeAsync<TResult> (jsonOptions, cancellationToken);
        }

        public static async Task<TResult> SendJsonAsync<TResult, TInput> (this HttpClient client, string path, HttpMethod method, TInput data, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken = default) {
            using var request = new HttpRequestMessage (method, path);
            request.Headers.Clear ();
            request.Headers.Accept.Add (new MediaTypeWithQualityHeaderValue ("application/json"));
            if (data != null) {
                try {
                    var ms = new MemoryStream ();
                    await JsonSerializer.SerializeAsync (ms, data, jsonOptions, cancellationToken).ConfigureAwait (false);

                    request.Content = new StreamContent (ms);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue ("application/json");
                } catch (JsonException e) {
                    string message = "Could not serialize input object of type " + typeof (TResult).FullName + " to json";
                    throw new JsonClientException (message, 0, null, e);
                }
            }

            using var response = await client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait (false);

            await response.ValidateSuccessfulStatusCode ().ConfigureAwait (false);

            return await response.DeserializeAsync<TResult> (jsonOptions, cancellationToken).ConfigureAwait (false);
        }

        public static Task<TResult> SendJsonAsync<TResult, TInput> (this HttpClient client, string path, HttpMethod method, TInput data, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<TResult, TInput> (path, method, data, _defaultJsonOptions, cancellationToken);
        }

        public static Task<T> SendJsonAsync<T> (this HttpClient client, string path, HttpMethod method, T data, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<T, T> (path, method, data, _defaultJsonOptions, cancellationToken);
        }

        public static Task<T> SendJsonAsync<T> (this HttpClient client, string path, HttpMethod method, T data, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<T, T> (path, method, data, jsonOptions, cancellationToken);
        }

        public static Task<T> PostJsonAsync<T> (this HttpClient client, string path, T data, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken = default) {
            return client.PostJsonAsync<T, T> (path, data, jsonOptions, cancellationToken);
        }
        public static Task<TResult> PostJsonAsync<TResult, TInput> (this HttpClient client, string path, TInput data, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<TResult, TInput> (path, HttpMethod.Post, data, jsonOptions, cancellationToken);
        }

        public static Task<T> PostJsonAsync<T> (this HttpClient client, string path, T data, CancellationToken cancellationToken = default) {
            return client.PostJsonAsync<T, T> (path, data, cancellationToken);
        }

        public static Task<TResult> PostJsonAsync<TResult, TInput> (this HttpClient client, string path, TInput data, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<TResult, TInput> (path, HttpMethod.Post, data, cancellationToken);
        }
        public static Task<T> PutJsonAsync<T> (this HttpClient client, string path, T data, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken = default) {
            return client.PutJsonAsync<T, T> (path, data, jsonOptions, cancellationToken);
        }
        public static Task<TResult> PutJsonAsync<TResult, TInput> (this HttpClient client, string path, TInput data, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<TResult, TInput> (path, HttpMethod.Put, data, jsonOptions, cancellationToken);
        }

        public static Task<T> PutJsonAsync<T> (this HttpClient client, string path, T data, CancellationToken cancellationToken = default) {
            return client.PutJsonAsync<T, T> (path, data, cancellationToken);
        }

        public static Task<TResult> PutJsonAsync<TResult, TInput> (this HttpClient client, string path, TInput data, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<TResult, TInput> (path, HttpMethod.Delete, data, cancellationToken);
        }

        public static Task<T> DeleteJsonAsync<T> (this HttpClient client, string path, T data, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<T, T> (path, HttpMethod.Delete, data, jsonOptions, cancellationToken);
        }
        public static Task<TResult> DeleteJsonAsync<TResult, TInput> (this HttpClient client, string path, TInput data, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<TResult, TInput> (path, HttpMethod.Delete, data, jsonOptions, cancellationToken);
        }

        public static Task<T> DeleteJsonAsync<T> (this HttpClient client, string path, T data, CancellationToken cancellationToken = default) {
            return client.DeleteJsonAsync<T, T> (path, data, cancellationToken);
        }
        public static Task<TResult> DeleteJsonAsync<TResult, TInput> (this HttpClient client, string path, TInput data, CancellationToken cancellationToken = default) {
            return client.SendJsonAsync<TResult, TInput> (path, HttpMethod.Delete, data, cancellationToken);
        }

        /// <summary>
        /// Deserializes the response content stream to the resulting type
        /// Throws an ApiJsonException if a JsonException is thrown during deserialization
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="response"></param>
        /// <param name="jsonOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<TResult> DeserializeAsync<TResult> (this HttpResponseMessage response, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken) {
            if (response.Content == null || response.Content.Headers?.ContentLength == 0) return default;

            using var stream = await response.Content.ReadAsStreamAsync ().ConfigureAwait (false);
            if (stream == null) return default;

            try {
                return await JsonSerializer.DeserializeAsync<TResult> (stream, jsonOptions, cancellationToken).ConfigureAwait (false);
            } catch (JsonException e) {
                string content = null;
                if (response.Content != null) {
                    content = await response.Content.ReadAsStringAsync ().ConfigureAwait (false);
                }
                string message = "Could not deserialize the response body stream as " + typeof (TResult).FullName;
                throw new JsonClientException (message, (int) response.StatusCode, content, e);
            }
        }

        /// <summary>
        /// Validates that a successful status code was returned.
        /// Throws an ApiException if the response was not successful
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static async Task ValidateSuccessfulStatusCode (this HttpResponseMessage response) {
            if (response.IsSuccessStatusCode) {
                return;
            }

            string content = null;
            if (response.Content != null) {
                content = await response.Content.ReadAsStringAsync ().ConfigureAwait (false);
            }
            var message = "THe HTTP status code of the response was not expected (" + response.StatusCode + ").";
            throw new JsonClientException (message, (int) response.StatusCode, content);
        }
    }
}