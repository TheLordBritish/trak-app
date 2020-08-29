using System;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Settings;

namespace Sparky.TrakApp.ViewModel.Test.Settings
{
    [TestFixture]
    public class RequestChangePasswordViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IAuthService> _authService;
        private TestScheduler _scheduler;

        private RequestChangePasswordViewModel _requestChangePasswordViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _authService = new Mock<IAuthService>();
            _scheduler = new TestScheduler();

            _requestChangePasswordViewModel = new RequestChangePasswordViewModel(_scheduler, _navigationService.Object,
                _storageService.Object, _authService.Object);
        }

        [Test]
        public void SendCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");

            _authService.Setup(m => m.RequestChangePasswordAsync(It.IsAny<string>()))
                .Throws(new ApiException());

            // Act
            _requestChangePasswordViewModel.SendCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_requestChangePasswordViewModel.IsError,
                "_requestChangePasswordViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _requestChangePasswordViewModel.ErrorMessage,
                "The error message is incorrect.");

            _navigationService.Verify(m => m.NavigateAsync("ChangePasswordPage"), Times.Never);
        }

        [Test]
        public void SendCommand_ThrowsException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");

            _authService.Setup(m => m.RequestChangePasswordAsync(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _requestChangePasswordViewModel.SendCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_requestChangePasswordViewModel.IsError,
                "_requestChangePasswordViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _requestChangePasswordViewModel.ErrorMessage,
                "The error message is incorrect.");

            _navigationService.Verify(m => m.NavigateAsync("ChangePasswordPage"), Times.Never);
        }

        [Test]
        public void SendCommand_WithNoErrors_NavigatesToChangePasswordPage()
        {
            // Arrange
            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");

            _authService.Setup(m => m.RequestChangePasswordAsync(It.IsAny<string>()))
                .Verifiable();

            _navigationService.Setup(m => m.NavigateAsync("ChangePasswordPage", It.IsAny<INavigationParameters>()));

            // Act
            _requestChangePasswordViewModel.SendCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsFalse(_requestChangePasswordViewModel.IsError,
                "vm.IsError should be false if login was successful.");

            _authService.Verify();
            _navigationService.Verify(m => m.NavigateAsync("ChangePasswordPage", It.IsAny<INavigationParameters>()),
                Times.Once);
        }
        
        [Test]
        public void ChangePasswordCommand_WithNoData_DoesntThrowException()
        {
            _navigationService.Setup(mock => mock.NavigateAsync("ChangePasswordPage", It.IsAny<INavigationParameters>()))
                .Verifiable();
            
            Assert.DoesNotThrow(() =>
            {
                // Act
                _requestChangePasswordViewModel.ChangePasswordCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
            
            // Assert
            _navigationService.Verify();
        }
    }
}