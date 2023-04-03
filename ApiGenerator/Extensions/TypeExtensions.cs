using System.Reflection;

namespace ApiGenerator.Logic.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>s or <see cref="MemberInfo"/>s.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Returns fully qualified name of concrete type: e.g., "Folder.Subfolder.Item" or generics name: e.g., "T".
        /// </summary>
        internal static string GetTypeOrGenericsName(this Type type)
        {
            return type.FullName ?? type.Name;  // NOTE: ".Name" is useful for generic types e.g., to get "T" name
        }
    }
}
