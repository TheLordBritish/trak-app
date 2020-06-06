using System;
using Acr.UserDialogs;
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
        private Mock<IUserDialogs> _userDialogs;

        private ForgottenPasswordViewModel _forgottenPasswordViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _authService = new Mock<IAuthService>();
            _userDialogs = new Mock<IUserDialogs>();
            
            _forgottenPasswordViewModel = new ForgottenPasswordViewModel(_navigationService.Object, _authService.Object, _userDialogs.Object);
        }

        [Test]
        public void ResetPasswordCommand_WithInvalidEmailAddress_doesntCallAuthService()
        {
            // Arrange
            _forgottenPasswordViewModel.EmailAddress.Value = "invalid";
            
            // Act
            _forgottenPasswordViewModel.ResetPasswordCommand.Execute(null);
            
            // Assert
            _authService.Verify(a => a.ResetPasswordAsync(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ResetPasswordCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _forgottenPasswordViewModel.EmailAddress.Value = "test.email@test.com";

            _authService.Setup(a => a.ResetPasswordAsync(It.IsAny<string>()))
                .Throws(new ApiException());
            
            // Act
            _forgottenPasswordViewModel.ResetPasswordCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(_forgottenPasswordViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _forgottenPasswordViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void ResetPasswordCommand_ThrowsNonApiException_SetErrorMessageAsGenericError()
        {
            // Arrange
            _forgottenPasswordViewModel.EmailAddress.Value = "test.email@test.com";
            
            _authService.Setup(a => a.ResetPasswordAsync(It.IsAny<string>()))
                .Throws(new Exception());
            
            // Act
            _forgottenPasswordViewModel.ResetPasswordCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(_forgottenPasswordViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _forgottenPasswordViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void ResetPaswordCommand_WithValidRequest_DisplaysAlertToUser()
        {
            // Arrange
            _forgottenPasswordViewModel.EmailAddress.Value = "test.email@test.com";
            
            _authService.Setup(a => a.ResetPasswordAsync(It.IsAny<string>()))
                .Verifiable();
            
            _userDialogs.Setup(u => u.AlertAsync(It.IsAny<AlertConfig>(), null))
                .Verifiable();
            
            // Act
            _forgottenPasswordViewModel.ResetPasswordCommand.Execute(null);
            
            // Assert
            _authService.Verify();
            _userDialogs.Verify();
        }
    }
}