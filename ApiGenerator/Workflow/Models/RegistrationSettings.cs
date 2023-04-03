using ApiGenerator.Logic.Extensions;

namespace ApiGenerator.Logic.Workflow.Models
{
    /// <summary>
    /// Specific settings about registration of <see langword="interface"/>s and <see langword="class"/>es for Dependency Injection mechanism.
    /// </summary>
    internal sealed class RegistrationSettings
    {
        #region Properties
        /// <summary>
        /// The name of a file with registrations inside.
        /// </summary>
        internal string FileName { get; }
        
        /// <summary>
        /// The path subfolders (rooted from target project) of a file with registrations inside.
        /// <para>
        ///   <example>
        ///     TargetProjectPath \ [<see cref="FilePathSubfolders"/>]
        ///   </example>
        /// </para>
        /// </summary>
        internal string[] FilePathSubfolders { get; }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Which Dependency Injection mechanism will be used (e.g., "Microsoft.Extensions.DependencyInjection.Abstractions").
        /// </summary>
        internal string DI_Using { get; }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Which Dependency Injection service will be used (e.g., "IServiceCollection").
        /// </summary>
        internal string DI_Service { get; }

        // ReSharper disable once InconsistentNaming        
        /// <summary>
        /// Which Dependency Injection service's method should be used to register specific <see langword="interface"/> and its implementation.
        /// </summary>
        internal string DI_Method { get; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationSettings"/> struct.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal RegistrationSettings(string fileName, string[] filePathSubfolders, string diUsing, string diService, string diMethod)
        {
            FileName = fileName.GuardAgainstMissing();
            FilePathSubfolders = filePathSubfolders;
            DI_Using = diUsing.GuardAgainstMissing();
            DI_Service = diService.GuardAgainstMissing();
            DI_Method = diMethod.GuardAgainstMissing();
        }
    }
}
