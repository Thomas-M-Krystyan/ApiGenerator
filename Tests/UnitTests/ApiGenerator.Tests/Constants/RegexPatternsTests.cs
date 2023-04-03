using ApiGenerator.Logic.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiGenerator.Tests.Constants
{
    [TestClass]
    public class RegexPatternsTests
    {
        [DataTestMethod]
        // Access modifiers and class keywords + cases with only class name
        [DataRow("\tpublic class Spike", "\tpublic class ", @"Spike", @"", @"")]
        [DataRow("\tinternal class Spike", "\tinternal class ", @"Spike", @"", @"")]
        [DataRow("\tpublic sealed class Spike", "\tpublic sealed class ", @"Spike", @"", @"")]
        [DataRow("\tinternal sealed class Spike", "\tinternal sealed class ", @"Spike", @"", @"")]
        [DataRow("\tpublic abstract class Spike", "\tpublic abstract class ", @"Spike", @"", @"")]
        [DataRow("\tinternal abstract class Spike", "\tinternal abstract class ", @"Spike", @"", @"")]
        // Cases with class name + inheritances
        [DataRow("\tpublic abstract class Spike : IValue", "\tpublic abstract class ", @"Spike", @"", @"IValue")]
        [DataRow("\tpublic abstract class Spike : IValue, IModel", "\tpublic abstract class ", @"Spike", @"", @"IValue, IModel")]
        // Cases with class name + generics (without constraints)
        [DataRow("\tpublic abstract class Spike<T>", "\tpublic abstract class ", @"Spike<T>", @"", @"")]
        [DataRow("\tpublic abstract class Spike<T, Item>", "\tpublic abstract class ", @"Spike<T, Item>", @"", @"")]
        // Cases with class name + generic constraints
        [DataRow("\tpublic abstract class Spike<T> where T : IValue", "\tpublic abstract class ", @"Spike<T>", @" where T : IValue", @"")]
        [DataRow("\tpublic abstract class Spike<T, Item> where T : IValue", "\tpublic abstract class ", @"Spike<T, Item>", @" where T : IValue", @"")]
        // Cases with class name + generic constraints + inheritances
        [DataRow("\tpublic abstract class Spike<T> where T : IValue, ItemA", "\tpublic abstract class ", @"Spike<T>", @" where T : IValue, ", @"ItemA")]
        [DataRow("\tpublic abstract class Spike<T> where T : IValue, ItemA, ItemB", "\tpublic abstract class ", @"Spike<T>", @" where T : IValue, ", @"ItemA, ItemB")]
        [DataRow("\tpublic abstract class Spike<T> where T : IValue, IModel, ItemA, ItemB", "\tpublic abstract class ", @"Spike<T>", @" where T : IValue, ", @"IModel, ItemA, ItemB")]
        // Complex cases when inheritances have generics as well
        [DataRow("\tpublic abstract class Spike<T> where T : IValue, IModel, IAction<Item>", "\tpublic abstract class ", @"Spike<T>", @" where T : IValue, ", @"IModel, IAction<Item>")]
        [DataRow("\tpublic abstract class Spike<T> where T : IValue, IModel<Item>, IAction", "\tpublic abstract class ", @"Spike<T>", @" where T : IValue, ", @"IModel<Item>, IAction")]
        // Interfaces
        [DataRow("\tpublic interface ITestInterface", "\tpublic interface ", @"ITestInterface", @"", @"")]
        [DataRow("\tinternal interface ITestInterface", "\tinternal interface ", @"ITestInterface", @"", @"")]
        [DataRow("\tpublic interface ITestInterface<T>", "\tpublic interface ", @"ITestInterface<T>", @"", @"")]
        [DataRow("\tpublic interface ITestInterface<T, V>", "\tpublic interface ", @"ITestInterface<T, V>", @"", @"")]
        [DataRow("\tpublic interface ITestInterface<T> where T : Item", "\tpublic interface ", @"ITestInterface<T>", @" where T : Item", @"")]
        //[DataRow("\tpublic interface ITestInterface<T, V> where T : Item where V : Model", "\tpublic interface ", @"ITestInterface<T, V>", @" where T : Item where V : Model", @"")] TODO: Finish this case
        [DataRow("\tpublic interface ITestInterface<T> where T : Item, IDependentInterface", "\tpublic interface ", @"ITestInterface<T>", @" where T : Item, ", @"IDependentInterface")]
        [DataRow("\tpublic interface ITestInterface<T> where T : Item, IDependentInterfaceA, IDependentInterfaceB", "\tpublic interface ", @"ITestInterface<T>", @" where T : Item, ", @"IDependentInterfaceA, IDependentInterfaceB")]
        // Complex cases with interfaces having "out" keywords and changed order of generic constraints vs inheritances
        [DataRow("\tpublic interface IRestrictedGroupLayerViewModel<out TModel> : IGroupLayerViewModel where TModel : Item", "\tpublic interface ", @"IRestrictedGroupLayerViewModel<out TModel>", @"", @"IGroupLayerViewModel where TModel : Item")]  // NOTE: This result might be questionable
        [DataRow("\tpublic interface IRestrictedGroupLayerViewModel<out TModel> where TModel : Item, IGroupLayerViewModel", "\tpublic interface ", @"IRestrictedGroupLayerViewModel<out TModel>", @" where TModel : Item, ", @"IGroupLayerViewModel")]
        public void CheckPattern_ClassDeclarationRegex_WithCornerCases(
            string testInput, string expectedClassDeclaration, string expectedClassName,
            string expectedGenericConstraint, string expectedInheritances)
        {
            // Act
            var match = RegexPatterns.ClassDeclarationRegex.Match(testInput);

            // Assert
            Assert.IsTrue(match.Success);

            Assert.AreEqual(expectedClassDeclaration, match.Groups[RegexPatterns.GroupClassDeclaration].Value);
            Assert.AreEqual(expectedClassName, match.Groups[RegexPatterns.GroupClassName].Value);
            Assert.AreEqual(expectedGenericConstraint, match.Groups[RegexPatterns.GroupGenericConstraint].Value);
            Assert.AreEqual(expectedInheritances, match.Groups[RegexPatterns.GroupClassInheritance].Value);
        }
    }
}
