using System;
using System.Collections.Generic;
using ApiGenerator.Examples.Classes;
using ApiGenerator.Logic.Logic.FluentNamesBuilder;
using ApiGenerator.Logic.Workflow.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiGenerator.Tests.Logic.FluentNamesBuilder
{
    // ReSharper disable InconsistentNaming

    [TestClass]
    public class NamesBuilderTests
    {
        #region Test Settings
        private static readonly RegistrationSettings RegistrationSettings = new
        (
            @"Test",
            Array.Empty<string>(),
            @"Test",
            @"Test",
            @"Test"
        );

        private static readonly GenerationStrategy GenerationStrategy = new(RegistrationSettings)
        {
            UseFullyQualifiedNames = true
        };

        private static readonly GenerationSettings TestSettings = new
        (
            @"Test",
            @"Test",
            @"Test",
            @"Test",
            Array.Empty<string>(),
            Array.Empty<string>(),
            GenerationStrategy
        );
        #endregion

        #region GetGenericClassDefinitionName
        [DataTestMethod]
        [DataRow(false, "", "", null)]
        [DataRow(false, " ", "", null)]
        [DataRow(false, null, "", null)]
        [DataRow(false, "System.String", "System.String", typeof(string))]
        [DataRow(false, "System.Collections.Generic.List<System.String>", "System.Collections.Generic.List`1[System.String]", typeof(List<string>))]
        [DataRow(false, "System.Collections.Generic.IList<System.Int16>", "System.Collections.Generic.IList`1[System.Int16]", typeof(IList<short>))]
        [DataRow(false, "System.Collections.Generic.Dictionary<System.String, System.Int32>", "System.Collections.Generic.Dictionary`2[System.String, System.Int32]", typeof(Dictionary<string, int>))]
        [DataRow(true, "ApiGenerator.Examples.Classes.Spike<ApiGenerator.Examples.Classes.Item>", "ApiGenerator.Examples.Classes.Spike`1", null)]
        public void TestMethod_GetGenericClassDefinitionName(bool isGeneric, string testString, string expectedGenTypeName, Type expectedType)
        {
            // Act #1
            var actualGenTypeName = testString.GetGenericClassDefinitionName(isGeneric);

            // Assert #1
            Assert.AreEqual(expectedGenTypeName, actualGenTypeName);

            // Act #2 (Validation if the generic type name can be used to get the respective type)
            var actualType = Type.GetType(actualGenTypeName);

            // Assert #2
            Assert.AreEqual(expectedType, actualType);  // NOTE: Basic types can be identified with a simple "Type.GetType(string)"
                                                        // method. Types not placed in .NET libraries requires full assembly name
        }
        #endregion

        #region Build()
        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Spike`1")]
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface`2")]
        public void Test_BuilderMethods_Simplified_OriginalName_Build(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().OriginalName().Build();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Spike`1")]
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface`2")]
        public void Test_BuilderMethods_FullyQualified_OriginalName_Build(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().OriginalName().Build();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Renam3dSpike`1")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface`2")]
        public void Test_BuilderMethods_Simplified_CustomName_Build(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().CustomName().Build();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Renam3dSpike`1")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface`2")]
        public void Test_BuilderMethods_FullyQualified_CustomName_Build(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().CustomName().Build();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"IRenam3dSpike`1")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface`2")]
        public void Test_BuilderMethods_Simplified_InterfaceName_Build(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().InterfaceName().Build();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.IRenam3dSpike`1")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface`2")]
        public void Test_BuilderMethods_FullyQualified_InterfaceName_Build(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().InterfaceName().Build();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        #endregion

        #region WithoutGenerics()
        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Spike")]
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface")]
        public void Test_BuilderMethods_Simplified_OriginalName_WithoutGenerics(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().OriginalName().WithoutGenerics();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Renam3dSpike")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface")]
        public void Test_BuilderMethods_Simplified_CustomName_WithoutGenerics(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().CustomName().WithoutGenerics();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Spike")]
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface")]
        public void Test_BuilderMethods_FullyQualified_OriginalName_WithoutGenerics(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().OriginalName().WithoutGenerics();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Renam3dSpike")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface")]
        public void Test_BuilderMethods_FullyQualified_CustomName_WithoutGenerics(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().CustomName().WithoutGenerics();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"IRenam3dSpike")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface")]
        public void Test_BuilderMethods_Simplified_InterfaceName_WithoutGenerics(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().InterfaceName().WithoutGenerics();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.IRenam3dSpike")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface")]
        public void Test_BuilderMethods_FullyQualified_InterfaceName_WithoutGenerics(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().InterfaceName().WithoutGenerics();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        #endregion

        #region WithGenerics().Named()
        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Spike<Item>")]
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface<TValue, TModel>")]
        public void Test_BuilderMethods_Simplified_OriginalName_WithGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().OriginalName().WithGenerics().Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Renam3dSpike<Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface<TValue, TModel>")]
        public void Test_BuilderMethods_Simplified_CustomName_WithGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().CustomName().WithGenerics().Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"IRenam3dSpike<Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface<TValue, TModel>")]
        public void Test_BuilderMethods_Simplified_InterfaceName_WithGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().InterfaceName().WithGenerics().Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Spike<ApiGenerator.Examples.Classes.Item>")]
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface<TValue, TModel>")]
        public void Test_BuilderMethods_FullyQualified_OriginalName_WithGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().OriginalName().WithGenerics().Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Renam3dSpike<ApiGenerator.Examples.Classes.Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface<TValue, TModel>")]
        public void Test_BuilderMethods_FullyQualified_CustomName_WithGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().CustomName().WithGenerics().Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.IRenam3dSpike<ApiGenerator.Examples.Classes.Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface<TValue, TModel>")]
        public void Test_BuilderMethods_FullyQualified_InterfaceName_WithGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().InterfaceName().WithGenerics().Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        #endregion
        
        #region WithGenerics().Typed()
        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Spike<Item>")]
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface<Item, NestedApiClass>")]
        public void Test_BuilderMethods_Simplified_OriginalName_WithGenerics_Typed(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().OriginalName().WithGenerics().Typed(TestSettings);

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Renam3dSpike<Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface<Item, NestedApiClass>")]
        public void Test_BuilderMethods_Simplified_CustomName_WithGenerics_Typed(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().CustomName().WithGenerics().Typed(TestSettings);

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"IRenam3dSpike<Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface<Item, NestedApiClass>")]
        public void Test_BuilderMethods_Simplified_InterfaceName_WithGenerics_Typed(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().InterfaceName().WithGenerics().Typed(TestSettings);

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Spike<ApiGenerator.Examples.Classes.Item>")]
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface<ApiGenerator.Examples.Classes.Item, ApiGenerator.Examples.Classes.Dependencies.NestedApiClass>")]
        public void Test_BuilderMethods_FullyQualified_OriginalName_WithGenerics_Typed(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().OriginalName().WithGenerics().Typed(TestSettings);

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Renam3dSpike<ApiGenerator.Examples.Classes.Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface<ApiGenerator.Examples.Classes.Item, ApiGenerator.Examples.Classes.Dependencies.NestedApiClass>")]
        public void Test_BuilderMethods_FullyQualified_CustomName_WithGenerics_Typed(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().CustomName().WithGenerics().Typed(TestSettings);

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.IRenam3dSpike<ApiGenerator.Examples.Classes.Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface<ApiGenerator.Examples.Classes.Item, ApiGenerator.Examples.Classes.Dependencies.NestedApiClass>")]
        public void Test_BuilderMethods_FullyQualified_InterfaceName_WithGenerics_Typed(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().InterfaceName().WithGenerics().Typed(TestSettings);

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        #endregion
        
        #region WithOutGenerics_Named()
        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Spike<Item>")]
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface<out TValue, out TModel>")]  // Only for generics in interfaces out should be added
        public void Test_BuilderMethods_Simplified_OriginalName_WithOutGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().OriginalName().WithOutGenerics_Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"Renam3dSpike<Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface<out TValue, out TModel>")]  // Only for generics in interfaces out should be added
        public void Test_BuilderMethods_Simplified_CustomName_WithOutGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().CustomName().WithOutGenerics_Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"Item")]
        [DataRow(typeof(Spike<Item>), @"IRenam3dSpike<Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"IComplexInterface<out TValue, out TModel>")]  // Only for generics in interfaces out should be added
        public void Test_BuilderMethods_Simplified_InterfaceName_WithOutGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.Simplified().InterfaceName().WithOutGenerics_Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Spike<ApiGenerator.Examples.Classes.Item>")]
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface<out TValue, out TModel>")]  // Only for generics in interfaces out should be added
        public void Test_BuilderMethods_FullyQualified_OriginalName_WithOutGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().OriginalName().WithOutGenerics_Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Renam3dSpike<ApiGenerator.Examples.Classes.Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface<out TValue, out TModel>")]  // Only for generics in interfaces out should be added
        public void Test_BuilderMethods_FullyQualified_CustomName_WithOutGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().CustomName().WithOutGenerics_Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        
        [DataTestMethod]
        [DataRow(typeof(Item), @"ApiGenerator.Examples.Classes.Item")]
        [DataRow(typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.IRenam3dSpike<ApiGenerator.Examples.Classes.Item>")]  // Only this name should be customized
        [DataRow(typeof(ISimpleInterface), @"ApiGenerator.Examples.Classes.ISimpleInterface")]
        [DataRow(typeof(IComplexInterface<,>), @"ApiGenerator.Examples.Classes.IComplexInterface<out TValue, out TModel>")]  // Only for generics in interfaces out should be added
        public void Test_BuilderMethods_FullyQualified_InterfaceName_WithOutGenerics_Named(Type testType, string expectedName)
        {
            // Act
            var actualName = testType.FullyQualified().InterfaceName().WithOutGenerics_Named();

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        #endregion

        #region Overriding fully qualified property
        [TestMethod]
        public void Test_BuilderMethods_OverrideSimplifiedName_ForGenerics()
        {
            // Act
            var actualName = typeof(Spike<Item>).Simplified().OriginalName().WithGenerics_FullyQualified().Typed(TestSettings);

            // Assert
            Assert.AreEqual(@"Spike<ApiGenerator.Examples.Classes.Item>", actualName);
        }
        #endregion
    }
}
