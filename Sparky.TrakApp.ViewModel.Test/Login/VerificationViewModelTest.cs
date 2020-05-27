﻿using System;
using System.Net;
using Acr.UserDialogs;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Test.Login
{
    public class VerificationViewModelTest
    {
        private Mock<IAuthService> _authService;
        private Mock<IStorageService> _storageService;
        private Mock<INavigationService> _navigationService;
        private Mock<IUserDialogs> _userDialogs;

        private VerificationViewModel _verificationViewModel;

        [SetUp]
        public void SetUp()
        {
            _authService = new Mock<IAuthService>();
            _storageService = new Mock<IStorageService>();
            _navigationService = new Mock<INavigationService>();
            _userDialogs = new Mock<IUserDialogs>();

            _verificationViewModel = new VerificationViewModel(_navigationService.Object, _authService.Object,
                _storageService.Object, _userDialogs.Object);
        }

        [Test]
        public void VerifyCommand_WithInvalidVerificationCode_doesntCallAnyService()
        {
            // Act
            _verificationViewModel.VerifyCommand.Execute(null);

            // Assert
            _storageService.Verify(s => s.GetUsernameAsync(), Times.Never);
        }

        [Test]
        public void VerifyCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _verificationViewModel.VerificationCode.Value = "SV1SD";

            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync("username");
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _authService.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.Unauthorized});

            // Act
            _verificationViewModel.VerifyCommand.Execute(null);

            // Assert
            Assert.IsTrue(_verificationViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _verificationViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void VerifyCommand_ThrowsNonApiException_SetsErrorMessageAsGeneric()
        {
            // Arrange
            _verificationViewModel.VerificationCode.Value = "SV1SD";

            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync("username");
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _authService.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _verificationViewModel.VerifyCommand.Execute(null);

            // Assert
            Assert.IsTrue(_verificationViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _verificationViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void VerifyCommand_WithIncorrectVerificationCode_SetsErrorMessageAndDoesntNavigate()
        {
            // Arrange
            _verificationViewModel.VerificationCode.Value = "SV1SD";

            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync("username");
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _authService.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new CheckedResponse<bool> {Data = false, Error = true, ErrorMessage = "error message"});

            // Act
            _verificationViewModel.VerifyCommand.Execute(null);

            // Assert
            Assert.IsTrue(_verificationViewModel.IsError,
                "vm.IsError should be true if the verification code is incorrect.");
            Assert.AreEqual("error message", _verificationViewModel.ErrorMessage,
                "The error message is incorrect.");

            _navigationService.Verify(n => n.NavigateAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void VerifyCommand_WithCorrectVerificationCode_NavigatesToHomePage()
        {
            // Arrange
            _verificationViewModel.VerificationCode.Value = "SV1SD";

            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync("username");
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _authService.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new CheckedResponse<bool> {Data = true});

            _navigationService.Setup(mock => mock.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _verificationViewModel.VerifyCommand.Execute(null);

            // Assert
            Assert.IsFalse(_verificationViewModel.IsError,
                "vm.IsError should be false if verification code was successful.");
            _navigationService.Verify(n => n.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage"),
                Times.Once);
        }

        [Test]
        public void ResendVerificationCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync("username");
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _authService.Setup(mock => mock.ReVerifyAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.Unauthorized});

            // Act
            _verificationViewModel.ResendVerificationCommand.Execute(null);

            // Assert
            Assert.IsTrue(_verificationViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _verificationViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void ResendVerificationCommand_ThrowsNonApiException_SetsErrorMessageAsGeneric()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync("username");
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _authService.Setup(mock => mock.ReVerifyAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _verificationViewModel.ResendVerificationCommand.Execute(null);

            // Assert
            Assert.IsTrue(_verificationViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _verificationViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void ResendVerificationCommand_WithNoErrors_SendsEmailAndDisplaysAlert()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync("username");
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _authService.Setup(mock => mock.ReVerifyAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();

            _userDialogs.Setup(mock => mock.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Verifiable();

            // Act
            _verificationViewModel.ResendVerificationCommand.Execute(null);

            // Assert
            Assert.IsFalse(_verificationViewModel.IsError,
                "vm.IsError should be false if resending verification was successful.");
            _authService.Verify();
            _userDialogs.Verify();
        }
    }
}