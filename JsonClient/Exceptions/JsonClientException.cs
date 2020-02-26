using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Redpill.JsonClient.Exceptions {
    public class JsonClientException : Exception {
        public int StatusCode { get; set; }
        public string Response { get; set; }

        public JsonClientException (string message, int statusCode, string response, Exception innerException = null) : base (message + "\n\nStatus: " + statusCode + "\nResponse: \n" + response == null ? "" :
            response.Substring (0, response.Length >= 512 ? 512 : response.Length), innerException) {
            StatusCode = statusCode;
            Response = response;
        }

        public override string ToString () {
            return $"Http Response: \n\n{Response}\n\n{base.ToString()}";
        }
    }
}