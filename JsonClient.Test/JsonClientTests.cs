using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JsonClient.Test.TestData;
using Moq;
using Moq.Protected;
using Redpill.JsonClient;
using Redpill.JsonClient.Exceptions;
using Xunit;

namespace JsonClient.Test {
    public class JsonClientTests : IClassFixture<TestFixture> {
        private readonly TestFixture _fixture;

        public JsonClientTests (TestFixture fixture) {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetShouldDeserializeCorrectly () {
            var httpMessageHandler = new Mock<HttpMessageHandler> ();
            var testObject = _fixture.CreatePerson ();
            var json = JsonSerializer.Serialize (testObject);

            httpMessageHandler.Protected ()
                .Setup<Task<HttpResponseMessage>> ("SendAsync", ItExpr.IsAny<HttpRequestMessage> (),
                    ItExpr.IsAny<CancellationToken> ())
                .Returns (Task.FromResult (new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                        Content = new StringContent (json)
                }));

            var client = new HttpClient (httpMessageHandler.Object);
            client.BaseAddress = new Uri ("https://www.boo.com");

            // Act
            var result = await client.GetJsonAsync<Person> ("");

            // Assert
            Assert.Equal (testObject.Id, result.Id);
            Assert.NotEmpty (testObject.Children);
            Assert.Equal (testObject.Age, result.Age);
            Assert.Equal (testObject.Name, result.Name);
            Assert.Equal (testObject.Children[0].Id, result.Children[0].Id);
            Assert.Equal (testObject.Children.Count, result.Children.Count);
        }

        [Fact]
        public async Task GetWithNonSuccessfulResponseCodeShouldThrowException () {
            var httpMessageHandler = new Mock<HttpMessageHandler> ();

            httpMessageHandler.Protected ()
                .Setup<Task<HttpResponseMessage>> ("SendAsync", ItExpr.IsAny<HttpRequestMessage> (),
                    ItExpr.IsAny<CancellationToken> ())
                .Returns (Task.FromResult (new HttpResponseMessage {
                    StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent ("error!")
                }));

            var client = new HttpClient (httpMessageHandler.Object);
            client.BaseAddress = new Uri ("https://www.boo.com");

            // Act
            var result = await Assert.ThrowsAsync<JsonClientException> (() => client.GetJsonAsync<Person> (""));

            // Assert
            Assert.Equal (400, result.StatusCode);
            Assert.Equal ("error!", result.Response);
        }

        [Fact]
        public async Task GetWithInvalidReturnJsonShouldThrowException () {
            var httpMessageHandler = new Mock<HttpMessageHandler> ();

            httpMessageHandler.Protected ()
                .Setup<Task<HttpResponseMessage>> ("SendAsync", ItExpr.IsAny<HttpRequestMessage> (),
                    ItExpr.IsAny<CancellationToken> ())
                .Returns (Task.FromResult (new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                        Content = new StringContent ("<html></html>")
                }));

            var client = new HttpClient (httpMessageHandler.Object);
            client.BaseAddress = new Uri ("https://www.boo.com");

            // Act
            var result = await Assert.ThrowsAsync<JsonClientException> (() => client.GetJsonAsync<Person> (""));

            // Assert
            Assert.NotNull (result.InnerException);
            Assert.IsType<JsonException> (result.InnerException);
        }

        [Fact]
        public async Task GetWithIncompatibleReturnJsonShouldThrowException () {
            var httpMessageHandler = new Mock<HttpMessageHandler> ();

            httpMessageHandler.Protected ()
                .Setup<Task<HttpResponseMessage>> ("SendAsync", ItExpr.IsAny<HttpRequestMessage> (),
                    ItExpr.IsAny<CancellationToken> ())
                .Returns (Task.FromResult (new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                        Content = new StringContent ("{ test: 'boo', foo: 4, val: true }")
                }));

            var client = new HttpClient (httpMessageHandler.Object);
            client.BaseAddress = new Uri ("https://www.boo.com");

            // Act
            var result = await Assert.ThrowsAsync<JsonClientException> (() => client.GetJsonAsync<Person> (""));

            // Assert
            Assert.NotNull (result.InnerException);
            Assert.IsType<JsonException> (result.InnerException);
        }

        [Fact]
        public async Task PostShouldDeserializeCorrectly () {
            var httpMessageHandler = new Mock<HttpMessageHandler> ();
            var testObject = _fixture.CreatePerson ();
            var json = JsonSerializer.Serialize (testObject);

            httpMessageHandler.Protected ()
                .Setup<Task<HttpResponseMessage>> ("SendAsync", ItExpr.IsAny<HttpRequestMessage> (),
                    ItExpr.IsAny<CancellationToken> ())
                .Returns (Task.FromResult (new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                        Content = new StringContent (json)
                }));

            var client = new HttpClient (httpMessageHandler.Object);
            client.BaseAddress = new Uri ("https://www.boo.com");

            // Act
            var result = await client.PostJsonAsync<Person>("persons", testObject);

            // Assert
            Assert.Equal (testObject.Id, result.Id);
            Assert.NotEmpty (testObject.Children);
            Assert.Equal (testObject.Age, result.Age);
            Assert.Equal (testObject.Name, result.Name);
            Assert.Equal (testObject.Children[0].Id, result.Children[0].Id);
            Assert.Equal (testObject.Children.Count, result.Children.Count);
        }

        [Fact]
        public async Task PostWithInvalidInputJsonShouldThrowException () {
            var httpMessageHandler = new Mock<HttpMessageHandler> ();

            httpMessageHandler.Protected ()
                .Setup<Task<HttpResponseMessage>> ("SendAsync", ItExpr.IsAny<HttpRequestMessage> (),
                    ItExpr.IsAny<CancellationToken> ())
                .Returns (Task.FromResult (new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                        Content = new StringContent ("{ test: 'boo', foo: 4, val: true }")
                }));

            var client = new HttpClient (httpMessageHandler.Object);
            client.BaseAddress = new Uri ("https://www.boo.com");

            // Act
            var result = await Assert.ThrowsAsync<JsonClientException> (() => client.PostJsonAsync<string> ("test", "{}"));

            // Assert
            Assert.NotNull (result.InnerException);
            Assert.IsType<JsonException> (result.InnerException);
        }

        [Fact]
        public async Task GetHttpExceptionShouldThrowOriginalException () {
            var httpMessageHandler = new Mock<HttpMessageHandler> ();
            var testObject = _fixture.CreatePerson ();
            var json = JsonSerializer.Serialize (testObject);

            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException()); 

            var client = new HttpClient (httpMessageHandler.Object);
            client.BaseAddress = new Uri ("https://www.boo.com");

            // Act
            var result = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetJsonAsync<Person> (""));

            // Assert
        }

        [Fact]
        public async Task PutShouldDeserializeCorrectly () {
            var httpMessageHandler = new Mock<HttpMessageHandler> ();
            var testObject = _fixture.CreatePerson ();
            var json = JsonSerializer.Serialize (testObject);

            httpMessageHandler.Protected ()
                .Setup<Task<HttpResponseMessage>> ("SendAsync", ItExpr.IsAny<HttpRequestMessage> (),
                    ItExpr.IsAny<CancellationToken> ())
                .Returns (Task.FromResult (new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                        Content = new StringContent (json)
                }));

            var client = new HttpClient (httpMessageHandler.Object);
            client.BaseAddress = new Uri ("https://www.boo.com");

            // Act
            var result = await client.PutJsonAsync<Person>("persons", testObject);

            // Assert
            Assert.Equal (testObject.Id, result.Id);
            Assert.NotEmpty (testObject.Children);
            Assert.Equal (testObject.Age, result.Age);
            Assert.Equal (testObject.Name, result.Name);
            Assert.Equal (testObject.Children[0].Id, result.Children[0].Id);
            Assert.Equal (testObject.Children.Count, result.Children.Count);
        }

        [Fact]
        public async Task DeleteShouldDeserializeCorrectly () {
            var httpMessageHandler = new Mock<HttpMessageHandler> ();
            var testObject = _fixture.CreatePerson ();
            var json = JsonSerializer.Serialize (testObject);

            httpMessageHandler.Protected ()
                .Setup<Task<HttpResponseMessage>> ("SendAsync", ItExpr.IsAny<HttpRequestMessage> (),
                    ItExpr.IsAny<CancellationToken> ())
                .Returns (Task.FromResult (new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                        Content = new StringContent (json)
                }));

            var client = new HttpClient (httpMessageHandler.Object);
            client.BaseAddress = new Uri ("https://www.boo.com");

            // Act
            var result = await client.DeleteJsonAsync<Person>("persons", testObject);

            // Assert
            Assert.Equal (testObject.Id, result.Id);
            Assert.NotEmpty (testObject.Children);
            Assert.Equal (testObject.Age, result.Age);
            Assert.Equal (testObject.Name, result.Name);
            Assert.Equal (testObject.Children[0].Id, result.Children[0].Id);
            Assert.Equal (testObject.Children.Count, result.Children.Count);
        }
    }
}