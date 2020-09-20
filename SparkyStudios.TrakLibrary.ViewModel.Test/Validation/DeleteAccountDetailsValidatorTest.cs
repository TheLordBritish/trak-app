using System.Linq;
using NUnit.Framework;
using SparkyStudios.TrakLibrary.Model.Settings.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Validation
{
    [TestFixture]
    public class DeleteAccountDetailsValidatorTest
    {
        [Test]
        public void Validate_WithNullDeleteMe_ValidationFails()
        {
            // Arrange
            var validator = new DeleteAccountDetailsValidator();

            // Act
            var result = validator.Validate(new DeleteAccountDetails());

            // Assert
            Assert.AreEqual(Messages.DeleteAccountIncorrectDeleteMessage,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for a null delete me.");
        }
        
        [Test]
        public void Validate_WithEmptyDeleteMe_ValidationFails()
        {
            // Arrange
            var validator = new DeleteAccountDetailsValidator();

            // Act
            var result = validator.Validate(new DeleteAccountDetails
            {
                DeleteMe = string.Empty
            });

            // Assert
            Assert.AreEqual(Messages.DeleteAccountIncorrectDeleteMessage,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an empty delete me.");
        }

        [Test]
        public void Validate_WithDeleteMeNotMatchingDeleteMeText_ValidationFails()
        {
            // Arrange
            var validator = new DeleteAccountDetailsValidator();

            // Act
            var result = validator.Validate(new DeleteAccountDetails
            {
                DeleteMe = "RandomMessage"
            });
            
            // Assert
            Assert.AreEqual(Messages.DeleteAccountIncorrectDeleteMessage,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an incorrect message.");
        }
        
        [Test]
        public void Validate_WithMatchingTextInWrongCase_ValidationFails()
        {
            // Arrange
            var validator = new DeleteAccountDetailsValidator();

            // Act
            var result = validator.Validate(new DeleteAccountDetails
            {
                DeleteMe = "Delete Me"
            });
            
            // Assert
            Assert.AreEqual(Messages.DeleteAccountIncorrectDeleteMessage,
                result.Errors.First().ErrorMessage);
            Assert.IsFalse(result.IsValid, "result.IsValid should be false for an incorrect casing.");
        }
        
        [Test]
        public void Validate_WithMatchingText_ValidationSucceeds()
        {
            // Arrange
            var validator = new DeleteAccountDetailsValidator();

            // Act
            var result = validator.Validate(new DeleteAccountDetails
            {
                DeleteMe = Messages.DeleteAccountPageDeleteMe
            });
            
            // Assert
            Assert.IsTrue(result.IsValid, "result.IsValid should be true is the delete me text is equal to DELETE ME.");
        }
    }
}