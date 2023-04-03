using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using ApiGenerator.Annotations;

namespace ApiGenerator.Examples.Classes.Dependencies
{
    #region Suppressing warnings in test Example class
    #pragma warning disable CS1591
    // ReSharper disable UnusedParameter.Global
    // ReSharper disable UnusedMember.Global
    #endregion

    // --------------------
    // Testing only methods
    // --------------------

    /// <summary>
    /// 
    /// </summary>
    [ApiClass, ExcludeFromCodeCoverage]
    public abstract class NestedApiClass
    {
        // -----------------------------------------
        // Testing collections with various of types
        // -----------------------------------------

        [ApiMember]
        public int[] GetArray() => Array.Empty<int>();  // Array
        
        [ApiMember]
        public int?[] GetNullableArray() => Array.Empty<int?>();  // Nullable Array

        [ApiMember]
        public IEnumerable<Point> GerEnumerable() => new List<Point>();  // IEnumerable interface
                                                                         // External object used as type
        [ApiMember]
        public IList<ulong> GetList() => new List<ulong>();              // IList interface
                                                                         // Atypical data type
        [ApiMember]
        public IDictionary<string, (Guid, bool?)> GetDictionary() => new Dictionary<string, (Guid, bool?)>();  // IDictionary interface
                                                                                                               // Tuple as a value
        [ApiMember]
        public Queue<ParallelApiClass> GetQueue() => new();  // Exotic concrete collection
                                                             // Internal API object used as type
        [ApiMember]
        public Tuple<string, (int, ArrayList)> GetTuple()  // ValueTuple with ArrayList nested in Tuple
        {
            return new Tuple<string, (int, ArrayList)>(@"Test", (1, new ArrayList()));
        }

        [ApiMember]
        public void SetValueTuple((int, bool) values)
        {
            throw new NotImplementedException();
        }
    }
}
