using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.ViewModel.Login;

namespace Sparky.TrakApp.ViewModel.Test.Login
{
    public class LoadingViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IRestService> _restService;
        private Mock<SecurityTokenHandler> _securityTokenHandler;
        private TestScheduler _scheduler;

        private LoadingViewModel _loadingViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _restService = new Mock<IRestService>();
            _securityTokenHandler = new Mock<SecurityTokenHandler>();
            _scheduler = new TestScheduler();

            _loadingViewModel = new LoadingViewModel(_scheduler, _navigationService.Object, _storageService.Object,
                _restService.Object, _securityTokenHandler.Object);
        }

        [Test]
        public void OnNavigatedTo_WithEmptyToken_NavigatesToLoginPage()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync(string.Empty);
            
            _navigationService.Setup(mock => mock.NavigateAsync("/LoginPage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);
            
            // Act
            _loadingViewModel.OnNavigatedTo(null);
            
            // Assert
            _restService.Verify(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<NotificationRegistrationRequest>()),
                Times.Never);
            _navigationService.Verify(mock => mock.NavigateAsync("/LoginPage"), Times.Once);
        }
        
        [Test]
        public void OnNavigatedTo_ThrowsException_NavigatesToLoginPage()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _securityTokenHandler.Setup(mock => mock.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken());

            _restService.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<NotificationRegistrationRequest>()))
                .Throws(new Exception());

            _navigationService.Setup(mock => mock.NavigateAsync("/LoginPage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _loadingViewModel.OnNavigatedTo(null);

            // Assert
            _navigationService.Verify(mock => mock.NavigateAsync("/LoginPage"), Times.Once);
        }

        [Test]
        public void OnNavigatedTo_WithNonVerifiedUser_NavigatesToLoginPage()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _securityTokenHandler.Setup(mock => mock.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken(claims: new List<Claim>
                {
                    new Claim("userId", 0L.ToString()),
                    new Claim("verified", bool.FalseString)
                }));

            _storageService.Setup(mock => mock.SetUsernameAsync(It.IsAny<string>()))
                .Verifiable();

            _restService.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<NotificationRegistrationRequest>()))
                .Verifiable();

            _navigationService.Setup(mock => mock.NavigateAsync("/LoginPage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _loadingViewModel.OnNavigatedTo(null);

            // Assert
            _storageService.Verify();
            _restService.Verify();
            _navigationService.Verify(mock => mock.NavigateAsync("/LoginPage"), Times.Once);
        }

        [Test]
        public void OnNavigatedTo_WithVerifiedUser_NavigatesToHomePage()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _securityTokenHandler.Setup(mock => mock.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken(claims: new List<Claim>
                {
                    new Claim("userId", 0L.ToString()),
                    new Claim("verified", bool.TrueString)
                }));
            
            _restService.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<NotificationRegistrationRequest>()))
                .Verifiable();

            _navigationService.Setup(mock => mock.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _loadingViewModel.OnNavigatedTo(null);

            // Assert
            _storageService.Verify();
            _restService.Verify();
            _navigationService.Verify(mock => mock.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage"),
                Times.Once);
        }
    }
}