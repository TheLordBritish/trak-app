using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Model.Settings;
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
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var content = new StringContent(JsonConvert.SerializeObject(userCredentials, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("auth", content);

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
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/{username}/verify?verification-code={verificationCode}", content);

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
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/{username}/reverify", content);

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
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/recover?email-address={emailAddress}", content);

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
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var content = new StringContent(JsonConvert.SerializeObject(registrationRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("auth/users", content);

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
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var content = new StringContent(JsonConvert.SerializeObject(recoveryRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PutAsync("auth/users", content);

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

        public async Task RequestChangePasswordAsync(string username)
        {
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/{username}/request-change-password", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }
        }

        public async Task<CheckedResponse<bool>> ChangePasswordAsync(string username, ChangePasswordRequest changePasswordRequest)
        {
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var content = new StringContent(JsonConvert.SerializeObject(changePasswordRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/{username}/change-password", content);

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

        public async Task<CheckedResponse<bool>> ChangeEmailAddressAsync(string username, ChangeEmailAddressRequest changeEmailAddressRequest)
        {
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var content = new StringContent(JsonConvert.SerializeObject(changeEmailAddressRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/{username}/change-email-address", content);

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
    }
}