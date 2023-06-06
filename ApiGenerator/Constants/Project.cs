namespace ApiGenerator.Logic.Constants
{
    // ReSharper disable InconsistentNaming
    
    /// <summary>
    /// Very common project names to be used FROM and TO logic of API generation.
    /// </summary>
    internal static class Project
    {
        // -------
        // TESTING
        // -------

        // API Generator projects
        internal const string ApiExamples = nameof(ApiExamples);

        // -------
        // RELEASE
        // -------

        // MapView projects
        internal const string Api = @"My.Api";
        internal const string Presentation_Core = @"My.UI.Core";
    }
}
