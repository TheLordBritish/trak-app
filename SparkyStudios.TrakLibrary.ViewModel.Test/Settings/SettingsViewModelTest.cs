using System;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.ViewModel.Settings;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Settings
{
    [TestFixture]
    public class SettingsViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private TestScheduler _scheduler;

        private SettingsViewModel _settingsViewModel;
        
        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _scheduler = new TestScheduler();

            _settingsViewModel = new SettingsViewModel(_scheduler, _navigationService.Object, _storageService.Object);
        }
        
        [Test]
        public void ChangePasswordCommand_WithNoData_DoesntThrowException()
        {
            _navigationService.Setup(mock => mock.NavigateAsync("ChangePasswordPage"))
                .Verifiable();
            
            Assert.DoesNotThrow(() =>
            {
                // Act
                _settingsViewModel.ChangePasswordCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
            
            // Assert
            _navigationService.Verify();
        }
           
        [Test]
        public void ChangeEmailAddressCommand_WithNoData_DoesntThrowException()
        {
            _navigationService.Setup(mock => mock.NavigateAsync("ChangeEmailAddressPage"))
                .Verifiable();
            
            Assert.DoesNotThrow(() =>
            {
                // Act
                _settingsViewModel.ChangeEmailAddressCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
            
            // Assert
            _navigationService.Verify();
        }
        
        [Test]
        public void DeleteAccountCommand_WithNoData_DoesntThrowException()
        {
            _navigationService.Setup(mock => mock.NavigateAsync("DeleteAccountPage"))
                .Verifiable();
            
            Assert.DoesNotThrow(() =>
            {
                // Act
                _settingsViewModel.DeleteAccountCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
            
            // Assert
            _navigationService.Verify();
        }
    }
}