using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ApiGenerator.Annotations;
using ApiGenerator.Logic.Logic;
using ApiGenerator.Logic.Workflow.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiGenerator.Tests.Logic
{
    // ReSharper disable InconsistentNaming

    [TestClass]
    public class WriterTests
    {
        #region Test settings
        private static readonly RegistrationSettings RegistrationSettings = new
        (
            @"Test",
            Array.Empty<string>(),
            @"Test",
            @"Test",
            @"Test"
        );

        private static readonly GenerationSettings TestSettings_FullyQualified = new
        (
            @"ApiGenerator",
            @"ApiGenerator",
            @"Test",
            @"Test",
            Array.Empty<string>(),
            Array.Empty<string>(),
            new GenerationStrategy(RegistrationSettings)
            {
                UseFullyQualifiedNames = true
            }
        );
        
        private static readonly GenerationSettings TestSettings_Simplified = new
        (
            @"ApiGenerator",
            @"ApiGenerator",
            @"Test",
            @"Test",
            Array.Empty<string>(),
            Array.Empty<string>(),
            new GenerationStrategy(RegistrationSettings)
            {
                UseFullyQualifiedNames = false
            }
        );
        #endregion

        #region Test classes
        [ApiClass]
        private class Animal { }

        [ApiClass]
        private class Cat { }

        [ApiClass]
        private class Dog { }
        #endregion

        [ClassInitialize]
        #pragma warning disable IDE0060  // TestContext parameter is not used but necessary for [ClassInitialize] method
        public static void SetupTests(TestContext context)
        #pragma warning restore IDE0060
        {
            Register.ClearClassNames();

            Register.TryAddGeneratedPair(new Generator(typeof(Animal), TestSettings_FullyQualified));
            Register.TryAddGeneratedPair(new Generator(typeof(Cat), TestSettings_FullyQualified));
            Register.TryAddGeneratedPair(new Generator(typeof(Dog), TestSettings_Simplified));
        }
        
        [ClassCleanup]
        public static void CleanupTests()
        {
            Register.ClearClassNames();
        }

        #region UpdateReferences
        [DataTestMethod]
        // Do not change type which is not registered
        [DataRow("\tpublic Zebra Get()", "Animal", "\tpublic Zebra Get()")]
        // Do not change class declarations for the same source class
        [DataRow("\tpublic class Animal", "Animal", "\tpublic class Animal")]
        [DataRow("\tinternal class Animal", "Animal", "\tinternal class Animal")]
        [DataRow("\tpublic abstract class Animal", "Animal", "\tpublic abstract class Animal")]
        [DataRow("\tinternal abstract class Animal", "Animal", "\tinternal abstract class Animal")]
        [DataRow("\tpublic sealed class Animal", "Animal", "\tpublic sealed class Animal")]
        [DataRow("\tinternal sealed class Animal", "Animal", "\tinternal sealed class Animal")]
        // Do not change class constructor for the same source class
        [DataRow("\tpublic Animal()", "Animal", "\tpublic Animal()")]
        [DataRow("\tinternal Animal()", "Animal", "\tinternal Animal()")]
        [DataRow("\tpublic Animal(string name)", "Animal", "\tpublic Animal(string name)")]
        [DataRow("\tinternal Animal(string name)", "Animal", "\tinternal Animal(string name)")]
        // Update class declaration for a different source class => Not a real case, but still possible
        [DataRow("\tpublic class Cat", "Animal", "\tpublic class ApiGenerator.ICat")]
        [DataRow("\tinternal class Cat", "Animal", "\tinternal class ApiGenerator.ICat")]
        [DataRow("\tpublic abstract class Cat", "Animal", "\tpublic abstract class ApiGenerator.ICat")]
        [DataRow("\tinternal abstract class Cat", "Animal", "\tinternal abstract class ApiGenerator.ICat")]
        [DataRow("\tpublic sealed class Cat", "Animal", "\tpublic sealed class ApiGenerator.ICat")]
        [DataRow("\tinternal sealed class Cat", "Animal", "\tinternal sealed class ApiGenerator.ICat")]
        // Update class constructor for a different source class => Not a real case, but still possible
        [DataRow("\tpublic Cat()", "Animal", "\tpublic ApiGenerator.ICat()")]
        [DataRow("\tinternal Cat()", "Animal", "\tinternal ApiGenerator.ICat()")]
        [DataRow("\tpublic Cat(string name)", "Animal", "\tpublic ApiGenerator.ICat(string name)")]
        [DataRow("\tinternal Cat(string name)", "Animal", "\tinternal ApiGenerator.ICat(string name)")]
        // Update return type
        [DataRow("\tpublic Cat GetAnimal(string name)", "Animal", "\tpublic ApiGenerator.ICat GetAnimal(string name)")]
        // Update many return types
        [DataRow("\tpublic (Cat, Dog) GetPair(string name)", "Animal", "\tpublic (ApiGenerator.ICat, IDog) GetPair(string name)")]
        // Update parameter
        [DataRow("\tpublic string GetName(Dog dog)", "Animal", "\tpublic string GetName(IDog dog)")]
        [DataRow("\tpublic string GetName(out Dog dog)", "Animal", "\tpublic string GetName(out IDog dog)")]
        [DataRow("\tpublic string GetName(ref Dog dog)", "Animal", "\tpublic string GetName(ref IDog dog)")]
        // Update one of many parameters
        [DataRow("\tpublic string GetName(int id, Dog dog)", "Animal", "\tpublic string GetName(int id, IDog dog)")]
        [DataRow("\tpublic string GetName(Dog dog, int id)", "Animal", "\tpublic string GetName(IDog dog, int id)")]
        [DataRow("\tpublic string GetName(int id, Dog dog, short size)", "Animal", "\tpublic string GetName(int id, IDog dog, short size)")]
        [DataRow("\tpublic string GetNames(int id, Dog dog, short size, Cat cat)", "Animal", "\tpublic string GetNames(int id, IDog dog, short size, ApiGenerator.ICat cat)")]
        // Update generic types
        [DataRow("\tpublic string CheckAnimals<Dog>()", "Animal", "\tpublic string CheckAnimals<IDog>()")]
        [DataRow("\tpublic IList<Dog> GetDogs()", "Animal", "\tpublic IList<IDog> GetDogs()")]
        [DataRow("\tpublic IList<IList<Dog>> GetDogRaces()", "Animal", "\tpublic IList<IList<IDog>> GetDogRaces()")]
        [DataRow("\tpublic KeyValuePair<string, Dog> GetKeyValuePair()", "Animal", "\tpublic KeyValuePair<string, IDog> GetKeyValuePair()")]
        // Very complex case
        [DataRow("\tpublic (Animal, T) Get<Zebra, T>(Cat reference1, T reference2) where T : Dog", "Animal", "\tpublic (ApiGenerator.IAnimal, T) Get<Zebra, T>(ApiGenerator.ICat reference1, T reference2) where T : IDog")]
        // Updating only the left part of the line (without object assignment or lambda expression returning concrete implementation)
        [DataRow(@"private Cat m_object;", "Animal", @"private ApiGenerator.ICat m_object;")]
        [DataRow(@"private readonly Cat m_object = null;", "Animal", @"private readonly ApiGenerator.ICat m_object = null;")]
        [DataRow(@"private readonly Cat m_object = default;", "Animal", @"private readonly ApiGenerator.ICat m_object = default;")]
        [DataRow(@"private readonly Cat m_object = new();", "Animal", @"private readonly ApiGenerator.ICat m_object = new();")]
        [DataRow(@"private readonly Cat m_object = new Cat();", "Animal", @"private readonly ApiGenerator.ICat m_object = new Cat();")]
        [DataRow(@"public Cat Object { get; }", "Animal", @"public ApiGenerator.ICat Object { get; }")]
        [DataRow(@"public Cat Object { set; }", "Animal", @"public ApiGenerator.ICat Object { set; }")]
        [DataRow(@"public Cat Object { get; set; }", "Animal", @"public ApiGenerator.ICat Object { get; set; }")]
        [DataRow(@"public Cat Object { get; } = null;", "Animal", @"public ApiGenerator.ICat Object { get; } = null;")]
        [DataRow(@"public Cat Object { get; set; } = null;", "Animal", @"public ApiGenerator.ICat Object { get; set; } = null;")]
        [DataRow(@"public Cat Object { get; } = default;", "Animal", @"public ApiGenerator.ICat Object { get; } = default;")]
        [DataRow(@"public Cat Object { get; set; } = default;", "Animal", @"public ApiGenerator.ICat Object { get; set; } = default;")]
        [DataRow(@"public Cat Object { get; } = new();", "Animal", @"public ApiGenerator.ICat Object { get; } = new();")]
        [DataRow(@"public Cat Object { get; set; } = new();", "Animal", @"public ApiGenerator.ICat Object { get; set; } = new();")]
        [DataRow(@"public Cat Object { get; } = new Cat();", "Animal", @"public ApiGenerator.ICat Object { get; } = new Cat();")]
        [DataRow(@"public Cat Object { get; set; } = new Cat();", "Animal", @"public ApiGenerator.ICat Object { get; set; } = new Cat();")]
        [DataRow(@"public Cat GetObject(NestedApiClass nested)", "Animal", @"public ApiGenerator.ICat GetObject(NestedApiClass nested)")]
        [DataRow(@"public Cat GetObject(NestedApiClass nested) => null;", "Animal", @"public ApiGenerator.ICat GetObject(NestedApiClass nested) => null;")]
        [DataRow(@"public Cat GetObject(NestedApiClass nested) => default;", "Animal", @"public ApiGenerator.ICat GetObject(NestedApiClass nested) => default;")]
        [DataRow(@"public Cat GetObject(NestedApiClass nested) { }", "Animal", @"public ApiGenerator.ICat GetObject(NestedApiClass nested) { }")]
        [DataRow(@"public Cat GetObject(NestedApiClass nested) { return null; }", "Animal", @"public ApiGenerator.ICat GetObject(NestedApiClass nested) { return null; }")]
        [DataRow(@"public Cat GetObject(NestedApiClass nested) { return default; }", "Animal", @"public ApiGenerator.ICat GetObject(NestedApiClass nested) { return default; }")]
        [DataRow(@"public Cat GetObject(NestedApiClass nested) => new Cat();", "Animal", @"public ApiGenerator.ICat GetObject(NestedApiClass nested) => new Cat();")]
        public void TestMethod_UseGeneratedInterfaces_ForGivenInput_ReturnsExpectedOutput(string testInput, string testClassName, string expectedResult)
        {
            // Arrange
            var separators = Writer.CSharpDelimiters;

            // Act
            var actualResult = Writer.UpdateReferences(testInput, testClassName, separators);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }
        #endregion

        #region "CleanApiAttributes" tests
        [DataTestMethod]
        [DynamicData(nameof(LinesAndContentCases), DynamicDataSourceType.Method)]
        public void TestMethod_CleanApiAttributes_ReturnsTrimmedContent(int[] lineNumbers, List<string> contentLines, string[] expectedResult)
        {
            // Act
            Writer.CleanApiAttributes(lineNumbers, contentLines);

            // Assert
            Assert.AreEqual(string.Join(string.Empty, expectedResult), string.Join(string.Empty, contentLines));
        }
        #endregion

        #region "CleanApiAttributes" cases
        public static IEnumerable<object[]> LinesAndContentCases()
        {
            // Plain text (to be ignored)
            yield return new object[] { new[] { 0 }, new List<string> { @"//ApiMember" }, new[] { @"//ApiMember" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"// ApiMember" }, new[] { @"// ApiMember" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"//[ApiMember" }, new[] { @"//[ApiMember" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"// [ApiMember" }, new[] { @"// [ApiMember" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"//ApiMember]" }, new[] { @"//ApiMember]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"//ApiMember] " }, new[] { @"//ApiMember] " } };
            yield return new object[] { new[] { 0 }, new List<string> { @"typeof(ApiMemberAttribute)" }, new[] { @"typeof(ApiMemberAttribute)" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"<see cref=""ApiMemberAttribute""/>)" }, new[] { @"<see cref=""ApiMemberAttribute""/>)" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"// An example of ""ApiMember"" attribute)" }, new[] { @"// An example of ""ApiMember"" attribute)" } };

            // Only API attributes
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiClass]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiMember]" }, Array.Empty<string>() };

            yield return new object[] { new[] { 0 }, new List<string> { @"[ ApiClass]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[  ApiClass]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiClass ]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiClass  ]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ ApiClass ]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[  ApiClass  ]" }, Array.Empty<string>() };

            yield return new object[] { new[] { 0 }, new List<string> { @"[ ApiMember]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[   ApiMember]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiMember ]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiMember   ]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ ApiMember ]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"[  ApiMember   ]" }, Array.Empty<string>() };

            yield return new object[] { new[] { 0 }, new List<string> { @" [ApiClass]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @" [ApiMember]" }, Array.Empty<string>() };

            yield return new object[] { new[] { 0 }, new List<string> { @"  [ApiClass]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"  [ApiMember]" }, Array.Empty<string>() };

            // Inline API attributes
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiClass]public void" }, new[] { @"public void" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiMember] public void" }, new[] { @" public void" } };  // Trimmed space
            yield return new object[] { new[] { 0 }, new List<string> { @"//[ApiClass]public void" }, new[] { @"//public void" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"//[ApiClass] public void" }, new[] { @"// public void" } };

            // Commented API attributes
            yield return new object[] { new[] { 0 }, new List<string> { @"//[ApiClass]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"//[ApiMember]" }, Array.Empty<string>() };

            yield return new object[] { new[] { 0 }, new List<string> { @"//[ ApiClass]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"//[ApiClass ]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"//[ ApiClass ]" }, Array.Empty<string>() };

            yield return new object[] { new[] { 0 }, new List<string> { @"//[ ApiMember]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"//[ApiMember ]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"//[ ApiMember ]" }, Array.Empty<string>() };

            yield return new object[] { new[] { 0 }, new List<string> { @"// [ApiClass]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"// [ApiMember]" }, Array.Empty<string>() };

            yield return new object[] { new[] { 0 }, new List<string> { @"//  [ApiClass]" }, Array.Empty<string>() };
            yield return new object[] { new[] { 0 }, new List<string> { @"//  [ApiMember]" }, Array.Empty<string>() };

            // Multiple attributes
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete][ApiClass]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ Obsolete][ApiClass]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete ][ApiClass]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ Obsolete ][ApiClass]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete] [ApiClass]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiClass][Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiClass] [Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiClass]  [Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember][ApiClass][Obsolete]" }, new[] { @"[DataMember][Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember] [ApiClass][Obsolete]" }, new[] { @"[DataMember][Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember][ApiClass] [Obsolete]" }, new[] { @"[DataMember][Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember] [ApiClass] [Obsolete]" }, new[] { @"[DataMember][Obsolete]" } };  // Double space

            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete][ApiMember]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete] [ApiMember]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiMember][Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiMember] [Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiMember]  [Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember][ApiMember][Obsolete]" }, new[] { @"[DataMember][Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember] [ApiMember][Obsolete]" }, new[] { @"[DataMember][Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember][ApiMember] [Obsolete]" }, new[] { @"[DataMember][Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember] [ApiMember] [Obsolete]" }, new[] { @"[DataMember][Obsolete]" } };  // Double space

            // Combined attributes
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete,ApiClass]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete, ApiClass]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiClass,Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiClass, Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember,ApiClass,Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };    // Double comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember, ApiClass,Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };   // Double comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember, ApiClass ,Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };  // Double comma, space before comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember , ApiClass,Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };  // Double comma, space before comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember,ApiClass, Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };   // Double comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember, ApiClass, Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };  // Double comma

            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete,ApiMember]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete, ApiMember]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiMember,Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[ApiMember, Obsolete]" }, new[] { @"[Obsolete]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember,ApiMember,Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };    // Double comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember, ApiMember,Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };   // Double comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember, ApiMember ,Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };  // Double comma, space before comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember , ApiMember,Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };  // Double comma, space before comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember,ApiMember, Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };   // Double comma
            yield return new object[] { new[] { 0 }, new List<string> { @"[DataMember, ApiMember, Obsolete]" }, new[] { @"[DataMember, Obsolete]" } };  // Double comma

            // Complex cases
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete][DataMember, ApiClass]" }, new[] { @"[Obsolete][DataMember]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete][DataMember,ApiClass]" }, new[] { @"[Obsolete][DataMember]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete] [DataMember, ApiClass]" }, new[] { @"[Obsolete][DataMember]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete] [DataMember,ApiClass]" }, new[] { @"[Obsolete][DataMember]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete, DataMember][ApiClass]" }, new[] { @"[Obsolete, DataMember]" } };
            yield return new object[] { new[] { 0 }, new List<string> { @"[Obsolete,DataMember][ApiClass]" }, new[] { @"[Obsolete, DataMember]" } };

            // Multiline content
            yield return new object[] { new[] { 5, 2 },
                Regex.Split($@"namespace XYZ{Environment.NewLine}" +
                            $@"{{{Environment.NewLine}" +
                            $@"    [ApiClass]{Environment.NewLine}" +
                            $@"    public class Spike{Environment.NewLine}" +
                            $@"    {{{Environment.NewLine}" +
                            $@"        [ApiMember]{Environment.NewLine}" +
                            $@"        public int Age {{ get; set; }}{Environment.NewLine}" +
                            $@"    }}{Environment.NewLine}" +
                             @"}", Environment.NewLine).ToList(),
                Regex.Split($@"namespace XYZ{Environment.NewLine}" +
                            $@"{{{Environment.NewLine}" +
                            $@"    public class Spike{Environment.NewLine}" +
                            $@"    {{{Environment.NewLine}" +
                            $@"        public int Age {{ get; set; }}{Environment.NewLine}" +
                            $@"    }}{Environment.NewLine}" +
                             @"}", $@"{Environment.NewLine}")
            };

            yield return new object[] { new[] { 5, 2 },
                Regex.Split($@"namespace XYZ{Environment.NewLine}" +
                            $@"{{{Environment.NewLine}" +
                            $@"    [ApiClass, DataContext]{Environment.NewLine}" +
                            $@"    public class Spike{Environment.NewLine}" +
                            $@"    {{{Environment.NewLine}" +
                            $@"        [ApiMember, Obsolete, DataMember]{Environment.NewLine}" +
                            $@"        public int Age {{ get; set; }}{Environment.NewLine}" +
                            $@"    }}{Environment.NewLine}" +
                             @"}", $@"{Environment.NewLine}").ToList(),
                Regex.Split($@"namespace XYZ{Environment.NewLine}" +
                            $@"{{{Environment.NewLine}" +
                             @"    [DataContext]" +
                            $@"    public class Spike{Environment.NewLine}" +
                            $@"    {{{Environment.NewLine}" +
                            $@"        [Obsolete, DataMember]{Environment.NewLine}" +
                            $@"        public int Age {{ get; set; }}{Environment.NewLine}" +
                            $@"    }}{Environment.NewLine}" +
                             @"}", $@"{Environment.NewLine}")
            };
        }
        #endregion
    }
}
