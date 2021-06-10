using System.Linq;
using NUnit.Framework;
using SparkyStudios.TrakLibrary.Model.Settings.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Validation
{
    [TestFixture]
    public class ChangePasswordDetailsValidatorTest
    {
        [Test]
        public void Validate_WithNullCurrentPassword_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails());

            // Assert
            Assert.AreEqual(Messages.ChangePasswordErrorMessageCurrentPasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null recovery token.");
        }

        [Test]
        public void Validate_WithEmptyCurrentPassword_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.ChangePasswordErrorMessageCurrentPasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty recovery token.");
        }

        [Test]
        public void Validate_WithCurrentPasswordContainingWhitespace_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = "password   "
            });

            // Assert
            Assert.AreEqual(Messages.ChangePasswordErrorMessageCurrentPasswordWhitespace,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false for a recovery token containing white space.");
        }
        
        [Test]
        public void Validate_WithNullNewPassword_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                ConfirmNewPassword = "Password123"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null new password.");
        }

        [Test]
        public void Validate_WithEmptyNewPassword_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = string.Empty,
                ConfirmNewPassword = "Password123"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty new password.");
        }

        [Test]
        public void Validate_WithNewPasswordLessThanEightCharacters_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = "Less1",
                ConfirmNewPassword = "Password123"
            });
            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordMinimumLength,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if the new password is less than eight characters.");
        }

        [Test]
        public void Validate_WithNewPasswordWithNoUppercaseCharacters_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = "password123",
                ConfirmNewPassword = "Password123"
            });
            
            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordUppercaseCharacter,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if the new password has no uppercase characters.");
        }

        [Test]
        public void Validate_WithNewPasswordWithNoLowercaseCharacters_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = "PASSWORD123",
                ConfirmNewPassword = "Password123"
            });
            
            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordLowercaseCharacter,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false if the new password has no lowercase characters.");
        }

        [Test]
        public void Validate_WithNewPasswordWithNoNumbers_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = "PasswordIsLong",
                ConfirmNewPassword = "Password123"
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordNumber,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the new password has no numbers.");
        }

        [Test]
        public void Validate_WithNewPasswordContainingWhitespace_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = "Password 123",
                ConfirmNewPassword = "Password123"
            });
            
            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessagePasswordWhitespace,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the new password contains whitespace.");
        }

        [Test]
        public void Validate_WithNullConfirmNewPassword_ValidationFails()
        {
            // Arrange
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = "Password123"
            });
            
            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageConfirmPasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null confirm new password.");
        }

        [Test]
        public void Validate_WithEmptyConfirmNewPassword_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = "Password123",
                ConfirmNewPassword = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageConfirmPasswordEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty confirm new password.");
        }

        [Test]
        public void Validate_WithNonMatchingConfirmNewPassword_ValidationFails()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = "Password1234",
                ConfirmNewPassword = "Password123"
            });
            
            // Assert
            Assert.AreEqual(Messages.RegistrationErrorMessageConfirmPasswordMismatch,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the passwords don't match.");
        }

        [Test]
        public void Validate_WithValidRegistrationDetails_ValidationPasses()
        {
            // Arrange
            var validator = new ChangePasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ChangePasswordDetails
            {
                CurrentPassword = string.Concat(Enumerable.Repeat("a", 30)),
                NewPassword = "Password123",
                ConfirmNewPassword = "Password123"
            });

            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true if validation was successful.");
        }
    }
}