using System.Linq;
using NUnit.Framework;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Games.Validation;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Test.Validation
{
    public class AddGameDetailsValidatorTest
    {
        [Test]
        public void Validate_WithNullPlatform_ValidationFails()
        {
            // Arrange
            var validator = new AddGameDetailsValidator();

            // Act
            var result = validator.Validate(new AddGameDetails());

            // Assert
            Assert.AreEqual(Messages.AddGameErrorMessagePlatformNull,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null platform.");
        }

        [Test]
        public void Validate_WithStatusSetToNone_ValidationFails()
        {
            // Arrange
            var validator = new AddGameDetailsValidator();

            // Act
            var result = validator.Validate(new AddGameDetails
            {
                Platform = new Platform(),
                Status = GameUserEntryStatus.None
            });

            // Assert
            Assert.AreEqual(Messages.AddGameErrorMessageStatusNone,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null platform.");
        }

        [Test]
        public void Validate_WithValidAddGameDetails_ValidationPasses()
        {
            // Arrange
            var validator = new AddGameDetailsValidator();

            // Act
            var result = validator.Validate(new AddGameDetails
            {
                Platform = new Platform(),
                Status = GameUserEntryStatus.Backlog
            });

            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true if validation was successful.");
        }
    }
}