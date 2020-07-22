using System.Linq;
using NUnit.Framework;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Model.Login.Validation;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Test.Validation
{
    public class ForgottenPasswordDetailsValidatorTest
    {
        [Test]
        public void Validate_WithNullEmailAddress_ValidationFails()
        {
            // Arrange
            var validator = new ForgottenPasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ForgottenPasswordDetails());

            // Assert
            Assert.AreEqual(Messages.ForgottenPasswordErrorMessageEmailAddressEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null email address.");
        }

        [Test]
        public void Validate_WithEmptyEmailAddress_ValidationFails()
        {
            // Arrange
            var validator = new ForgottenPasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ForgottenPasswordDetails {EmailAddress = string.Empty});

            // Assert
            Assert.AreEqual(Messages.ForgottenPasswordErrorMessageEmailAddressEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty email address.");
        }

        [Test]
        public void Validate_WithEmailAddressWithWhitespace_ValidationFails()
        {
            // Arrange
            var validator = new ForgottenPasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ForgottenPasswordDetails {EmailAddress = "email "});

            // Assert
            Assert.AreEqual(Messages.VerificationErrorMessageVerificationCodeWhitespace,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if a email address contains whitespace.");
        }

        [Test]
        public void Validate_WithInvalidEmailAddress_ValidationFails()
        {
            // Arrange
            var validator = new ForgottenPasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ForgottenPasswordDetails {EmailAddress = "email"});

            // Assert
            Assert.AreEqual(Messages.ForgottenPasswordErrorMessageEmailAddressInvalid,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false if the email address is invalid.");
        }

        [Test]
        public void Validate_WithValidEmailAddress_ValidationPasses()
        {
            // Arrange
            var validator = new ForgottenPasswordDetailsValidator();

            // Act
            var result = validator.Validate(new ForgottenPasswordDetails {EmailAddress = "email@address.com"});

            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true if validation was successful.");
        }
    }
}