using System.Linq;
using NUnit.Framework;
using SparkyStudios.TrakLibrary.Model.Settings.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Validation
{
    [TestFixture]
    public class ChangeEmailAddressDetailsValidatorTest
    {
        [Test]
        public void Validate_WithNullEmailAddress_ValidationFails()
        {
            // Arrange
            var validator = new ChangeEmailAddressDetailsValidator();

            // Act
            var result = validator.Validate(new ChangeEmailAddressDetails());

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageEmailAddressEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null email address.");
        }
        
        [Test]
        public void Validate_WithEmptyEmailAddress_ValidationFails()
        {
            // Arrange
            var validator = new ChangeEmailAddressDetailsValidator();

            // Act
            var result = validator.Validate(new ChangeEmailAddressDetails
            {
                EmailAddress = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageEmailAddressEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty email address.");
        }
        
        [Test]
        public void Validate_WithInvalidEmailAddress_ValidationFails()
        {
            // Arrange
            var validator = new ChangeEmailAddressDetailsValidator();

            // Act
            var result = validator.Validate(new ChangeEmailAddressDetails
            {
                EmailAddress = "invalid-email"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageEmailAddressInvalid,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an invalid email address.");
        }
        
        [Test]
        public void Validate_WithValidEmailAddress_ValidationPasses()
        {
            // Arrange
            var validator = new ChangeEmailAddressDetailsValidator();

            // Act
            var result = validator.Validate(new ChangeEmailAddressDetails
            {
                EmailAddress = "test@traklibrary.com"
            });

            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true for a valid email address.");
        }
    }
}