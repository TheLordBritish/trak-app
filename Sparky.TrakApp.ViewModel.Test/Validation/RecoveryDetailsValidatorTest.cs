using System.Linq;
using NUnit.Framework;
using Sparky.TrakApp.Model.Login.Validation;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Test.Validation
{
    public class RecoveryDetailsValidatorTest
    {
        [Test]
        public void Validate_WithNullUsername_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails());

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageUsernameEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null username.");
        }

        [Test]
        public void Validate_WithEmptyUsername_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageUsernameEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty username.");
        }

        [Test]
        public void Validate_WithUsernameContainingWhitespace_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "User name"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageUsernameWhitespace,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a username containing white space.");
        }

        [Test]
        public void Validate_WithUsernameExceeding255Characters_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = string.Concat(Enumerable.Repeat("a", 256))
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageUsernameLength,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a username exceeding 255 characters.");
        }

        [Test]
        public void Validate_WithUsernameContainingInvalidCharacters_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "@#'$*"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageUsernameInvalidCharacters,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false for a username containing invalid characters.");
        }

        [Test]
        public void Validate_WithNullRecoveryToken_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageRecoveryTokenEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null recovery token.");
        }

        [Test]
        public void Validate_WithEmptyRecoveryToken_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageRecoveryTokenEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty recovery token.");
        }

        [Test]
        public void Validate_WithRecoveryTokenContainingWhitespace_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = "token   "
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageRecoveryTokenWhitespace,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false for a recovery token containing white space.");
        }

        [Test]
        public void Validate_WithRecoveryTokenOfIncorrectLength_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = "token"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageRecoveryTokenLength,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if a recovery token is not 30 characters long.");
        }

        [Test]
        public void Validate_WithRecoveryTokenWithNonAlphaNumericCharacters_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 29)) + "@"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageRecoveryTokenAlphanumeric,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if a recovery token contains non-alphanumeric characters.");
        }

        [Test]
        public void Validate_WithNullPassword_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30))
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessagePasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null password.");
        }

        [Test]
        public void Validate_WithEmptyPassword_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessagePasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty password.");
        }

        [Test]
        public void Validate_WithPasswordLessThanEightCharacters_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = "less"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessagePasswordMinimumLength,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if the password is less than eight characters.");
        }

        [Test]
        public void Validate_WithPasswordWithNoUppercaseCharacters_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = "password"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessagePasswordUppercaseCharacter,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if the password has no uppercase characters.");
        }

        [Test]
        public void Validate_WithPasswordWithNoLowercaseCharacters_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = "PASSWORD"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessagePasswordLowercaseCharacter,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if the password has no lowercase characters.");
        }

        [Test]
        public void Validate_WithPasswordWithNoNumbers_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = "Password"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessagePasswordNumber,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the password has no numbers.");
        }

        [Test]
        public void Validate_WithPasswordContainingWhitespace_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = "Password123 "
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessagePasswordWhitespace,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the password contains whitespace.");
        }

        [Test]
        public void Validate_WithNullConfirmPassword_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = "Password123"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageConfirmPasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null confirm password.");
        }

        [Test]
        public void Validate_WithEmptyConfirmPassword_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = "Password123",
                ConfirmPassword = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageConfirmPasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty confirm password.");
        }

        [Test]
        public void Validate_WithNonMatchingConfirmPassword_ValidationFails()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = "Password123",
                ConfirmPassword = "Password1234"
            });

            // Assert
            Assert.AreEqual(Messages.RecoveryErrorMessageConfirmPasswordMismatch,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the password don't match.");
        }

        [Test]
        public void Validate_WithValidRecoveryDetails_ValidationPasses()
        {
            // Arrange
            var validator = new RecoveryDetailsValidator();

            // Act
            var result = validator.Validate(new RecoveryDetails
            {
                Username = "Username",
                RecoveryToken = string.Concat(Enumerable.Repeat("a", 30)),
                Password = "Password123",
                ConfirmPassword = "Password123"
            });

            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true if validation was successful.");
        }
    }
}