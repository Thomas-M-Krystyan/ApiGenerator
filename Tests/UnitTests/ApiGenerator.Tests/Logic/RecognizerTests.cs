using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ApiGenerator.Examples.Classes;
using ApiGenerator.Logic.Logic;
using ApiGenerator.Logic.Workflow.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#nullable enable

namespace ApiGenerator.Tests.Logic
{
    // ReSharper disable InconsistentNaming

    [TestClass]
    public class RecognizerTests
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

        private static readonly GenerationSettings TestSettings_FullyQualifiedNames = new
        (
            @"TestProject",
            @"TestProject",
            @"Test",
            @"Test",
            Array.Empty<string>(),
            Array.Empty<string>(),
            new GenerationStrategy(RegistrationSettings)
            {
                UseFullyQualifiedNames = true
            }
        );
        
        private static readonly GenerationSettings TestSettings_SimplifiedNames = new
        (
            @"TestProject",
            @"TestProject",
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

        #region "TryGetAliasName" tests
        [DataTestMethod]
        [DynamicData(nameof(AliasesTestCases), DynamicDataSourceType.Method)]
        [DynamicData(nameof(FullyQualifiedNamesTestCases), DynamicDataSourceType.Method)]
        public async Task TestMethod_TryGetAliasName_WithFullyQualifiedNames_ReturnsExpectedTypeName(Type testType, string expectedAlias)
        {
            // Act
            var actualAlias = await testType.TryGetAliasName(TestSettings_FullyQualifiedNames);

            // Assert
            Assert.AreEqual(expectedAlias, actualAlias, @$"Converted alias for ""{testType}"" is different than expected");
        }

        [DataTestMethod]
        [DynamicData(nameof(AliasesTestCases), DynamicDataSourceType.Method)]
        [DynamicData(nameof(SimplifiedNamesTestCases), DynamicDataSourceType.Method)]
        public async Task TestMethod_TryGetAliasName_WithSimplifiedNames_ReturnsExpectedTypeName(Type testType, string expectedAlias)
        {
            // Act
            var actualAlias = await testType.TryGetAliasName(TestSettings_SimplifiedNames);

            // Assert
            Assert.AreEqual(expectedAlias, actualAlias, @$"Converted alias for ""{testType}"" is different than expected");
        }
        #endregion

        #region "TryGetAliasName" cases
        public static IEnumerable<object[]> AliasesTestCases()
        {
            // Value types
            yield return new object[] { typeof(byte), @"byte" };
            yield return new object[] { typeof(sbyte), @"sbyte" };
            yield return new object[] { typeof(short), @"short" };
            yield return new object[] { typeof(ushort), @"ushort" };
            yield return new object[] { typeof(int), @"int" };
            yield return new object[] { typeof(uint), @"uint" };
            yield return new object[] { typeof(long), @"long" };
            yield return new object[] { typeof(ulong), @"ulong" };
            yield return new object[] { typeof(float), @"float" };
            yield return new object[] { typeof(double), @"double" };
            yield return new object[] { typeof(decimal), @"decimal" };
            yield return new object[] { typeof(bool), @"bool" };
            yield return new object[] { typeof(char), @"char" };
            yield return new object[] { typeof(void), @"void" };  // Void

            // Reference types
            yield return new object[] { typeof(object), @"object" };
            yield return new object[] { typeof(string), @"string" };

            // Nullable value types
            yield return new object[] { typeof(byte?), @"byte?" };
            yield return new object[] { typeof(sbyte?), @"sbyte?" };
            yield return new object[] { typeof(short?), @"short?" };
            yield return new object[] { typeof(ushort?), @"ushort?" };
            yield return new object[] { typeof(int?), @"int?" };
            yield return new object[] { typeof(uint?), @"uint?" };
            yield return new object[] { typeof(long?), @"long?" };
            yield return new object[] { typeof(ulong?), @"ulong?" };
            yield return new object[] { typeof(float?), @"float?" };
            yield return new object[] { typeof(double?), @"double?" };
            yield return new object[] { typeof(decimal?), @"decimal?" };
            yield return new object[] { typeof(bool?), @"bool?" };
            yield return new object[] { typeof(char?), @"char?" };

            // Arrays
            yield return new object[] { typeof(byte[]), @"byte[]" };
            yield return new object[] { typeof(sbyte[]), @"sbyte[]" };
            yield return new object[] { typeof(short[]), @"short[]" };
            yield return new object[] { typeof(ushort[]), @"ushort[]" };
            yield return new object[] { typeof(int[]), @"int[]" };
            yield return new object[] { typeof(uint[]), @"uint[]" };
            yield return new object[] { typeof(long[]), @"long[]" };
            yield return new object[] { typeof(ulong[]), @"ulong[]" };
            yield return new object[] { typeof(float[]), @"float[]" };
            yield return new object[] { typeof(double[]), @"double[]" };
            yield return new object[] { typeof(decimal[]), @"decimal[]" };
            yield return new object[] { typeof(bool[]), @"bool[]" };
            yield return new object[] { typeof(char[]), @"char[]" };
            yield return new object[] { typeof(string[]), @"string[]" };
            yield return new object[] { typeof(object[]), @"object[]" };
            
            // Multidimensional arrays
            yield return new object[] { typeof(byte[,]), @"byte[,]" };
            yield return new object[] { typeof(byte[,,]), @"byte[,,]" };

            // Nullable arrays
            yield return new object[] { typeof(byte?[]), @"byte?[]" };
            yield return new object[] { typeof(sbyte?[]), @"sbyte?[]" };
            yield return new object[] { typeof(short?[]), @"short?[]" };
            yield return new object[] { typeof(ushort?[]), @"ushort?[]" };
            yield return new object[] { typeof(int?[]), @"int?[]" };
            yield return new object[] { typeof(uint?[]), @"uint?[]" };
            yield return new object[] { typeof(long?[]), @"long?[]" };
            yield return new object[] { typeof(ulong?[]), @"ulong?[]" };
            yield return new object[] { typeof(float?[]), @"float?[]" };
            yield return new object[] { typeof(double?[]), @"double?[]" };
            yield return new object[] { typeof(decimal?[]), @"decimal?[]" };
            yield return new object[] { typeof(bool?[]), @"bool?[]" };
            yield return new object[] { typeof(char?[]), @"char?[]" };
            yield return new object[] { typeof(string?[]), @"string[]" };  // Nullable reference type "string?" is redundant in .NET
            yield return new object[] { typeof(object?[]), @"object[]" };  // Nullable reference type "object?" is redundant in .NET
        }

        public static IEnumerable<object[]> FullyQualifiedNamesTestCases()
        {
            // Lists
            yield return new object[] { typeof(List<>), @"System.Collections.Generic.List<object>" };  // Get name from a base type
            yield return new object[] { typeof(List<Item>), @"System.Collections.Generic.List<ApiGenerator.Examples.Classes.Item>" };
            yield return new object[] { typeof(List<int>), @"System.Collections.Generic.List<int>" };
            yield return new object[] { typeof(List<string>), @"System.Collections.Generic.List<string>" };
            yield return new object[] { typeof(List<object>), @"System.Collections.Generic.List<object>" };
            yield return new object[] { typeof(List<int?>), @"System.Collections.Generic.List<int?>" };
            yield return new object[] { typeof(List<string?>), @"System.Collections.Generic.List<string>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(List<object?>), @"System.Collections.Generic.List<object>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(List<int[]>), @"System.Collections.Generic.List<int[]>" };
            yield return new object[] { typeof(List<string[]>), @"System.Collections.Generic.List<string[]>" };
            yield return new object[] { typeof(List<object[]>), @"System.Collections.Generic.List<object[]>" };
            yield return new object[] { typeof(List<int?[]>), @"System.Collections.Generic.List<int?[]>" };
            yield return new object[] { typeof(List<string?[]>), @"System.Collections.Generic.List<string[]>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(List<object?[]>), @"System.Collections.Generic.List<object[]>" };  // Nullable reference type redundancy simplified

            // Dictionaries
            yield return new object[] { typeof(Dictionary<,>), @"System.Collections.Generic.Dictionary<object, object>" };  // Get names from base types
            yield return new object[] { typeof(Dictionary<int, byte>), @"System.Collections.Generic.Dictionary<int, byte>" };
            yield return new object[] { typeof(Dictionary<int, Item>), @"System.Collections.Generic.Dictionary<int, ApiGenerator.Examples.Classes.Item>" };
            yield return new object[] { typeof(Dictionary<string, byte>), @"System.Collections.Generic.Dictionary<string, byte>" };
            yield return new object[] { typeof(Dictionary<object, byte>), @"System.Collections.Generic.Dictionary<object, byte>" };
            yield return new object[] { typeof(Dictionary<int?, byte>), @"System.Collections.Generic.Dictionary<int?, byte>" };
            yield return new object[] { typeof(Dictionary<string?, byte>), @"System.Collections.Generic.Dictionary<string, byte>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(Dictionary<object?, byte>), @"System.Collections.Generic.Dictionary<object, byte>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(Dictionary<int[], byte>), @"System.Collections.Generic.Dictionary<int[], byte>" };
            yield return new object[] { typeof(Dictionary<string[], byte>), @"System.Collections.Generic.Dictionary<string[], byte>" };
            yield return new object[] { typeof(Dictionary<object[], byte>), @"System.Collections.Generic.Dictionary<object[], byte>" };
            yield return new object[] { typeof(Dictionary<int?[], byte>), @"System.Collections.Generic.Dictionary<int?[], byte>" };
            yield return new object[] { typeof(Dictionary<string?[], byte>), @"System.Collections.Generic.Dictionary<string[], byte>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(Dictionary<object?[], byte>), @"System.Collections.Generic.Dictionary<object[], byte>" };  // Nullable reference type redundancy simplified

            // Interfaces
            yield return new object[] { typeof(IEnumerable<string>), @"System.Collections.Generic.IEnumerable<string>" };
            yield return new object[] { typeof(ICollection<string>), @"System.Collections.Generic.ICollection<string>" };
            yield return new object[] { typeof(IList<int>), @"System.Collections.Generic.IList<int>" };
            yield return new object[] { typeof(IDictionary<int, byte>), @"System.Collections.Generic.IDictionary<int, byte>" };
            yield return new object[] { typeof(IReadOnlyCollection<string>), @"System.Collections.Generic.IReadOnlyCollection<string>" };
            yield return new object[] { typeof(IReadOnlyList<string>), @"System.Collections.Generic.IReadOnlyList<string>" };
            yield return new object[] { typeof(IReadOnlyDictionary<string, string>), @"System.Collections.Generic.IReadOnlyDictionary<string, string>" };
            
            // Rarer collections
            yield return new object[] { typeof(KeyValuePair<string, string>), @"System.Collections.Generic.KeyValuePair<string, string>" };
            yield return new object[] { typeof(LinkedList<string>), @"System.Collections.Generic.LinkedList<string>" };
            yield return new object[] { typeof(Queue<string>), @"System.Collections.Generic.Queue<string>" };
            yield return new object[] { typeof(Stack<string>), @"System.Collections.Generic.Stack<string>" };
            yield return new object[] { typeof(HashSet<string>), @"System.Collections.Generic.HashSet<string>" };
            yield return new object[] { typeof(SortedList<string, string>), @"System.Collections.Generic.SortedList<string, string>" };
            yield return new object[] { typeof(SortedDictionary<string, string>), @"System.Collections.Generic.SortedDictionary<string, string>" };
            yield return new object[] { typeof(SortedSet<string>), @"System.Collections.Generic.SortedSet<string>" };
            yield return new object[] { typeof(ConcurrentQueue<string>), @"System.Collections.Concurrent.ConcurrentQueue<string>" };
            yield return new object[] { typeof(ConcurrentStack<string>), @"System.Collections.Concurrent.ConcurrentStack<string>" };
            yield return new object[] { typeof(ConcurrentDictionary<string, string>), @"System.Collections.Concurrent.ConcurrentDictionary<string, string>" };
            yield return new object[] { typeof(ReadOnlyCollection<string>), @"System.Collections.ObjectModel.ReadOnlyCollection<string>" };
            yield return new object[] { typeof(ArrayList), @"System.Collections.ArrayList" };

            // Tuples (old: "System.Tuple")
            yield return new object[] { typeof(Tuple<>), @"System.Tuple<object>" };  // Get name from a base type
            yield return new object[] { typeof(Tuple<byte>), @"System.Tuple<byte>" };
            yield return new object[] { typeof(Tuple<byte, short>), @"System.Tuple<byte, short>" };
            yield return new object[] { typeof(Tuple<byte, short, int?>), @"System.Tuple<byte, short, int?>" };
            yield return new object[] { typeof(Tuple<byte, Item>), @"System.Tuple<byte, ApiGenerator.Examples.Classes.Item>" };
            yield return new object[] { typeof(Tuple<byte, Item, int?>), @"System.Tuple<byte, ApiGenerator.Examples.Classes.Item, int?>" };
            yield return new object[] { typeof(Tuple<byte, short, int, long, float, double, decimal, bool>), @"System.Tuple<byte, short, int, long, float, double, decimal, bool>" };

            // Tuples (new: "System.ValueTuple")
            yield return new object[] { typeof((byte, short)), @"(byte, short)" };
            yield return new object[] { typeof((byte, short)?), @"(byte, short)?" };  // Nullable ValueTuple type
            yield return new object[] { typeof((byte, short, int?)), @"(byte, short, int?)" };
            yield return new object[] { typeof((byte, Item)), @"(byte, ApiGenerator.Examples.Classes.Item)" };
            yield return new object[] { typeof((byte, Item, int?)), @"(byte, ApiGenerator.Examples.Classes.Item, int?)" };
            yield return new object[] { typeof((byte, short, int, long, float, double, decimal)), @"(byte, short, int, long, float, double, decimal)" };
            yield return new object[] { typeof((byte, short, int, long, float, double, decimal, bool)), @"(byte, short, int, long, float, double, decimal, bool)" };  // ValueTuple with more than 7 arguments

            // Complex cases
            yield return new object[] { typeof(Dictionary<string, List<int>>), @"System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>" };
            yield return new object[] { typeof(Dictionary<string, List<(int, short?)>>), @"System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<(int, short?)>>" };
            yield return new object[] { typeof(List<List<List<int>>>), @"System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<int>>>" };
            yield return new object[] { typeof(Tuple<int, (long, uint)?>), @"System.Tuple<int, (long, uint)?>" };
            yield return new object[] { typeof((byte, (short, int))), @"(byte, (short, int))" };
            yield return new object[] { typeof((byte, (short, (int, long)))), @"(byte, (short, (int, long)))" };
            yield return new object[] { typeof((byte, (short, (sbyte, byte, short, ushort, int, uint, long, ulong,
                                                               float, double, decimal, bool, char, string, object,
                                                               System.Drawing.Point, System.Drawing.Size, List<Item>)))),
                @"(byte, (short, (sbyte, byte, short, ushort, int, uint, long, ulong, float, double, decimal, bool, char, string, object, System.Drawing.Point, System.Drawing.Size, System.Collections.Generic.List<ApiGenerator.Examples.Classes.Item>)))" };

            yield return new object[] { typeof(Item), @"ApiGenerator.Examples.Classes.Item" };
            yield return new object[] { typeof(Spike<Item>), @"ApiGenerator.Examples.Classes.Renam3dSpike<ApiGenerator.Examples.Classes.Item>" };
        }

        public static IEnumerable<object[]> SimplifiedNamesTestCases()
        {
            // Lists
            yield return new object[] { typeof(List<>), @"List<object>" };  // Get name from a base type
            yield return new object[] { typeof(List<Item>), @"List<Item>" };
            yield return new object[] { typeof(List<int>), @"List<int>" };
            yield return new object[] { typeof(List<string>), @"List<string>" };
            yield return new object[] { typeof(List<object>), @"List<object>" };
            yield return new object[] { typeof(List<int?>), @"List<int?>" };
            yield return new object[] { typeof(List<string?>), @"List<string>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(List<object?>), @"List<object>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(List<int[]>), @"List<int[]>" };
            yield return new object[] { typeof(List<string[]>), @"List<string[]>" };
            yield return new object[] { typeof(List<object[]>), @"List<object[]>" };
            yield return new object[] { typeof(List<int?[]>), @"List<int?[]>" };
            yield return new object[] { typeof(List<string?[]>), @"List<string[]>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(List<object?[]>), @"List<object[]>" };  // Nullable reference type redundancy simplified

            // Dictionaries
            yield return new object[] { typeof(Dictionary<,>), @"Dictionary<object, object>" };  // Get names from base types
            yield return new object[] { typeof(Dictionary<int, byte>), @"Dictionary<int, byte>" };
            yield return new object[] { typeof(Dictionary<int, Item>), @"Dictionary<int, Item>" };
            yield return new object[] { typeof(Dictionary<string, byte>), @"Dictionary<string, byte>" };
            yield return new object[] { typeof(Dictionary<object, byte>), @"Dictionary<object, byte>" };
            yield return new object[] { typeof(Dictionary<int?, byte>), @"Dictionary<int?, byte>" };
            yield return new object[] { typeof(Dictionary<string?, byte>), @"Dictionary<string, byte>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(Dictionary<object?, byte>), @"Dictionary<object, byte>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(Dictionary<int[], byte>), @"Dictionary<int[], byte>" };
            yield return new object[] { typeof(Dictionary<string[], byte>), @"Dictionary<string[], byte>" };
            yield return new object[] { typeof(Dictionary<object[], byte>), @"Dictionary<object[], byte>" };
            yield return new object[] { typeof(Dictionary<int?[], byte>), @"Dictionary<int?[], byte>" };
            yield return new object[] { typeof(Dictionary<string?[], byte>), @"Dictionary<string[], byte>" };  // Nullable reference type redundancy simplified
            yield return new object[] { typeof(Dictionary<object?[], byte>), @"Dictionary<object[], byte>" };  // Nullable reference type redundancy simplified

            // Interfaces
            yield return new object[] { typeof(IEnumerable<string>), @"IEnumerable<string>" };
            yield return new object[] { typeof(ICollection<string>), @"ICollection<string>" };
            yield return new object[] { typeof(IList<int>), @"IList<int>" };
            yield return new object[] { typeof(IDictionary<int, byte>), @"IDictionary<int, byte>" };
            yield return new object[] { typeof(IReadOnlyCollection<string>), @"IReadOnlyCollection<string>" };
            yield return new object[] { typeof(IReadOnlyList<string>), @"IReadOnlyList<string>" };
            yield return new object[] { typeof(IReadOnlyDictionary<string, string>), @"IReadOnlyDictionary<string, string>" };
            
            // Rarer collections
            yield return new object[] { typeof(KeyValuePair<string, string>), @"KeyValuePair<string, string>" };
            yield return new object[] { typeof(LinkedList<string>), @"LinkedList<string>" };
            yield return new object[] { typeof(Queue<string>), @"Queue<string>" };
            yield return new object[] { typeof(Stack<string>), @"Stack<string>" };
            yield return new object[] { typeof(HashSet<string>), @"HashSet<string>" };
            yield return new object[] { typeof(SortedList<string, string>), @"SortedList<string, string>" };
            yield return new object[] { typeof(SortedDictionary<string, string>), @"SortedDictionary<string, string>" };
            yield return new object[] { typeof(SortedSet<string>), @"SortedSet<string>" };
            yield return new object[] { typeof(ConcurrentQueue<string>), @"ConcurrentQueue<string>" };
            yield return new object[] { typeof(ConcurrentStack<string>), @"ConcurrentStack<string>" };
            yield return new object[] { typeof(ConcurrentDictionary<string, string>), @"ConcurrentDictionary<string, string>" };
            yield return new object[] { typeof(ReadOnlyCollection<string>), @"ReadOnlyCollection<string>" };
            yield return new object[] { typeof(ArrayList), @"ArrayList" };

            // Tuples (old: "System.Tuple")
            yield return new object[] { typeof(Tuple<>), @"Tuple<object>" };  // Get name from a base type
            yield return new object[] { typeof(Tuple<byte>), @"Tuple<byte>" };
            yield return new object[] { typeof(Tuple<byte, short>), @"Tuple<byte, short>" };
            yield return new object[] { typeof(Tuple<byte, short, int?>), @"Tuple<byte, short, int?>" };
            yield return new object[] { typeof(Tuple<byte, Item>), @"Tuple<byte, Item>" };
            yield return new object[] { typeof(Tuple<byte, Item, int?>), @"Tuple<byte, Item, int?>" };
            yield return new object[] { typeof(Tuple<byte, short, int, long, float, double, decimal, bool>), @"Tuple<byte, short, int, long, float, double, decimal, bool>" };

            // Tuples (new: "System.ValueTuple")
            yield return new object[] { typeof((byte, short)), @"(byte, short)" };
            yield return new object[] { typeof((byte, short)?), @"(byte, short)?" };  // Nullable ValueTuple type
            yield return new object[] { typeof((byte, short, int?)), @"(byte, short, int?)" };
            yield return new object[] { typeof((byte, Item)), @"(byte, Item)" };
            yield return new object[] { typeof((byte, Item, int?)), @"(byte, Item, int?)" };
            yield return new object[] { typeof((byte, short, int, long, float, double, decimal)), @"(byte, short, int, long, float, double, decimal)" };
            yield return new object[] { typeof((byte, short, int, long, float, double, decimal, bool)), @"(byte, short, int, long, float, double, decimal, bool)" };  // ValueTuple with more than 7 arguments

            // Complex cases
            yield return new object[] { typeof(Dictionary<string, List<int>>), @"Dictionary<string, List<int>>" };
            yield return new object[] { typeof(Dictionary<string, List<(int, short?)>>), @"Dictionary<string, List<(int, short?)>>" };
            yield return new object[] { typeof(List<List<List<int>>>), @"List<List<List<int>>>" };
            yield return new object[] { typeof(Tuple<int, (long, uint)?>), @"Tuple<int, (long, uint)?>" };
            yield return new object[] { typeof((byte, (short, int))), @"(byte, (short, int))" };
            yield return new object[] { typeof((byte, (short, (int, long)))), @"(byte, (short, (int, long)))" };
            yield return new object[] { typeof((byte, (short, (sbyte, byte, short, ushort, int, uint, long, ulong,
                                                               float, double, decimal, bool, char, string, object,
                                                               System.Drawing.Point, System.Drawing.Size, List<Item>)))),
                @"(byte, (short, (sbyte, byte, short, ushort, int, uint, long, ulong, float, double, decimal, bool, char, string, object, Point, Size, List<Item>)))" };

            yield return new object[] { typeof(Item), @"Item" };
            yield return new object[] { typeof(Spike<Item>), @"Renam3dSpike<Item>" };
        }
        #endregion
    }
}
