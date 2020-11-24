using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using Acr.UserDialogs;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Login;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Login
{
    public class VerificationViewModelTest
    {
        private Mock<IAuthService> _authService;
        private Mock<IStorageService> _storageService;
        private Mock<INavigationService> _navigationService;
        private Mock<IUserDialogs> _userDialogs;
        private TestScheduler _scheduler;

        private VerificationViewModel _verificationViewModel;

        [SetUp]
        public void SetUp()
        {
            _authService = new Mock<IAuthService>();
            _storageService = new Mock<IStorageService>();
            _navigationService = new Mock<INavigationService>();
            _userDialogs = new Mock<IUserDialogs>();
            _scheduler = new TestScheduler();

            _verificationViewModel = new VerificationViewModel(_scheduler, _navigationService.Object,
                _authService.Object,
                _storageService.Object, _userDialogs.Object);
        }

        [Test]
        public void ClearValidationCommand_WithNoData_DoesntThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _verificationViewModel.ClearValidationCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
        }
        
        [Test]
        public void VerifyCommand_WithInvalidVerificationCode_doesntCallAnyService()
        {
            // Act
            _verificationViewModel.VerifyCommand.Execute().Subscribe();
            _scheduler.Start();

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

            _authService.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.Unauthorized});

            // Act
            _verificationViewModel.VerifyCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

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

            _authService.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _verificationViewModel.VerifyCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

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

            _authService.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new CheckedResponse<bool> {Data = false, Error = true, ErrorMessage = "error message"});

            // Act
            _verificationViewModel.VerifyCommand.Execute().Subscribe();
            _scheduler.Start();

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
            
            _authService.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new CheckedResponse<bool> {Data = true});

            _navigationService.Setup(mock => mock.NavigateAsync("/BaseFlyoutPage/NavigationPage/HomePage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _verificationViewModel.VerifyCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsFalse(_verificationViewModel.IsError,
                "vm.IsError should be false if verification code was successful.");
            _navigationService.Verify(n => n.NavigateAsync("/BaseFlyoutPage/NavigationPage/HomePage"),
                Times.Once);
        }

        [Test]
        public void ResendVerificationCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUsernameAsync())
                .ReturnsAsync("username");
            
            _authService.Setup(mock => mock.ReVerifyAsync(It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.Unauthorized});

            // Act
            _verificationViewModel.ResendVerificationCommand.Execute().Catch(Observable.Return(Unit.Default))
                .Subscribe();
            _scheduler.Start();

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
            
            _authService.Setup(mock => mock.ReVerifyAsync(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _verificationViewModel.ResendVerificationCommand.Execute().Catch(Observable.Return(Unit.Default))
                .Subscribe();
            _scheduler.Start();

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
            
            _authService.Setup(mock => mock.ReVerifyAsync(It.IsAny<string>()))
                .Verifiable();

            _userDialogs.Setup(mock => mock.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Verifiable();

            // Act
            _verificationViewModel.ResendVerificationCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsFalse(_verificationViewModel.IsError,
                "vm.IsError should be false if resending verification was successful.");
            _authService.Verify();
            _userDialogs.Verify();
        }
    }
}