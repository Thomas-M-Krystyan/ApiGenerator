using System.Collections;
using System.Runtime.CompilerServices;
using ApiGenerator.Logic.Constants;
using ApiGenerator.Logic.Logic.FluentNamesBuilder;
using ApiGenerator.Logic.Workflow.Models;

namespace ApiGenerator.Logic.Logic
{
    /// <summary>
    /// Restores (return and parameters) <see cref="Type"/>s declarations by recognizing their aliases.
    /// </summary>
    internal static class Recognizer
    {
        /// <summary>
        /// Retrieves simplified alias name instead of .NET name from a certain type name:
        /// </summary>
        /// <returns>
        ///   <code>
        ///     "System.String" => "<see langword="string"/>"
        ///   </code>
        ///   <para>
        ///     Otherwise, <see cref="string.Empty"/> if alias could not be obtained.
        ///   </para>
        /// </returns>
        internal static async Task<string> TryGetAliasName(this string typeName, GenerationSettings settings, bool? overridenFullName = null)
        {
            // 1st attempt
            var type = Type.GetType(typeName);  // Simple types, easy to guess: "string", "bool", "int".

            if (type == null)
            {
                // 2nd attempt
                type = Type.GetType(typeName.GetGenericClassDefinitionName(false));  // Specified parameters: "Spike<Item>"

                if (type == null)
                {
                    // 3rd attempt
                    type = Type.GetType(typeName.GetGenericClassDefinitionName());  // Non-specified generics: "Spike<T>"

                    if (type == null)
                    {
                        return typeName;
                    }
                }
            }

            return await TryGetAliasName(type, settings, overridenFullName);
        }

        /// <summary>
        /// Retrieves simplified alias name instead of .NET name from a certain <see cref="Type"/>(s):
        /// </summary>
        /// <returns>
        ///   <code>
        ///     System.String => "<see langword="string"/>"
        ///   </code>
        ///   <para>
        ///     Otherwise, <see cref="string.Empty"/> if alias could not be obtained.
        ///   </para>
        /// </returns>
        internal static async Task<string> TryGetAliasName(this Type type, GenerationSettings settings, bool? overridenFullName = null)
        {
            var useFullName = overridenFullName ?? settings.Strategy.UseFullyQualifiedNames;

            // Retrieve alias for a given type (if registered)
            if (Aliases.Supported.TryGetValue(type, out var alias))
            {
                return type.IsArrayList() ? alias.GetArrayList(settings) : alias;
            }

            // Handle "Nullable`1" cases
            (bool IsSuccess, string Alias) result = await type.TryGetNullable(settings);
            
            if (result.IsSuccess)
            {
                return result.Alias;
            }

            // Handle collections and KeyValuePair
            if (type.IsCollection() || type.IsKeyValuePair())
            {
                // Handle arrays
                result = await type.TryGetArray(settings);

                if (result.IsSuccess)
                {
                    return result.Alias;
                }

                // Handle the remaining "System.Collections.IEnumerable`" and "KeyValuePair<TKey, TValue>" cases
                return useFullName ? type.FullyQualified().OriginalName().WithGenerics().Typed(settings)
                                   : type.Simplified().OriginalName().WithGenerics().Typed(settings);
            }

            // Handle class with generics "ExampleClass<T, V>" cases
            result = type.TryGetClassWithGenerics(settings);
            
            if (result.IsSuccess)
            {
                return result.Alias;
            }

            // Handle tuples
            if (type.IsTuple())
            {
                // Handle "System.Tuple`" cases
                if (type.IsSystemTuple())
                {
                    return useFullName ? type.FullyQualified().OriginalName().WithGenerics().Typed(settings)
                                       : type.Simplified().OriginalName().WithGenerics().Typed(settings);
                }
                
                // Handle "System.ValueTuple`" cases
                if (type.IsSystemValueTuple())
                {
                    return await type.GetSystemValueTuple(settings);
                }
            }

            return await Generator.GetNestedClass(type, settings, overridenFullName);
        }
        
        #region Helper methods
        private static async Task<(bool IsSuccess, string Alias)> TryGetNullable(this Type type, GenerationSettings settings)
        {
            if (Nullable.GetUnderlyingType(type) != null)
            {
                // Get the actual type of "System.Nullable`1": e.g., from "System.Nullable<System.Boolean>" => "System.Boolean"
                var typeFromNullable = type.GetGenericArguments()[0];

                // Retrieve alias for a given Nullable type (if registered) => "bool"
                if (Aliases.Supported.TryGetValue(typeFromNullable, out var alias))
                {
                    // Restore the original alias-nullable naming convention => "bool?"
                    return (true, $@"{alias}?");
                }

                // Handle "System.Nullable`1[System.ValueTuple`[,]]" cases
                if (typeFromNullable.IsSystemValueTuple())
                {
                    // Retrieve arguments from ValueTuple and use the modern naming convention: e.g., "(byte, short)"
                    alias = await type.GetSystemValueTuple(settings);

                    // Replace double round brackets
                    alias = alias.Replace(@"((", @"(")
                                 .Replace(@"))", @")");

                    // Restore the original alias-nullable naming convention => "(byte, short)?"
                    return (true, $@"{alias}?");
                }
            }

            return (false, string.Empty);
        }

        private static bool IsArrayList(this Type type)
        {
            return type == typeof(ArrayList);
        }

        private static string GetArrayList(this string alias, GenerationSettings settings)
        {
            return settings.Strategy.UseFullyQualifiedNames ? $@"System.Collections.{alias}" : alias;
        }

        private static bool IsCollection(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static bool IsKeyValuePair(this Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() != null &&
                       type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
            }

            return false;
        }

        private static async Task<(bool IsSuccess, string Alias)> TryGetArray(this Type type, GenerationSettings settings)
        {
            if (type.IsArray)
            {
                // Retrieve element type from the array: e.g., "string" from "string[]"
                var elementType = type.GetElementType()!;

                // Restore the original alias-array naming convention => "string[]"
                var alias = $@"{await elementType.TryGetAliasName(settings)}{(
                    settings.Strategy.UseFullyQualifiedNames ? type.FullName!.GetSquareBrackets()  // Folder.Subfolder.Item[]
                                                             : type.Name.GetSquareBrackets())}";   // Item[]

                return (true, alias);
            }

            return (false, string.Empty);
        }

        private static string GetSquareBrackets(this string typeFullName)
        {
            var match = RegexPatterns.ArrayBracketsRegex.Match(typeFullName);

            return match.Success ? match.Value : @"[]";  // Return "[]", "[,]", "[,,]", etc.
        }

        private static (bool IsSuccess, string Alias) TryGetClassWithGenerics(this Type type, GenerationSettings settings)
        {
            if (type.IsClass && type.IsGenericType)
            {
                return (true, settings.Strategy.UseFullyQualifiedNames
                    ? type.FullyQualified().CustomName().WithGenerics().Typed(settings)
                    : type.Simplified().CustomName().WithGenerics().Typed(settings));
            }

            return (false, string.Empty);
        }

        internal static async Task<IEnumerable<string>> GetArguments(this Type type, GenerationSettings settings, bool? overridenFullName = null)
        {
            // Retrieve element types from collections or tuples: e.g., "string" and "bool" from "IDictionary`2" (IDictionary<string, bool>)
            var elementTypes = type.GetGenericArguments();
            var elementAliases = new List<string>();

            foreach (var elementType in elementTypes)
            {
                /* NOTE: ValueTuple is accepting 7 generic types and the 8th is repacked into another ValueTuple and so on...
                         Example: ValueTuple with 10 arguments "(int, int, int, int, int, int, int, (int, int, int))" */
                if (elementTypes.Length > 7 && elementType.IsSystemValueTuple())
                {
                    // Get arguments from nested sub-tuple e.g., "(int, int, int)" from the example above
                    var nestedTupleArguments = await elementType.GetArguments(settings, overridenFullName);

                    elementAliases.AddRange(nestedTupleArguments);
                }
                // Retrieve base type name from generic argument
                else if (elementType.ContainsGenericParameters)
                {
                    // Get alias from base type full name: "T" => "System.Object" => "object"
                    if (!Aliases.Supported.TryGetValue(elementType.BaseType, out var alias))
                    {
                        // Otherwise get full name e.g., "T" => "ApiGenerator.Example.Item" (with full name) or "Item" (without)
                        alias = overridenFullName ?? settings.Strategy.UseFullyQualifiedNames ? elementType.BaseType!.FullName
                                                                                              : elementType.BaseType!.Name;
                    }

                    elementAliases.Add(alias);
                }
                else  // Normal, not nested argument
                {
                    elementAliases.Add(await elementType.TryGetAliasName(settings, overridenFullName));
                }
            }

            return elementAliases;
        }

        private static bool IsTuple(this Type type)
        {
            return typeof(ITuple).IsAssignableFrom(type);
        }

        private static bool IsSystemTuple(this Type type)
        {
            return type.Name.StartsWith(@"Tuple");
        }

        private static bool IsSystemValueTuple(this Type type)
        {
            return type.Name.StartsWith(@"ValueTuple");
        }

        private static async Task<string> GetSystemValueTuple(this Type type, GenerationSettings settings)
        {
            // Prefer simplified naming convention: "(string, int)" over the struct one: "ValueTuple<string, int>"
            return $@"({string.Join(@", ", await type.GetArguments(settings))})";
        }
        #endregion
    }
}
