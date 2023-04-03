using SystemPath = System.IO.Path;

namespace ApiGenerator.Logic.Constants
{
    /// <summary>
    /// Very common folder names, structuring MapView solution.
    /// </summary>
    internal static class Folder
    {
        // Nested folder name (fallback option when a place for nested folders is not specified)
        internal const string Nested = @"Common";

        // Utilities folder name
        internal const string Utilities = nameof(Utilities);

        // --------
        // .Sources
        // --------
        internal static string Sources(this string path) => SystemPath.Combine(path, nameof(Sources));

        // ----
        // .Api
        // ----
        internal static string Api(this string path) => SystemPath.Combine(path, nameof(Api));

        // -------
        // .Layers
        // -------
        internal static string Layers(this string path) => SystemPath.Combine(path, nameof(Layers));
        
        // -------------
        // .Presentation
        // -------------
        internal static string Presentation(this string path) => SystemPath.Combine(path, nameof(Presentation));
    }
}
