namespace ApiGenerator.Logic.Constants
{
    /// <summary>
    /// Very common paths used to retrieve components of your source project (projects, subfolders).
    /// </summary>
    internal static class Paths
    {
        // [Project]\bin\Debug
        private static string WorkingDirectory { get; } = Environment.CurrentDirectory;

        // [Project]
        internal static string RepositoryDirectory { get; } = Directory.GetParent(WorkingDirectory)!.Parent!.FullName;

        // [Project]\Sources
        internal static string SourcesDirectory { get; } = RepositoryDirectory.Sources();
    }
}
