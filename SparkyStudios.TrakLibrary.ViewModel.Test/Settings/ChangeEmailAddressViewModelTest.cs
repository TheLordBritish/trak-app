using System;
using System.Reactive;
using System.Reactive.Linq;
using Acr.UserDialogs;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Model.Settings;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Settings;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Settings
{
    [TestFixture]
    public class ChangeEmailAddressViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IRestService> _restService;
        private Mock<IAuthService> _authService;
        private Mock<IUserDialogs> _userDialogs;
        private TestScheduler _scheduler;

        private ChangeEmailAddressViewModel _changeEmailAddressViewModel;
        
        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _restService = new Mock<IRestService>();
            _authService = new Mock<IAuthService>();
            _userDialogs = new Mock<IUserDialogs>();
            _scheduler = new TestScheduler();

            _changeEmailAddressViewModel = new ChangeEmailAddressViewModel(_scheduler, _navigationService.Object,
                _storageService.Object, _authService.Object, _restService.Object, _userDialogs.Object);
        }
        
        [Test]
        public void ClearValidationCommand_WithNoData_DoesntThrowException()
        {
            // Assert
            Assert.DoesNotThrow(() =>
            {
                _changeEmailAddressViewModel.ClearValidationCommand.Execute().Subscribe();
                _scheduler.Start();
            });
        }
        
        [Test]
        public void ChangeEmailAddressCommand_WithInvalidData_DoesntSendChangePasswordRequest()
        {
            // Act
            _changeEmailAddressViewModel.ChangeEmailAddressCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.ChangeEmailAddressAsync(It.IsAny<string>(), It.IsAny<ChangeEmailAddressRequest>()), Times.Never);
        }
        
        [Test]
        public void ChangeEmailAddressCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _changeEmailAddressViewModel.EmailAddress.Value = "test@traklibrary.com";

            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");

            _authService.Setup(m => m.ChangeEmailAddressAsync(It.IsAny<string>(), It.IsAny<ChangeEmailAddressRequest>()))
                .Throws(new ApiException());

            // Act
            _changeEmailAddressViewModel.ChangeEmailAddressCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_changeEmailAddressViewModel.IsError,
                "_changeEmailAddressViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _changeEmailAddressViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }
        
        [Test]
        public void ChangeEmailAddressCommand_ThrowsException_SetsErrorMessageAsApiGeneric()
        {
            // Arrange
            _changeEmailAddressViewModel.EmailAddress.Value = "test@traklibrary.com";

            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");

            _authService.Setup(m => m.ChangeEmailAddressAsync(It.IsAny<string>(), It.IsAny<ChangeEmailAddressRequest>()))
                .Throws(new Exception());

            // Act
            _changeEmailAddressViewModel.ChangeEmailAddressCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_changeEmailAddressViewModel.IsError,
                "_changeEmailAddressViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _changeEmailAddressViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }
        
        [Test]
        public void ChangeEmailAddressCommand_WithCheckedResponseError_SetsErrorMessage()
        {
            // Arrange
            _changeEmailAddressViewModel.EmailAddress.Value = "test@traklibrary.com";

            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");

            var response = new CheckedResponse<bool>
            {
                Error = true,
                ErrorMessage = "error-message"
            };
            
            _authService.Setup(m => m.ChangeEmailAddressAsync(It.IsAny<string>(), It.IsAny<ChangeEmailAddressRequest>()))
                .ReturnsAsync(response);
            
            // Act
            _changeEmailAddressViewModel.ChangeEmailAddressCommand.Execute();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_changeEmailAddressViewModel.IsError,
                "_changeEmailAddressViewModel.IsError should be true if the response has an error.");
            Assert.AreEqual(response.ErrorMessage, _changeEmailAddressViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }
        
        [Test]
        public void ChangeEmailAddressCommand_WithSuccessfulCheckedResponse_LogsOutAndNavigatesToLoginPage()
        {
            // Arrange
            _changeEmailAddressViewModel.EmailAddress.Value = "test@traklibrary.com";

            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");
            
            _authService.Setup(m => m.ChangeEmailAddressAsync(It.IsAny<string>(), It.IsAny<ChangeEmailAddressRequest>()))
                .ReturnsAsync(new CheckedResponse<bool>
                {
                    Error = false
                });

            _storageService.Setup(m => m.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(m => m.GetDeviceIdAsync())
                .ReturnsAsync(Guid.Empty);
            
            _restService.Setup(m => m.DeleteAsync(It.IsAny<string>()))
                .Verifiable();

            _storageService.Setup(m => m.SetUsernameAsync(It.IsAny<string>()))
                .Verifiable();
            _storageService.Setup(m => m.SetAuthTokenAsync(It.IsAny<string>()))
                .Verifiable();
            _storageService.Setup(m => m.SetUserIdAsync(It.IsAny<long>()))
                .Verifiable();

            _navigationService.Setup(m => m.NavigateAsync("/LoginPage"))
                .ReturnsAsync(new NavigationResult());

            _userDialogs.Setup(m => m.AlertAsync(It.IsAny<AlertConfig>(), null))
                .Verifiable();
            
            // Act
            _changeEmailAddressViewModel.ChangeEmailAddressCommand.Execute();
            _scheduler.Start();
            
            // Assert
            Assert.IsFalse(_changeEmailAddressViewModel.IsError, "vm.IsError should be false if login was successful.");

            _restService.Verify();
            _storageService.Verify();
            _userDialogs.Verify();
        }
    }
}