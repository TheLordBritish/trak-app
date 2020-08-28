using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sparky.TrakApp.Service.Handlers
{
    public class RefreshTokenHandler : DelegatingHandler
    {
        private readonly IStorageService _storageService;

        public RefreshTokenHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Make the standard request.
            var response = await base.SendAsync(request, cancellationToken);
            
            // See if the refresh token header is in the response, if it is update the authentication token.
            if (response.Headers.TryGetValues("Refresh-Token", out var values))
            {
                await _storageService.SetAuthTokenAsync(values.First());
            }
            
            return response;
        }
    }
}