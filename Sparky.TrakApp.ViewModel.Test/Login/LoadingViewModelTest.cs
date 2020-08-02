using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Login;

namespace Sparky.TrakApp.ViewModel.Test.Login
{
    public class LoadingViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IAuthService> _authService;
        private Mock<IRestService> _restService;
        private TestScheduler _scheduler;

        private LoadingViewModel _loadingViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _authService = new Mock<IAuthService>();
            _restService = new Mock<IRestService>();
            _scheduler = new TestScheduler();

            _loadingViewModel = new LoadingViewModel(_scheduler, _navigationService.Object, _storageService.Object,
                _authService.Object, _restService.Object);
        }
        
        [Test]
        public void OnNavigatedTo_ThrowsException_NavigatesToLoginPage()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync(string.Empty);

            _storageService.Setup(mock => mock.GetPasswordAsync())
                .ReturnsAsync(string.Empty);

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<UserCredentials>()))
                .Throws(new ApiException());

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
            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync(string.Empty);

            _storageService.Setup(mock => mock.GetPasswordAsync())
                .ReturnsAsync(string.Empty);

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<UserCredentials>()))
                .ReturnsAsync("token");

            _storageService.Setup(mock => mock.SetAuthTokenAsync(It.IsAny<string>()))
                .Verifiable();

            _restService.Setup(mock =>
                    mock.PostAsync(It.IsAny<string>(), It.IsAny<NotificationRegistrationRequest>(), It.IsAny<string>()))
                .Verifiable();

            _authService.Setup(mock => mock.GetFromUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UserResponse {Id = 1, Username = "username", Verified = false});

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
            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync(string.Empty);

            _storageService.Setup(mock => mock.GetPasswordAsync())
                .ReturnsAsync(string.Empty);

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<UserCredentials>()))
                .ReturnsAsync("token");

            _storageService.Setup(mock => mock.SetAuthTokenAsync(It.IsAny<string>()))
                .Verifiable();

            _restService.Setup(mock =>
                    mock.PostAsync(It.IsAny<string>(), It.IsAny<NotificationRegistrationRequest>(), It.IsAny<string>()))
                .Verifiable();

            _authService.Setup(mock => mock.GetFromUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UserResponse {Id = 1, Username = "username", Verified = true});

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