using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Login;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Login
{
    public class RecoveryViewModelTest
    {
        private Mock<IAuthService> _authService;
        private Mock<IStorageService> _storageService;
        private Mock<INavigationService> _navigationService;
        private Mock<IRestService> _restService;
        private TestScheduler _scheduler;

        private RecoveryViewModel _recoveryViewModel;

        [SetUp]
        public void SetUp()
        {
            _authService = new Mock<IAuthService>();
            _storageService = new Mock<IStorageService>();
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _scheduler = new TestScheduler();

            _recoveryViewModel = new RecoveryViewModel(_scheduler, _navigationService.Object, _authService.Object,
                _storageService.Object, _restService.Object);
        }

        [Test]
        public void ClearValidationCommand_WithNoData_DoesntThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _recoveryViewModel.ClearValidationCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
        }
        
        [Test]
        public void RecoverCommand_WithInvalidUsername_doesntCallRecoverAsync()
        {
            // Arrange
            _recoveryViewModel.RecoveryToken.Value = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            _recoveryViewModel.Password.Value = "Password123";
            _recoveryViewModel.ConfirmPassword.Value = "Password123";

            // Act
            _recoveryViewModel.RecoverCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.RecoverAsync(It.IsAny<RecoveryRequest>()), Times.Never);
        }

        [Test]
        public void RecoverCommand_WithInvalidRecoveryToken_doesntCallRecoverAsync()
        {
            // Arrange
            _recoveryViewModel.Username.Value = "TestUsername";
            _recoveryViewModel.Password.Value = "Password123";
            _recoveryViewModel.ConfirmPassword.Value = "Password123";

            // Act
            _recoveryViewModel.RecoverCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.RecoverAsync(It.IsAny<RecoveryRequest>()), Times.Never);
        }

        [Test]
        public void RecoverCommand_WithInvalidPassword_doesntCallRecoverAsync()
        {
            // Arrange
            _recoveryViewModel.Username.Value = "TestUsername";
            _recoveryViewModel.RecoveryToken.Value = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            _recoveryViewModel.ConfirmPassword.Value = "Password123";

            // Act
            _recoveryViewModel.RecoverCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.RecoverAsync(It.IsAny<RecoveryRequest>()), Times.Never);
        }

        [Test]
        public void RecoverCommand_WithInvalidConfirmPassword_doesntCallRecoverAsync()
        {
            // Arrange
            _recoveryViewModel.Username.Value = "TestUsername";
            _recoveryViewModel.RecoveryToken.Value = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            _recoveryViewModel.Password.Value = "Password123";
            _recoveryViewModel.ConfirmPassword.Value = "Password1234";

            // Act
            _recoveryViewModel.RecoverCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.RecoverAsync(It.IsAny<RecoveryRequest>()), Times.Never);
        }

        [Test]
        public void RecoverCommand_ThrowsTaskCanceledException_SetsErrorMessageAsNoInternet()
        {
            // Arrange
            _recoveryViewModel.Username.Value = "TestUsername";
            _recoveryViewModel.RecoveryToken.Value = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            _recoveryViewModel.Password.Value = "Password123";
            _recoveryViewModel.ConfirmPassword.Value = "Password123";

            _authService.Setup(mock => mock.RecoverAsync(It.IsAny<RecoveryRequest>()))
                .Throws(new TaskCanceledException());

            // Act
            _recoveryViewModel.RecoverCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_recoveryViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageNoInternet, _recoveryViewModel.ErrorMessage,
                "The error message is incorrect.");
        }
        
        [Test]
        public void RecoverCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _recoveryViewModel.Username.Value = "TestUsername";
            _recoveryViewModel.RecoveryToken.Value = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            _recoveryViewModel.Password.Value = "Password123";
            _recoveryViewModel.ConfirmPassword.Value = "Password123";

            _authService.Setup(mock => mock.RecoverAsync(It.IsAny<RecoveryRequest>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.Unauthorized});

            // Act
            _recoveryViewModel.RecoverCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_recoveryViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _recoveryViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void RecoverCommand_ThrowsNonApiException_SetsErrorMessageAsGeneric()
        {
            // Arrange
            _recoveryViewModel.Username.Value = "TestUsername";
            _recoveryViewModel.RecoveryToken.Value = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            _recoveryViewModel.Password.Value = "Password123";
            _recoveryViewModel.ConfirmPassword.Value = "Password123";

            _authService.Setup(mock => mock.RecoverAsync(It.IsAny<RecoveryRequest>()))
                .Throws(new Exception());

            // Act
            _recoveryViewModel.RecoverCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_recoveryViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _recoveryViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void RecoverCommand_WithErroneousUserResponse_SetsErrorMessage()
        {
            // Arrange
            _recoveryViewModel.Username.Value = "TestUsername";
            _recoveryViewModel.RecoveryToken.Value = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            _recoveryViewModel.Password.Value = "Password123";
            _recoveryViewModel.ConfirmPassword.Value = "Password123";

            _authService.Setup(mock => mock.RecoverAsync(It.IsAny<RecoveryRequest>()))
                .ReturnsAsync(new CheckedResponse<UserResponse> {Error = true, ErrorMessage = "error"});

            // Act
            _recoveryViewModel.RecoverCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_recoveryViewModel.IsError, "vm.IsError should be true if the user response has an error.");
            Assert.AreEqual("error", _recoveryViewModel.ErrorMessage, "The error message is incorrect.");

            _authService.Verify(s => s.GetTokenAsync(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Test]
        public void RecoverCommand_WithValidUserResponse_SavesCredentialsAndNavigatesToHomePage()
        {
            // Arrange
            _recoveryViewModel.Username.Value = "TestUsername";
            _recoveryViewModel.RecoveryToken.Value = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            _recoveryViewModel.Password.Value = "Password123";
            _recoveryViewModel.ConfirmPassword.Value = "Password123";

            _authService.Setup(mock => mock.RecoverAsync(It.IsAny<RecoveryRequest>()))
                .ReturnsAsync(new CheckedResponse<UserResponse>
                {
                    Data = new UserResponse
                    {
                        Id = 5L,
                        Username = "Username"
                    }
                });

            _authService.Setup(mock => mock.GetTokenAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync("token");

            _storageService.Setup(mock => mock.SetAuthTokenAsync(It.IsAny<string>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetUserIdAsync(It.IsAny<long>()))
                .Verifiable();
            _storageService.Setup(mock => mock.SetUsernameAsync(It.IsAny<string>()))
                .Verifiable();

            _restService.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<NotificationRegistrationRequest>()))
                .Verifiable();

            _navigationService.Setup(mock => mock.NavigateAsync("/BaseFlyoutPage/NavigationPage/HomePage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _recoveryViewModel.RecoverCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsFalse(_recoveryViewModel.IsError,
                "vm.IsError should be false if recovery was successful.");

            _storageService.Verify();
            _restService.Verify();
            _navigationService.Verify(s => s.NavigateAsync("/BaseFlyoutPage/NavigationPage/HomePage"),
                Times.Once);
        }

        [Test]
        public void LoginCommand_WithNoData_NavigatesToLoginPage()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("LoginPage"))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _recoveryViewModel.LoginCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify(n => n.NavigateAsync("LoginPage"), Times.Once);
        }
    }
}