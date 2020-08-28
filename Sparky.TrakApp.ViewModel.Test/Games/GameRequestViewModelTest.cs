using System;
using System.Reactive;
using System.Reactive.Linq;
using Acr.UserDialogs;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Games;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Test.Games
{
    public class GameRequestViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IRestService> _restService;
        private Mock<IStorageService> _storageService;
        private Mock<IUserDialogs> _userDialogs;
        private TestScheduler _scheduler;
        
        private GameRequestViewModel _gameRequestViewModel;
        
        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _storageService = new Mock<IStorageService>();
            _userDialogs = new Mock<IUserDialogs>();
            _scheduler = new TestScheduler();
            
            _gameRequestViewModel = new GameRequestViewModel(_scheduler, _navigationService.Object, _restService.Object, _storageService.Object, _userDialogs.Object);
        }

        [Test]
        public void ClearValidationCommand_WithNoData_DoesntThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _gameRequestViewModel.ClearValidationCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
        }
        
        [Test]
        public void RequestCommand_WithInvalidTitle_doesntCallRestService()
        {
            // Arrange
            _gameRequestViewModel.Title.Value = string.Empty;
            
            // Act
            _gameRequestViewModel.RequestCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            _restService.Verify(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<GameRequest>()), Times.Never);
        }

        [Test]
        public void RequestCommand_WithInvalidNotes_doesntCallRestService()
        {
            // Arrange
            _gameRequestViewModel.Title.Value = "Title";
            _gameRequestViewModel.Notes.Value =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Scelerisque felis imperdiet proin fermentum leo. Diam sollicitudin tempor id eu nisl nunc mi ipsum. Scelerisque eu ultrices vitae auctor eu augue ut. Id cursus metus aliquam eleifend mi in. Platea dictumst vestibulum rhoncus est pellentesque elit ullamcorper dignissim. Tortor vitae purus faucibus ornare suspendisse sed nisi. Placerat vestibulum lectus mauris ultrices eros in. Id neque aliquam vestibulum morbi blandit cursus risus at. Sapien et ligula ullamcorper malesuada proin libero nunc consequat. Euismod nisi porta lorem mollis. Et malesuada fames ac turpis. Lectus magna fringilla urna porttitor rhoncus dolor purus. Ullamcorper a lacus vestibulum sed arcu. In ornare quam viverra orci sagittis. Turpis cursus in hac habitasse platea dictumst. Arcu odio ut sem nulla pharetra. In nisl nisi scelerisque eu ultrices vitae auctor. Tincidunt tortor aliquam nulla facilisi cras fermentum odio eu feugiat. Amet risus nullam eget felis. Curabitur vitae nunc sed velit dignissim. Sed lectus vestibulum mattis ullamcorper velit sed. Nisi porta lorem mollis aliquam ut porttitor leo. Nulla facilisi cras fermentum odio. Risus nullam eget felis eget. Dignissim cras tincidunt lobortis feugiat vivamus at augue eget. Egestas egestas fringilla phasellus faucibus scelerisque eleifend donec pretium. Et netus et malesuada fames. Lacus sed turpis tincidunt id aliquet risus. Sit amet nisl suscipit adipiscing. Semper risus in hendrerit gravida rutrum quisque non. Consequat nisl vel pretium lectus quam id leo. Justo laoreet sit amet cursus sit amet dictum sit amet. Fames ac turpis egestas sed. Nulla at volutpat diam ut. Sit amet mattis vulputate enim nulla aliquet porttitor lacus luctus. Sit amet nisl suscipit adipiscing bibendum est. Tincidunt eget nullam non nisi est. Quis lectus nulla at volutpat diam ut venenatis tellus. Tellus mauris a diam maecenas. Turpis egestas pretium aenean pharetra magna. Lacus suspendisse faucibus interdum posuere lorem ipsum. Tincidunt dui ut ornare lectus sit amet est placerat in. Vitae ultricies leo integer malesuada nunc vel risus commodo viverra. Tincidunt lobortis feugiat vivamus at augue eget arcu. Elit ullamcorper dignissim cras tincidunt lobortis feugiat vivamus. Sagittis id consectetur purus ut faucibus pulvinar elementum. Ultrices sagittis orci a scelerisque purus semper eget duis at. Ac auctor augue mauris augue neque. Pulvinar etiam non quam lacus suspendisse faucibus interdum posuere lorem. Donec massa sapien faucibus et molestie ac. Arcu non odio euismod lacinia at. Morbi tincidunt ornare massa eget egestas purus viverra. Neque aliquam vestibulum morbi blandit cursus risus at ultrices. Eget mauris pharetra et ultrices neque ornare aenean. Eget nulla facilisi etiam dignissim diam quis enim lobortis scelerisque. Mauris sit amet massa vitae tortor. Curabitur vitae nunc sed velit dignissim sodales ut eu sem. Sapien et ligula ullamcorper malesuada proin libero nunc consequat interdum. Nulla posuere sollicitudin aliquam ultrices sagittis orci a scelerisque. Nulla facilisi nullam vehicula ipsum a arcu cursus vitae. Nisl vel pretium lectus quam id leo in vitae turpis. Rutrum quisque non tellus orci ac. Quam pellentesque nec nam aliquam sem et tortor consequat id. Pretium viverra suspendisse potenti nullam ac tortor vitae purus faucibus. In vitae turpis massa sed elementum. Porttitor massa id neque aliquam vestibulum. Morbi tristique senectus et netus. Mauris augue neque gravida in fermentum et sollicitudin ac. Purus ut faucibus pulvinar elementum integer enim neque.";
            
            // Act
            _gameRequestViewModel.RequestCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            _restService.Verify(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<GameRequest>()), Times.Never);
        }

        [Test]
        public void RequestCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _gameRequestViewModel.Title.Value = "Title";

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(5L);
            
            _restService.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<GameRequest>()))
                .ThrowsAsync(new ApiException());
            
            // Act
            _gameRequestViewModel.RequestCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_gameRequestViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _gameRequestViewModel.ErrorMessage,
                "The error message is incorrect.");
        }
        
        [Test]
        public void RequestCommand_ThrowsException_SetsErrorMessageAsGenericError()
        {
            // Arrange
            _gameRequestViewModel.Title.Value = "Title";

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(5L);
            
            _restService.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<GameRequest>()))
                .ThrowsAsync(new Exception());
            
            // Act
            _gameRequestViewModel.RequestCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_gameRequestViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _gameRequestViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void RequestCommand_WithValidData_DisplaysAlertAndNavigatesBack()
        {
            // Arrange
            _gameRequestViewModel.Title.Value = "Title";

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(5L);
            
            _restService.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<GameRequest>()))
                .ReturnsAsync(new GameRequest());
            
            _userDialogs.Setup(mock => mock.AlertAsync(It.IsAny<AlertConfig>(), null))
                .Verifiable();
            
            _navigationService.Setup(mock => mock.GoBackAsync())
                .Verifiable();
            
            // Act
            _gameRequestViewModel.RequestCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            _userDialogs.Verify();
            _navigationService.Verify();
        }
    }
}