using ApiGenerator.Annotations;
using ApiGenerator.Logic.Constants;
using ApiGenerator.Logic.Extensions;
using ApiGenerator.Logic.Logic.FluentNamesBuilder;

namespace ApiGenerator.Logic.Logic
{
    // NOTE: Suppress ReSharper warnings about string formatting - left as it's now for readability
    // ReSharper disable LocalizableElement

    /// <summary>
    /// Stores list of API registrations to be made.
    /// </summary>
    internal static class Register
    {
        #region Project paths
        /// <summary>
        /// Returns paths to the .csproj projects.
        /// <list type="bullet">
        ///   <item>Key: Project name</item>
        ///   <item>Value: Project path</item>
        /// </list>
        /// </summary>
        private static readonly IDictionary<string, string> ProjectsPaths = new Dictionary<string, string>();

        /// <summary>
        /// Registers new project paths.
        /// </summary>
        internal static void TryAddProjectPaths(params (string Name, string Path)[] projectPaths)
        {
            foreach (var projectPath in projectPaths)
            {
                if (!ProjectsPaths.ContainsKey(projectPath.Name))
                {
                    ProjectsPaths.Add(projectPath.Name, projectPath.Path);
                }
            }
        }

        /// <summary>
        /// Gets the path for a given project name.
        /// </summary>
        /// <param name="projectName">Use <see cref="Project"/> constants!</param>
        /// <returns>Empty <see langword="string"/> if the project path is not yet registered (supported).</returns>
        internal static string TryGetProjectPath(string projectName)
        {
            _ = ProjectsPaths.TryGetValue(projectName, out var projectPath);

            return projectPath ?? string.Empty;
        }
        #endregion

        #region Class => API Interface pairs
        /// <summary>
        /// The API candidate <see langword="class"/> name paired with its newly generated API <see langword="interface"/>.
        /// <list type="bullet">
        ///   <item>Key: <see langword="class"/> name</item>
        ///   <item>Value: <see langword="interface"/> full name</item>
        /// </list>
        /// </summary>
        private static readonly IDictionary<string, string> CustomNamePairs = new Dictionary<string, string>();

        /// <summary>
        /// Clears the register with source <see langword="class"/>es paired to API <see langword="interface"/>s.
        /// </summary>
        internal static void ClearClassNames()
        {
            CustomNamePairs.Clear();
        }

        /// <summary>
        /// Adds pair of <see langword="class"/> name and matching <see langword="interface"/> full name.
        /// </summary>
        /// <param name="generator">
        ///   The current instance of API <see cref="Generator"/>
        ///   to be used for a specific, task-based API generation.
        /// </param>
        internal static void TryAddGeneratedPair(Generator generator)
        {
            // NOTE: In .NET 6 "TryAdd(key, value)" method is possible
            if (!CustomNamePairs.ContainsKey(generator.Type.Name))
            {
                CustomNamePairs.Add(
                    // Name of the source class (API candidate)
                    generator.Type.Name,
                    // Name of an API interface generated from this class
                    generator.Settings.Strategy.UseFullyQualifiedNames
                        ? generator.NewInterfaceName.Generation_FullyQualified
                        : generator.NewInterfaceName.Generation_Simplified);
            }
        }
        
        /// <summary>
        /// Gets the full name of an <see langword="interface"/> (value) paired with this <see langword="class"/> name (key).
        /// </summary>
        /// <param name="className">The name of a <see langword="class"/>.</param>
        /// <param name="interfaceFullName">The full name of paired <see langword="interface"/>.</param>
        internal static bool TryGetGeneratedInterface(string className, out string interfaceFullName)
        {
            var isSuccess = CustomNamePairs.TryGetValue(className, out var result);
            
            interfaceFullName = result ?? string.Empty;

            return isSuccess;
        }
        #endregion

        #region Created API interfaces
        /// <summary>
        /// The new API <see langword="interface"/>s to be added to the source <see langword="class"/>es based on which they were generated.
        /// <list type="bullet">
        ///   <item>Key: <see langword="interface"/> name to be added to its source <see langword="class"/></item>
        ///   <item>Value: Triplet of (<see langword="interface"/> full name, <see langword="class"/> name, file path of a source <see langword="class"/>)</item>
        /// </list>
        /// </summary>
        internal static IDictionary
            <string, (string InterfaceFullName, string SourceClassName, string SourceClassFilePath)> NewInterfaces { get; }
            = new Dictionary<string, (string, string, string)>();

        /// <summary>
        /// Adds new <see langword="interface"/> name to be appended to its source <see langword="class"/>.
        /// </summary>
        /// <param name="generator">
        ///   The current instance of API <see cref="Generator"/>
        ///   to be used for a specific, task-based API generation.
        /// </param>
        /// <param name="sourceClassFilePath">The file path to the source class to be modified.</param>
        internal static void TryAddApiInterface(Generator generator, string sourceClassFilePath)
        {
            // NOTE: In .NET 6 "TryAdd(key, value)" method is possible
            if (!NewInterfaces.ContainsKey(generator.NewInterfaceName.Generation_Simplified))
            {
                NewInterfaces.Add(generator.NewInterfaceName.Generation_Simplified,
                    // Interface name to be used when replacing references: class name => generated new API interface name
                    (generator.Settings.Strategy.UseFullyQualifiedNames ? generator.NewInterfaceName.Generation_FullyQualified
                            : generator.NewInterfaceName.Generation_Simplified,
                        // Class name to trigger replacement (in API interface was already generated for this class)
                        generator.Type.Name,
                        // An absolute path to the file that should be scanned
                        sourceClassFilePath));
            }
        }
        #endregion

        #region Registration bindings
        /// <summary>
        /// Pairs of <see langword="interface"/> + <see langword="class"/> names to be registered in Module.cs for a specific project.
        /// <list type="bullet">
        ///   <item>Key: Project name for which the bindings registration should be made</item>
        ///   <item>Value: Pair of (IClassName, ClassName) fully qualified bindings to be registered</item>
        /// </list>
        /// </summary>
        internal static IDictionary
            <string, IList<(string InterfaceFullName, string ClassFullName)>> BindingsToRegister { get; }
            = new Dictionary<string, IList<(string, string)>>();

        /// <summary>
        /// Adds new (<see langword="interface"/>, <see langword="class"/>) pair
        /// of bindings to be registered using Dependency Injection (DI) service.
        /// </summary>
        /// <param name="generator">
        ///   The current instance of API <see cref="Generator"/>
        ///   to be used for a specific, task-based API generation.
        /// </param>
        internal static void TryAddBinding(Generator generator)
        {
            // Skip registration of a certain API candidates (to be specified in API Attribute)
            if (!generator.Type.GetShouldBeRegistered())
            {
                return;
            }

            // Ensure to always get .NET name of a class (even the one containing generics) to avoid: "Class`1" cases
            var classNameWithGenerics = generator.Settings.Strategy.UseFullyQualifiedNames
                ? generator.Type.FullyQualified().OriginalName().WithGenerics().Typed(generator.Settings)
                : generator.Type.Simplified().OriginalName().WithGenerics().Typed(generator.Settings);

            // Add new registrations for an existing project (one-to-many relationship)
            if (BindingsToRegister.ContainsKey(generator.Settings.SourceProjectName))
            {
                BindingsToRegister[generator.Settings.SourceProjectName]
                    .Add((generator.Settings.Strategy.UseFullyQualifiedNames ? generator.NewInterfaceName.Registration_FullyQualified
                                                                             : generator.NewInterfaceName.Registration_Simplified, classNameWithGenerics));
            }
            // Create first registration for a new project
            else
            {
                BindingsToRegister[generator.Settings.SourceProjectName] =
                    new List<(string, string)>
                    {
                        (generator.Settings.Strategy.UseFullyQualifiedNames ? generator.NewInterfaceName.Registration_FullyQualified
                                                                            : generator.NewInterfaceName.Registration_Simplified, classNameWithGenerics)
                    };
            }
        }
        #endregion

        #region API annotations in files
        /// <summary>
        /// Location of all [<see cref="ApiClassAttribute"/>] and [<see cref="ApiMemberAttribute"/>] annotations in source file paths.
        /// <list type="bullet">
        ///   <item>Key: The file path (of the source class with annotations)</item>
        ///   <item>Value: The collection of lines numbers in the specific file</item>
        /// </list>
        /// <para>NOTE: The lines numbers will be accessed/processed in reverse order.</para>
        /// </summary>
        private static readonly IDictionary<string, ISet<int>> AnnotationsInPath = new Dictionary<string, ISet<int>>();

        /// <summary>
        /// Adds the position of API annotations in a specific file path.
        /// </summary>
        internal static void TryAddAnnotationPosition(string filePath, int lineNumber)
        {
            if (AnnotationsInPath.ContainsKey(filePath))
            {
                AnnotationsInPath[filePath].Add(lineNumber);
            }
            else
            {
                AnnotationsInPath.Add(filePath, new SortedSet<int> { lineNumber });
            }
        }

        /// <summary>
        /// Clears the register holding positions of API annotations.
        /// </summary>
        internal static void ClearAnnotationsPositions()
        {
            AnnotationsInPath.Clear();
        }

        /// <summary>
        /// Gets the file paths with the list of API annotations positions (in reverse order).
        /// </summary>
        internal static IEnumerable<(string FilePath, int[] LinesNumbers)> GetFilesWithAnnotations()
        {
            return AnnotationsInPath
                .Select(keyValuePair => (keyValuePair.Key, GetAnnotationsPositions(keyValuePair.Key)))
                .ToArray();
        }

        private static int[] GetAnnotationsPositions(string filePath)
        {
            return AnnotationsInPath[filePath].Reverse().ToArray();
        }
        #endregion
    }
}
