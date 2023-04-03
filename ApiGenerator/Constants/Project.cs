namespace ApiGenerator.Logic.Constants
{
    // ReSharper disable InconsistentNaming
    
    /// <summary>
    /// Very common project names to be used FOR and TO logic of API generation.
    /// </summary>
    internal static class Project
    {
        // -----------
        // DEVELOPMENT
        // -----------

        // API Generator projects
        internal const string ApiExamples = nameof(ApiExamples);

        // -------
        // RELEASE
        // -------

        // MapView projects
        internal const string Api = @"Mapping.UI.Api";
        internal const string Presentation_Core = @"Mapping.UI.Windows.Core";
    }
}
