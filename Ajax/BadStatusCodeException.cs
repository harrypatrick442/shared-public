using Snippets.BrochureGeneratorClient.Core.Interfaces;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Ajax
{
    [DataContract]
    public class BadStatusCodeException:Exception, IGetResponseBody, IGetRequestUri
    {
        private HttpResponseMessage _HttpResponseMessage;
        private HttpStatusCode _HttpStatusCode;
        [JsonPropertyName("httpStatusCode")]
        [JsonInclude]
        [DataMember(Name= "httpStatusCode")]
        public HttpStatusCode HttpStatusCode { get{ return _HttpStatusCode; } }
        public string GetResponseBody() {
            if (_HttpResponseMessage == null) return null;
            if (_HttpResponseMessage.Content == null) return null;
            Task<string> task = _HttpResponseMessage.Content.ReadAsStringAsync();
            task.Wait();
            return task.Result;
        }
        public Uri RequestUri { get {
                if (_HttpResponseMessage == null) return null;
                if (_HttpResponseMessage.RequestMessage == null) return null;
                return _HttpResponseMessage.RequestMessage.RequestUri;
            } }
        public BadStatusCodeException(HttpStatusCode httpStatusCode)
        {
            _HttpStatusCode = httpStatusCode;
        }
        public BadStatusCodeException(HttpStatusCode httpStatusCode, string message) : base(message)
        {
            _HttpStatusCode = httpStatusCode;
        }
        public BadStatusCodeException(HttpStatusCode httpStatusCode, string message, Exception innerException) : base(message, innerException)
        {
            _HttpStatusCode = httpStatusCode;
        }
        public BadStatusCodeException(HttpResponseMessage httpResponseMessage) : base()
        {
            _HttpResponseMessage = httpResponseMessage;
            _HttpStatusCode = httpResponseMessage.StatusCode;
        }
        protected BadStatusCodeException() { 
            
        }
        public override string ToString()
        {
            return $"Bad status code \"{HttpStatusCode}\" {(int)HttpStatusCode}";
        }
    }
}
