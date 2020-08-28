using System;
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
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.0+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var stringContent = new StringContent(JsonConvert.SerializeObject(userCredentials, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("auth", stringContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TokenResponse>(json, _deserializerSettings)?.Token;
        }

        public async Task<CheckedResponse<bool>> VerifyAsync(string username, string verificationCode)
        {
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.0+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress,
                    $"auth/users/{username}/verify?verification-code={verificationCode}"),
                Method = HttpMethod.Put
            };

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

        public async Task ReVerifyAsync(string username)
        {
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.0+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, $"auth/users/{username}/reverify"),
                Method = HttpMethod.Put
            };

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
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.0+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

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
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.0+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var stringContent = new StringContent(JsonConvert.SerializeObject(registrationRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
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
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1.0+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var stringContent = new StringContent(JsonConvert.SerializeObject(recoveryRequest, _serializerSettings),
                Encoding.UTF8, "application/json");

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