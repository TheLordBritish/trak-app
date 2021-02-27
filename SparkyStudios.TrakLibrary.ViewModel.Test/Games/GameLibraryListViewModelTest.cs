using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Games;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Games
{
    [TestFixture]
    public class GameLibraryListViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IRestService> _restService;
        private Mock<IUserDialogs> _userDialogs;
        private TestScheduler _scheduler;
        
        private GameLibraryListViewModel _gameLibraryListViewModel;
        
        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _userDialogs = new Mock<IUserDialogs>();
            _scheduler = new TestScheduler();

            _gameLibraryListViewModel = new GameLibraryListViewModel(_scheduler, _navigationService.Object, _restService.Object, _userDialogs.Object);
        }

        [Test]
        public void SearchCommand_ThrowsTaskCanceledException_SetsMessageAsNoInternet()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .Throws(new TaskCanceledException());

            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameDetails>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageNoInternet, _gameLibraryListViewModel.Message,
                "The message is incorrect.");
        }
        
        [Test]
        public void SearchCommand_ThrowsApiException_SetsMessageAsApiError()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameDetails>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.GameLibraryListPageEmptyServerError, _gameLibraryListViewModel.Message,
                "The message is incorrect.");
        }

        [Test]
        public void SearchCommand_ThrowsGenericException_SetsMessageAsGenericError()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameDetails>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if a generic exception is thrown.");
            Assert.AreEqual(Messages.GameLibraryListPageEmptyGenericError, _gameLibraryListViewModel.Message,
                "The message is incorrect.");
        }

        [Test]
        public void SearchCommand_WithNoGames_SetMessageToEmpty()
        {
            // Arrange
            _gameLibraryListViewModel.SearchQuery = "search";
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameDetails>
                {
                    Embedded = new HateoasResources<GameDetails>
                    {
                        Data = Enumerable.Empty<GameDetails>()
                    }
                });
            
            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.AreEqual(0, _gameLibraryListViewModel.Items.Count, "There should be no items in the list.");
            Assert.AreEqual(
                string.Format(Messages.GameLibraryPageEmptySearchResult, _gameLibraryListViewModel.SearchQuery), _gameLibraryListViewModel.Message);
        }
        
        [Test]
        public void SearchCommand_WithResultsWithNullPlatforms_ConvertsToItems()
        {
            // Arrange
            _gameLibraryListViewModel.SearchQuery = "search";
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameDetails>
                {
                    Embedded = new HateoasResources<GameDetails>
                    {
                        Data = new []
                        {
                            new GameDetails
                            {
                                Title = "test-title",
                                Publishers = new SortedSet<Publisher>
                                {
                                    new Publisher()
                                },
                                Genres = new SortedSet<Genre>
                                {
                                    new Genre()
                                }
                            }
                        }
                    }
                });
            
            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.AreEqual(1, _gameLibraryListViewModel.Items.Count, "There should be items in the list.");
        }
        
        [Test]
        public void SearchCommand_WithResults_ConvertsToItems()
        {
            // Arrange
            _gameLibraryListViewModel.SearchQuery = "search";
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameDetails>
                {
                    Embedded = new HateoasResources<GameDetails>
                    {
                        Data = new []
                        {
                            new GameDetails
                            {
                                Title = "test-title",
                                Platforms = new SortedSet<Platform>
                                {
                                    new Platform()
                                },
                                Publishers = new SortedSet<Publisher>
                                {
                                    new Publisher()
                                },
                                Genres = new SortedSet<Genre>
                                {
                                    new Genre()
                                }
                            }
                        }
                    }
                });
            
            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.AreEqual(1, _gameLibraryListViewModel.Items.Count, "There should be items in the list.");
        }
        
        [Test]
        public void LoadMoreCommand_ThrowsTaskCanceledException_CallsUserDialog()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .Throws(new TaskCanceledException());

            _userDialogs.Setup(mock => mock.Toast(It.IsAny<ToastConfig>()))
                .Verifiable();
            
            // Act
            _gameLibraryListViewModel.LoadMoreCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameDetails>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            _userDialogs.Verify();
        }
        
        [Test]
        public void LoadMoreCommand_ThrowsApiException_CallsUserDialog()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            _userDialogs.Setup(mock => mock.Toast(It.IsAny<ToastConfig>()))
                .Verifiable();
            
            // Act
            _gameLibraryListViewModel.LoadMoreCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameDetails>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            _userDialogs.Verify();
        }

        [Test]
        public void LoadMoreCommand_ThrowsGenericException_CallsUserDialog()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .Throws(new Exception());

            _userDialogs.Setup(mock => mock.Toast(It.IsAny<ToastConfig>()))
                .Verifiable();
            
            // Act
            _gameLibraryListViewModel.LoadMoreCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameDetails>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if a generic exception is thrown.");
            _userDialogs.Verify();
        }
        
        [Test]
        public void LoadMoreCommand_WithEmptyResult_DoesntAddNewItems()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameDetails>());
            
            // Act
            _gameLibraryListViewModel.LoadMoreCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.AreEqual(0, _gameLibraryListViewModel.Items.Count, "New items shouldn't be added if the result for loading more is empty.");
        }
        
        [Test]
        public void LoadMoreCommand_WithResults_ConvertsToItems()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameDetails>
                {
                    Embedded = new HateoasResources<GameDetails>
                    {
                        Data = new []
                        {
                            new GameDetails
                            {
                                Title = "test-title",
                                Platforms = new SortedSet<Platform>
                                {
                                    new Platform()
                                },
                                Publishers = new SortedSet<Publisher>
                                {
                                    new Publisher()
                                },
                                Genres = new SortedSet<Genre>
                                {
                                    new Genre()
                                }
                            }
                        }
                    }
                });
            
            // Act
            _gameLibraryListViewModel.LoadMoreCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(1, _gameLibraryListViewModel.Items.Count, "There should be items in the list.");
        }
        
        [Test]
        public void RequestCommand_WithNoData_DoesntThrowException()
        {
            _navigationService.Setup(mock => mock.NavigateAsync(It.IsAny<string>()))
                .Verifiable();
            
            Assert.DoesNotThrow(() =>
            {
                // Act
                _gameLibraryListViewModel.RequestCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
            
            // Assert
            _navigationService.Verify();
        }
    }
}