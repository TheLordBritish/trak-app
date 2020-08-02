using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using Acr.UserDialogs;
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
    public class GameUserEntryListViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IUserDialogs> _userDialogs;
        private Mock<IRestService> _restService;
        private TestScheduler _scheduler;

        private GameUserEntryListViewModel _gameUserEntryListViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _userDialogs = new Mock<IUserDialogs>();
            _restService = new Mock<IRestService>();
            _scheduler = new TestScheduler();

            _gameUserEntryListViewModel = new GameUserEntryBacklogListViewModel(_scheduler, _navigationService.Object,
                _storageService.Object, _userDialogs.Object, _restService.Object);
        }
        
        [Test]
        public void LoadCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _gameUserEntryListViewModel.IsActive = true;
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _gameUserEntryListViewModel.LoadCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameUserEntry>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameUserEntryListViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.GameLibraryListPageEmptyServerError, _gameUserEntryListViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void LoadCommand_ThrowsGenericException_SetsErrorMessageAsGenericError()
        {
            // Arrange
            _gameUserEntryListViewModel.IsActive = true;
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _gameUserEntryListViewModel.LoadCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameUserEntry>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameUserEntryListViewModel.IsError, "vm.IsError should be true if a generic exception is thrown.");
            Assert.AreEqual(Messages.GameLibraryListPageEmptyGenericError, _gameUserEntryListViewModel.ErrorMessage,
                "The error message is incorrect.");
        }
        
        [Test]
        public void LoadCommand_WithIsActiveIsFalse_DoesntInvokeRestService()
        {
            // Act
            _gameUserEntryListViewModel.LoadCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            _restService.Verify(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.IsTrue(_gameUserEntryListViewModel.IsEmpty, "_gameUserEntryListViewModel.IsEmpty should be true if the page isn't active.");
        }
        
        [Test]
        public void LoadCommand_WithEmptyResult_SetIsEmptyToTrue()
        {
            // Arrange
            _gameUserEntryListViewModel.IsActive = true;
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>());
            
            // Act
            _gameUserEntryListViewModel.LoadCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_gameUserEntryListViewModel.IsEmpty, "_gameUserEntryListViewModel.IsEmpty should be true if the API returns no results.");
            Assert.AreEqual(0, _gameUserEntryListViewModel.Items.Count, "There should be no items in the list for an empty result.");
        }
        
        [Test]
        public void LoadCommand_WithResults_SetIsEmptyToFalseAndConvertsToItems()
        {
            // Arrange
            _gameUserEntryListViewModel.IsActive = true;
            
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
                        Data = new []
                        {
                            new GameUserEntry
                            {
                                GameReleaseDate = DateTime.Now,
                                Publishers = new []
                                {
                                    "publisher"
                                }
                            }
                        }
                    }
                });
            
            // Act
            _gameUserEntryListViewModel.LoadCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsFalse(_gameUserEntryListViewModel.IsEmpty, "_gameUserEntryListViewModel.IsEmpty should be false if the API returns results.");
            Assert.AreEqual(1, _gameUserEntryListViewModel.Items.Count, "There should be items in the list.");
        }
        
        [Test]
        public void LoadMoreCommand_ThrowsApiException_CallsUserDialog()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            _userDialogs.Setup(mock => mock.Toast(It.IsAny<ToastConfig>()))
                .Verifiable();
            
            // Act
            _gameUserEntryListViewModel.LoadMoreCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameUserEntry>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameUserEntryListViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            _userDialogs.Verify();
        }

        [Test]
        public void LoadMoreCommand_ThrowsGenericException_CallsUserDialog()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            _userDialogs.Setup(mock => mock.Toast(It.IsAny<ToastConfig>()))
                .Verifiable();
            
            // Act
            _gameUserEntryListViewModel.LoadMoreCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameUserEntry>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameUserEntryListViewModel.IsError, "vm.IsError should be true if a generic exception is thrown.");
            _userDialogs.Verify();
        }

         [Test]
        public void LoadMoreCommand_WithEmptyResult_DoesntAddNewItems()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>());
            
            // Act
            _gameUserEntryListViewModel.LoadMoreCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.AreEqual(0, _gameUserEntryListViewModel.Items.Count, "New items shouldn't be added if the result for loading more is empty.");
        }
        
        [Test]
        public void LoadMoreCommand_WithResults_SetIsEmptyToFalseAndConvertsToItems()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token");

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>
                {
                    Embedded = new HateoasResources<GameUserEntry>
                    {
                        Data = new []
                        {
                            new GameUserEntry
                            {
                                GameReleaseDate = DateTime.Now,
                                Publishers = new []
                                {
                                    "publisher"
                                }
                            }
                        }
                    }
                });
            
            // Act
            _gameUserEntryListViewModel.LoadMoreCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(1, _gameUserEntryListViewModel.Items.Count, "There should be items in the list.");
        }
        
        [Test]
        public void AddCommand_WithNoData_DoesntThrowException()
        {
            _navigationService.Setup(mock => mock.NavigateAsync(It.IsAny<string>(), It.IsAny<INavigationParameters>()))
                .Verifiable();
            
            Assert.DoesNotThrow(() =>
            {
                // Act
                _gameUserEntryListViewModel.AddGameCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
            
            // Assert
            _navigationService.Verify();
        }
    }
}