using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sparky.TrakApp.Service.Exception;

namespace Sparky.TrakApp.Service.Impl
{
    internal class RestServiceHttpClientImpl : IRestService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RestServiceHttpClientImpl(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task<T> GetAsync<T>(string url, string authToken)
        {
            // Ensure we use the correct TLS version before making the request.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            
            using var client = _httpClientFactory.CreateClient("Trak");
            
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, url),
                Method = HttpMethod.Get
            };
            
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authToken);
            
            using var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = json
                };
            }
            
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DateParseHandling = DateParseHandling.None
            });
        }
    }
}