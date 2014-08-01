using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    public interface IHttpService
    {
        Task<HttpResult<TResponse>> PostJsonAsync<TResponse>(string jsonContent, string url);
        Task<HttpResult<TResponse>> PostAsJsonAsync<TRequest, TResponse>(TRequest content, string url);
    }

}
