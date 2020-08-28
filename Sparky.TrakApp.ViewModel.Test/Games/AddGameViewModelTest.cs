using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Games;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Test.Games
{
    public class AddGameViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IRestService> _restService;
        private TestScheduler _scheduler;

        private AddGameViewModel _addGameViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _storageService = new Mock<IStorageService>();
            _scheduler = new TestScheduler();

            _addGameViewModel = new AddGameViewModel(_scheduler, _navigationService.Object, _storageService.Object,
                _restService.Object);
        }

        [Test]
        public void OnNavigatedTo_WithValidNavigationParameters_PopulatesCorrectValues()
        {
            // Arrange
            var gameId = 5L;
            var gameUrl = new Uri("https://games.url");
            var platforms = new[]
            {
                new Platform()
            };

            var parameters = new NavigationParameters
            {
                {"game-id", gameId},
                {"game-url", gameUrl},
                {"platforms", platforms}
            };

            // Act
            _addGameViewModel.OnNavigatedTo(parameters);

            // Assert
            Assert.AreEqual(gameId, _addGameViewModel.GameId, "The game ID passed in during navigation should match.");
            Assert.AreEqual(gameUrl, _addGameViewModel.GameUrl,
                "The game URL passed in during navigation should match.");
            Assert.AreEqual(platforms, _addGameViewModel.Platforms,
                "The platforms should contain the values passed in during navigation.");
        }

        [Test]
        public void OnNavigatedFrom_WithNoData_ContainsCorrectParameters()
        {
            // Arrange
            var parameters = new NavigationParameters();

            // Act
            _addGameViewModel.OnNavigatedFrom(parameters);

            // Assert
            Assert.IsTrue(parameters.ContainsKey("game-url"), "The navigation parameters should contain the game URL.");
            Assert.IsTrue(parameters.ContainsKey("platform-id"),
                "The navigation parameters should contain the platform ID.");
            Assert.IsTrue(parameters.ContainsKey("in-library"),
                "The navigation parameters should contain if it's in the library.");
            Assert.IsTrue(parameters.ContainsKey("status"), "The navigation parameters should contain the status.");
        }

        [Test]
        public void ClearValidationCommand_WithNoData_DoesntThrowException()
        {
            // Assert
            Assert.DoesNotThrow(() =>
            {
                _addGameViewModel.ClearValidationCommand.Execute().Subscribe();
                _scheduler.Start();
            });
        }

        [Test]
        public void AddGameCommand_WithInvalidSelectedPlatform_DoesntCallRestService()
        {
            // Arrange
            _addGameViewModel.SelectedPlatform.Value = null;

            // Act
            _addGameViewModel.AddGameCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify(a => a.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()),
                Times.Never());
        }

        [Test]
        public void AddGameCommand_WithInvalidSelectedStatus_DoesntCallRestService()
        {
            // Arrange
            _addGameViewModel.SelectedPlatform.Value = new Platform();
            _addGameViewModel.SelectedStatus.Value = GameUserEntryStatus.None;

            // Act
            _addGameViewModel.AddGameCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify(a => a.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()),
                Times.Never());
        }

        [Test]
        public void AddGameCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _addGameViewModel.SelectedPlatform.Value = new Platform();
            _addGameViewModel.SelectedStatus.Value = GameUserEntryStatus.Backlog;

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _addGameViewModel.AddGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_addGameViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _addGameViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void AddGameCommand_ThrowsGenericException_SetsErrorMessageAsGenericError()
        {
            // Arrange
            _addGameViewModel.SelectedPlatform.Value = new Platform();
            _addGameViewModel.SelectedStatus.Value = GameUserEntryStatus.Backlog;

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _addGameViewModel.AddGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_addGameViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _addGameViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void AddGameCommand_WithExistingGameUserEntry_UpdatesExistingEntry()
        {
            // Arrange
            _addGameViewModel.SelectedPlatform.Value = new Platform();
            _addGameViewModel.SelectedStatus.Value = GameUserEntryStatus.Backlog;

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>
                {
                    Embedded = new HateoasResources<GameUserEntry>
                    {
                        Data = new[]
                        {
                            new GameUserEntry()
                        }
                    }
                });

            _restService.Setup(mock => mock.PutAsync(It.IsAny<string>(), It.IsAny<GameUserEntry>()))
                .ReturnsAsync(new GameUserEntry());

            _navigationService.Setup(mock => mock.GoBackAsync())
                .ReturnsAsync(new NavigationResult());

            // Act
            _addGameViewModel.AddGameCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify(s => s.PutAsync(It.IsAny<string>(), It.IsAny<GameUserEntry>()),
                Times.Once);
            _navigationService.Verify(mock => mock.GoBackAsync(), Times.Once);
        }

        [Test]
        public void AddGameCommand_WithNewGameUserEntry_CreatesNewEntry()
        {
            // Arrange
            _addGameViewModel.SelectedPlatform.Value = new Platform();
            _addGameViewModel.SelectedStatus.Value = GameUserEntryStatus.Backlog;

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>());

            _restService
                .Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<GameUserEntry>()))
                .ReturnsAsync(new GameUserEntry());

            _navigationService.Setup(mock => mock.GoBackAsync())
                .ReturnsAsync(new NavigationResult());

            // Act
            _addGameViewModel.AddGameCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify(s => s.PostAsync(It.IsAny<string>(), It.IsAny<GameUserEntry>()), Times.Once);
            _navigationService.Verify(mock => mock.GoBackAsync(), Times.Once);
        }
    }
}