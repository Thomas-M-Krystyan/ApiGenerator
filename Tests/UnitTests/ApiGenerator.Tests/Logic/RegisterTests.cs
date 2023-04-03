using System.Linq;
using ApiGenerator.Logic.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiGenerator.Tests.Logic
{
    [TestClass]
    public class RegisterTests
    {
        [DataTestMethod]
        [DataRow(@"Test\Path", new[] { 1, 2, 3, 4, 5 }, @"5, 4, 3, 2, 1")]
        [DataRow(@"Test\Path", new int[] { }, @"")]
        public void TestMethod_GetFilesWithAnnotations_ReturnsRevertedOrder(string filePath, int[] linesNumbers, string expectedLinesOrder)
        {
            // Arrange
            Register.ClearAnnotationsPositions();

            foreach (var lineNumber in linesNumbers)
            {
                Register.TryAddAnnotationPosition(filePath, lineNumber);
            }

            // Act
            (string FilePath, int[] LinesNumbers)[] actualResults = Register.GetFilesWithAnnotations().ToArray();

            // Assert
            foreach (var (actualFilePath, actualLinesNumbers) in actualResults)
            {
                var actualLinesOrder = string.Join(@", ", actualLinesNumbers);

                Assert.AreEqual(@"Test\Path", actualFilePath);
                Assert.AreEqual(expectedLinesOrder, actualLinesOrder);
            }
        }
    }
}
