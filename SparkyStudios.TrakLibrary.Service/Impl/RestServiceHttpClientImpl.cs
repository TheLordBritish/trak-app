using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SparkyStudios.TrakLibrary.Service.Exception;

namespace SparkyStudios.TrakLibrary.Service.Impl
{
    internal class RestServiceHttpClientImpl : IRestService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly JsonSerializerSettings _deserializerSettings;

        public RestServiceHttpClientImpl(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            // Ensure we use the correct TLS version before making the request.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // Serialization settings.
            _serializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            // Deserialization settings.
            _deserializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DateParseHandling = DateParseHandling.None
            };
        }

        public async Task<T> GetAsync<T>(string url)
        {
            using var client = _httpClientFactory.CreateClient("Trak");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.hal+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, url),
                Method = HttpMethod.Get
            };

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

            return JsonConvert.DeserializeObject<T>(json, _deserializerSettings);
        }

        public async Task<T> PostAsync<T>(string url, T requestBody)
        {
            // Create the client to send the requests to.
            using var client = _httpClientFactory.CreateClient("Trak");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.hal+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            // Serialize the request body to json.
            var content = new StringContent(JsonConvert.SerializeObject(requestBody, _serializerSettings), Encoding.UTF8);

            // Specify the URI and the content of the post request.
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, url),
                Content = content,
                Method = HttpMethod.Post
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            // Make the request.
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

            // Only de-serialize the response on a successful call. 
            return JsonConvert.DeserializeObject<T>(json, _deserializerSettings);
        }

        public async Task<T> PutAsync<T>(string url, T requestBody)
        {
            // Create the client to send the requests to.
            using var client = _httpClientFactory.CreateClient("Trak");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.hal+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            // Serialize the request body to json.
            var content = new StringContent(JsonConvert.SerializeObject(requestBody, _serializerSettings), Encoding.UTF8);

            // Specify the URI and the content of the post request.
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, url),
                Content = content,
                Method = HttpMethod.Put
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Make the request.
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

            // Only de-serialize the response on a successful call. 
            return JsonConvert.DeserializeObject<T>(json, _deserializerSettings);
        }

        public async Task<T> PatchAsync<T>(string url, IDictionary<string, object> values)
        {
            // Create the client to send the requests to.
            using var client = _httpClientFactory.CreateClient("Trak");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.hal+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            // Serialize the request body to json.
            var content = new StringContent(JsonConvert.SerializeObject(values, _serializerSettings),
                Encoding.UTF8, "application/merge-patch+json");

            // Specify the URI and the content of the post request.
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, url),
                Content = content,
                Method = new HttpMethod("PATCH")
            };

            // Make the request.
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

            // Only de-serialize the response on a successful call. 
            return JsonConvert.DeserializeObject<T>(json, _deserializerSettings);
        }

        public async Task DeleteAsync(string url)
        {
            // Create the client to send the requests to.
            using var client = _httpClientFactory.CreateClient("Trak");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.hal+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            // Specify the URI and that the request is a delete request..
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, url),
                Method = HttpMethod.Delete
            };

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode
                };
            }
        }
    }
}