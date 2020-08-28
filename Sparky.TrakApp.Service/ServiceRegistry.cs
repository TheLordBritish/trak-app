using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Prism.Ioc;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.Service.Handlers;
using Sparky.TrakApp.Service.Impl;

namespace Sparky.TrakApp.Service
{
    public static class ServiceRegistry
    {
        public static void RegisterTypes(IContainerRegistry containerRegistry, IStorageService storageService, string environmentUrl)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .Or<ApiException>()
                .WaitAndRetryAsync(new []
                {
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(8)
                });

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddHttpClient("Trak", client =>
                {
                    client.BaseAddress = new Uri(environmentUrl);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy)
                .AddHttpMessageHandler(c => new AuthTokenHandler(storageService))
                .AddHttpMessageHandler(c => new RefreshTokenHandler(storageService));

            serviceCollection
                .AddHttpClient("TrakAuth", client =>
                {
                    client.BaseAddress = new Uri(environmentUrl);
                })
                .AddPolicyHandler(timeoutPolicy)
                .AddHttpMessageHandler(c => new AuthTokenHandler(storageService))
                .AddHttpMessageHandler(c => new RefreshTokenHandler(storageService));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            // Services
            containerRegistry.RegisterInstance(serviceProvider.GetService<IHttpClientFactory>());
            containerRegistry.Register<IAuthService, AuthServiceHttpClientImpl>();
            containerRegistry.Register<IRestService, RestServiceHttpClientImpl>();
        }
    }
}