using System.Linq;
using NUnit.Framework;
using Sparky.TrakApp.Model.Login.Validation;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Test.Validation
{
    public class RegistrationDetailsValidatorTest
    {
        [Test]
        public void Validate_WithNullUsername_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails());

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageUsernameEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null username.");
        }

        [Test]
        public void Validate_WithEmptyUsername_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageUsernameEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty username.");
        }

        [Test]
        public void Validate_WithUsernameContainingWhitespace_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "User name"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageUsernameWhitespace,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a username containing white space.");
        }

        [Test]
        public void Validate_WithUsernameExceeding255Characters_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = string.Concat(Enumerable.Repeat("a", 256))
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageUsernameLength,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a username exceeding 255 characters.");
        }

        [Test]
        public void Validate_WithUsernameContainingInvalidCharacters_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "@#'$*"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageUsernameInvalidCharacters,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false for a username containing invalid characters.");
        }

        [Test]
        public void Validate_WithNullEmailAddress_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageEmailAddressEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null email address.");
        }

        [Test]
        public void Validate_WithEmptyEmailAddress_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
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
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email address"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageEmailAddressInvalid,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an invalid email address.");
        }

        [Test]
        public void Validate_WithNullPassword_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null password.");
        }

        [Test]
        public void Validate_WithEmptyPassword_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty password.");
        }

        [Test]
        public void Validate_WithPasswordLessThanEightCharacters_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = "less"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordMinimumLength,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if the password is less than eight characters.");
        }

        [Test]
        public void Validate_WithPasswordWithNoUppercaseCharacters_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = "password"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordUppercaseCharacter,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if the password has no uppercase characters.");
        }

        [Test]
        public void Validate_WithPasswordWithNoLowercaseCharacters_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = "PASSWORD"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordLowercaseCharacter,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if the password has no lowercase characters.");
        }

        [Test]
        public void Validate_WithPasswordWithNoNumbers_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = "Password"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordNumber,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the password has no numbers.");
        }

        [Test]
        public void Validate_WithPasswordContainingWhitespace_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = "Password123 "
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordWhitespace,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the password contains whitespace.");
        }

        [Test]
        public void Validate_WithNullConfirmPassword_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = "Password123"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageConfirmPasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null confirm password.");
        }

        [Test]
        public void Validate_WithEmptyConfirmPassword_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = "Password123",
                ConfirmPassword = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageConfirmPasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty confirm password.");
        }

        [Test]
        public void Validate_WithNonMatchingConfirmPassword_ValidationFails()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = "Password123",
                ConfirmPassword = "Password1234"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageConfirmPasswordMismatch,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the password don't match.");
        }

        [Test]
        public void Validate_WithValidRegistrationDetails_ValidationPasses()
        {
            // Arrange
            var validator = new RegistrationDetailsValidator();

            // Act
            var result = validator.Validate(new RegistrationDetails
            {
                Username = "Username",
                EmailAddress = "email@address.com",
                Password = "Password123",
                ConfirmPassword = "Password123"
            });

            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true if validation was successful.");
        }
    }
}