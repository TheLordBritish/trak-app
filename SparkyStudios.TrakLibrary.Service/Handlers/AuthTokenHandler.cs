using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SparkyStudios.TrakLibrary.Service.Handlers
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IStorageService _storageService;

        public AuthTokenHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var uri = request.RequestUri.OriginalString;

            if (uri.EndsWith("/api/auth/token") && request.Method == HttpMethod.Post ||
                uri.EndsWith("/api/auth/users") && request.Method == HttpMethod.Post ||
                uri.EndsWith("/api/auth/users") && request.Method == HttpMethod.Put ||
                uri.EndsWith("/api/auth/users/recover") && request.Method == HttpMethod.Put)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var token = await _storageService.GetAuthTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}