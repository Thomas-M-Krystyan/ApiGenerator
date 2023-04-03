using System.Globalization;
using ApiGenerator.Annotations;
using ApiGenerator.Logic.Extensions;
using ApiGenerator.Logic.Workflow.Models;

namespace ApiGenerator.Logic.Logic.FluentNamesBuilder
{
    /// <summary>
    /// Builder Design Pattern of subtype "Fluent" using extension methods as helpers.
    /// </summary>
    internal static class NamesBuilder
    {
        /// <summary>
        /// Retrieves generic <see langword="class"/> definition from a <see langword="class"/> name:
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat&lt;T&gt; => "Cat`1"
        /// 
        ///     Cat&lt;T, V&gt; => "Cat`2".
        ///   </code>
        /// </returns>
        internal static string GetGenericClassDefinitionName(this string className, bool isGeneric = true)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                return string.Empty;
            }

            // The position of "<"
            var openingBracketPosition = className.IndexOf(@"<");

            // Class has generic parameters
            if (openingBracketPosition != -1)
            {
                // 1. Count generic parameters (each "," means +1)
                var genericsAmount = className.Count(character => character.Equals(','));

                // 2. Class arguments
                var arguments = className.Substring(openingBracketPosition + 1).Replace(@">", string.Empty);

                // 3. Class name without "<T, V>"
                className = className.Substring(0, openingBracketPosition);

                return isGeneric
                    // 4a. Converts "System.Collections.Generic.List<T, V>" into "System.Collections.Generic.List`2"
                    ? $@"{className}`{1 + genericsAmount}"
                    // 4b. Converts "System.Collections.Generic.List<System.String>" into "System.Collections.Generic.List`1[System.String]"
                    : $@"{className}`{1 + genericsAmount}[{arguments}]";
            }

            return className;
        }

        // --------------------------
        // Namespace building methods
        // --------------------------
        
        /// <summary>
        /// Leaves <see cref="Type"/> name (e.g. <see langword="class"/> or <see langword="interface"/>) unchanged.
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat => "Cat"
        /// 
        ///     ICat => "ICat"
        ///   </code>
        /// </returns>
        internal static Namespace Simplified(this Type member)
        {
            return new Namespace(member, fullyQualified: false);
        }
        
        /// <summary>
        /// Adds namespace before <see cref="Type"/> name (e.g. <see langword="class"/> or <see langword="interface"/>).
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat => "Folder.Subfolder.Cat"
        /// 
        ///     ICat => "Folder.Subfolder.ICat"
        ///   </code>
        /// </returns>
        internal static Namespace FullyQualified(this Type member)
        {
            return new Namespace(member, fullyQualified: true);
        }
        
        // ---------------------
        // Name building methods
        // ---------------------
        
        /// <summary>
        /// Gets custom <see cref="Type"/> name (e.g. <see langword="class"/> or <see langword="interface"/>).
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat => "Cat"
        /// 
        ///     ICat => "ICat"
        ///   
        ///     [<see cref="ApiClassAttribute"/>(customName: "Tom")]
        ///     Cat => "Tom"
        ///   </code>
        /// </returns>
        internal static Name CustomName(this Namespace @namespace)
        {
            var typeName = @namespace.Type.GetClassName().EventuallyFullyQualified(@namespace);

            return new Name(@namespace, typeName, isCustomName: true);
        }
        
        /// <summary>
        /// Gets unchanged <see langword="interface"/> name or custom <see langword="class"/> name with "I" prefix.
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat => "ICat"
        /// 
        ///     ICat => "ICat"
        ///   
        ///     [<see cref="ApiClassAttribute"/>(customName: "Tom")]
        ///     Cat => "ITom"
        ///   </code>
        /// </returns>
        internal static Name InterfaceName(this Namespace @namespace)
        {
            var typeName = @namespace.Type.GetClassName();

            // Add "I" at the beginning of the name.
            // Example: if for class "Cat" the user will use custom name "Sparky" then result will be "ISparky"
            //          but if the user will already provide an interface name, the result will be "IISparky"
            var interfaceName = (@namespace.Type.IsApiClass()
                ? $@"I{typeName}"
                : typeName).EventuallyFullyQualified(@namespace);

            return new Name(@namespace, interfaceName, isCustomName: true, isInterfaceNameName: true);
        }

        /// <summary>
        /// Gets unchanged <see cref="Type"/> name (<see langword="class"/> or <see langword="interface"/>).
        /// </summary>
        internal static Name OriginalName(this Namespace @namespace)
        {
            var typeName = @namespace.Type.Name.EventuallyFullyQualified(@namespace);

            return new Name(@namespace, typeName, isCustomName: false);
        }
        
        private static string GetClassName(this Type type)
        {
            var typeName = type.GetCustomName();
            var firstLetter = typeName[0];

            return char.IsLetter(firstLetter) && char.IsLower(firstLetter)
                ? $@"{char.ToUpper(typeName[0], CultureInfo.InvariantCulture) + typeName.Substring(1)}"  // For case: "person" => "Person"
                : $@"{typeName}";                                                                        // For case: "Person" => "Person"
        }

        private static string EventuallyFullyQualified(this string typeName, Namespace @namespace)
        {
            return @namespace.FullyQualified
                ? $@"{@namespace.Type.Namespace}.{typeName}"  // "Folder.Subfolder.Item"
                : typeName;                                   // "Item"
        }
        
        // -------------------------
        // Generics building methods
        // -------------------------
        
        /// <summary>
        /// Gets <see cref="Type"/> name (e.g. <see langword="class"/> or <see langword="interface"/>) without generic arguments.
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat => "ICat"
        ///   
        ///     Cat&lt;T&gt; where T : Item => "ICat"
        ///   </code>
        /// </returns>
        internal static string WithoutGenerics(this Name name)
        {
            var typeName = name.Build();
            var genericApostropheIndex = typeName.IndexOf(@"`");

            var typeNameWithoutGenerics = genericApostropheIndex != -1
                // Retrieve simple name: e.g., from "IList`1" => "IList" or "Cat`25" => "Cat"
                ? $@"{typeName.Substring(0, genericApostropheIndex)}"
                : typeName;

            return typeNameWithoutGenerics;
        }
        
        /// <summary>
        /// Gets <see cref="Type"/> name (e.g. <see langword="class"/> or <see langword="interface"/>) with generic arguments.
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat => "ICat"
        ///   
        ///     Cat&lt;T&gt; where T : Item => "ICat&lt;T&gt;"
        ///   </code>
        /// </returns>
        internal static Generics WithGenerics(this Name name)
        {
            return new Generics(name, withOutKeyword: false);
        }

        /// <summary>
        /// <inheritdoc cref="WithGenerics"/> Enforce fully qualified generic attributes.
        /// </summary>
        /// <returns>
        ///   <inheritdoc cref="WithGenerics"/>
        /// </returns>
        internal static Generics WithGenerics_FullyQualified(this Name name)
        {
            return new Generics(name, withOutKeyword: false, fullyQualified: true);
        }
        
        /// <summary>
        /// Gets <see cref="Type"/> name (e.g. <see langword="class"/> or <see langword="interface"/>) with generic arguments
        /// preceded by "out" keyword.
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat => "ICat"
        ///   
        ///     Cat&lt;T&gt; where T : Item => "ICat&lt;out T&gt;"
        ///   </code>
        /// </returns>
        internal static string WithOutGenerics_Named(this Name name)
        {
            return new Generics(name, withOutKeyword: true).Named();
        }
        
        /// <summary>
        /// Gets plane names of generic arguments.
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat&lt;Item&gt; => "Cat&lt;Item&gt;"
        /// 
        ///     ICat&lt;Item&gt; => "ICat&lt;Item&gt;"
        /// 
        ///     Cat&lt;T&gt; where T : Item => "Cat&lt;T&gt;"
        /// 
        ///     ICat&lt;T&gt; where T : Item => "ICat&lt;T&gt;"
        ///   </code>
        /// </returns>
        internal static string Named(this Generics generics)
        {
            return generics.AddGenericArgumentNames();
        }
        
        /// <summary>
        /// Gets base types behind generic arguments.
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Cat&lt;Item&gt; => "Cat&lt;Item&gt;"
        /// 
        ///     ICat&lt;Item&gt; => "ICat&lt;Item&gt;"
        /// 
        ///     Cat&lt;T&gt; where T : Item => "Cat&lt;Item&gt;"
        /// 
        ///     ICat&lt;T&gt; where T : Item => "ICat&lt;Item&gt;"
        ///   </code>
        /// </returns>
        internal static string Typed(this Generics generics, GenerationSettings settings)
        {
            return generics.AddGenericArgumentTypes(settings);
        }
        
        private static string AddGenericArgumentNames(this Generics generics)
        {
            var interfaceName = generics.Name.Build().TrimGenerics();

            // Read generic arguments names: e.g., "Examples.Item", "Item" or "T", "TModel", "TValue", etc.
            var genericArgumentsNames = generics.Name.Namespace.Type.GetGenericArguments()
                .Select(argumentType =>
                {
                    if (argumentType.ContainsGenericParameters)
                    {
                        return generics.AddOutKeyword(argumentType.Name);  // Case #1: "T" or "out T"
                    }

                    // Get name from type
                    return generics.FullyQualified ?? generics.Name.Namespace.FullyQualified
                        ? argumentType.FullName  // Case #2: "Examples.Item"
                        : argumentType.Name;     // Case #3: "Item"
                })
                .ToList();

            return genericArgumentsNames.Count == 0
                ? $@"{interfaceName}"
                : $@"{interfaceName}<{string.Join(@", ", genericArgumentsNames)}>";
        }

        private static string AddGenericArgumentTypes(this Generics generics, GenerationSettings settings)
        {
            // Get base types underlying behind generic types: e.g., "T" => "Examples.Item" (with full name) or "Item" (without)
            var genericArgumentsNames =
                Task.Run(async () =>
                    // Get base types from generic arguments
                    await generics.Name.Namespace.Type.GetArguments(settings,
                        generics.FullyQualified ?? generics.Name.Namespace.FullyQualified)).Result
                .ToArray();

            var typeName = generics.Name.Build().TrimGenerics();

            return genericArgumentsNames.Length == 0
                ? $@"{typeName}"
                : $@"{typeName}<{string.Join(@", ", genericArgumentsNames)}>";
        }

        internal static string TrimGenerics(this string typeName)
        {
            var indexOfApostrophe = typeName.IndexOf(@"`");

            return indexOfApostrophe != -1
                ? typeName.Substring(0, indexOfApostrophe)
                : typeName;
        }
    }

    internal class Namespace
    {
        internal Type Type { get; }

        /// <summary>
        /// Determines the entire workflow for name generation to include/exclude fully qualified names.
        /// </summary>
        internal bool FullyQualified { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Namespace"/> class.
        /// </summary>
        internal Namespace(Type type, bool fullyQualified)
        {
            Type = type;
            FullyQualified = fullyQualified;
        }
    }

    internal class Name
    {
        private readonly string m_typeName;
        private readonly bool m_isCustomName;

        internal Namespace Namespace { get; }

        /// <summary>
        /// Checks whether this name was generated using interface rules.
        /// </summary>
        internal bool IsInterfaceName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="m_typeName"/> class.
        /// </summary>
        internal Name(Namespace @namespace, string typeName, bool isCustomName)
        {
            m_typeName = typeName;
            m_isCustomName = isCustomName;
            
            Namespace = @namespace;
        }

        /// <inheritdoc cref="Name(FluentNamesBuilder.Namespace, string, bool)"/>
        internal Name(Namespace @namespace, string typeName, bool isCustomName, bool isInterfaceNameName)
            : this(@namespace, typeName, isCustomName)
        {
            IsInterfaceName = isInterfaceNameName;
        }

        /// <summary>
        /// Returns <see cref="Type"/> name.
        /// </summary>
        internal string Build()
        {
            if (!m_isCustomName || m_typeName.Contains(@"`"))
            {
                return m_typeName;
            }

            // Restore class generic definition if needed
            var genericArgumentsAmount = Namespace.Type.GenericTypeArguments.Length;
        
            return genericArgumentsAmount > 0
                ? $@"{m_typeName}`{genericArgumentsAmount}"  // Add "`1"
                : m_typeName;
        }
    }

    internal class Generics
    {
        private readonly bool m_withOutKeywordKeyword;

        internal Name Name { get; }

        /// <summary>
        /// Overrides settings from <see cref="Namespace.FullyQualified"/>.
        /// </summary>
        internal bool? FullyQualified { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Generics"/> class.
        /// </summary>
        internal Generics(Name name, bool withOutKeyword)
        {
            m_withOutKeywordKeyword = withOutKeyword;

            Name = name;
        }

        /// <inheritdoc cref="Generics(FluentNamesBuilder.Name, bool)" />
        internal Generics(Name name, bool withOutKeyword, bool fullyQualified) : this(name, withOutKeyword)
        {
            FullyQualified = fullyQualified;
        }

        /// <summary>
        /// Prepends argument name with <see langword="out"/> keyword.
        /// </summary>
        internal string AddOutKeyword(string argumentName)
        {
            // Only interfaces with named generic types can eventually have "out" keywords: "IProxy<out T>"
            if (Name.Namespace.Type.IsInterface || Name.IsInterfaceName)
            {
                return m_withOutKeywordKeyword ? $@"out {argumentName}" : argumentName;
            }

            return argumentName;
        }
    }
}
