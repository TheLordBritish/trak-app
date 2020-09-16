using System.Linq;
using NUnit.Framework;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Validation
{
    public class UserCredentialsValidatorTest
    {
        [Test]
        public void Validate_WithNullUsername_ValidationFails()
        {
            // Arrange
            var validator = new UserCredentialsValidator();

            // Act
            var result = validator.Validate(new UserCredentials {Password = "Password"});

            // Assert
            Assert.AreEqual(Messages.LoginErrorMessageUsernameEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null username.");
        }

        [Test]
        public void Validate_WithEmptyUsername_ValidationFails()
        {
            // Arrange
            var validator = new UserCredentialsValidator();

            // Act
            var result = validator.Validate(new UserCredentials {Username = string.Empty, Password = "Password"});

            // Assert
            Assert.AreEqual(Messages.LoginErrorMessageUsernameEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty username.");
        }

        [Test]
        public void Validate_WithNullPassword_ValidationFails()
        {
            // Arrange
            var validator = new UserCredentialsValidator();

            // Act
            var result = validator.Validate(new UserCredentials {Username = "Username"});

            // Assert
            Assert.AreEqual(Messages.LoginErrorMessagePasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null password.");
        }

        [Test]
        public void Validate_WithEmptyPassword_ValidationFails()
        {
            // Arrange
            var validator = new UserCredentialsValidator();

            // Act
            var result = validator.Validate(new UserCredentials {Username = "Username", Password = string.Empty});

            // Assert
            Assert.AreEqual(Messages.LoginErrorMessagePasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty password.");
        }

        [Test]
        public void Validate_WithValidUserCredentials_ValidationPasses()
        {
            // Arrange
            var validator = new UserCredentialsValidator();

            // Act
            var result = validator.Validate(new UserCredentials {Username = "Username", Password = "Password"});

            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true for valid user credentials.");
        }
    }
}