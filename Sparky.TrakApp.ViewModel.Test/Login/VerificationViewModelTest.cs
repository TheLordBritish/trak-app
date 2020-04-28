using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Test.Login
{
    public class VerificationViewModelTest
    {
        [Test]
        public void VerifyCommand_WithApiExceptionThrownWhenVerifying_FlagsError()
        {
            // Arrange
            var navigationServiceMock = new Mock<INavigationService>();
            
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<string>()))
                .Throws(new ApiException());
            
            var storageServiceMock = new Mock<IStorageService>();
            storageServiceMock.Setup(mock => mock.GetUsernameAsync())
                .Returns(Task.FromResult("username"));

            storageServiceMock.Setup(mock => mock.GetAuthTokenAsync())
                .Returns(Task.FromResult("token"));
            
            var vm = new VerificationViewModel(navigationServiceMock.Object, authServiceMock.Object, storageServiceMock.Object);
            
            // Act
            vm.VerifyCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(vm.IsError, "There should be errors if an exception is thrown.");
            Assert.IsFalse(vm.IsBusy, "Bust should be disabled if an exception is thrown.");
            Assert.AreEqual(vm.ErrorMessage, Messages.ErrorMessageApiError);
            
            navigationServiceMock.Verify(mock => mock.NavigateAsync(It.IsAny<string>()), Times.Never());
        }
        
        [Test]
        public void VerifyCommand_WithGenericExceptionThrownWhenVerifying_FlagsError()
        {
            // Arrange
            var navigationServiceMock = new Mock<INavigationService>();
            
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<string>()))
                .Throws(new Exception());
            
            var storageServiceMock = new Mock<IStorageService>();
            storageServiceMock.Setup(mock => mock.GetUsernameAsync())
                .Returns(Task.FromResult("username"));

            storageServiceMock.Setup(mock => mock.GetAuthTokenAsync())
                .Returns(Task.FromResult("token"));
            
            var vm = new VerificationViewModel(navigationServiceMock.Object, authServiceMock.Object, storageServiceMock.Object);
            
            // Act
            vm.VerifyCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(vm.IsError, "There should be errors if an exception is thrown.");
            Assert.IsFalse(vm.IsBusy, "Bust should be disabled if an exception is thrown.");
            Assert.AreEqual(vm.ErrorMessage, Messages.ErrorMessageGeneric);
            
            navigationServiceMock.Verify(mock => mock.NavigateAsync(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void VerifyCommand_OnSuccessfulVerification_NavigatesAwayFromPage()
        {
            // Arrange
            var navigationServiceMock = new Mock<INavigationService>();
            navigationServiceMock.Setup(mock => mock.NavigateAsync("/NavigationPage/HomePage"))
                .Returns(Task.FromResult(new Mock<INavigationResult>().Object));
            
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(mock => mock.VerifyAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            
            var storageServiceMock = new Mock<IStorageService>();
            storageServiceMock.Setup(mock => mock.GetUsernameAsync())
                .Returns(Task.FromResult("username"));

            storageServiceMock.Setup(mock => mock.GetAuthTokenAsync())
                .Returns(Task.FromResult("token"));
            
            var vm = new VerificationViewModel(navigationServiceMock.Object, authServiceMock.Object, storageServiceMock.Object);
            
            // Act
            vm.VerifyCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(vm.IsError, "Navigation should result in no errors.");
            Assert.IsFalse(vm.IsBusy, "Busy should be false when method has finished.");
            
            navigationServiceMock.Verify(mock => mock.NavigateAsync(It.IsAny<string>()), Times.Once());
        }
    }
}