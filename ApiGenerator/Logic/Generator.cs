using System.Reflection;
using System.Text;
using ApiGenerator.Annotations;
using ApiGenerator.Logic.Constants;
using ApiGenerator.Logic.Extensions;
using ApiGenerator.Logic.Logic.FluentNamesBuilder;
using ApiGenerator.Logic.Logic.Models;
using ApiGenerator.Logic.Workflow.Models;

namespace ApiGenerator.Logic.Logic
{
    // NOTE: Suppress ReSharper warnings about string formatting - left as it's now for readability
    // ReSharper disable LocalizableElement

    /// <summary>
    /// The generator of API classes or related logic.
    /// </summary>
    internal sealed class Generator
    {
        #region Fields
        private readonly Reflector m_reflector;
        private bool m_propertiesPresent;
        private bool m_methodsPresent;
        private static int m_registrationsCount;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of this specific API candidate member.
        /// </summary>
        internal Type Type { get; }

        /// <summary>
        /// Gets API generation settings for this specific task.
        /// </summary>
        internal GenerationSettings Settings { get; }

        /// <summary>
        /// Gets the different variants of names for a newly generated API <see langword="interface"/>.
        /// </summary>
        internal InterfaceNames NewInterfaceName { get; }

        /// <summary>
        /// Checks whether not at least something was registered.
        /// </summary>
        internal static bool NothingWasRegistered => m_registrationsCount == 0;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class.
        /// </summary>
        /// 
        /// <param name="apiCandidate">
        ///   A member of <see langword="class"/> type marked with [<see cref="ApiClassAttribute"/>] annotation.
        /// </param>
        /// <param name="taskSettings">
        ///   FROM where and TO where a new API should be generated. Also, under which <see langword="namespace"/>.
        /// </param>
        internal Generator(Type apiCandidate, GenerationSettings taskSettings)
        {
            // Explicit setup for this specific API generation
            Type = apiCandidate;
            Settings = taskSettings;

            // Implicit (determined) setup of the remaining necessary data/components
            m_reflector = new Reflector(Type);

            var simpleInterfaceName = Type.Simplified().InterfaceName();
            var simpleInterfaceWithGenerics = simpleInterfaceName.WithGenerics();

            NewInterfaceName = new InterfaceNames
            {
                AsFileName = simpleInterfaceName.WithoutGenerics(),
                
                Declaration_Simplified = simpleInterfaceName.WithOutGenerics_Named(),

                Generation_Simplified = simpleInterfaceWithGenerics.Named(),
                Generation_FullyQualified = $@"{Settings.TargetNamespace}.{simpleInterfaceWithGenerics.Named()}",
                
                Registration_Simplified = simpleInterfaceWithGenerics.Typed(Settings),
                Registration_FullyQualified = $@"{Settings.TargetNamespace}.{simpleInterfaceName.WithGenerics_FullyQualified().Typed(Settings)}"
            };
        }

        #region Creation of API Interface
        /// <summary>
        /// Gets an API <see langword="interface"/> for the registered <see cref="System.Type"/>.
        /// A file with all required copyrights and "auto-generated" stamps in it will be created.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if API creation was successful; otherwise: <see langword="false"/>.
        /// </returns>
        internal async Task<bool> CreateApi()
        {
            // Only classes and interfaces with Api annotation are allowed
            if (!Type.IsApiClass())
            {
                return false;
            }

            return await Writer.SaveFile(
                targetPath: Settings.TargetCatalogPath,
                fileName: NewInterfaceName.AsFileName,
                // Preparing file content
                fileContent: Settings.Strategy.Copyrights +
                             Settings.Strategy.FileStamp +
                             GetGeneratedInterface());
        }

        // ---------
        // Interface
        // ---------
        private string GetGeneratedInterface()
        {
            return $"namespace {Settings.TargetNamespace}{Environment.NewLine}" +
                   $"{{{Environment.NewLine}" +
                   $"{GetGeneratedSummary(Type, indentationLevel: 1)}" +
                   $"{GetTabs(1)}public interface {NewInterfaceName.Declaration_Simplified}{GetInterfaceDependencies()}{Environment.NewLine}" +
                   $"{GetTabs(1)}{{" +
                   $"{GetGeneratedProperties()}" +
                   $"{GetGeneratedMethods()}" +
                   $"{AddFinalNewline()}" +
                   $"{GetTabs(1)}}}{Environment.NewLine}" +
                    "}";
        }

        private string GetInterfaceDependencies()
        {
            var interfaceDependencies = Task.WhenAll(Type.GetDerivedFrom()
                .Select(async type => await TypeAliasOrName(type)))
                .Result;

            return interfaceDependencies.Length > 0
                ? $@" : {string.Join(", ", interfaceDependencies)}"
                : string.Empty;
        }

        private string AddFinalNewline()
        {
            return !m_propertiesPresent && !m_methodsPresent
                ? Environment.NewLine  // The file is empty (no properties and no methods)
                : string.Empty;        // Add the final newline after properties or methods
        }

        // ---------
        // Summaries
        // ---------
        private string GetGeneratedSummary(MemberInfo member, ushort indentationLevel = 2)
        {
            // 1. Retrieving member's documentation from raw .xml file
            var xmlDocumentation = member.GetDocumentation();

            if (IsSummaryEmpty(member, indentationLevel, ref xmlDocumentation))
            {
                return xmlDocumentation;  // Placeholder summary
            }

            // 2. Parsing the block of documentation code to change how it will look like in the end
            const string tempSeparator = "\n";

            var separateLines =
                // Remove original indentation from XML file but keep the basic new lines
                RegexPatterns.WhitespacesRegex.Replace(xmlDocumentation, tempSeparator)
                     // Split the summary into single lines
                     .Split(tempSeparator.ToCharArray());

            separateLines = separateLines
                     // Skip the first and the last newline which are basically empty
                     .Skip(1)  // NOTE: In .NET 6+ "SkipLast(1)" is possible
                     .Take(separateLines.Length - 2)
                     .ToArray();
            
            for (int index = 0; index < separateLines.Length; index++)
            {
                if (index + 1 < separateLines.Length)
                {
                    if (TrimmedLine(separateLines[index], false).EndsWith(">") ||       // This line is enclosed with a tag "Some content</summary>"
                        TrimmedLine(separateLines[index + 1], true).StartsWith("<") ||  // Next line is started with tag    "Some content" => "</summary>
                        TrimmedLine(separateLines[index + 1], false).EndsWith(">"))     // Next line is enclosed with tag   "Some content" => "continuation</summary>"
                    {
                        separateLines[index] = Reformat(Cleanup(SimplifyCref(separateLines[index])), indentationLevel);
                    }
                    else
                    {
                        separateLines[index] = Reformat(SimplifyCref(separateLines[index]), indentationLevel);
                    }

                    continue;
                }
                
                // Reformat the summary to restore the original C# styling
                separateLines[index] = Reformat(Cleanup(SimplifyCref(separateLines[index])), indentationLevel);
            }

            // 3. Creating a single block of documentation code once again to put it into an interface
            return string.Join(tempSeparator, separateLines) + tempSeparator;
        }

        private static string TrimmedLine(string line, bool fromStart)
        {
            return fromStart ? line.TrimStart() : line.TrimEnd();
        }

        internal static string Reformat(string lineContent, ushort indentationLevel)
        {
            return $@"{GetTabs(indentationLevel)}/// {lineContent}";
        }

        internal static string Cleanup(string lineContent)
        {
            if (!lineContent.StartsWith("</"))  // Ignore lonely closing tags
            {
                var match = RegexPatterns.DotsInSummariesRegex.Match(lineContent);
                
                if (match.Success)
                {
                    var textBetweenTags = match.Groups[RegexPatterns.GroupContent].Value;
                    
                    lineContent = match.Groups[RegexPatterns.GroupOpeningTag].Value +
                                  AdjustTrailingChars(textBetweenTags) +
                                  match.Groups[RegexPatterns.GroupClosingTag].Value;
                }
            }

            return lineContent;
        }

        private static string AdjustTrailingChars(string textBetweenTags)
        {
            // Cleanup whitespaces
            var trimmedContent = textBetweenTags.Trim(); // Remove leading and trailing spaces

            // Cleanup dots
            var trailingMatch = RegexPatterns.TrailingCharactersRegex.Match(trimmedContent);

            if (trailingMatch.Success)
            {
                var leadingContent = trailingMatch.Groups[RegexPatterns.GroupContent].Value;
                var trailingChars = trailingMatch.Groups[RegexPatterns.GroupTrailingChars].Value;
                var dotsCount = trailingChars.Count(character => character.Equals('.'));

                return dotsCount switch
                {
                    > 2    => $"{leadingContent}...",  // Keep "..."
                    0 or 2 => $"{leadingContent}.",    // Keep "."
                    _      => $"{leadingContent}{trailingChars.Trim()}"
                };
            }

            return trimmedContent;
        }

        internal string SimplifyCref(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            var match = RegexPatterns.MemberNameInCrefRegex.Match(line);
            
            return match.Success
                // Converts: "<see cref="P:ApiGenerator.Examples.Spike.Age" />" into "<see cref="Age" />"
                ? ReplaceFullTypes($"{match.Groups[RegexPatterns.GroupBeforeText].Value}" +
                                     $"{GetSeeCref(match.Groups[RegexPatterns.GroupMemberName].Value)}" +
                                     $"{match.Groups[RegexPatterns.GroupAfterText].Value}")
                : line;
        }
        
        internal string ReplaceFullTypes(string lineContent)
        {
            // Case #1: Keep "System.Int32" types in the original content
            if (Settings.Strategy.UseFullyQualifiedNames)
            {
                return lineContent;
            }

            // Case #2: Replace multiple .NET types provided as method parameters: e.g., cref="SetData(System.String, System.Int32)"
            // by aliases to eliminate fully qualified names
            var paramsMatch = RegexPatterns.ParamsNetTypesRegex.Match(lineContent);

            if (paramsMatch.Success)
            {
                var parametersNames = paramsMatch.Value
                    .Split(',')
                    .Select(parameter => parameter.Trim())
                    .ToArray();

                var replacement = parametersNames.Length == 0
                    // Sub-case A: Single parameter
                    ? TypeAliasOrName(paramsMatch.Value)
                    // Sub-case B: Multiple parameters
                    : string.Join(", ", parametersNames.Select(TypeAliasOrName));

                // Replaced line content
                return RegexPatterns.ParamsNetTypesRegex.Replace(lineContent, replacement);
            }

            // Case #3: Replace single .NET type preceded by "cref" XML tag, e.g. cref="System.Type"
            // by aliases to eliminate fully qualified names
            var crefMatch = RegexPatterns.CrefNetTypeRegex.Match(lineContent);

            if (crefMatch.Success)
            {
                var replacement = TypeAliasOrName(crefMatch.Value);

                // Replaced line content
                return RegexPatterns.CrefNetTypeRegex.Replace(lineContent, replacement);
            }

            // Unchanged line
            return lineContent;
        }
        
        private bool IsSummaryEmpty(MemberInfo memberInfo, ushort indentationLevel, ref string summary)
        {
            // Documentation from XML wasn't restored (fix the "Reader" part) or is not existing (add summaries to the code)
            if (string.IsNullOrWhiteSpace(summary))
            {
                summary = GetPlaceholderSummary(memberInfo, indentationLevel);

                return true;
            }
            
            var match = RegexPatterns.SummaryContentRegex.Match(summary);

            if (match.Success)
            {
                var summaryContent = match.Groups[RegexPatterns.GroupContent].Value;

                // Documentation from XML is existing but there is no text between <summary> tags
                if (string.IsNullOrWhiteSpace(summaryContent))
                {
                    summary = GetPlaceholderSummary(memberInfo, indentationLevel);

                    return true;
                }
            }
            
            return false;
        }

        private string GetPlaceholderSummary(MemberInfo memberInfo, ushort indentationLevel)
        {
            return $"{GetTabs(indentationLevel)}/// <summary>{Environment.NewLine}" +
                   $"{GetTabs(indentationLevel)}/// {GetMemberSpecificSummary(memberInfo)}{Environment.NewLine}" +
                   $"{GetTabs(indentationLevel)}/// </summary>{Environment.NewLine}";
        }

        private string GetMemberSpecificSummary(MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.TypeInfo => $"The interface for {GetSeeCref(GetSourceClass())} class.",
                MemberTypes.Property => GetPropertyPlaceholderSummary((PropertyInfo)memberInfo),
                MemberTypes.Method => $"The method to {GetSeeCref(memberInfo.Name)}.",
                _ => string.Empty
            };
        }

        private string GetSourceClass()
        {
            return Settings.Strategy.UseFullyQualifiedNames
                ? Type.GetTypeOrGenericsName()
                : Type.Name;
        }

        private static string GetPropertyPlaceholderSummary(PropertyInfo propertyInfo)
        {
            var isGetVisible = IsGetVisible(propertyInfo);
            var isSetVisible = IsSetVisible(propertyInfo);
            var isFullAutoProperty = isGetVisible && isSetVisible;

            string firstWords = isFullAutoProperty ? "Gets or sets"
                                                   : OnlySetOrGet(isGetVisible);

            return $"{firstWords} the value of {GetSeeCref(propertyInfo.Name)} property.";
        }

        private static string OnlySetOrGet(bool isGetVisible)
        {
            return isGetVisible ? "Gets" : "Sets";
        }

        private static string GetSeeCref(string textToSurround)
        {
            return $"<see cref=\"{textToSurround}\"/>".Replace(@" />", @"/>");
        }

        // ----------
        // Properties
        // ----------
        private string GetGeneratedProperties()
        {
            var properties = m_reflector.GetApiProperties();
            
            return properties.Length == 0
                ? string.Empty  // Skip this part if there is no public API methods
                : HandleProperties();
            
            // Add a new block of API properties
            string HandleProperties()
            {
                m_propertiesPresent = true;

                return GenerateSeriesOf(properties, GeneratedProperty);
            }
        }

        private string GeneratedProperty(PropertyInfo property)
        {
            return $"{Environment.NewLine}" +
                   $"{GetGeneratedSummary(property)}" +
                   $"{GetTabs(2)}public {TypeAliasOrName(property.PropertyType).Result} {property.Name} {GetAndSet(property)}";
        }

        private static string GetAndSet(PropertyInfo property)
        {
            var getter = IsGetVisible(property) ? "get; " : string.Empty;
            var setter = IsSetVisible(property) ? "set; " : string.Empty;

            return $"{{ {getter + setter}}}";
        }

        private static bool IsGetVisible(PropertyInfo propertyInfo) => propertyInfo.GetGetMethod() != null;

        private static bool IsSetVisible(PropertyInfo propertyInfo) => propertyInfo.GetSetMethod() != null;
        
        // -------
        // Methods
        // -------
        private string GetGeneratedMethods()
        {
            var methods = m_reflector.GetApiMethods();

            return methods.Length == 0
                ? string.Empty  // Skip this part if there is no public API methods
                : HandleMethods();

            // Add a new block of API methods
            string HandleMethods()
            {
                m_methodsPresent = true;
                
                return GenerateSeriesOf(methods, GeneratedMethod);
            }
        }

        private string GeneratedMethod(MethodInfo method)
        {
            return $"{Environment.NewLine}" +
                   $"{GetGeneratedSummary(method)}" +
                   $"{GetTabs(2)}public {TypeAliasOrName(method.ReturnType).Result} {method.Name}({GeneratedParameters(method)});";
        }

        // --------------------
        // Members (in general)
        // --------------------
        private static string GenerateSeriesOf<T>(IReadOnlyList<T> members, Func<T, string> generateRecipe)
            where T : MemberInfo  // Accepts PropertyInfo, MethodInfo, etc.
        {
            var stringBuilder = new StringBuilder(generateRecipe(members[0]) + Environment.NewLine);

            for (var index = 1; index < members.Count; index++)
            {
                stringBuilder.Append(generateRecipe(members[index]));
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        // ----------
        // Parameters
        // ----------
        private string GeneratedParameters(MethodBase method)
        {
            var parameters = method.GetParameters().Select(GeneratedParameter);

            return string.Join(", ", parameters);
        }

        private string GeneratedParameter(ParameterInfo parameter)
        {
            return $"{TypeAliasOrName(parameter.ParameterType).Result} {parameter.Name}";
        }

        // ----------------------
        // Types names or aliases
        // ----------------------
        private string TypeAliasOrName(string typeName)
        {
            return Task.Run(() => typeName.TryGetAliasName(Settings)).Result;
        }

        private async Task<string> TypeAliasOrName(Type type)
        {
            // Type alias: e.g., "string" from .NET "System.String" or "int" from .NET "System.Int32"
            var alias = await type.TryGetAliasName(Settings);

            if (!string.IsNullOrEmpty(alias))
            {
                return alias;
            }

            // Member name: e.g., "Program" if the member is invalid (interface or value type) or not an API candidate (without attribute)
            // or newly generated API interface name: e.g., "IProgram" / custom name if the class was an API candidate (with attribute)
            return await GetNestedClass(type, Settings);
        }

        /// <summary>
        /// Gets a name of API interface (new or already created) for a nested API candidate / or its name if it's not an API candidate.
        /// </summary>
        internal static async Task<string> GetNestedClass(Type type, GenerationSettings settings, bool? overridenFullName = null)
        {
            var useFullName = overridenFullName ?? settings.Strategy.UseFullyQualifiedNames;

            // Case #1: Nested API candidate with API attribute
            if (type.IsApiClass())
            {
                // The type was already created and registered
                if (Register.TryGetGeneratedInterface(type.Name, out var pairedInterface))
                {
                    return pairedInterface;
                }

                // Generating and registering a new interface inside of a nested folder
                var nestedGenerator = new Generator(type, settings.ForNestedFolder());
                
                Register.TryAddGeneratedPair(nestedGenerator);
                
                if (await nestedGenerator.CreateApi())
                {
                    return useFullName ? nestedGenerator.NewInterfaceName.Generation_FullyQualified
                                       : nestedGenerator.NewInterfaceName.Generation_Simplified;
                }
            }

            // Case #2: Class without API attribute
            return useFullName ? type.GetTypeOrGenericsName()
                               : type.Name;
        }
        #endregion

        #region Creation of Register bindings
        /// <summary>
        /// Generates the class defining new APIs bindings/registrations.
        /// </summary>
        /// <param name="settings">The settings used for bindings registration for the provided project.</param>
        internal static async Task CreateRegister(GenerationSettings settings)
        {
            if (Register.BindingsToRegister.ContainsKey(settings.SourceProjectName))
            {
                // Preparations
                var utilitiesPath = Register
                    .TryGetProjectPath(settings.SourceProjectName)
                    .WithPathSubfolders(settings.Strategy.Registration.FilePathSubfolders);

                var bindingsPairs = Register.BindingsToRegister[settings.SourceProjectName];

                // Preparing file content
                var fileContent = settings.Strategy.Copyrights + GetRegistrationMethodContent(settings, bindingsPairs);

                // Generating file with registrations
                await Writer.SaveFile(utilitiesPath, settings.Strategy.Registration.FileName, fileContent);
            }
        }
        
        private static string GetRegistrationMethodContent(GenerationSettings settings, IEnumerable<(string, string)> bindings)
        {
            var namespaceName = settings.SourceProjectPath
                .GetRootNamespace(settings.SourceProjectName)
                .WithNamespaceSubfolders(settings.Strategy.Registration.FilePathSubfolders);

            return $"using {settings.Strategy.Registration.DI_Using};\n\n" +
                   $"namespace {namespaceName}\n" +
                    "{\n" +
                   $"{GetTabs(1)}internal static class {settings.Strategy.Registration.FileName}\n" +
                   $"{GetTabs(1)}{{\n" +
                   $"{GetTabs(2)}/// <summary>\n" +
                   $"{GetTabs(2)}/// Registers the automatically generated API <see langword=\"interface\"/>s in a class handling registrations of Dependency Injection.\n" +
                   $"{GetTabs(2)}/// </summary>\n" +
                   $"{GetTabs(2)}internal static {settings.Strategy.Registration.DI_Service} RegisterApi(this {settings.Strategy.Registration.DI_Service} services)\n" +
                   $"{GetTabs(2)}{{\n" +
                   $"{GetGeneratedPairsOf(settings, bindings)}\n" +
                   $"{GetTabs(3)}return services;\n" +
                   $"{GetTabs(2)}}}\n" +
                   $"{GetTabs(1)}}}\n" +
                    "}";
        }
        
        private static string GetGeneratedPairsOf(GenerationSettings settings, IEnumerable<(string, string)> bindings)
        {
            var stringBuilder = new StringBuilder();

            foreach (var (interfaceGeneric, classGeneric) in bindings)
            {
                stringBuilder.AppendLine($@"{GetTabs(3)}services.{settings.Strategy.Registration.DI_Method}<{interfaceGeneric}, {classGeneric}>();");
                
                m_registrationsCount++;
            }

            return stringBuilder.ToString();
        }
        #endregion

        // ---------
        // Utilities
        // ---------
        internal static string GetTabs(ushort amount)
        {
            return new string(' ', 4 * amount);
        }
    }
}
