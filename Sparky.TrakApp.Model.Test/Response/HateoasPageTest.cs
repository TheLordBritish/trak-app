using System.Collections.Generic;
using NUnit.Framework;
using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Model.Test.Response
{
    public class HateoasPageTest
    {
        private class HateoasResourceTestClass : HateoasResource
        {
        
        }

        [Test]
        public void HasNext_WithNullLinks_ReturnsFalse()
        {
            // Arrange
            var hateoasPage = new HateoasPage<HateoasResourceTestClass>();
            
            // Act
            var result = hateoasPage.HasNext;
            
            // Assert
            Assert.IsFalse(result, "Has next should be false if there are no links.");
        }
        
        [Test]
        public void HasNext_WithNonExistentNextLink_ReturnsFalse()
        {
            // Arrange
            var hateoasPage = new HateoasPage<HateoasResourceTestClass>
            {
                Links = new Dictionary<string, HateoasLink>()
            };
            
            // Act
            var result = hateoasPage.HasNext;
            
            // Assert
            Assert.IsFalse(result, "Has next should be false if there is no next link.");
        }
        
        [Test]
        public void HasNext_WithNextLink_ReturnsTrue()
        {
            // Arrange
            var hateoasPage = new HateoasPage<HateoasResourceTestClass>
            {
                Links = new Dictionary<string, HateoasLink>
                {
                    {"next", new HateoasLink()}
                }
            };
            
            // Act
            var result = hateoasPage.HasNext;
            
            // Assert
            Assert.IsTrue(result, "Has next should be true if there is a next link.");
        }
        
        [Test]
        public void HasPrevious_WithNullLinks_ReturnsFalse()
        {
            // Arrange
            var hateoasPage = new HateoasPage<HateoasResourceTestClass>();
            
            // Act
            var result = hateoasPage.HasPrevious;
            
            // Assert
            Assert.IsFalse(result, "Has previous should be false if there are no links.");
        }
        
        [Test]
        public void HasPrevious_WithNonExistentPrevLink_ReturnsFalse()
        {
            // Arrange
            var hateoasPage = new HateoasPage<HateoasResourceTestClass>
            {
                Links = new Dictionary<string, HateoasLink>()
            };
            
            // Act
            var result = hateoasPage.HasPrevious;
            
            // Assert
            Assert.IsFalse(result, "Has previous should be false if there is no next link.");
        }
        
        [Test]
        public void HasNext_WithPrevLink_ReturnsTrue()
        {
            // Arrange
            var hateoasPage = new HateoasPage<HateoasResourceTestClass>
            {
                Links = new Dictionary<string, HateoasLink>
                {
                    {"prev", new HateoasLink()}
                }
            };
            
            // Act
            var result = hateoasPage.HasPrevious;
            
            // Assert
            Assert.IsTrue(result, "Has previous should be true if there is a next link.");
        }
    }
}