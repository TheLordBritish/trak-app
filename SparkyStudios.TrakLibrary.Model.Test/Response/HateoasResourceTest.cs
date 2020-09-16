using System;
using System.Collections.Generic;
using NUnit.Framework;
using SparkyStudios.TrakLibrary.Model.Response;

namespace SparkyStudios.TrakLibrary.Model.Test.Response
{
    public class HateoasResourceTest
    {
        private class HateoasResourceTestClass : HateoasResource
        {
        
        }
        
        [Test]
        public void GetLink_WithNullLinks_ReturnsNull()
        {
            // Arrange
            var hateoasResource = new HateoasResourceTestClass();
            
            // Act
            var result = hateoasResource.GetLink("link");
            
            // Assert
            Assert.IsNull(result, "The link should be null if the object has no links.");
        }
        
        [Test]
        public void GetLink_WithNonExistentLink_ReturnsNull()
        {
            // Arrange
            var hateoasResource = new HateoasResourceTestClass
            {
                Links = new Dictionary<string, HateoasLink>()
            };
            
            // Act
            var result = hateoasResource.GetLink("link");
            
            // Assert
            Assert.IsNull(result, "The link should be null if the object doesn't contain the requested link.");
        }
        
        [Test]
        public void GetLink_WithValidLink_ReturnsLink()
        {
            // Arrange
            var hateoasResource = new HateoasResourceTestClass
            {
                Links = new Dictionary<string, HateoasLink>
                {
                    {"link", new HateoasLink
                    {
                        Href = new Uri("https://traklibrary.com")
                    }}
                }
            };
            
            // Act
            var result = hateoasResource.GetLink("link");
            
            // Assert
            Assert.AreEqual(hateoasResource.Links["link"].Href, result, "The url requested should match the one within the resource.");
        }
    }
}