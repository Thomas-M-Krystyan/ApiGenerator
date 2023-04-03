namespace ApiGenerator.Logic.Logic.Models
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Caches the predicted names for a new API interface which is going to be created.
    /// </summary>
    internal sealed class InterfaceNames
    {
        /// <summary>
        /// Gets the name of the new API <see langword="interface"/> to be used as the file name.
        /// </summary>
        internal string AsFileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the simplified name of the new API <see langword="interface"/> to be used in the member declaration.
        /// </summary>
        internal string Declaration_Simplified { get; set; } = string.Empty;

        /// <summary>
        /// Gets the simplified name of the new API <see langword="interface"/> to be used for referencing in the code / inheritances.
        /// </summary>
        internal string Generation_Simplified { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the fully qualified name of the new API <see langword="interface"/> to be used for referencing in the code / inheritances.
        /// </summary>
        internal string Generation_FullyQualified { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the simplified name of the new API <see langword="interface"/> to be used for registration.
        /// </summary>
        internal string Registration_Simplified { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the fully qualified name of the new API <see langword="interface"/> to be used for registration.
        /// </summary>
        internal string Registration_FullyQualified { get; set; } = string.Empty;
    }
}
