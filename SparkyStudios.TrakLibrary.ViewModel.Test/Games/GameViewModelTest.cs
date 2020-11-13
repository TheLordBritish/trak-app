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
        private Mock<IRestService> _restService;
        private Mock<IStorageService> _storageService;
        private TestScheduler _scheduler;

        private GameViewModel _gameViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _storageService = new Mock<IStorageService>();
            _scheduler = new TestScheduler();

            _gameViewModel = new GameViewModel(_scheduler, _navigationService.Object, _restService.Object, _storageService.Object);
        }

        [Test]
        public void OptionsCommand_WithNoData_NavigatesToGameOptionsPage()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("GameOptionsPage", It.IsAny<INavigationParameters>()))
                .Verifiable();

            // Act
            _gameViewModel.OptionsCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify();
        }

        [Test]
        public void LoadGameDetailsCommand_WithShouldReloadFalse_DoesntRetrieveData()
        {
            // Arrange
            _gameViewModel.ShouldReload = false;
            
            // Act
            _gameViewModel.LoadGameDetailsCommand.Execute().Subscribe();
            
            // Assert
            _restService
                .Verify(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()), Times.Never);
        }
        
        [Test]
        public void LoadGameDetailsCommand_ThrowsApiException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameViewModel.ShouldReload = true;
            
            _restService
                .Setup(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _gameViewModel.LoadGameDetailsCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
        }

        [Test]
        public void LoadGameDetailsCommand_ThrowsGenericException_SetsIsErrorToTrue()
        {
            // Arrange
            _gameViewModel.ShouldReload = true;
            
            _restService
                .Setup(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _gameViewModel.LoadGameDetailsCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameViewModel.IsError, "vm.IsError should be true if an exception is thrown.");
        }

        [Test]
        public void LoadGameDetailsCommand_WithGameNotInLibrary_LoadsData()
        {
            // Arrange
            var northAmericaDate = DateTime.Now;
            var europeDate = DateTime.Now;
            var japanDate = DateTime.Now;
            
            _gameViewModel.ShouldReload = true;
            _gameViewModel.GameUrl = new Uri("https://traklibrary.com");

            var gameDetails = new GameDetails
            {
                Id = 5L,
                Title = "test-title",
                Description = "test-description",
                GameModes = new List<GameMode>(),
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
                ReleaseDates = new List<GameReleaseDate>
                {
                    new GameReleaseDate
                    {
                        Region = GameRegion.NorthAmerica,
                        ReleaseDate = northAmericaDate
                    },
                    new GameReleaseDate
                    {
                        Region = GameRegion.Pal,
                        ReleaseDate = europeDate
                    },
                    new GameReleaseDate
                    {
                        Region = GameRegion.Japan,
                        ReleaseDate = japanDate
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
                                "gameDetails", new HateoasLink
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
                        "genres", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com")
                        }
                    }
                }
            };

            _restService
                .Setup(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()))
                .ReturnsAsync(gameDetails);
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameDetails>
                {
                    Embedded = new HateoasResources<GameDetails>
                    {
                        Data = new[]
                        {
                            new GameDetails
                            {
                                Id = gameDetails.Id
                            }
                        }
                    }
                });

            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L);

            _restService.Setup(mock => mock.GetAsync<HateoasPage<GameUserEntry>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameUserEntry>());
                
            // Act
            _gameViewModel.LoadGameDetailsCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(gameDetails.GetLink("image").OriginalString, _gameViewModel.ImageUrl.OriginalString,
                "The image url should match.");
            Assert.AreEqual(gameDetails.Title, _gameViewModel.GameTitle, "The titles should match.");
            Assert.AreEqual(gameDetails.Description, _gameViewModel.Description, "The titles should match.");
            Assert.AreEqual(gameDetails.Publishers, _gameViewModel.Publishers, "The publishers should match.");
            Assert.AreEqual(northAmericaDate.ToString("dd MMMM yyyy"), _gameViewModel.NorthAmericaReleaseDate, "The North American dates should match.");
            Assert.AreEqual(europeDate.ToString("dd MMMM yyyy"), _gameViewModel.EuropeReleaseDate, "The North American dates should match.");
            Assert.AreEqual(japanDate.ToString("dd MMMM yyyy"), _gameViewModel.JapanReleaseDate, "The North American dates should match.");
            Assert.AreEqual(gameDetails.Genres, _gameViewModel.Genres, "The genres should match.");
        }

        [Test]
        public void LoadGameInfoCommand_WithGameInLibrary_LoadsDataAndGameUserEntryData()
        {
            // Arrange
            var northAmericaDate = DateTime.Now;
            var europeDate = DateTime.Now;
            var japanDate = DateTime.Now;
            
            _gameViewModel.ShouldReload = true;
            _gameViewModel.GameUrl = new Uri("https://traklibrary.com");
            
            var gameDetails = new GameDetails
            {
                Id = 5L,
                Title = "test-title",
                Description = "test-description",
                GameModes = new List<GameMode>(),
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
                ReleaseDates = new List<GameReleaseDate>
                {
                    new GameReleaseDate
                    {
                        Region = GameRegion.NorthAmerica,
                        ReleaseDate = northAmericaDate
                    },
                    new GameReleaseDate
                    {
                        Region = GameRegion.Pal,
                        ReleaseDate = europeDate
                    },
                    new GameReleaseDate
                    {
                        Region = GameRegion.Japan,
                        ReleaseDate = japanDate
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
                                "gameDetails", new HateoasLink
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
                        "genres", new HateoasLink
                        {
                            Href = new Uri("https://traklibrary.com")
                        }
                    }
                }
            };

            _restService
                .Setup(mock => mock.GetAsync<GameDetails>(It.IsAny<string>()))
                .ReturnsAsync(gameDetails);

            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameDetails>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameDetails>
                {
                    Embedded = new HateoasResources<GameDetails>
                    {
                        Data = new[]
                        {
                            new GameDetails
                            {
                                Id = gameDetails.Id
                            }
                        }
                    }
                });
            
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
                                GameUserEntryPlatforms = new List<GameUserEntryPlatform>()
                            }
                        }
                    }
                });

            // Act
            _gameViewModel.LoadGameDetailsCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(gameDetails.GetLink("image").OriginalString, _gameViewModel.ImageUrl.OriginalString,
                "The image url should match.");
            Assert.AreEqual(gameDetails.Title, _gameViewModel.GameTitle, "The titles should match.");
            Assert.AreEqual(gameDetails.Description, _gameViewModel.Description, "The titles should match.");
            Assert.AreEqual(gameDetails.Publishers, _gameViewModel.Publishers, "The publishers should match.");
            Assert.AreEqual(2, _gameViewModel.Platforms.Count(),
                "There should be two elements in the platforms list.");
            Assert.AreEqual(northAmericaDate.ToString("dd MMMM yyyy"), _gameViewModel.NorthAmericaReleaseDate, "The North American dates should match.");
            Assert.AreEqual(europeDate.ToString("dd MMMM yyyy"), _gameViewModel.EuropeReleaseDate, "The North American dates should match.");
            Assert.AreEqual(japanDate.ToString("dd MMMM yyyy"), _gameViewModel.JapanReleaseDate, "The North American dates should match.");
            Assert.AreEqual(4, _gameViewModel.Rating, "The rating should match the game user entry.");
            Assert.AreEqual(GameUserEntryStatus.Backlog, _gameViewModel.Status, "The status should match the game user entry.");
            Assert.AreEqual(gameDetails.Genres, _gameViewModel.Genres, "The genres should match.");
        }
    }
}