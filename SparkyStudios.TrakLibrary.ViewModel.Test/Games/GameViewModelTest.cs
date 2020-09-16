using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Games;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Games
{
    public class GameViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IRestService> _restService;
        private TestScheduler _scheduler;

        private GameViewModel _gameViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _storageService = new Mock<IStorageService>();
            _scheduler = new TestScheduler();

            _gameViewModel = new GameViewModel(_scheduler, _navigationService.Object, _storageService.Object,
                _restService.Object);
        }

        [Test]
        public void OnNavigatedTo_WithValidNavigationParameters_PopulatesCorrectValues()
        {
            // Arrange
            var gameUrl = new Uri("https://games.url");
            var platformId = 1L;
            short rating = 4;
            var status = GameUserEntryStatus.Completed;

            var parameters = new NavigationParameters
            {
                {"game-url", gameUrl},
                {"platform-id", platformId},
                {"in-library", true},
                {"rating", rating},
                {"status", status}
            };

            // Act
            _gameViewModel.OnNavigatedTo(parameters);

            // Assert
            Assert.AreEqual(gameUrl, _gameViewModel.GameUrl, "The game URL passed in during navigation should match.");
            Assert.AreEqual(platformId, _gameViewModel.PlatformId, "The platform ID should match.");
            Assert.AreEqual(true, _gameViewModel.InLibrary, "The in library should match.");
            Assert.AreEqual(rating, _gameViewModel.Rating, "The rating should match.");
            Assert.AreEqual(status, _gameViewModel.Status, "The status should match.");
        }

        [Test]
        public void OptionsCommand_WithGameInLibrary_NavigatesToGameOptionsPage()
        {
            // Arrange
            _gameViewModel.InLibrary = true;

            _navigationService.Setup(mock => mock.NavigateAsync("GameOptionsPage", It.IsAny<INavigationParameters>()))
                .Verifiable();

            // Act
            _gameViewModel.OptionsCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify();
        }

        [Test]
        public void OptionsCommand_WithGameNotInLibrary_NavigatesToAddGamePage()
        {
            // Arrange
            _gameViewModel.InLibrary = false;

            _navigationService.Setup(mock => mock.NavigateAsync("AddGamePage", It.IsAny<INavigationParameters>()))
                .Verifiable();

            // Act
            _gameViewModel.OptionsCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify();
        }

        [Test]
        public void LoadGameInfoCommand_ThrowsApiException_SetsIsErrorToTrue()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<GameInfo>(It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _gameViewModel.LoadGameInfoCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
        }

        [Test]
        public void LoadGameInfoCommand_ThrowsGenericException_SetsIsErrorToTrue()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<GameInfo>(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _gameViewModel.LoadGameInfoCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
        }

        [Test]
        public void LoadGameInfoCommand_WithGameNotInLibrary_LoadsDataAndAllPlatforms()
        {
            // Arrange
            _gameViewModel.GameUrl = new Uri("https://traklibrary.com");
            _gameViewModel.InLibrary = false;
            
            var gameInfo = new GameInfo
            {
                Id = 5L,
                Title = "test-title",
                ReleaseDate = DateTime.Now,
                Description = "test-description",
                Platforms = new List<Platform>
                {
                    new Platform
                    {
                        Id = 1L
                    },
                    new Platform
                    {
                        Id = 2L
                    }
                },
                Publishers = new List<Publisher>
                {
                    new Publisher()
                },
                Genres = new List<Genre>
                {
                    new Genre
                    {
                        Links = new Dictionary<string, HateoasLink>
                        {
                            {
                                "gameInfos", new HateoasLink
                                {
                                    Href = new Uri("https://traklibrary.com")
                                }
                            }
                        }
                    }
                },
                Links = new Dictionary<string, HateoasLink>
                {
                    {
                        "image", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com/image")
                        }
                    },
                    {
                        "publishers", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com")
                        }
                    },
                    {
                        "platforms", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com")
                        }
                    },
                    {
                        "genres", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com")
                        }
                    }
                }
            };

            _restService
                .Setup(mock => mock.GetAsync<GameInfo>(It.IsAny<string>()))
                .ReturnsAsync(gameInfo);
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameInfo>
                {
                    Embedded = new HateoasResources<GameInfo>
                    {
                        Data = new[]
                        {
                            new GameInfo
                            {
                                Id = gameInfo.Id
                            }
                        }
                    }
                });

            // Act
            _gameViewModel.LoadGameInfoCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(gameInfo.GetLink("image").OriginalString, _gameViewModel.ImageUrl.OriginalString,
                "The image url should match.");
            Assert.AreEqual(gameInfo.Title, _gameViewModel.GameTitle, "The titles should match.");
            Assert.AreEqual(gameInfo.ReleaseDate, _gameViewModel.ReleaseDate, "The release dates should match.");
            Assert.AreEqual(gameInfo.Description, _gameViewModel.Description, "The titles should match.");
            Assert.AreEqual(gameInfo.Publishers, _gameViewModel.Publishers, "The publishers should match.");
            Assert.AreEqual(gameInfo.Platforms, _gameViewModel.Platforms, "The platforms should match.");
            Assert.AreEqual(gameInfo.Genres, _gameViewModel.Genres, "The genres should match.");
        }

        [Test]
        public void LoadGameInfoCommand_WithGameInLibrary_LoadsDataAndSpecifiedPlatform()
        {
            // Arrange
            _gameViewModel.GameUrl = new Uri("https://traklibrary.com");
            _gameViewModel.InLibrary = true;
            _gameViewModel.PlatformId = 2L;
            
            var gameInfo = new GameInfo
            {
                Id = 5L,
                Title = "test-title",
                ReleaseDate = DateTime.Now,
                Description = "test-description",
                Platforms = new List<Platform>
                {
                    new Platform
                    {
                        Id = 1L
                    },
                    new Platform
                    {
                        Id = 2L
                    }
                },
                Publishers = new List<Publisher>
                {
                    new Publisher()
                },
                Genres = new List<Genre>
                {
                    new Genre
                    {
                        Links = new Dictionary<string, HateoasLink>
                        {
                            {
                                "gameInfos", new HateoasLink
                                {
                                    Href = new Uri("https://traklibrary.com")
                                }
                            }
                        }
                    }
                },
                Links = new Dictionary<string, HateoasLink>
                {
                    {
                        "image", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com/image")
                        }
                    },
                    {
                        "publishers", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com")
                        }
                    },
                    {
                        "platforms", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com")
                        }
                    },
                    {
                        "genres", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com")
                        }
                    }
                }
            };

            _restService
                .Setup(mock => mock.GetAsync<GameInfo>(It.IsAny<string>()))
                .ReturnsAsync(gameInfo);

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameInfo>
                {
                    Embedded = new HateoasResources<GameInfo>
                    {
                        Data = new[]
                        {
                            new GameInfo
                            {
                                Id = gameInfo.Id
                            }
                        }
                    }
                });

            // Act
            _gameViewModel.LoadGameInfoCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(gameInfo.GetLink("image").OriginalString, _gameViewModel.ImageUrl.OriginalString,
                "The image url should match.");
            Assert.AreEqual(gameInfo.Title, _gameViewModel.GameTitle, "The titles should match.");
            Assert.AreEqual(gameInfo.ReleaseDate, _gameViewModel.ReleaseDate, "The release dates should match.");
            Assert.AreEqual(gameInfo.Description, _gameViewModel.Description, "The titles should match.");
            Assert.AreEqual(gameInfo.Publishers, _gameViewModel.Publishers, "The publishers should match.");
            Assert.AreEqual(1, _gameViewModel.Platforms.Count(),
                "There should be only one element in the platforms list.");
            Assert.AreEqual(gameInfo.Platforms.First(p => p.Id == _gameViewModel.PlatformId), _gameViewModel.Platforms.FirstOrDefault());
            Assert.AreEqual(gameInfo.Genres, _gameViewModel.Genres, "The genres should match.");
        }

        [Test]
        public void OnRatingTappedCommand_ThrowsException_SetIsErrorToTrue()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(5L);
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()))
                .Throws(new ApiException());

            // Act
            _gameViewModel.OnRatingTappedCommand.Execute("5").Catch(Observable.Return(true)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
        }

        [Test]
        public void OnRatingTappedCommand_WithNoMatchingEntryToUpdate_DoesntPatchGameUserEntry()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(5L);
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>());

            // Act
            _gameViewModel.OnRatingTappedCommand.Execute("5").Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify(
                mock => mock.PatchAsync<GameUserEntry>(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()), Times.Never);
        }

        [Test]
        public void OnRatingTappedCommand_WithMatchingEntryToUpdate_PatchesGameUserEntry()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(5L);
            
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

            _restService.Setup(mock => mock.PatchAsync<GameUserEntry>(It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>()))
                .ReturnsAsync(new GameUserEntry());

            // Act
            _gameViewModel.OnRatingTappedCommand.Execute("5").Subscribe();
            _scheduler.Start();

            // Assert
            _restService.Verify(
                mock => mock.PatchAsync<GameUserEntry>(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
        }
    }
}