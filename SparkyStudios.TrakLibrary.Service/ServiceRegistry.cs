using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Prism.Ioc;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.Service.Handlers;
using SparkyStudios.TrakLibrary.Service.Impl;

namespace SparkyStudios.TrakLibrary.Service
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
                .AddHttpMessageHandler(c => new AuthTokenHandler(storageService));

            serviceCollection
                .AddHttpClient("TrakAuth", client =>
                {
                    client.BaseAddress = new Uri(environmentUrl);
                })
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(20))
                .AddHttpMessageHandler(c => new AuthTokenHandler(storageService));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            // Services
            containerRegistry.RegisterInstance(serviceProvider.GetService<IHttpClientFactory>());
            containerRegistry.Register<IAuthService, AuthServiceHttpClientImpl>();
            containerRegistry.Register<IRestService, RestServiceHttpClientImpl>();
        }
    }
}