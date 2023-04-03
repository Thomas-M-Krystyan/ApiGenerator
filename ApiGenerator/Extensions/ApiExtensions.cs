#nullable enable

using System.Reflection;
using ApiGenerator.Annotations;

namespace ApiGenerator.Logic.Extensions
{
    /// <summary>
    /// Extension methods used to work with [<see cref="ApiMemberAttribute"/>] annotation.
    /// </summary>
    internal static class ApiExtensions
    {
        /// <summary>
        /// Recognizes [<see cref="ApiClassAttribute"/>] annotation used for the provided <see langword="class"/>.
        /// </summary>
        internal static bool IsApiClass(this Type type)
        {
            return type.IsClass && Attribute.IsDefined(type, typeof(ApiClassAttribute));
        }

        /// <summary>
        /// Recognizes [<see cref="ApiMemberAttribute"/>] annotation used for the provided member.
        /// </summary>
        internal static bool IsApiMember(this MemberInfo memberInfo)
        {
            return Attribute.IsDefined(memberInfo, typeof(ApiMemberAttribute));
        }

        /// <summary>
        /// Gets the value of custom name from [<see cref="ApiMemberAttribute"/>] annotation.
        /// </summary>
        internal static string GetCustomName(this Type type)
        {
            var typeName = type.GetApiClassAnnotation()?.CustomName!;

            return string.IsNullOrEmpty(typeName)
                ? type.Name  // For case: "[ApiClass] public class Person"           => "Person"
                : typeName;  // For case: "[ApiClass("Human")] public class Person"  => "Human"                                
        }

        /// <summary>
        /// Gets the value of registration indicator from [<see cref="ApiMemberAttribute"/>] annotation.
        /// </summary>
        internal static bool GetShouldBeRegistered(this Type type)
        {
            return type.GetApiClassAnnotation()?.ToBeRegistered ?? false;
        }

        /// <summary>
        /// Gets the value of interface dependencies from [<see cref="ApiMemberAttribute"/>] annotation.
        /// </summary>
        internal static IEnumerable<Type> GetDerivedFrom(this Type type)
        {
            return type.GetApiClassAnnotation()?.DerivedFrom
                ?? Array.Empty<Type>();  // If the provided type is not an API class candidate
        }

        #region Helper methods
        private static ApiClassAttribute? GetApiClassAnnotation(this MemberInfo member)
        {
            return member.GetCustomAttribute<ApiClassAttribute>();
        }
        #endregion
    }
}
