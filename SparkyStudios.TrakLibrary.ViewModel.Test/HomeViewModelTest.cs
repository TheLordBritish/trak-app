using System;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;

namespace SparkyStudios.TrakLibrary.ViewModel.Test
{
    public class HomeViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private TestScheduler _scheduler;

        private HomeViewModel _homeViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _scheduler = new TestScheduler();

            _homeViewModel = new HomeViewModel(_scheduler, _navigationService.Object);
        }

        [Test]
        public void LoadGamesCommand_WithNoData_NavigatesToGameUserEntriesTabbedPage()
        {
            // Arrange
            _navigationService.Setup(mock =>
                    mock.NavigateAsync("GameUserEntriesTabbedPage", It.IsAny<NavigationParameters>()))
                .ReturnsAsync(new Mock<INavigationResult>().Object);

            // Act
            _homeViewModel.LoadGamesCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify(
                n => n.NavigateAsync("GameUserEntriesTabbedPage", It.IsAny<NavigationParameters>()), Times.Once);
        }
    }
}