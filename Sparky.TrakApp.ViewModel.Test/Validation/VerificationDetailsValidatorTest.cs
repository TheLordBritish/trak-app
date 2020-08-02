using System.Linq;
using NUnit.Framework;
using Sparky.TrakApp.Model.Login.Validation;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Test.Validation
{
    public class VerificationDetailsValidatorTest
    {
        [Test]
        public void Validate_WithNullVerificationCode_ValidationFails()
        {
            // Arrange
            var validator = new VerificationDetailsValidator();

            // Act
            var result = validator.Validate(new VerificationDetails());

            // Assert
            Assert.AreEqual(Messages.VerificationErrorMessageVerificationCodeEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null verification code.");
        }

        [Test]
        public void Validate_WithEmptyVerificationCode_ValidationFails()
        {
            // Arrange
            var validator = new VerificationDetailsValidator();

            // Act
            var result = validator.Validate(new VerificationDetails {VerificationCode = string.Empty});

            // Assert
            Assert.AreEqual(Messages.VerificationErrorMessageVerificationCodeEmpty,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty verification code.");
        }

        [Test]
        public void Validate_WithVerificationCodeContainingWhitespace_ValidationFails()
        {
            // Arrange
            var validator = new VerificationDetailsValidator();

            // Act
            var result = validator.Validate(new VerificationDetails {VerificationCode = "11111 "});

            // Assert
            Assert.AreEqual(Messages.VerificationErrorMessageVerificationCodeWhitespace,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false for an verification code containing white space.");
        }

        [Test]
        public void Validate_WithVerificationCodeWithMoreThanFiveCharacter_ValidationFails()
        {
            // Arrange
            var validator = new VerificationDetailsValidator();

            // Act
            var result = validator.Validate(new VerificationDetails {VerificationCode = "111111"});

            // Assert
            Assert.AreEqual(Messages.VerificationErrorMessageVerificationCodeLength,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false for an verification code containing more than five characters.");
        }

        [Test]
        public void Validate_WithVerificationCodeWithNonAlphaNumericCharacters_ValidationFails()
        {
            // Arrange
            var validator = new VerificationDetailsValidator();

            // Act
            var result = validator.Validate(new VerificationDetails {VerificationCode = "11_$%"});

            // Assert
            Assert.AreEqual(Messages.VerificationErrorMessageVerificationCodeInvalidCharacters,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid,
                "result.IsValid should be false for an verification code containing non alphanumeric characters.");
        }

        [Test]
        public void Validate_WithValidVerificationCode_ValidationPasses()
        {
            // Arrange
            var validator = new VerificationDetailsValidator();

            // Act
            var result = validator.Validate(new VerificationDetails {VerificationCode = "12AV3"});

            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true for a valid verification code.");
        }
    }
}