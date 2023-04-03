namespace ApiGenerator.Logic.Logic
{
    internal static class Pathfinder
    {
        /// <summary>
        /// Gets the content from &lt;RootNamespace&gt; tag from .csproj file.
        /// </summary>
        /// <returns>Returns project name if the tag couldn't be found.</returns>
        internal static string GetRootNamespace(this string projectPath, string projectName)
        {
            var csprojFilePath = Path.Combine(projectPath, $@"{projectName}.csproj");

            return Reader.GetRootNamespaceFromFile(csprojFilePath, out var rootNamespace)
                ? rootNamespace
                : projectName;
        }
        
        /// <summary>
        /// Gets the content from &lt;AssemblyName&gt; tag from .csproj file.
        /// </summary>
        /// <returns>Returns project name if the tag couldn't be found.</returns>
        internal static string GetAssemblyName(this string projectPath, string projectName)
        {
            var csprojFilePath = Path.Combine(projectPath, $@"{projectName}.csproj");

            return Reader.GetAssemblyNameFromFile(csprojFilePath, out var assemblyName)
                ? assemblyName
                : projectName;
        }

        /// <summary>
        /// Adds collection of subfolders names (e.g., "Test", "Folder") to the given path as:
        /// <code>
        ///   "Path\Test\Folder".
        /// </code>
        /// </summary>
        internal static string WithPathSubfolders(this string path, params string[] subfolders)
        {
            return subfolders.Length == 0
                ? path
                : Path.Combine(path, Path.Combine(subfolders));
        }

        /// <summary>
        /// Adds collection of subfolders names (e.g., "Test", "Folder") to the given namespace as:
        /// <code>
        ///   "Namespace.Test.Folder".
        /// </code>
        /// </summary>
        internal static string WithNamespaceSubfolders(this string @namespace, params string[] subfolders)
        {
            return subfolders.Length == 0
                ? @namespace
                : $@"{@namespace}.{string.Join(@".", subfolders)}";
        }

        /// <summary>
        /// Gets the paths of C# [.cs] files.
        /// </summary>
        internal static IEnumerable<string> GetCSharpFilesPaths(string initialSourcePath)
        {
            return Directory.GetFiles(initialSourcePath, @"*cs", SearchOption.TopDirectoryOnly);
        }
    }
}
