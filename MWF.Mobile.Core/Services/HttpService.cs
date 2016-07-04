using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ModernHttpClient;

namespace MWF.Mobile.Core.Services
{

    public class HttpService
        : IHttpService
    {

        public async Task<HttpResult<TResponse>> PostJsonAsync<TResponse>(string jsonContent, string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                return await SendAsync<TResponse>(request);
            }
        }

        public async Task<HttpResult<TResponse>> PostAsJsonAsync<TRequest, TResponse>(TRequest content, string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Content = new ObjectContent<TRequest>(content, GetJsonFormatter());
                return await SendAsync<TResponse>(request);
            }
        }

        public async Task<HttpResult<TResponse>> SendAsync<TResponse>(HttpRequestMessage request)
        {
            var handler = new NativeMessageHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            }
            var client = new HttpClient(handler);

            var result = new HttpResult<TResponse>();
                using (var response = await client.SendAsync(request))
                {
                    result.StatusCode = response.StatusCode;

                    if (response.IsSuccessStatusCode && response.Content != null)
                        result.Content = await response.Content.ReadAsAsync<TResponse>();


                }
           
            return result;
        }

        /// <summary>
        /// This is for when the response is a plain type i.e html or text.
        /// This would cause an error when you did readAsAsync on it.
        /// </summary>
        public async Task<HttpResult<TResponse>> SendAsyncPlainResponse<TResponse>(HttpRequestMessage request)
        {
            var handler = new NativeMessageHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            }
            var client = new HttpClient(handler);

            using (var response = await client.SendAsync(request))
            {
                return new HttpResult<TResponse> { StatusCode = response.StatusCode };
            }
        }

        static MediaTypeFormatter GetJsonFormatter()
        {
            var formatter = new JsonMediaTypeFormatter();
            formatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            return formatter;
        }

    }

}
