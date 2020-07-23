﻿using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Test.Login
{
    public class LoginViewModelTest
    {
        private Mock<IAuthService> _authService;
        private Mock<IStorageService> _storageService;
        private Mock<INavigationService> _navigationService;
        private Mock<IRestService> _restService;
        private TestScheduler _scheduler;

        private LoginViewModel _loginViewModel;

        [SetUp]
        public void SetUp()
        {
            _authService = new Mock<IAuthService>();
            _storageService = new Mock<IStorageService>();
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _scheduler = new TestScheduler();

            _loginViewModel = new LoginViewModel(_scheduler, _navigationService.Object, _authService.Object,
                _storageService.Object, _restService.Object);
        }

        [Test]
        public void ClearValidationCommand_WithNoData_DoesntThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _loginViewModel.ClearValidationCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
        }
        
        [Test]
        public void LoginCommand_WithInvalidUsername_DoesntCallAuthService()
        {
            // Arrange
            _loginViewModel.Password.Value = "Password";

            // Act
            _loginViewModel.LoginCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.GetTokenAsync(It.IsAny<UserCredentials>()), Times.Never);
        }

        [Test]
        public void LoginCommand_WithInvalidPassword_DoesntCallAuthService()
        {
            // Arrange
            _loginViewModel.Username.Value = "Username";

            // Act
            _loginViewModel.LoginCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.GetTokenAsync(It.IsAny<UserCredentials>()), Times.Never);
        }

        [Test]
        public void LoginCommand_ThrowsUnauthorizedApiException_SetsErrorMessageAsUnauthorized()
        {
            // Arrange
            _loginViewModel.Username.Value = "Username";
            _loginViewModel.Password.Value = "Password";

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<UserCredentials>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.Unauthorized});

            // Act
            _loginViewModel.LoginCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_loginViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageIncorrectCredentials, _loginViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void LoginCommand_ThrowsOtherApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _loginViewModel.Username.Value = "Username";
            _loginViewModel.Password.Value = "Password";

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<UserCredentials>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.Conflict});

            // Act
            _loginViewModel.LoginCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_loginViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _loginViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void LoginCommand_WithNonApiException_SetsErrorMessageAsGeneric()
        {
            // Arrange
            _loginViewModel.Username.Value = "Username";
            _loginViewModel.Password.Value = "Password";

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<UserCredentials>()))
                .Throws(new Exception());

            // Act
            _loginViewModel.LoginCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_loginViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _loginViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void LoginCommand_WithNonVerifiedAccount_NavigatesToVerificationPage()
        {
            // Arrange
            _loginViewModel.Username.Value = "Username";
            _loginViewModel.Password.Value = "Password";

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<UserCredentials>()))
                .ReturnsAsync("token");

            _authService.Setup(mock => mock.GetFromUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UserResponse {Username = "username", Id = 1, Verified = false});

            _storageService.Setup(mock => mock.SetAuthTokenAsync(It.IsAny<string>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetUserIdAsync(It.IsAny<long>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetUsernameAsync(It.IsAny<string>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetPasswordAsync(It.IsAny<string>()))
                .Verifiable();

            _restService.Setup(mock =>
                    mock.PostAsync(It.IsAny<string>(), It.IsAny<NotificationRegistrationRequest>(), It.IsAny<string>()))
                .Verifiable();

            _navigationService.Setup(mock => mock.NavigateAsync("VerificationPage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _loginViewModel.LoginCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsFalse(_loginViewModel.IsError, "vm.IsError should be false if login was successful.");

            _storageService.Verify();
            _restService.Verify();
            _navigationService.Verify(n => n.NavigateAsync("VerificationPage"), Times.Once);
        }

        [Test]
        public void LoginCommand_WithVerifiedAccount_NavigatesToHomePage()
        {
            // Arrange
            _loginViewModel.Username.Value = "Username";
            _loginViewModel.Password.Value = "Password";

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<UserCredentials>()))
                .ReturnsAsync("token");

            _authService.Setup(mock => mock.GetFromUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UserResponse {Username = "username", Id = 1, Verified = true});

            _storageService.Setup(mock => mock.SetAuthTokenAsync(It.IsAny<string>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetUserIdAsync(It.IsAny<long>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetUsernameAsync(It.IsAny<string>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetPasswordAsync(It.IsAny<string>()))
                .Verifiable();

            _restService.Setup(mock =>
                    mock.PostAsync(It.IsAny<string>(), It.IsAny<NotificationRegistrationRequest>(), It.IsAny<string>()))
                .Verifiable();

            _navigationService.Setup(mock => mock.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _loginViewModel.LoginCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsFalse(_loginViewModel.IsError, "vm.IsError should be false if login was successful.");

            _storageService.Verify();
            _restService.Verify();
            _navigationService.Verify(n => n.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage"),
                Times.Once);
        }

        [Test]
        public void ForgottenPasswordCommand_WithNoData_NavigatesToForgottenPasswordPage()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("ForgottenPasswordPage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _loginViewModel.ForgottenPasswordCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify(n => n.NavigateAsync("ForgottenPasswordPage"), Times.Once);
        }

        [Test]
        public void RegisterCommand_WithNoData_NavigatesToRegisterPage()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("RegisterPage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _loginViewModel.RegisterCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify(n => n.NavigateAsync("RegisterPage"), Times.Once);
        }
    }
}