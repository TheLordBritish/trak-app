using System;
using System.Net;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Test.Login
{
    public class RegisterViewModelTest
    {
        private Mock<IAuthService> _authService;
        private Mock<IStorageService> _storageService;
        private Mock<INavigationService> _navigationService;

        private RegisterViewModel _registerViewModel;

        [SetUp]
        public void SetUp()
        {
            _authService = new Mock<IAuthService>();
            _storageService = new Mock<IStorageService>();
            _navigationService = new Mock<INavigationService>();
            
            _registerViewModel = new RegisterViewModel(_navigationService.Object, _authService.Object, _storageService.Object);
        }

        [Test]
        public void RegisterCommand_WithInvalidUsername_doesntCallRegister()
        {
            // Arrange
            _registerViewModel.EmailAddress.Value = "email@address.com";
            _registerViewModel.Password.Value = "Password123";
            _registerViewModel.ConfirmPassword.Value = "Password123";
            
            // Act
            _registerViewModel.RegisterCommand.Execute(null);
            
            // Assert
            _authService.Verify(a => a.RegisterAsync(It.IsAny<RegistrationRequest>()), Times.Never);
        }
        
        [Test]
        public void RegisterCommand_WithInvalidEmailAddress_doesntCallRegister()
        {
            // Arrange
            _registerViewModel.Username.Value = "Username";
            _registerViewModel.Password.Value = "Password123";
            _registerViewModel.ConfirmPassword.Value = "Password123";
            
            // Act
            _registerViewModel.RegisterCommand.Execute(null);
            
            // Assert
            _authService.Verify(a => a.RegisterAsync(It.IsAny<RegistrationRequest>()), Times.Never);
        }
        
        [Test]
        public void RegisterCommand_WithInvalidPassword_doesntCallRegister()
        {
            // Arrange
            _registerViewModel.Username.Value = "Username";
            _registerViewModel.EmailAddress.Value = "email@address.com";
            _registerViewModel.ConfirmPassword.Value = "Password123";
            
            // Act
            _registerViewModel.RegisterCommand.Execute(null);
            
            // Assert
            _authService.Verify(a => a.RegisterAsync(It.IsAny<RegistrationRequest>()), Times.Never);
        }
        
        [Test]
        public void RegisterCommand_WithInvalidConfirmPassword_doesntCallRegister()
        {
            // Arrange
            _registerViewModel.Username.Value = "Username";
            _registerViewModel.EmailAddress.Value = "email@address.com";
            _registerViewModel.Password.Value = "Password123";
            _registerViewModel.ConfirmPassword.Value = "Password1234";
            
            // Act
            _registerViewModel.RegisterCommand.Execute(null);
            
            // Assert
            _authService.Verify(a => a.RegisterAsync(It.IsAny<RegistrationRequest>()), Times.Never);
        }
        
        [Test]
        public void RegisterCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _registerViewModel.Username.Value = "Username";
            _registerViewModel.EmailAddress.Value = "email@address.com";
            _registerViewModel.Password.Value = "Password123";
            _registerViewModel.ConfirmPassword.Value = "Password123";
            
            _authService.Setup(mock => mock.RegisterAsync(It.IsAny<RegistrationRequest>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.Unauthorized});

            // Act
            _registerViewModel.RegisterCommand.Execute(null);

            // Assert
            Assert.IsTrue(_registerViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _registerViewModel.ErrorMessage,
                "The error message is incorrect.");
        }
        
        [Test]
        public void RegisterCommand_ThrowsNonApiException_SetsErrorMessageAsGeneric()
        {
            // Arrange
            _registerViewModel.Username.Value = "Username";
            _registerViewModel.EmailAddress.Value = "email@address.com";
            _registerViewModel.Password.Value = "Password123";
            _registerViewModel.ConfirmPassword.Value = "Password123";
            
            _authService.Setup(mock => mock.RegisterAsync(It.IsAny<RegistrationRequest>()))
                .Throws(new Exception());

            // Act
            _registerViewModel.RegisterCommand.Execute(null);

            // Assert
            Assert.IsTrue(_registerViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _registerViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void RegisterCommand_WithErroneousUserResponse_SetsErrorMessage()
        {
            // Arrange
            _registerViewModel.Username.Value = "Username";
            _registerViewModel.EmailAddress.Value = "email@address.com";
            _registerViewModel.Password.Value = "Password123";
            _registerViewModel.ConfirmPassword.Value = "Password123";

            _authService.Setup(mock => mock.RegisterAsync(It.IsAny<RegistrationRequest>()))
                .ReturnsAsync(new CheckedResponse<UserResponse> {Error = true, ErrorMessage = "error"});
            
            // Act
            _registerViewModel.RegisterCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(_registerViewModel.IsError, "vm.IsError should be true if the user response has an error.");
            Assert.AreEqual("error", _registerViewModel.ErrorMessage, "The error message is incorrect.");
            
            _authService.Verify(s => s.GetTokenAsync(It.IsAny<UserCredentials>()), Times.Never);
        }
        
        [Test]
        public void RegisterCommand_WithValidUserResponse_SavesCredentialsAndNavigatesToVerificationPage()
        {
            // Arrange
            _registerViewModel.Username.Value = "Username";
            _registerViewModel.EmailAddress.Value = "email@address.com";
            _registerViewModel.Password.Value = "Password123";
            _registerViewModel.ConfirmPassword.Value = "Password123";
            
            _authService.Setup(mock => mock.RegisterAsync(It.IsAny<RegistrationRequest>()))
                .ReturnsAsync(new CheckedResponse<UserResponse> {Data = new UserResponse
                {
                    Id = 5L,
                    Username = "Username"
                }});

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<UserCredentials>()))
                .ReturnsAsync("token");
            
            _storageService.Setup(mock => mock.SetAuthTokenAsync(It.IsAny<string>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetUserIdAsync(It.IsAny<long>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetUsernameAsync(It.IsAny<string>()))
                .Verifiable();

            _navigationService.Setup(mock => mock.NavigateAsync("VerificationPage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);
            
            // Act
            _registerViewModel.RegisterCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(_registerViewModel.IsError,
                "vm.IsError should be false if registration was successful.");

            _storageService.Verify();
            _navigationService.Verify(s => s.NavigateAsync("VerificationPage"), Times.Once);
        }
        
        [Test]
        public void LoginCommand_WithNoData_NavigatesToLoginPage()
        {
            // Arrange
            _navigationService.Setup(mock => mock.GoBackAsync())
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _registerViewModel.LoginCommand.Execute(null);
            
            // Assert
            _navigationService.Verify(n => n.GoBackAsync(), Times.Once);
        }
    }
}