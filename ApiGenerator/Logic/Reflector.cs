using System.Reflection;
using ApiGenerator.Annotations;
using ApiGenerator.Logic.Extensions;

namespace ApiGenerator.Logic.Logic
{
    /// <summary>
    /// Retrieves a specific metadata of C# members using Reflexion.
    /// </summary>
    internal sealed class Reflector
    {
        private readonly Type m_typeToMap;

        private const BindingFlags ValidApiCandidates = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="Reflector"/> class.
        /// </summary>
        /// 
        /// <param name="typeToMap">
        /// A member of <see langword="class"/> or <see langword="interface"/> type marked with [<see cref="ApiMemberAttribute"/>] annotation.
        /// </param>
        internal Reflector(Type typeToMap)
        {
            m_typeToMap = typeToMap;
        }

        /// <summary>
        /// Gets the properties for API.
        /// </summary>
        internal PropertyInfo[] GetApiProperties()
        {
            return m_typeToMap.GetProperties(ValidApiCandidates)
                .Where(property => property.IsApiMember())
                .ToArray();
        }
        
        /// <summary>
        /// Gets the methods for API.
        /// </summary>
        internal MethodInfo[] GetApiMethods()
        {
            return m_typeToMap.GetMethods(ValidApiCandidates)
                .Where(method => method.IsApiMember())
                .ToArray();
        }

        /// <summary>
        /// Gets the fully qualified name of the assembly.
        /// </summary>
        /// <param name="sourceProjectName">The name of the source project.</param>
        /// <param name="sourceProjectPath">The path to the source project.</param>
        /// <param name="assemblyFullName">Fully qualified name of the assembly.</param>
        /// <returns>
        ///   <see langword="true"/> if the desired <see cref="Assembly"/> was found; otherwise: <see langword="false"/>.
        /// </returns>
        internal static bool TryGetAssemblyName(string sourceProjectName, string sourceProjectPath, out string assemblyFullName)
        {
            assemblyFullName = string.Empty;

            try
            {
                var assemblyName = sourceProjectPath.GetAssemblyName(sourceProjectName);
                var assembly = AssemblyName.GetAssemblyName($@".\{assemblyName}.dll");
                assemblyFullName = assembly.FullName;  // Fully qualified name (with version, culture, etc.)

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
