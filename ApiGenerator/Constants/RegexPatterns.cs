using System.Text.RegularExpressions;
using ApiGenerator.Annotations;

namespace ApiGenerator.Logic.Constants
{
    /// <summary>
    /// Compiled Regular Expression patterns.
    /// </summary>
    internal static class RegexPatterns
    {
        // -------------------------------------------------------------
        internal const string GroupFileName = nameof(GroupFileName);

        /// <summary>
        /// Captures only the file name, without path and file extension.
        /// </summary>
        internal static readonly Regex FileNameRegex = new($@"(?:.+\\)(?<{GroupFileName}>.+)\.cs", RegexOptions.Compiled);

        // -------------------------------------------------------------
        internal const string GroupNamespace = nameof(GroupNamespace);

        /// <summary>
        /// Captures the name of the namespace from a given file.
        /// </summary>
        internal static readonly Regex NamespaceNameRegex = new($@"namespace\ (?<{GroupNamespace}>.+)", RegexOptions.Compiled);

        // -------------------------------------------------------------
        internal const string GroupRootNamespace = nameof(GroupRootNamespace);

        /// <summary>
        /// Captures the content between &lt;RootNamespace&gt; tags in .csproj file.
        /// </summary>
        internal static readonly Regex RootNamespaceRegex = new($@"\<RootNamespace\>(?<{GroupRootNamespace}>.+)\<", RegexOptions.Compiled);
        
        // -------------------------------------------------------------
        internal const string GroupAssemblyName = nameof(GroupAssemblyName);

        /// <summary>
        /// Captures the content between &lt;RootNamespace&gt; tags in .csproj file.
        /// </summary>
        internal static readonly Regex AssemblyNameRegex = new($@"\<AssemblyName\>(?<{GroupAssemblyName}>.+)\<", RegexOptions.Compiled);

        // -------------------------------------------------------------
        internal const string GroupClassDeclaration = nameof(GroupClassDeclaration);
        internal const string GroupClassName = nameof(GroupClassName);
        internal const string GroupGenericConstraint = nameof(GroupGenericConstraint);
        internal const string GroupClassInheritance = nameof(GroupClassInheritance);

        /// <summary>
        /// Captures <see langword="class"/> declaration, name, generic constraints, and inheritances
        /// <code>
        ///   <see langword="public class"/> Name&lt;T&gt; <see langword="where"/> T : Item, A, B, IName
        /// </code>
        /// </summary>
        internal static readonly Regex ClassDeclarationRegex = new($@"(?<{GroupClassDeclaration}>.*(?:public|internal)(?:|\ (?:abstract|sealed))\ (?:class|interface)\ )(?<{GroupClassName}>\w+\<.+?\>|\w+)(?<{GroupGenericConstraint}>\ ?where\ \w+\ \:\ \w+(?:\,\ )?)?(?:\ \:\ )?(?<{GroupClassInheritance}>.*)", RegexOptions.Compiled);

        /// <summary>
        /// Captures only the "where" generic constraint.
        /// </summary>
        internal static readonly Regex GenericConstraintRegex = new($@"(?<{GroupGenericConstraint}>\ ?where\ \w+\ \:\ \w+)", RegexOptions.Compiled);

        // -------------------------------------------------------------
        internal const string GroupBeforeAssignmentOrLambda = nameof(GroupBeforeAssignmentOrLambda);
        internal const string GroupWithAssignmentOrLambda = nameof(GroupWithAssignmentOrLambda);

        /// <summary>
        /// Captures field/property/method with or without object initialization assignment or lambda expression, such as:
        /// <code>
        ///   _field = <see langword="new"/> Model();
        ///
        ///   Property { <see langword="get"/>; <see langword="set"/>; } = <see langword="new"/> Model();
        ///
        ///   Method() => <see langword="new"/> Model();
        /// </code>
        /// </summary>
        internal static readonly Regex ObjectInitializationRegex = new($@"(?<{GroupBeforeAssignmentOrLambda}>.+)(?<{GroupWithAssignmentOrLambda}>\ \=(?:|\>)\ new\ .+)", RegexOptions.Compiled);

        // -------------------------------------------------------------
        internal const string GroupMemberName = nameof(GroupMemberName);
        internal const string GroupBeforeText = nameof(GroupBeforeText);
        internal const string GroupAfterText = nameof(GroupAfterText);

        /// <summary>
        /// Captures the last component in "see cref=" or "seealso cref=" tags e.g., "Namespace.Class.[Property]" or "Namespace.Class.[Method(System.String,System.Int32)]".
        /// </summary>
        internal static readonly Regex MemberNameInCrefRegex = new($@"(?<{GroupBeforeText}>.*)\<see(|also)\ cref.+\.(?<{GroupMemberName}>\w+|\w+\(.+\))\""(|\ )\/\>(?<{GroupAfterText}>.*)", RegexOptions.Compiled);

        // -------------------------------------------------------------
        internal const string GroupOpeningTag = nameof(GroupOpeningTag);
        internal const string GroupContent = nameof(GroupContent);
        internal const string GroupClosingTag = nameof(GroupClosingTag);

        /// <summary>
        /// Captures 0|1 opening tag, 0|1 content between tags, and 0|1 closing tag (expect 0-3 capturing groups, where 0 => no match).
        /// </summary>
        internal static readonly Regex DotsInSummariesRegex = new($@"(?<{GroupOpeningTag}>\<\w+\>)?(?<{GroupContent}>.*?(?=\<\/|$))?(?<{GroupClosingTag}>\<\/\w+\>)?", RegexOptions.Compiled);

        /// <summary>
        /// Captures text between "summary" tags.
        /// </summary>
        internal static readonly Regex SummaryContentRegex = new($@"<summary>(?<{GroupContent}>.+)\<\/summary>",
            RegexOptions.Singleline |  // Reads newlines such as: CR, LF, CRLF, NEL
            RegexOptions.Compiled);

        // -------------------------------------------------------------
        /// <summary>
        /// Captures .NET types provided as method parameters in XML: e.g., "SetName(<see langword="string"/>)" or "SetData(<see langword="string"/>, <see langword="int"/>)".
        /// </summary>
        internal static readonly Regex ParamsNetTypesRegex = new(@"(?<=\w\().+?(?=\""|\))", RegexOptions.Compiled);

        // -------------------------------------------------------------
        /// <summary>
        /// Captures .NET types after "cref" XML tag.
        /// </summary>
        internal static readonly Regex CrefNetTypeRegex = new(@"(?<=\ cref\=\"").+?(?=\""|\))", RegexOptions.Compiled);

        // -------------------------------------------------------------        
        /// <summary>
        /// Captures newlines and white characters.
        /// </summary>
        internal static readonly Regex WhitespacesRegex = new(@"\n\s+", RegexOptions.Compiled);

        // -------------------------------------------------------------
        internal const string GroupTrailingChars = nameof(GroupTrailingChars);

        /// <summary>
        /// Captures the last characters in a sentence, including whitespaces before and after dots (multiple possible).
        /// </summary>
        internal static readonly Regex TrailingCharactersRegex = new($@"(?<{GroupContent}>.+[^\.])(?<{GroupTrailingChars}>(?<=\w|\w\.).*$)", RegexOptions.Compiled);

        // -------------------------------------------------------------
        internal const string GroupWindowsCrLfNewline = "\r\n";

        /// <summary>
        /// Captures predefined line endings.
        /// </summary>
        internal static readonly Regex NewlineRegex = new($@"{GroupWindowsCrLfNewline}|\r|\n", RegexOptions.Compiled);

        // -------------------------------------------------------------
        /// <summary>
        /// Captures square brackets from arrays, including level of multidimensional nesting e.g., "System.Byte[,,,]" => "[,,,]".
        /// </summary>
        internal static readonly Regex ArrayBracketsRegex = new(@"\[\,*\]$", RegexOptions.Compiled);
        
        // -------------------------------------------------------------
        internal const string GroupBeforeApiAttribute = nameof(GroupBeforeApiAttribute);
        internal const string GroupApiAttribute = nameof(GroupApiAttribute);
        internal const string GroupAfterApiAttribute = nameof(GroupAfterApiAttribute);

        /// <summary>
        /// Captures [<see cref="ApiClassAttribute"/>] or [<see cref="ApiMemberAttribute"/>] in annotations.
        /// </summary>
        internal static readonly Regex ApiAttributeRegex = new($@"(?<{GroupBeforeApiAttribute}>\/\/\s*\[|\s*\[.*)(?<{GroupApiAttribute}>{GetAttributeName<ApiClassAttribute>()}|{GetAttributeName<ApiMemberAttribute>()})(?<{GroupAfterApiAttribute}>.*\].*)", RegexOptions.Compiled);

        private static string GetAttributeName<T>() where T : Attribute
        {
            return $@"{typeof(T).Name.Replace(@"Attribute", string.Empty)}";
        }
    }
}
