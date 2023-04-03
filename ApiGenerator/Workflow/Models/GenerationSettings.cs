using ApiGenerator.Logic.Constants;
using ApiGenerator.Logic.Extensions;
using ApiGenerator.Logic.Logic;

namespace ApiGenerator.Logic.Workflow.Models
{
    /// <summary>
    /// Settings required by <see cref="Generator"/> to complete its task:
    /// <para>
    ///   Generating API interfaces for a project from a source Logic project into a target Api project within MapView.
    /// </para>
    /// </summary>
    internal sealed class GenerationSettings
    {
        #region Properties
        // -------
        // Sources
        // -------
        internal string SourceProjectName { get; }

        internal string SourceProjectPath { get; }

        internal string SourceCatalogPath { get; }

        // -------
        // Targets
        // -------
        internal string TargetCatalogPath { get; private set; }

        internal string TargetNamespace { get; private set; }
        
        // --------
        // Settings
        // --------

        /// <inheritdoc cref="GenerationStrategy"/>
        internal GenerationStrategy Strategy { get; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationSettings"/> class.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        internal GenerationSettings(string sourceProjectName, string targetProjectName,
                                    string sourceProjectPath, string targetProjectPath,
                                    string[] sourceSubfolders, string[] targetSubfolders,
                                    GenerationStrategy strategy)
        {
            // Sources
            SourceProjectName = sourceProjectName.GuardAgainstMissing();
            SourceProjectPath = sourceProjectPath.GuardAgainstMissing();
            SourceCatalogPath = SourceProjectPath.WithPathSubfolders(sourceSubfolders);

            // Targets
            TargetCatalogPath = targetProjectPath.GuardAgainstMissing().WithPathSubfolders(targetSubfolders);
            TargetNamespace = targetProjectPath.GetRootNamespace(targetProjectName.GuardAgainstMissing())
                                               .WithNamespaceSubfolders(targetSubfolders);
            // Settings
            Strategy = strategy.GuardAgainstMissing();
        }

        /// <summary>
        /// <inheritdoc cref="GenerationSettings(string, string, string, string, string[], string[], GenerationStrategy)"/>
        /// <para>
        ///   This constructor is using predefined project paths.
        /// </para>
        /// </summary>
        internal GenerationSettings(string sourceProjectName, string targetProjectName,
                                    string[] sourceSubfolders, string[] targetSubfolders,
                                    GenerationStrategy strategy)
            : this(sourceProjectName, targetProjectName,
                   Register.TryGetProjectPath(sourceProjectName),
                   Register.TryGetProjectPath(targetProjectName),
                   sourceSubfolders, targetSubfolders,
                   strategy)
        {
        }

        /// <summary>
        /// <inheritdoc cref="GenerationSettings(string, string, string, string, string[], string[], GenerationStrategy)"/>
        /// <para>
        ///   This constructor is using predefined project paths and default MapView API project as target.
        /// </para>
        /// </summary>
        internal GenerationSettings(string sourceProjectName, string[] sourceSubfolders,
                                    string[] targetSubfolders, GenerationStrategy strategy)
            : this(sourceProjectName, Project.Api,
                   sourceSubfolders, targetSubfolders,
                   strategy)
        {
        }

        /// <summary>
        /// Updates given <see cref="GenerationSettings"/> to consider a new level of nesting.
        /// </summary>
        internal GenerationSettings ForNestedFolder()
        {
            // Add nested folder to the path and namespace but prevent recursive loop ..Path\Nested\Nested\Nested...
            if (TargetCatalogPath.Contains(Strategy.NestedFolderName))
            {
                return this;
            }

            // Updating settings
            TargetCatalogPath = Path.Combine(TargetCatalogPath, Strategy.NestedFolderName);
            TargetNamespace = $@"{TargetNamespace}.{Strategy.NestedFolderName}";

            return this;
        }
    }
}
