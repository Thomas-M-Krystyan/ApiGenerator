// Loading XML Documentation for XML element types of "member":
// https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/october/csharp-accessing-xml-documentation-via-reflection

using System.Text.RegularExpressions;
using System.Xml;
using ApiGenerator.Logic.Constants;
using ApiGenerator.Logic.Extensions;
using ApiGenerator.Logic.Logic.FluentNamesBuilder;
using ApiGenerator.Logic.Workflow;
using ApiGenerator.Logic.Workflow.Models;

namespace ApiGenerator.Logic.Logic
{
    /// <summary>
    /// Reads specific content from a given file path e.g., XML documentation, file name, etc.
    /// </summary>
    internal static class Reader
    {
        /// <summary>
        /// A dictionary of XML documentations loaded from certain projects.
        /// </summary>
        internal static Dictionary<string, string> LoadedXmlDocumentation { get; } = new();
        
        /// <summary>
        /// Loads XML documentation for a specified project.
        /// </summary>
        internal static void LoadXmlDocumentation(string sourceProjectName, string sourceProjectPath)
        {
            if (string.IsNullOrWhiteSpace(sourceProjectName))
            {
                throw new ArgumentException(@"Source project name cannot be null or empty");
            }

            var assemblyName = sourceProjectPath.GetAssemblyName(sourceProjectName);
            var xmlDocumentName = $@"{assemblyName}.xml";

            // First attempt: reading XML documentation file name just in its original form
            var content = GetFileContent_String(xmlDocumentName); // This method by default will go into ...\bin\Debug\ folder.
                                                                        // To make everything work dedicated projects should generate
                                                                        // their .xml documentation files into this output location
            if (string.IsNullOrEmpty(content))
            {
                throw new FileLoadException(@"The content of the XML documentation file is empty");
            }

            using var reader = new StringReader(content);
            using var xmlReader = XmlReader.Create(reader);

            while (xmlReader.Read())
            {
                // Get only <summary> content for classes, properties, methods. Skip other parts of XML document
                if (xmlReader.NodeType == XmlNodeType.Element &&
                    xmlReader.Name == "member")
                {
                    var rawName = xmlReader["name"]!;
                    LoadedXmlDocumentation[rawName] = xmlReader.ReadInnerXml();
                }
            }
        }

        /// <summary>
        /// Gets the entire file content (as a whole piece) from the provided path.
        /// </summary>
        private static string GetFileContent_String(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Gets the entire file content (as a separate lines) from the provided path.
        /// </summary>
        internal static IList<string> GetFileContent_Lines(string filePath)
        {
            return File.ReadAllLines(filePath).ToList();
        }

        /// <summary>
        /// Retrieves the <see cref="Type"/> of a class from the given file path and <see cref="GenerationSettings"/>.
        /// </summary>
        /// <returns>.NET type.</returns>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ArgumentException"/>
        internal static Type GetTypeFromFile(this string filePath, GenerationSettings settings)
        {
            // Get the class namespace
            var isNamespaceRetrieved = GetNamespaceFromFile(filePath, out var @namespace);

            // Get file name (used to get the class name)
            var isFileNameRetrieved = GetFileNameFromPath(filePath, out var fileName);

            // Get assembly where the file is located
            var isAssemblyRetrieved = Reflector.TryGetAssemblyName(
                settings.SourceProjectName,
                settings.SourceProjectPath, out var assemblyFullName);

            if (isNamespaceRetrieved && isFileNameRetrieved && isAssemblyRetrieved)
            {
                var fullyQualifiedClassName = $@"{@namespace}.{fileName}, {assemblyFullName}";

                try
                {
                    // Get type of the class using its namespace + file name
                    return Type.GetType(fullyQualifiedClassName) ?? throw new InvalidOperationException(message:
                        $@"Cannot recognize the type!{Environment.NewLine}" +
                        $@"File name ""{fileName}"" cannot be used to obtain the class type. Possible reasons:{Environment.NewLine}" +
                        $@"- File name (e.g., ""Example.cs"" is different than class name declared inside the file (e.g., ""class ExampleX"" which is against MSDN standards){Environment.NewLine}" +
                        $@"- Retrieving complex class name (e.g., ""Example<T>"") from its file (e.g., ""Example.cs"") failed for some reason{Environment.NewLine}" +
                        $@"- Internal error occurred in {nameof(ApiGenerationManager)}.cs - class type is located in a different project assembly");
                }
                // Fallback retry
                catch (InvalidOperationException exception)
                {
                    // Get class name from a file content (more time consuming but also more reliable approach)
                    if (Reader.GetClassNameFromFile(filePath, out var className))
                    {
                        fullyQualifiedClassName = $@"{@namespace}.{className.GetGenericClassDefinitionName()}, {assemblyFullName}";
                        
                        // Get type of the class using its namespace + name
                        return Type.GetType(fullyQualifiedClassName)
                           ?? throw exception.Enrich($@"{Environment.NewLine}- Specified file contains multiple entities " +
                                                      @"(classes and/or interfaces) which is against OOP good practices");
                    }

                    // Fallback to retrieve "class name different than file name" or "class name with generic parameters" failed
                    throw;
                }
            }

            throw new ArgumentException(@"Either namespace, class name, or assembly could not be successfully retrieved");
        }

        /// <summary>
        /// Gets the <see langword="namespace"/> name from a file.
        /// </summary>
        /// <returns>Empty value if nothing was found.</returns>
        private static bool GetNamespaceFromFile(string filePath, out string @namespace)
        {
            return GetValueFromFileContent(filePath,
                RegexPatterns.NamespaceNameRegex,
                RegexPatterns.GroupNamespace, out @namespace);
        }

        /// <summary>
        /// Gets the value of RootNamespace from a file.
        /// </summary>
        /// <returns><inheritdoc cref="GetNamespaceFromFile"/></returns>
        internal static bool GetRootNamespaceFromFile(string filePath, out string rootNamespace)
        {
            return GetValueFromFileContent(filePath,
                RegexPatterns.RootNamespaceRegex,
                RegexPatterns.GroupRootNamespace, out rootNamespace);
        }
        
        /// <summary>
        /// Gets the value of AssembleName from a file.
        /// </summary>
        /// <returns><inheritdoc cref="GetNamespaceFromFile"/></returns>
        internal static bool GetAssemblyNameFromFile(string filePath, out string assemblyName)
        {
            return GetValueFromFileContent(filePath,
                RegexPatterns.AssemblyNameRegex,
                RegexPatterns.GroupAssemblyName, out assemblyName);
        }

        /// <summary>
        /// Gets the file name from a file path.
        /// </summary>
        /// <returns>The file name which
        ///   <para>
        ///   NOTE: It should match to the inner <see langword="class"/> name.
        ///   </para>
        /// </returns>
        private static bool GetFileNameFromPath(string filePath, out string fileName)
        {
            var classNameMatch = RegexPatterns.FileNameRegex.Match(filePath);
            fileName = classNameMatch.Groups[RegexPatterns.GroupFileName].Value;

            return classNameMatch.Success;
        }

        /// <summary>
        /// Gets the <see langword="class"/> name from a file content.
        /// </summary>
        /// <returns>The <see langword="class"/> name.</returns>
        private static bool GetClassNameFromFile(string filePath, out string className)
        {
            return GetValueFromFileContent(filePath,
                RegexPatterns.ClassDeclarationRegex,
                RegexPatterns.GroupClassName, out className);
        }

        /// <summary>
        /// Generic method to get a <see langword="string"/> value from the file content
        /// (retrieved by its path) when using a given <see cref="Regex"/> pattern.
        /// </summary>
        private static bool GetValueFromFileContent(string filePath, Regex regex, string groupName, out string matchResult)
        {
            matchResult = string.Empty;

            try
            {
                // Read the file content line by line (not all text at once)
                foreach (var line in File.ReadLines(filePath))
                {
                    var namespaceMatch = regex.Match(line);

                    if (namespaceMatch.Success)
                    {
                        matchResult = namespaceMatch.Groups[groupName].Value;

                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}
