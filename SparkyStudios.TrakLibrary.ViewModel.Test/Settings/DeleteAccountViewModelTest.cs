using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Settings;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Settings
{
    [TestFixture]
    public class DeleteAccountViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IAuthService> _authService;
        private Mock<IRestService> _restService;
        private Mock<IUserDialogs> _userDialogs;
        private TestScheduler _scheduler;

        private DeleteAccountViewModel _deleteAccountViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _restService = new Mock<IRestService>();
            _authService = new Mock<IAuthService>();
            _userDialogs = new Mock<IUserDialogs>();
            _scheduler = new TestScheduler();

            _deleteAccountViewModel = new DeleteAccountViewModel(_scheduler, _navigationService.Object,
                _storageService.Object, _authService.Object,  _restService.Object, _userDialogs.Object);
        }

        [Test]
        public void ClearValidationCommand_WithNoData_DoesntThrowException()
        {
            // Assert
            Assert.DoesNotThrow(() =>
            {
                _deleteAccountViewModel.ClearValidationCommand.Execute().Subscribe();
                _scheduler.Start();
            });
        }

        [Test]
        public void DeleteAccountCommand_WithInvalidData_DoesntDeleteAccount()
        {
            // Arrange 
            _deleteAccountViewModel.DeleteMe.Value = string.Empty;
            
            // Act
            _deleteAccountViewModel.DeleteAccountCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.DeleteByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void DeleteAccountCommand_ThrowsTaskCanceledException_SetsErrorMessageAsNoInternet()
        {
            // Arrange
            _deleteAccountViewModel.DeleteMe.Value = Messages.DeleteAccountPageDeleteMe;

            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");

            _authService.Setup(m => m.DeleteByUsernameAsync(It.IsAny<string>()))
                .Throws(new TaskCanceledException());

            // Act
            _deleteAccountViewModel.DeleteAccountCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_deleteAccountViewModel.IsError,
                "_deleteAccountViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageNoInternet, _deleteAccountViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }
        
        [Test]
        public void DeleteAccountCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _deleteAccountViewModel.DeleteMe.Value = Messages.DeleteAccountPageDeleteMe;

            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");

            _authService.Setup(m => m.DeleteByUsernameAsync(It.IsAny<string>()))
                .Throws(new ApiException());

            // Act
            _deleteAccountViewModel.DeleteAccountCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_deleteAccountViewModel.IsError,
                "_deleteAccountViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _deleteAccountViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void DeleteAccountCommand_ThrowsException_SetsErrorMessageAsApiGeneric()
        {
            // Arrange
            _deleteAccountViewModel.DeleteMe.Value = Messages.DeleteAccountPageDeleteMe;
            
            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");

            _authService.Setup(m => m.DeleteByUsernameAsync(It.IsAny<string>()))
                .Throws(new Exception());
            
            // Act
            _deleteAccountViewModel.DeleteAccountCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_deleteAccountViewModel.IsError,
                "_deleteAccountViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _deleteAccountViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void DeleteAccountCommand_WithSuccessfulDeletion_LogsOutAndNavigatesToLoginPage()
        {
            // Arrange
            _deleteAccountViewModel.DeleteMe.Value = Messages.DeleteAccountPageDeleteMe;

            _storageService.Setup(m => m.GetUsernameAsync())
                .ReturnsAsync("username");
            
            _authService.Setup(m => m.DeleteByUsernameAsync(It.IsAny<string>()))
                .Verifiable();

            _storageService.Setup(m => m.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(m => m.GetDeviceIdAsync())
                .ReturnsAsync(Guid.Empty);
            
            _restService.Setup(m => m.DeleteAsync(It.IsAny<string>()))
                .Verifiable();

            _storageService.Setup(m => m.ClearCredentialsAsync())
                .Verifiable();

            _navigationService.Setup(m => m.NavigateAsync("/LoginPage"))
                .ReturnsAsync(new NavigationResult());

            _userDialogs.Setup(m => m.AlertAsync(It.IsAny<AlertConfig>(), null))
                .Verifiable();
            
            // Act
            _deleteAccountViewModel.DeleteAccountCommand.Execute();
            _scheduler.Start();
            
            // Assert
            Assert.IsFalse(_deleteAccountViewModel.IsError, "vm.IsError should be false if deletion was successful.");

            _restService.Verify();
            _storageService.Verify();
            _userDialogs.Verify();
        }
    }
}