using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service.Exception;

namespace Sparky.TrakApp.Service.Impl
{
    internal class AuthServiceHttpClientImpl : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public AuthServiceHttpClientImpl(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task<string> GetTokenAsync(UserCredentials userCredentials)
        {
            // Ensure we use the correct TLS version before making the request.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userCredentials, settings));

            using var client = _httpClientFactory.CreateClient("Trak");
            using var response = await client.PostAsync("auth", stringContent);

            if (response.IsSuccessStatusCode)
            {
                return response.Headers.GetValues("Authorization").FirstOrDefault();
            }

            throw new ApiException
            {
                StatusCode = response.StatusCode,
                Content = string.Empty
            };
        }

        public async Task<bool> IsVerifiedAsync(string username, string authToken)
        {
            // Ensure we use the correct TLS version before making the request.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DateParseHandling = DateParseHandling.None
            };
            
            using var client = _httpClientFactory.CreateClient("Trak");
            
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, $"auth/users/{username}/verified"),
                Method = HttpMethod.Get
            };
            
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authToken);
            
            using var response =
                await client.SendAsync(request);
            
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = json
                };
            }
            
            var restResponse = JsonConvert.DeserializeObject<RestResponse<bool>>(json, settings);
            return restResponse != null && restResponse.Data;
        }

        public async Task VerifyAsync(string username, short verificationCode, string authToken)
        {
            // Ensure we use the correct TLS version before making the request.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            
            using var client = _httpClientFactory.CreateClient("Trak");
            
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, $"auth/users/{username}/verify?verification-code={verificationCode}"),
                Method = HttpMethod.Put
            };
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authToken);
            
            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }
        }

        public async Task RegisterAsync(RegistrationRequest registrationRequest)
        {
            // Ensure we use the correct TLS version before making the request.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(registrationRequest, settings), Encoding.UTF8, "application/json");
            
            using var client = _httpClientFactory.CreateClient("Trak");
            using var response = await client.PostAsync("auth/users", stringContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }
        }
    }
}