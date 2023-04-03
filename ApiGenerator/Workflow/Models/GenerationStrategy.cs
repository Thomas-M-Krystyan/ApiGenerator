using ApiGenerator.Annotations;
using ApiGenerator.Logic.Constants;
using ApiGenerator.Logic.Extensions;

namespace ApiGenerator.Logic.Workflow.Models
{
    /// <summary>
    /// Customizable settings changing the way of API generation.
    /// </summary>
    internal sealed class GenerationStrategy
    {
        #region Properties
        /// <summary>
        /// Gets or sets the copyrights (top of the class).
        /// </summary>
        internal string Copyrights { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file stamp (top of the class).
        /// </summary>
        internal string FileStamp { get; set; } = string.Empty;
        
        /// <summary>
        /// Determines whether the output names should be fully qualified or not:
        /// </summary>
        /// <returns>
        ///   <code>
        ///     Name:       "<see cref="GenerationSettings"/>"
        ///     Full name*: "<see cref="ApiGenerator"/>.<see cref="Workflow"/>.<see cref="Models"/>.<see cref="GenerationSettings"/>"
        ///
        ///     * Fully qualified type name
        ///   </code>
        /// </returns>
        internal bool UseFullyQualifiedNames { get; set; } = true;

        /// <summary>
        /// Gets the name of a folder to be used when a nested <see langword="class"/>
        /// is detected within API candidate (and which hasn't been mapped separately yet).
        /// <para>
        ///   The mentioned unmapped dependency will be created under this nested folder.
        /// </para>
        /// <para>
        ///   Example:
        ///   <code>
        ///     [<see cref="ApiClassAttribute"/>]
        ///     <see langword="public class"/> Root  // The main object to be converted into API interface
        ///     {
        ///       <see langword="public"/> Nested Dependency { <see langword="get"/>; }  // The nested object
        ///     }
        ///
        ///     [<see cref="ApiClassAttribute"/>]
        ///     <see langword="public class"/> Nested
        ///     {
        ///     }
        ///
        ///     Explanation:
        ///       For settings to generate an API interface from "Root" class
        ///       the "Nested" dependency class, if it wasn't considered separately
        ///       in dedicated settings, will be created in the following way:
        ///
        ///       <see cref="GenerationSettings.TargetCatalogPath"/>     => "IRoot" (API)
        ///         \ <see cref="NestedFolderName"/>  => "INested" (API)
        ///   </code>
        /// </para>
        /// </summary>
        internal string NestedFolderName { get; }

        /// <inheritdoc cref="RegistrationSettings"/>
        internal RegistrationSettings Registration { get; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationStrategy"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        internal GenerationStrategy(string nestedFolderName, RegistrationSettings registrationSettings)
        {
            NestedFolderName = nestedFolderName.GuardAgainstMissing();
            Registration = registrationSettings.GuardAgainstMissing();
        }

        /// <inheritdoc cref="GenerationStrategy(string, RegistrationSettings)"/>
        internal GenerationStrategy(RegistrationSettings registration)
            : this(Folder.Nested, registration)
        {
        }
    }
}
