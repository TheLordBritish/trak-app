using System;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Service;

namespace SparkyStudios.TrakLibrary.ViewModel.Test
{
    public class BaseFlyoutViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IRestService> _restService;
        private TestScheduler _scheduler;

        private BaseFlyoutViewModel _baseFlyoutViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _restService = new Mock<IRestService>();
            _scheduler = new TestScheduler();

            _baseFlyoutViewModel = new BaseFlyoutViewModel(_scheduler, _navigationService.Object,
                _storageService.Object, _restService.Object);
        }

        [Test]
        public void LoadHomeCommand_WithNoData_DoesntThrowException()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("NavigationPage/HomePage"))
                .Verifiable();

            // Act
            _baseFlyoutViewModel.LoadHomeCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify();
        }

        [Test]
        public void LoadGamesCommand_WithNoData_DoesntThrowException()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("NavigationPage/GameUserEntriesTabbedPage"))
                .Verifiable();

            // Act
            _baseFlyoutViewModel.LoadGamesCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify();
        }

        [Test]
        public void LoadSettingsCommand_WithNoData_DoesntThrowException()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("NavigationPage/SettingsPage"))
                .Verifiable();

            // Act
            _baseFlyoutViewModel.LoadSettingsCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify();
        }

        [Test]
        public void LogoutCommand_ThrowsException_NavigatesToLoginPage()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L)
                .Verifiable();

            _storageService.Setup(mock => mock.GetDeviceIdAsync())
                .ReturnsAsync(Guid.Empty)
                .Verifiable();
            
            _restService.Setup(mock => mock.DeleteAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());
            
            // Act
            _baseFlyoutViewModel.LogoutCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify(mock => mock.NavigateAsync("/LoginPage"), Times.Once);
        }

        [Test]
        public void LogoutCommand_WithNoissues_NavigatesToLoginPage()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L)
                .Verifiable();

            _storageService.Setup(mock => mock.GetDeviceIdAsync())
                .ReturnsAsync(Guid.Empty)
                .Verifiable();
            
            _restService.Setup(mock => mock.DeleteAsync(It.IsAny<string>()))
                .Verifiable();

            _storageService.Setup(mock => mock.SetUsernameAsync(It.IsAny<string>()))
                .Verifiable();

            _storageService.Setup(mock => mock.SetAuthTokenAsync(It.IsAny<string>()))
                .Verifiable();

            _storageService.Setup(mock => mock.SetUserIdAsync(It.IsAny<long>()))
                .Verifiable();
            
            _navigationService.Setup(mock => mock.NavigateAsync("/LoginPage"))
                .Verifiable();

            // Act
            _baseFlyoutViewModel.LogoutCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            _storageService.Verify();
            _restService.Verify();
            _navigationService.Verify();
        }
    }
}