using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Model.Games.Request;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Games;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Games
{
    [TestFixture]
    public class GameOptionsViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IRestService> _restService;
        private Mock<IStorageService> _storageService;
        private Mock<IUserDialogs> _userDialogs;
        private TestScheduler _scheduler;

        private GameOptionsViewModel _gameOptionsViewModel;
        
        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _storageService = new Mock<IStorageService>();
            _userDialogs = new Mock<IUserDialogs>();
            _scheduler = new TestScheduler();
            
            _gameOptionsViewModel = new GameOptionsViewModel(_scheduler, _navigationService.Object, _restService.Object, _storageService.Object, _userDialogs.Object);
        }

        [Test]
        public void OnNavigatedFrom_WithDefaultData_AddsRequiredParameters()
        {
            // Arrange
            var parameters = new NavigationParameters();
            
            // Act
            _gameOptionsViewModel.OnNavigatedFrom(parameters);
            
            // Assert
            Assert.IsTrue(parameters.ContainsKey("game-url"), "Parameters doesn't contain Game URL.");
            Assert.IsTrue(parameters.ContainsKey("in-library"), "Parameters doesn't contain in library.");
            Assert.IsTrue(parameters.ContainsKey("status"), "Parameters doesn't contain status.");
            Assert.IsTrue(parameters.ContainsKey("rating"), "Parameters doesn't contain rating.");
            Assert.IsTrue(parameters.ContainsKey("selected-platforms"), "Parameters doesn't contain selected platforms.");
        }
        
        [Test]
        public void LoadGameDetailsCommand_ThrowsTaskCanceledException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameOptionsViewModel.GameUrl = new Uri("https://traklibrary.com");
            
            _restService
                .Setup(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()))
                .Throws(new TaskCanceledException());

            // Act
            _gameOptionsViewModel.LoadGameDetailsCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameOptionsViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
        }
        
        [Test]
        public void LoadGameDetailsCommand_ThrowsApiException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameOptionsViewModel.GameUrl = new Uri("https://traklibrary.com");
            
            _restService
                .Setup(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _gameOptionsViewModel.LoadGameDetailsCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameOptionsViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
        }
        
        [Test]
        public void LoadGameDetailsCommand_ThrowsGenericException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameOptionsViewModel.GameUrl = new Uri("https://traklibrary.com");
            
            _restService
                .Setup(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _gameOptionsViewModel.LoadGameDetailsCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameOptionsViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
        }
        
        [Test]
        public void LoadGameDetailsCommand_WithGameNotInLibrary_LoadsDataAndDoesntMarkAsInLibrary()
        {
            // Arrange
            _gameOptionsViewModel.GameUrl = new Uri("https://traklibrary.com");
            
            var gameDetails = new GameDetails
            {
                Id = 5L,
                Title = "test-title",
                Platforms = new List<Platform>
                {
                    new Platform
                    {
                        Id = 1L,
                        Name = "name-1"
                    },
                    new Platform
                    {
                        Id = 2L,
                        Name = "name-2"
                    }
                },
                Links = new Dictionary<string, HateoasLink>
                {
                    {
                        "entries", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com/image")
                        }
                    }
                }
            };

            _restService
                .Setup(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()))
                .ReturnsAsync(gameDetails);

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _restService.Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>());
                
            // Act
            _gameOptionsViewModel.LoadGameDetailsCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(gameDetails.Title, _gameOptionsViewModel.GameTitle, "The titles should match.");
            Assert.AreEqual(2, _gameOptionsViewModel.Platforms.Count(), "There should be two platforms stored within the view model.");
            Assert.IsFalse(_gameOptionsViewModel.InLibrary, "The game shouldn't be flagged as in the users library.");
        }
        
        [Test]
        public void LoadGameDetailsCommand_WithGameInLibrary_LoadsDataAndMarksAsInLibrary()
        {
            // Arrange
            _gameOptionsViewModel.GameUrl = new Uri("https://traklibrary.com");
            
            var gameDetails = new GameDetails
            {
                Id = 5L,
                Title = "test-title",
                Platforms = new List<Platform>
                {
                    new Platform
                    {
                        Id = 1L,
                        Name = "name-1"
                    },
                    new Platform
                    {
                        Id = 2L,
                        Name = "name-2"
                    }
                },
                Links = new Dictionary<string, HateoasLink>
                {
                    {
                        "entries", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com/image")
                        }
                    }
                }
            };

            _restService
                .Setup(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()))
                .ReturnsAsync(gameDetails);

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _restService.Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>
                {
                    Embedded = new HateoasResources<GameUserEntry>
                    {
                        Data = new List<GameUserEntry>
                        {
                            new GameUserEntry
                            {
                                Status = GameUserEntryStatus.Backlog,
                                Rating = 4,
                                GameUserEntryPlatforms = new List<GameUserEntryPlatform>
                                {
                                    new GameUserEntryPlatform
                                    {
                                        PlatformId = 1L
                                    }
                                },
                                GameUserEntryDownloadableContents = new List<GameUserEntryDownloadableContent>
                                {
                                    new GameUserEntryDownloadableContent
                                    {
                                        DownloadableContentId = 2L
                                    }
                                }
                            }
                        }
                    }
                });
                
            // Act
            _gameOptionsViewModel.LoadGameDetailsCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(gameDetails.Title, _gameOptionsViewModel.GameTitle, "The titles should match.");
            Assert.AreEqual(2, _gameOptionsViewModel.Platforms.Count(), "There should be two platforms stored within the view model.");
            Assert.AreEqual(4, _gameOptionsViewModel.Rating, "The rating should match the game user entry.");
            Assert.AreEqual(GameUserEntryStatus.Backlog, _gameOptionsViewModel.Status, "The status should match the game user entry.");
            Assert.IsTrue(_gameOptionsViewModel.InLibrary, "The game should be flagged as in the users library.");
        }
        
        [Test]
        public void OnRatingTappedCommand_WithRating_UpdatesRatingValue()
        {
            // Arrange
            short rating = 4;
            
            // Act
            _gameOptionsViewModel.OnRatingTappedCommand.Execute(rating.ToString()).Subscribe();
            
            // Assert
            Assert.AreEqual(rating, _gameOptionsViewModel.Rating, "The rating was not updated.");
        }

        [Test]
        public void OnRatingRemovedCommand_WithNoData_SetsRatingToZero()
        {
            // Act
            _gameOptionsViewModel.OnRatingRemovedCommand.Execute(5).Subscribe();
            
            // Assert
            Assert.Zero(_gameOptionsViewModel.Rating);
        }
        
        [Test]
        public void OnStatusTappedCommand_WithStatus_UpdatesStatusValue()
        {
            // Arrange
            GameUserEntryStatus status = GameUserEntryStatus.Completed;
            
            // Act
            _gameOptionsViewModel.OnStatusTappedCommand.Execute(status).Subscribe();
            
            // Assert
            Assert.AreEqual(status, _gameOptionsViewModel.Status, "The status was not updated.");
        }
        
        [Test]
        public void AddGameCommand_WithNoSelectedPlatforms_PopulatesErrorMessageAndDoesntCallApi()
        {
            // Arrange
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>();
            
            // Act
            _gameOptionsViewModel.AddGameCommand.Execute().Subscribe();
            
            // Assert
            Assert.AreEqual(Messages.GameOptionsPageNoSelectedPlatforms, _gameOptionsViewModel.ErrorMessage, "The incorrect error message has been logged.");
            _restService.Verify(mock =>
                    mock.PostAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                        It.IsAny<GameUserEntryRequest>()), Times.Never);
        }
        
        [Test]
        public void AddGameCommand_WithNoSelectedStatus_PopulatesErrorMessageAndDoesntCallApi()
        {
            // Arrange
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>
            {
                new ItemEntryViewModel
                {
                    IsSelected = true
                }
            };

            _gameOptionsViewModel.Status = GameUserEntryStatus.None;
            
            // Act
            _gameOptionsViewModel.AddGameCommand.Execute().Subscribe();
            
            // Assert
            Assert.AreEqual(Messages.GameOptionsPageNoSelectedStatus, _gameOptionsViewModel.ErrorMessage, "The incorrect error message has been logged.");
            _restService.Verify(mock =>
                mock.PostAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                    It.IsAny<GameUserEntryRequest>()), Times.Never);
        }
        
        [Test]
        public void AddGameCommand_ThrowsTaskCanceledException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>
            {
                new ItemEntryViewModel
                {
                    IsSelected = true
                }
            };

            _gameOptionsViewModel.Status = GameUserEntryStatus.InProgress;
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _restService
                .Setup(mock => mock.PostAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                    It.IsAny<GameUserEntryRequest>()))
                .Throws(new TaskCanceledException());

            // Act
            _gameOptionsViewModel.AddGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual( Messages.ErrorMessageNoInternet, _gameOptionsViewModel.ErrorMessage, "The error message is incorrect.");
        }
        
        [Test]
        public void AddGameCommand_ThrowsApiException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>
            {
                new ItemEntryViewModel
                {
                    IsSelected = true
                }
            };

            _gameOptionsViewModel.Status = GameUserEntryStatus.InProgress;
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _restService
                .Setup(mock => mock.PostAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                    It.IsAny<GameUserEntryRequest>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _gameOptionsViewModel.AddGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual( Messages.ErrorMessageApiError, _gameOptionsViewModel.ErrorMessage, "The error message is incorrect.");
        }
        
        [Test]
        public void AddGameCommand_ThrowsGenericException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>
            {
                new ItemEntryViewModel
                {
                    IsSelected = true
                }
            };

            _gameOptionsViewModel.Status = GameUserEntryStatus.InProgress;
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _restService
                .Setup(mock => mock.PostAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                    It.IsAny<GameUserEntryRequest>()))
                .Throws(new Exception());

            // Act
            _gameOptionsViewModel.AddGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual( Messages.ErrorMessageGeneric, _gameOptionsViewModel.ErrorMessage, "The error message is incorrect.");
        }

        [Test]
        public void AddGameCommand_WithValidData_SavesGame()
        {
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>
            {
                new ItemEntryViewModel
                {
                    IsSelected = true
                }
            };

            _gameOptionsViewModel.Status = GameUserEntryStatus.InProgress;
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _restService
                .Setup(mock => mock.PostAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                    It.IsAny<GameUserEntryRequest>()))
                .ReturnsAsync(new GameUserEntry
                {
                    Rating = 3,
                    Status = GameUserEntryStatus.Completed
                });

            _navigationService.Setup(mock => mock.GoBackAsync())
                .Verifiable();

            _userDialogs.Setup(mock => mock.AlertAsync(It.IsAny<AlertConfig>(), null))
                .Verifiable();
            
            // Act
            _gameOptionsViewModel.AddGameCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            _navigationService.Verify();
            _userDialogs.Verify();
            
            Assert.AreEqual(3, _gameOptionsViewModel.Rating, "The rating should match the saved game user entry.");
            Assert.AreEqual(GameUserEntryStatus.Completed, _gameOptionsViewModel.Status, "The status should match the saved game user entry.");
            Assert.IsTrue(_gameOptionsViewModel.InLibrary, "The game should be flagged as in the users library.");
        }
        
        [Test]
        public void UpdateGameCommand_WithNoSelectedPlatforms_PopulatesErrorMessageAndDoesntCallApi()
        {
            // Arrange
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>();

            // Act
            _gameOptionsViewModel.UpdateGameCommand.Execute().Subscribe();
            
            // Assert
            Assert.AreEqual(Messages.GameOptionsPageNoSelectedPlatforms, _gameOptionsViewModel.ErrorMessage, "The incorrect error message has been logged.");
            _restService.Verify(mock =>
                    mock.PostAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                        It.IsAny<GameUserEntryRequest>()), Times.Never);
        }
        
        [Test]
        public void UpdateGameCommand_ThrowsTaskCanceledException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>
            {
                new ItemEntryViewModel
                {
                    IsSelected = true
                }
            };
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _restService
                .Setup(mock => mock.PutAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                    It.IsAny<GameUserEntryRequest>()))
                .Throws(new TaskCanceledException());

            // Act
            _gameOptionsViewModel.UpdateGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(Messages.ErrorMessageNoInternet, _gameOptionsViewModel.ErrorMessage, "The error message is incorrect.");
        }
        
        [Test]
        public void UpdateGameCommand_ThrowsApiException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>
            {
                new ItemEntryViewModel
                {
                    IsSelected = true
                }
            };
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _restService
                .Setup(mock => mock.PutAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                    It.IsAny<GameUserEntryRequest>()))
                .Throws(new ApiException());

            // Act
            _gameOptionsViewModel.UpdateGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(Messages.ErrorMessageApiError, _gameOptionsViewModel.ErrorMessage, "The error message is incorrect.");
        }
        
        [Test]
        public void UpdateGameCommand_ThrowsGenericException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>
            {
                new ItemEntryViewModel
                {
                    IsSelected = true
                }
            };
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);
            
            _restService
                .Setup(mock => mock.PutAsync(It.IsAny<string>(),
                    It.IsAny<GameUserEntryRequest>()))
                .Throws(new Exception());

            // Act
            _gameOptionsViewModel.UpdateGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual( Messages.ErrorMessageGeneric, _gameOptionsViewModel.ErrorMessage, "The error message is incorrect.");
        }
        
        [Test]
        public void UpdateGameCommand_WithValidData_UpdatesGame()
        {
            _gameOptionsViewModel.Platforms = new ObservableCollection<ItemEntryViewModel>
            {
                new ItemEntryViewModel
                {
                    IsSelected = true
                }
            };
            
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _restService
                .Setup(mock => mock.PutAsync<GameUserEntry, GameUserEntryRequest>(It.IsAny<string>(),
                    It.IsAny<GameUserEntryRequest>()))
                .ReturnsAsync(new GameUserEntry
                {
                    Rating = 3,
                    Status = GameUserEntryStatus.Completed
                });

            _navigationService.Setup(mock => mock.GoBackAsync())
                .Verifiable();
            
            // Act
            _gameOptionsViewModel.UpdateGameCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            _navigationService.Verify();
            
            Assert.AreEqual(3, _gameOptionsViewModel.Rating, "The rating should match the updated game user entry.");
            Assert.AreEqual(GameUserEntryStatus.Completed, _gameOptionsViewModel.Status, "The status should match the updated game user entry.");
            Assert.IsTrue(_gameOptionsViewModel.InLibrary, "The game should still be flagged as in the users library.");
        }
        
        [Test]
        public void DeleteGameCommand_ThrowsTaskCanceledException_SetsIsErrorToTrue()
        {
            // Arrange
            _restService
                .Setup(mock => mock.DeleteAsync(It.IsAny<string>()))
                .Throws(new TaskCanceledException());

            // Act
            _gameOptionsViewModel.DeleteGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(Messages.ErrorMessageNoInternet, _gameOptionsViewModel.ErrorMessage, "The error message is incorrect.");
        }
        
        [Test]
        public void DeleteGameCommand_ThrowsApiException_SetsIsErrorToTrue()
        {
            // Arrange
            _restService
                .Setup(mock => mock.DeleteAsync(It.IsAny<string>()))
                .Throws(new ApiException());

            // Act
            _gameOptionsViewModel.DeleteGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(Messages.ErrorMessageApiError, _gameOptionsViewModel.ErrorMessage, "The error message is incorrect.");
        }
        
        [Test]
        public void DeleteGameCommand_ThrowsGenericException_SetsIsErrorToTrue()
        {
            // Arrange
            _restService
                .Setup(mock => mock.DeleteAsync(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _gameOptionsViewModel.DeleteGameCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(Messages.ErrorMessageGeneric, _gameOptionsViewModel.ErrorMessage, "The error message is incorrect.");
        }
        
        [Test]
        public void DeleteGameCommand_WithValidData_DeletesGame()
        {
            _restService
                .Setup(mock => mock.DeleteAsync(It.IsAny<string>()))
                .Verifiable();
            
            _navigationService.Setup(mock => mock.GoBackAsync())
                .Verifiable();

            _userDialogs.Setup(mock => mock.AlertAsync(It.IsAny<AlertConfig>(), null))
                .Verifiable();
            
            // Act
            _gameOptionsViewModel.DeleteGameCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            _restService.Verify();
            _navigationService.Verify();
            _userDialogs.Verify();
            
            Assert.AreEqual(0, _gameOptionsViewModel.Rating, "The rating should be reset to 0.");
            Assert.AreEqual(GameUserEntryStatus.None, _gameOptionsViewModel.Status, "The status should be reset to none.");
            Assert.IsFalse(_gameOptionsViewModel.InLibrary, "The game should no longer be in their library.");
        }
    }
}