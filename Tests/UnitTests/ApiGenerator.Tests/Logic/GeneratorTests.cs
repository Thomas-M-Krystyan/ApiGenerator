using System;
using ApiGenerator.Examples.Classes;
using ApiGenerator.Logic.Logic;
using ApiGenerator.Logic.Workflow.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiGenerator.Tests.Logic
{
    [TestClass]
    public class GeneratorTests
    {
        #region Test settings
        private static readonly GenerationSettings TestSettings = new
        (
            @"Test",
            @"Test",
            @"Test",
            @"Test",
            Array.Empty<string>(),
            Array.Empty<string>(),
            new GenerationStrategy
            (
                new RegistrationSettings
                (
                    @"Test",
                    Array.Empty<string>(),
                    @"Test",
                    @"Test",
                    @"Test"
                )
            )
            {
                UseFullyQualifiedNames = false
            }
        );
        #endregion

        #region Reformat
        [DataTestMethod]
        [DataRow("Text", 0, "/// Text")]
        [DataRow("Text", 1, "    /// Text")]
        [DataRow("Text", 2, "        /// Text")]
        public void TestMethod_Reformat_ReturnsReplacedContent(string originalLine, int indentationLevel, string expectedLine)
        {
            // Act
            var actualLine = Generator.Reformat(originalLine, (ushort)indentationLevel);

            // Assert
            Assert.AreEqual(expectedLine, actualLine);
        }
        #endregion
        
        #region Cleanup
        [DataTestMethod]
        // Empty tags
        [DataRow("<summary>", "<summary>")]
        [DataRow("</summary>", "</summary>")]
        // Without opening tag (simplified dots, checking mostly tags)
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method</summary>", "Test summary for <see cref=\"SetName(string)\"/> method.</summary>")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method.</summary>", "Test summary for <see cref=\"SetName(string)\"/> method.</summary>")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method .</summary>", "Test summary for <see cref=\"SetName(string)\"/> method.</summary>")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method. </summary>", "Test summary for <see cref=\"SetName(string)\"/> method.</summary>")]
        // Without closing tag (simplified dots, checking mostly tags)
        [DataRow("<summary>Test summary for <see cref=\"SetName(string)\"/> method", "<summary>Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("<summary>Test summary for <see cref=\"SetName(string)\"/> method.", "<summary>Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("<summary>Test summary for <see cref=\"SetName(string)\"/> method .", "<summary>Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("<summary>Test summary for <see cref=\"SetName(string)\"/> method. ", "<summary>Test summary for <see cref=\"SetName(string)\"/> method.")]
        // Single dots
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method.", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method. ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method.   ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method .", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method  .", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method  . ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method  .  ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        // Two dots
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method..", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method.. ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method ..", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method .. ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method. .", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method. . ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method . .", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method . . ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method . .  ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method .  . ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method  . . ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method  . .  ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method  .  .  ", "Test summary for <see cref=\"SetName(string)\"/> method.")]
        // Handling of triple dots
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method...", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method... ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method ...", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method ... ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method. ..", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method. .. ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method . ..", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method . .. ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method .  .. ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method.. .", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method.. . ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method .. .", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method .. . ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method. . .", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method. . . ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method . . .", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method . . . ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method....", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method.... ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method ....", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test summary for <see cref=\"SetName(string)\"/> method .... ", "Test summary for <see cref=\"SetName(string)\"/> method...")]
        // Multiple sentences in one line
        [DataRow("<summary>Test. Summary for <see cref=\"SetName(string)\"/> method", "<summary>Test. Summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("<summary>Test. Summary for <see cref=\"SetName(string)\"/> method.", "<summary>Test. Summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("<summary>Test. Summary for <see cref=\"SetName(string)\"/> method. ", "<summary>Test. Summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("<summary>Test. Summary for <see cref=\"SetName(string)\"/> method .", "<summary>Test. Summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("<summary>Test. Summary for <see cref=\"SetName(string)\"/> method . ", "<summary>Test. Summary for <see cref=\"SetName(string)\"/> method.")]
        [DataRow("Test. Summary for <see cref=\"SetName(string)\"/> method...", "Test. Summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test. Summary for <see cref=\"SetName(string)\"/> method... ", "Test. Summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test. Summary for <see cref=\"SetName(string)\"/> method ...", "Test. Summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test. Summary for <see cref=\"SetName(string)\"/> method ... ", "Test. Summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test. Summary for <see cref=\"SetName(string)\"/> method. ..", "Test. Summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test. Summary for <see cref=\"SetName(string)\"/> method.. .", "Test. Summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test. Summary for <see cref=\"SetName(string)\"/> method .. .", "Test. Summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test. Summary for <see cref=\"SetName(string)\"/> method. . .", "Test. Summary for <see cref=\"SetName(string)\"/> method...")]
        [DataRow("Test. Summary for <see cref=\"SetName(string)\"/> method . . .", "Test. Summary for <see cref=\"SetName(string)\"/> method...")]
        public void TestMethod_Cleanup_ReturnsReplacedContent(string testLine, string expectedSummaryLine)
        {
            // Act
            var actualSummaryLine = Generator.Cleanup(testLine);

            // Assert
            Assert.AreEqual(expectedSummaryLine, actualSummaryLine);
        }
        #endregion

        #region SimplifyRef
        [DataTestMethod]
        [DataRow("", "")]
        [DataRow(" ", " ")]
        [DataRow(null, "")]
        [DataRow("<see cref=\"Age\"/>", "<see cref=\"Age\"/>")]
        [DataRow("<see cref=\"P:ApiGenerator.Examples.Spike.Age\" />", "<see cref=\"Age\"/>")]
        [DataRow("<see cref=\"P:ApiGenerator.Examples.Spike.Age\"/>", "<see cref=\"Age\"/>")]
        public void TestMethod_SimplifyRef_ReturnsReplacedContent(string originalLine, string expectedLine)
        {
            // Arrange
            var generator = new Generator(typeof(Item), TestSettings);

            // Act
            var actualLine = generator.SimplifyCref(originalLine);

            // Assert
            Assert.AreEqual(expectedLine, actualLine);
        }
        #endregion

        #region ReplaceSystemTypes
        [DataTestMethod]
        // Do not replace anything
        [DataRow("\t{", "\t{")]
        [DataRow("\t/// Test summary for <see cref=\"SetName(string)\"/> method.", "\t/// Test summary for <see cref=\"SetName(string)\"/> method.")]
        // Replace single cases in "cref" tags
        [DataRow("\t/// Test summary for <see cref=\"System.String\"/> method.", "\t/// Test summary for <see cref=\"string\"/> method.")]
        // Replace single cases declared as parameter
        [DataRow("\t/// Test summary for <see cref=\"SetName(System.Int32)\"/> method.", "\t/// Test summary for <see cref=\"SetName(int)\"/> method.")]
        [DataRow("\t/// Test summary for <see cref=\"SetName(System.Collections.Generic.List<System.Int64>)\"/> method.", "\t/// Test summary for <see cref=\"SetName(List<long>)\"/> method.")]
        // Replace multiple cases declared as parameters
        [DataRow("\t/// Test summary for <see cref=\"SetName(System.String, System.Int32)\"/> method.", "\t/// Test summary for <see cref=\"SetName(string, int)\"/> method.")]
        public void TestMethod_ReplaceSystemTypes_ReturnsReplacedContent(string originalLine, string expectedLine)
        {
            // Arrange
            var generator = new Generator(typeof(Item), TestSettings);

            // Act
            var actualLine = generator.ReplaceFullTypes(originalLine);

            // Assert
            Assert.AreEqual(expectedLine, actualLine);
        }
        #endregion
        
        #region GetTabs
        [DataTestMethod]
        [DataRow(0, "")]
        [DataRow(1, "    ")]
        [DataRow(2, "        ")]
        public void TestMethod_GetTabs_ToGetValidAliasName(int amount, string expectedTabulation)
        {
            // Act
            var actualTabulation = Generator.GetTabs((ushort)amount);

            // Assert
            Assert.AreEqual(expectedTabulation, actualTabulation);
        }
        #endregion
    }
}
