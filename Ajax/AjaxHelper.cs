using System;
using JSON;
using Shutdown;

namespace Ajax
{
    public class AjaxHelper
    {
        private const int DEFAULT_TIMEOUT_MILLISECONDS = 360000;
        private const string APPLICATION_JSON = "application/json";
        private static readonly CancellationTokenSource _CancellationTokenDisposed = new CancellationTokenSource();
        static AjaxHelper(){
            ShutdownManager.Instance.Add(() => _CancellationTokenDisposed.Cancel(), ShutdownOrder.Ajax);
        }
        public static AjaxResult PostSync<T>(string url, T requestBody,
            IJsonParser jsonParser, Header[] headers = null, int timeoutMilliseconds = DEFAULT_TIMEOUT_MILLISECONDS) where T : class
        {
            StringContent httpContent;
            try
            {
                string requestBodyString = jsonParser.Serialize<T>(requestBody);
                httpContent = new StringContent(requestBodyString, System.Text.Encoding.UTF8, APPLICATION_JSON);
            }
            catch (Exception ex)
            {
                return new AjaxResult(ParseException(ex, withRequest: true));
            }
            return Post(httpContent, url, headers, jsonParser, timeoutMilliseconds);
        }
        public static void PostWithoutWaitingForResponse<T>(string url, T requestBody,
            IJsonParser jsonParser, Header[] headers = null, int timeoutMilliseconds = DEFAULT_TIMEOUT_MILLISECONDS) where T : class
        {
            string requestBodyString = jsonParser.Serialize<T>(requestBody);
            StringContent httpContent = new StringContent(requestBodyString, System.Text.Encoding.UTF8, APPLICATION_JSON);
            PostWithoutWaitingForResponse(httpContent, url, headers, timeoutMilliseconds);
        }/*
        public static Task<AjaxResult> PostAsync<T>(string url, T requestBody,
            IJsonParser jsonParser, Header[] headers = null) where T : class
        {
            return Dispatcher.RunOnFreeThread(() =>
            {
                return PostSync<T>(url, requestBody, jsonParser, headers);
            });
        }
        public static Task<AjaxResult> PostAsync(HttpContent httpContent, string url,
            IJsonParser jsonParser, int timeoutMilliseconds,  Header[] headers = null)
        {
            return Dispatcher.RunOnFreeThread(() =>
            {
                return Post(httpContent, url, headers, jsonParser, timeoutMilliseconds);
            });
        }*/
        public static AjaxResult Post(HttpContent httpContent, string url, Header[] headers,
            IJsonParser jsonParser, int? timeoutMilliseconds)
        {

            try
            {
                var httpClient = new HttpClient();
                if (timeoutMilliseconds != null)
                    httpClient.Timeout = TimeSpan.FromMilliseconds((int)timeoutMilliseconds);
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Post,
                    Content = httpContent,
                };
                AddHeaders(request, headers);
                Task<HttpResponseMessage> task;
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    task = httpClient.SendAsync(request, cancellationTokenSource.Token);
                }
                catch (InvalidOperationException ex)
                {
                    return new AjaxResult(ParseException(ex, withRequest: true));
                }
                if (timeoutMilliseconds == null)
                    task.Wait();
                else
                {
                    if (!task.Wait((int)timeoutMilliseconds))
                    {
                        cancellationTokenSource.Cancel();
                        throw new TimeoutException();
                    }
                }
                HttpResponseMessage httpResponseMessage = task.Result;
                return new AjaxResult(httpResponseMessage, jsonParser);
            }
            catch (AggregateException ex)
            {
                return new AjaxResult(ParseException(ex));
            }
        }
        public static void PostWithoutWaitingForResponse(HttpContent httpContent, string url, Header[] headers,
            int? timeoutMilliseconds)
        {
            var httpClient = new HttpClient();
            if (timeoutMilliseconds != null)
                httpClient.Timeout = TimeSpan.FromMilliseconds((int)timeoutMilliseconds);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Post,
                Content = httpContent,
            };
            AddHeaders(request, headers);
            httpClient.SendAsync(request, _CancellationTokenDisposed.Token);
        }
        /*
        public static Task<AjaxResult> GetAsync(string url,
            IJsonParser jsonParser, Header[] headers, int? timeoutMilliseconds)
        {
            return Dispatcher.RunOnFreeThread(() =>
            {
                return Get(url, jsonParser, headers, timeoutMilliseconds);
            });
        }*/
        public static AjaxResult Get(string url,
            IJsonParser jsonParser,  Header[] headers, int? timeoutMilliseconds)
        {
            var httpClient = new HttpClient();
            try
            {
                var request = new HttpRequestMessage()
                {   
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get,
                };
                AddHeaders(request, headers);
                Task<HttpResponseMessage> task; 
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    task = httpClient.SendAsync(request, cancellationTokenSource.Token);
                }
                catch (InvalidOperationException ex)
                {
                    return new AjaxResult(ParseException(ex, withRequest: true));
                }
                if (timeoutMilliseconds == null)
                    task.Wait();
                else
                {
                    if (!task.Wait((int)timeoutMilliseconds))
                    {
                        cancellationTokenSource.Cancel();
                        throw new TimeoutException();
                    }
                }
                HttpResponseMessage httpResponseMessage = task.Result;
                return new AjaxResult(httpResponseMessage, jsonParser);
            }
            catch (AggregateException ex)
            {
                return new AjaxResult(ParseException(ex));
            }
        }
        private static void AddHeaders(HttpRequestMessage httpRequestMessage, Header[] headers)
        {
            if (headers == null) return;
            foreach (Header header in headers)
            {
                if (!header.MultipleValues)
                    httpRequestMessage.Headers.Add(header.Name, header.Value);
                else
                    httpRequestMessage.Headers.Add(header.Name, header.Values);
            }
        }
        private static AjaxException ParseException(Exception ex, bool withRequest = false)
        {
            if (withRequest)
            {
                return AjaxException.Request(ex);
            }
            if (typeof(HttpRequestException).IsAssignableFrom(ex.GetType()))
            {
                return AjaxException.Remote(ex);
            }
            if (typeof(TaskCanceledException).IsAssignableFrom(ex.GetType()))
            {
                return AjaxException.Cancelled(ex);
            }
            return AjaxException.Internal(ex);
        }
    }
}

