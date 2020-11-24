using System.Linq;
using NUnit.Framework;
using SparkyStudios.TrakLibrary.Model.Games.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Validation
{
    public class GameRequestDetailsValidatorTest
    {
        [Test]
        public void Validate_WithNullTitle_ValidationFails()
        {
            // Arrange
            var validator = new GameRequestDetailsValidator();

            // Act
            var result = validator.Validate(new GameRequestDetails());

            // Assert
            Assert.AreEqual(Messages.GameRequestErrorMessageTitleEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null title.");
        }

        [Test]
        public void Validate_WithEmptyTitle_ValidationFails()
        {
            // Arrange
            var validator = new GameRequestDetailsValidator();

            // Act
            var result = validator.Validate(new GameRequestDetails
            {
                Title = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.GameRequestErrorMessageTitleEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty title.");
        }

        [Test]
        public void Validate_WithTitleExceeding255Characters_ValidationFails()
        {
            // Arrange
            var validator = new GameRequestDetailsValidator();

            // Act
            var result = validator.Validate(new GameRequestDetails
            {
                Title = string.Concat(Enumerable.Repeat("a", 300))
            });

            // Assert
            Assert.AreEqual(Messages.GameRequestErrorMessageTitleMaxLength,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a title exceeding 255 characters.");
        }

        [Test]
        public void Validate_WithNotesExceeding2048Characters_ValidationFails()
        {
            // Arrange
            var validator = new GameRequestDetailsValidator();

            // Act
            var result = validator.Validate(new GameRequestDetails
            {
                Title = "test-title",
                Comments = string.Concat(Enumerable.Repeat("a", 3000))
            });

            // Assert
            Assert.AreEqual(Messages.GameRequestErrorMessageCommentsMaxLength,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if notes exceed 2048 characters.");
        }

        [Test]
        public void Validate_WithValidGameRequestDetails_ValidationPasses()
        {
            // Arrange
            var validator = new GameRequestDetailsValidator();

            // Act
            var result = validator.Validate(new GameRequestDetails
            {
                Title = "test-title",
                Comments = "test-notes"
            });

            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true if validation was successful.");
        }
    }
}