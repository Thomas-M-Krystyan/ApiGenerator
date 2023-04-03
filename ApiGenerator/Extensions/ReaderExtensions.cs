// Reading XML summaries for [T:] and [P:] members:
// https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/october/csharp-accessing-xml-documentation-via-reflection

#nullable enable

using System.Reflection;
using System.Text.RegularExpressions;
using ApiGenerator.Logic.Logic;

namespace ApiGenerator.Logic.Extensions
{
    internal static class Extensions
    {
        /// <summary>
        /// Gets the documentation for the specific member from .xml file.
        /// </summary>
        internal static string GetDocumentation(this MemberInfo memberInfo)
        {
            // Classes
            if (memberInfo.MemberType.HasFlag(MemberTypes.TypeInfo) ||
                memberInfo.MemberType.HasFlag(MemberTypes.NestedType))
            {
                return ((TypeInfo)memberInfo).GetDocumentation();
            }

            // Properties
            if (memberInfo.MemberType.HasFlag(MemberTypes.Property))
            {
                return ((PropertyInfo)memberInfo).GetDocumentation();
            }

            // Methods
            if (memberInfo.MemberType.HasFlag(MemberTypes.Method))
            {
                return ((MethodInfo)memberInfo).GetDocumentation();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the documentation of classes.
        /// </summary>
        private static string GetDocumentation(this Type type)
        {
            var key = @"T:" + XmlDocumentationKeyHelper($@"{type.FullName}", null);

            Reader.LoadedXmlDocumentation.TryGetValue(key, out var documentation);

            return documentation ?? string.Empty;
        }

        /// <summary>
        /// Gets the documentation of properties.
        /// </summary>
        private static string GetDocumentation(this PropertyInfo propertyInfo)
        {
            var key = @"P:" + XmlDocumentationKeyHelper(
                $@"{propertyInfo.DeclaringType?.FullName}",  // Namespace
                propertyInfo.Name);  // Name of the property

            Reader.LoadedXmlDocumentation.TryGetValue(key, out var documentation);

            return documentation ?? string.Empty;
        }

        /// <summary>
        /// Gets the documentation of methods.
        /// </summary>
        private static string GetDocumentation(this MethodBase methodInfo)
        {
            var key = @"M:" + XmlDocumentationKeyHelper(
                $@"{methodInfo.DeclaringType?.FullName}",  // Namespace
                $@"{methodInfo.Name}{GetParameters(methodInfo)}");  // Name of the method

            Reader.LoadedXmlDocumentation.TryGetValue(key, out var documentation);

            return documentation ?? string.Empty;
        }

        private static string GetParameters(MethodBase methodInfo)
        {
            var methodParameters = methodInfo.GetParameters();

            return methodParameters.Length == 0
                // #1 No parameters: GetName() => "GetName"
                ? string.Empty
                // #2 With parameters: GetName(int age, string surname) => "GetName(System.Int32,System.String)"
                : $@"({string.Join(@",", methodParameters.Select(GetParameterType))})";
        }

        private static string GetParameterType(ParameterInfo parameterInfo)
        {
            var parameterType = parameterInfo.ParameterType.ToString();

            var isNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null;
            if (isNullable)
            {
                // Replaces: "System.Nullable`1[System.Boolean]" with "System.Nullable{System.Boolean}"
                parameterType = parameterType
                    .Replace(@"`1[", @"{")
                    .Replace(@"]", @"}");
            }

            // Replaces: "Spike+Animal" with "Spike.Animal"
            return parameterType.Replace(@"+", @".");
        }

        /// <summary>
        /// Helper method to format the key strings.
        /// </summary>
        private static string XmlDocumentationKeyHelper(string typeFullNameString, string? memberNameString)
        {
            var key = Regex.Replace(typeFullNameString, @"\[.*\]", string.Empty)
                .Replace('+', '.');

            if (memberNameString != null)
            {
                key += @"." + memberNameString;
            }

            return key;
        }
    }
}
