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
    public class GameOptionsViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IRestService> _restService;
        private TestScheduler _scheduler;

        private GameOptionsViewModel _gameOptionsViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _storageService = new Mock<IStorageService>();
            _scheduler = new TestScheduler();

            _gameOptionsViewModel = new GameOptionsViewModel(_scheduler, _navigationService.Object,
                _storageService.Object,
                _restService.Object);
        }

        [Test]
        public void OnNavigatedTo_WithValidNavigationParameters_PopulatesCorrectValues()
        {
            // Arrange
            var gameUrl = new Uri("https://games.url");
            var gameId = 5L;
            var platformId = 6L;
            var status = GameUserEntryStatus.Completed;

            var parameters = new NavigationParameters
            {
                {"game-url", gameUrl},
                {"game-id", gameId},
                {"platform-id", platformId},
                {"status", status}
            };

            // Act
            _gameOptionsViewModel.OnNavigatedTo(parameters);

            // Assert
            Assert.AreEqual(gameUrl, _gameOptionsViewModel.GameUrl,
                "The game URL passed in during navigation should match.");
            Assert.AreEqual(gameId, _gameOptionsViewModel.GameId,
                "The game ID passed in during navigation should match.");
            Assert.AreEqual(platformId, _gameOptionsViewModel.PlatformId,
                "The platforms should contain the values passed in during navigation.");
            Assert.AreEqual(status, _gameOptionsViewModel.SelectedStatus,
                "The status should match the value passed in during navigation.");
        }

        [Test]
        public void OnNavigatedFrom_WithNoData_ContainsCorrectParameters()
        {
            // Arrange
            var parameters = new NavigationParameters();

            // Act
            _gameOptionsViewModel.OnNavigatedFrom(parameters);

            // Assert
            Assert.IsTrue(parameters.ContainsKey("game-url"), "The navigation parameters should contain the game URL.");
            Assert.IsTrue(parameters.ContainsKey("platform-id"),
                "The navigation parameters should contain the platform ID.");
            Assert.IsTrue(parameters.ContainsKey("in-library"),
                "The navigation parameters should contain if it's in the library.");
            Assert.IsTrue(parameters.ContainsKey("status"), "The navigation parameters should contain the status.");
        }

        [Test]
        public void UpdateGameCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _gameOptionsViewModel.UpdateGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameOptionsViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _gameOptionsViewModel.ErrorMessage,
                "The error message is incorrect.");

            Assert.IsFalse(_gameOptionsViewModel.IsUpdated, "IsUpdated should be false if an exception was thrown.");
        }

        [Test]
        public void UpdateGameCommand_ThrowsGenericException_SetsErrorMessageAsGenericError()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _gameOptionsViewModel.UpdateGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameOptionsViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _gameOptionsViewModel.ErrorMessage,
                "The error message is incorrect.");

            Assert.IsFalse(_gameOptionsViewModel.IsUpdated, "IsUpdated should be false if an exception was thrown.");
        }

        [Test]
        public void UpdateGameCommand_WithNonExistentEntry_DoesntUpdateEntry()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>());

            _navigationService.Setup(mock => mock.GoBackAsync())
                .ReturnsAsync(new NavigationResult());

            // Act
            _gameOptionsViewModel.UpdateGameCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify(
                mock => mock.PutAsync(It.IsAny<string>(), It.IsAny<GameUserEntry>(), It.IsAny<string>()), Times.Never);
            _navigationService.Verify(mock => mock.GoBackAsync(), Times.Once);

            Assert.IsTrue(_gameOptionsViewModel.IsUpdated, "IsUpdated should be true if no exception was thrown.");
        }

        [Test]
        public void UpdateGameCommand_WithExistingEntry_UpdatesEntry()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
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

            _restService.Setup(mock => mock.PutAsync(It.IsAny<string>(), It.IsAny<GameUserEntry>(), It.IsAny<string>()))
                .ReturnsAsync(new GameUserEntry());

            _navigationService.Setup(mock => mock.GoBackAsync())
                .ReturnsAsync(new NavigationResult());

            // Act
            _gameOptionsViewModel.UpdateGameCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify(
                mock => mock.PutAsync(It.IsAny<string>(), It.IsAny<GameUserEntry>(), It.IsAny<string>()), Times.Once);
            _navigationService.Verify(mock => mock.GoBackAsync(), Times.Once);

            Assert.IsTrue(_gameOptionsViewModel.IsUpdated, "IsUpdated should be true if no exception was thrown.");
        }

        [Test]
        public void DeleteGameCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _gameOptionsViewModel.DeleteGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameOptionsViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _gameOptionsViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void DeleteGameCommand_ThrowsGenericException_SetsErrorMessageAsGenericError()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _gameOptionsViewModel.DeleteGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameOptionsViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _gameOptionsViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void DeleteGameCommand_WithNonExistentEntry_DoesntDeleteEntry()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>());

            _navigationService.Setup(mock => mock.GoBackAsync())
                .ReturnsAsync(new NavigationResult());

            // Act
            _gameOptionsViewModel.DeleteGameCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify(mock => mock.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _navigationService.Verify(mock => mock.GoBackAsync(), Times.Once);
        }

        [Test]
        public void DeleteGameCommand_WithExistingEntry_DeletesEntry()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
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

            _restService.Setup(mock => mock.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();

            _navigationService.Setup(mock => mock.GoBackAsync())
                .ReturnsAsync(new NavigationResult());

            // Act
            _gameOptionsViewModel.DeleteGameCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify();
            _navigationService.Verify(mock => mock.GoBackAsync(), Times.Once);
        }
    }
}