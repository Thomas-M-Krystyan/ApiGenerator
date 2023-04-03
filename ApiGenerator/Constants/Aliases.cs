using System.Collections;

namespace ApiGenerator.Logic.Constants
{
    internal static class Aliases
    {
        /// <summary>
        /// The map of .NET to alias types.
        /// </summary>
        internal static IDictionary<Type, string> Supported { get; } = new Dictionary<Type, string>
        {
            // Value types
            { typeof(byte), @"byte" },
            { typeof(sbyte), @"sbyte" },
            { typeof(short), @"short" },
            { typeof(ushort), @"ushort" },
            { typeof(int), @"int" },
            { typeof(uint), @"uint" },
            { typeof(long), @"long" },
            { typeof(ulong), @"ulong" },
            { typeof(float), @"float" },
            { typeof(double), @"double" },
            { typeof(decimal), @"decimal" },
            { typeof(bool), @"bool" },
            { typeof(char), @"char" },
            
            { typeof(string), @"string" },
            { typeof(object), @"object" },

            { typeof(void), @"void" },
            
            { typeof(ArrayList), @"ArrayList" }
        };
    }
}
