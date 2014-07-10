using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Services
{
    
    public class HttpService
        : IHttpService
    {

        public async Task<HttpResult<TResponse>> PostAsJsonAsync<TRequest, TResponse>(TRequest content, string url)
        {
            var client = new HttpClient();

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Content = new ObjectContent<TRequest>(content, GetJsonFormatter());

                using (var response = await client.SendAsync(request))
                {
                    var result = new HttpResult<TResponse> { StatusCode = response.StatusCode };
                    
                    if (response.Content != null)
                        result.Content = await response.Content.ReadAsAsync<TResponse>();

                    return result;
                }
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
