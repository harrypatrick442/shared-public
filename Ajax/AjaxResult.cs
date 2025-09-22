using JSON;
using System;
using System.Net;
namespace Ajax
{
    public class AjaxResult
    {

        private const int DEFAULT_TIMEOUT_READ_PAYLOAD = 10000;
        private AjaxException _AjaxException;
        private IJsonParser _JsonParser;
        private HttpResponseMessage _HttpResponseMessage;
        public AjaxException AjaxException { get { return _AjaxException; } }
        public bool HasExceptions { get { return _AjaxException != null; } }
        public bool SuccessStatusCode { get { return _HttpResponseMessage!=null && ((int)StatusCode >= 200) && ((int)StatusCode < 300); } }
        public bool Successful { get { return !HasExceptions && SuccessStatusCode; } }
        private string _RawPayload;
        public string GetRawPayload (int timeoutMilliseconds = DEFAULT_TIMEOUT_READ_PAYLOAD){
            if (_RawPayload == null) {
                try
                {
                    Task<string> readAsStringAsyncTask = _HttpResponseMessage.Content.ReadAsStringAsync();
                    if (!readAsStringAsyncTask.Wait(timeoutMilliseconds))//For extreme cases but just incase
                    {
                        throw AjaxException.Remote(new TimeoutException());
                    }
                    _RawPayload = readAsStringAsyncTask.Result;
                }
                catch (Exception ex) {
                    throw AjaxException.Remote(ex);
                }
            }
            return _RawPayload;
        }
        public HttpStatusCode StatusCode { 
            get
            {
                return _HttpResponseMessage.StatusCode;
            } 
        }
        public T GetJSONPayload<T>(int timeoutMilliseconds = DEFAULT_TIMEOUT_READ_PAYLOAD) where T:class
        {
            string rawPayload = GetRawPayload(timeoutMilliseconds);
            try
            {
                return Json.Deserialize<T>(rawPayload);
            }
            catch (Exception ex)
            {
                throw AjaxException.ParsingPayload(ex);
            }
        }
        internal AjaxResult(HttpResponseMessage httpResponseMessage, IJsonParser jsonParser)
        {
            _HttpResponseMessage = httpResponseMessage;
            _JsonParser = jsonParser;
        }
        internal AjaxResult(AjaxException ajaxException) {
            _AjaxException = ajaxException;
        }
    }
}
