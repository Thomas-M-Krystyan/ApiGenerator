using System;
using System.Collections.Generic;
using ApiGenerator.Logic.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiGenerator.Tests.Extensions
{
    [TestClass]
    public class ValidationExtensionsTests
    {
        [DataTestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void TestMethod_GuardAgainstMissing_String_ReturnsExpectedException(string testInput)
        {
            // Act
            var actualException = Assert.ThrowsException<ArgumentException>(() => testInput.GuardAgainstMissing());

            // Assert
            const string expectedExceptionMessage = @"The value of ""testInput"" is missing!";

            Assert.AreEqual(expectedExceptionMessage, actualException.Message);
        }
        
        [DataTestMethod]
        [DataRow(null)]
        public void TestMethod_GuardAgainstMissing_Object_ReturnsExpectedException(IList<string> testCollection)
        {
            // Act
            var actualException = Assert.ThrowsException<ArgumentNullException>(() => testCollection.GuardAgainstMissing());

            // Assert
            const string expectedExceptionMessage = "Value cannot be null.\r\nParameter name: The value of \"testCollection\" is null!";

            Assert.AreEqual(expectedExceptionMessage, actualException.Message);
        }
    }
}
