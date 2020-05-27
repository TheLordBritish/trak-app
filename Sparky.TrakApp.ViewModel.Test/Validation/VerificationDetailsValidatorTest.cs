using NUnit.Framework;
using Sparky.TrakApp.Model.Login;
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