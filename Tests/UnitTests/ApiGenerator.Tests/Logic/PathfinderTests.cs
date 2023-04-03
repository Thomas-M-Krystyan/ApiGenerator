using ApiGenerator.Logic.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiGenerator.Tests.Logic
{
    [TestClass]
    public class PathfinderTests
    {
        [DataTestMethod]
        [DataRow("Path", new string[] { }, @"Path")]
        [DataRow("Path", new[] { "Test" }, @"Path\Test")]
        [DataRow("Path", new[] { "Test", "Folder" }, @"Path\Test\Folder")]
        public void TestMethod_WithPathSubfolders_ReturnsExpectedPath(
            string testPath, string[] testSubfolders, string expectedPath)
        {
            // Act
            var actualPath = testPath.WithPathSubfolders(testSubfolders);

            // Assert
            Assert.AreEqual(expectedPath, actualPath);
        }

        [DataTestMethod]
        [DataRow("Namespace", new string[] { }, @"Namespace")]
        [DataRow("Namespace", new[] { "Test" }, @"Namespace.Test")]
        [DataRow("Namespace", new[] { "Test", "Folder" }, @"Namespace.Test.Folder")]
        public void TestMethod_WithNamespaceSubfolders_ReturnsExpectedNamespace(
            string testNamespace, string[] testSubfolders, string expectedNamespace)
        {
            // Act
            var actualNamespace = testNamespace.WithNamespaceSubfolders(testSubfolders);

            // Assert
            Assert.AreEqual(expectedNamespace, actualNamespace);
        }
    }
}
