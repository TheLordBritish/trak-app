﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Service;

namespace Sparky.TrakApp.ViewModel.Test
{
    public class BaseMasterDetailViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IRestService> _restService;
        private TestScheduler _scheduler;

        private BaseMasterDetailViewModel _baseMasterDetailViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _restService = new Mock<IRestService>();
            _scheduler = new TestScheduler();

            _baseMasterDetailViewModel = new BaseMasterDetailViewModel(_scheduler, _navigationService.Object,
                _storageService.Object, _restService.Object);
        }

        [Test]
        public void LoadHomeCommand_WithNoData_DoesntThrowException()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("BaseNavigationPage/HomePage"))
                .Verifiable();

            // Act
            _baseMasterDetailViewModel.LoadHomeCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify();
        }

        [Test]
        public void LoadGamesCommand_WithNoData_DoesntThrowException()
        {
            // Arrange
            _navigationService.Setup(mock => mock.NavigateAsync("BaseNavigationPage/GameUserEntriesTabbedPage"))
                .Verifiable();

            // Act
            _baseMasterDetailViewModel.LoadGamesCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify();
        }

        [Test]
        public void LogoutCommand_ThrowsException_NavigatesToLoginPage()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ThrowsAsync(new Exception());

            _navigationService.Setup(mock => mock.NavigateAsync("/LoginPage"))
                .Verifiable();

            // Act
            _baseMasterDetailViewModel.LogoutCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            _navigationService.Verify();
        }

        [Test]
        public void LogoutCommand_WithNoissues_NavigatesToLoginPage()
        {
            // Arrange
            _storageService.Setup(mock => mock.GetUserIdAsync())
                .ReturnsAsync(0L)
                .Verifiable();

            _storageService.Setup(mock => mock.GetDeviceIdAsync())
                .ReturnsAsync(Guid.Empty)
                .Verifiable();

            _storageService.Setup(mock => mock.GetAuthTokenAsync())
                .ReturnsAsync("token")
                .Verifiable();

            _storageService.Setup(mock => mock.SetUsernameAsync(It.IsAny<string>()))
                .Verifiable();

            _storageService.Setup(mock => mock.SetPasswordAsync(It.IsAny<string>()))
                .Verifiable();

            _storageService.Setup(mock => mock.SetAuthTokenAsync(It.IsAny<string>()))
                .Verifiable();

            _storageService.Setup(mock => mock.SetUserIdAsync(It.IsAny<long>()))
                .Verifiable();

            _restService.Setup(mock => mock.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();

            _navigationService.Setup(mock => mock.NavigateAsync("/LoginPage"))
                .Verifiable();

            // Act
            _baseMasterDetailViewModel.LogoutCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();

            // Assert
            _storageService.Verify();
            _restService.Verify();
            _navigationService.Verify();
        }
    }
}