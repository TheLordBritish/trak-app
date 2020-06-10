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
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly JsonSerializerSettings _deserializerSettings;

        public AuthServiceHttpClientImpl(IHttpClientFactory httpClientFactory)
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
        
        public async Task<string> GetTokenAsync(UserCredentials userCredentials)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(userCredentials, _serializerSettings));

            using var client = _httpClientFactory.CreateClient("TrakAuth");
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

        public async Task<UserResponse> GetFromUsernameAsync(string username, string authToken)
        {
            using var client = _httpClientFactory.CreateClient("TrakAuth");

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, $"auth/users/{username}"),
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

            return JsonConvert.DeserializeObject<UserResponse>(json, _deserializerSettings);
        }

        public async Task<CheckedResponse<bool>> VerifyAsync(string username, string verificationCode, string authToken)
        {
            using var client = _httpClientFactory.CreateClient("TrakAuth");

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress,
                    $"auth/users/{username}/verify?verification-code={verificationCode}"),
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
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CheckedResponse<bool>>(json, _deserializerSettings);
        }

        public async Task ReVerifyAsync(string username, string authToken)
        {
            using var client = _httpClientFactory.CreateClient("TrakAuth");

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, $"auth/users/{username}/reverify"),
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

        public async Task RequestRecoveryAsync(string emailAddress)
        {
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            using var response = await client.PutAsync($"auth/users/recover?email-address={emailAddress}", null);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }
        }

        public async Task<CheckedResponse<UserResponse>> RegisterAsync(RegistrationRequest registrationRequest)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(registrationRequest, _serializerSettings),
                Encoding.UTF8, "application/json");

            using var client = _httpClientFactory.CreateClient("TrakAuth");
            using var response = await client.PostAsync("auth/users", stringContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CheckedResponse<UserResponse>>(json, _deserializerSettings);
        }

        public async Task<CheckedResponse<UserResponse>> RecoverAsync(RecoveryRequest recoveryRequest)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(recoveryRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            using var response = await client.PutAsync("auth/users", stringContent);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CheckedResponse<UserResponse>>(json, _deserializerSettings);
        }
    }
}