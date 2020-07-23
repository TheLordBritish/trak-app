using System;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Test.Login
{
    public class ForgottenPasswordViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IAuthService> _authService;

        private ForgottenPasswordViewModel _forgottenPasswordViewModel;
        private TestScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _authService = new Mock<IAuthService>();
            _scheduler = new TestScheduler();

            _forgottenPasswordViewModel =
                new ForgottenPasswordViewModel(_scheduler, _navigationService.Object, _authService.Object);
        }

        [Test]
        public void ClearValidationCommand_WithNoData_DoesntThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _forgottenPasswordViewModel.ClearValidationCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
        }
        
        [Test]
        public void SendCommand_WithInvalidEmailAddress_doesntCallAuthService()
        {
            // Arrange
            _forgottenPasswordViewModel.EmailAddress.Value = "invalid";

            // Act
            _forgottenPasswordViewModel.SendCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.RequestRecoveryAsync(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void SendCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _forgottenPasswordViewModel.EmailAddress.Value = "test.email@test.com";

            _authService.Setup(a => a.RequestRecoveryAsync(It.IsAny<string>()))
                .Throws(new ApiException());

            // Act
            _forgottenPasswordViewModel.SendCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_forgottenPasswordViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _forgottenPasswordViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void SendCommand_ThrowsNonApiException_SetErrorMessageAsGenericError()
        {
            // Arrange
            _forgottenPasswordViewModel.EmailAddress.Value = "test.email@test.com";

            _authService.Setup(a => a.RequestRecoveryAsync(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _forgottenPasswordViewModel.SendCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_forgottenPasswordViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _forgottenPasswordViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void SendCommand_WithValidRequest_NavigatesToRecoveryPage()
        {
            // Arrange
            _forgottenPasswordViewModel.EmailAddress.Value = "test.email@test.com";

            _authService.Setup(a => a.RequestRecoveryAsync(It.IsAny<string>()))
                .Verifiable();

            _navigationService.Setup(mock => mock.NavigateAsync("RecoveryPage"))
                .Verifiable();

            // Act
            _forgottenPasswordViewModel.SendCommand.Execute();
            _scheduler.Start();

            // Assert
            _authService.Verify();
            _navigationService.Verify();
        }

        [Test]
        public void RecoveryCommand_WithNoData_NavigatesToRecoveryPage()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("RecoveryPage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _forgottenPasswordViewModel.RecoveryCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify(n => n.NavigateAsync("RecoveryPage"), Times.Once);
        }
    }
}