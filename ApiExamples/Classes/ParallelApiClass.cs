using System.Diagnostics.CodeAnalysis;
using ApiExamples.Enums;
using ApiGenerator.Annotations;
using ApiGenerator.Examples.Classes.Dependencies;

namespace ApiGenerator.Examples.Classes
{
    #region Suppressing warnings in test Example class
    #pragma warning disable CS1591
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    #endregion

    // -----------------------
    // Testing only properties
    // -----------------------

    /// <summary>
    /// 
    /// </summary>
    [ApiClass, ExcludeFromCodeCoverage]
    public class ParallelApiClass
    {
        // ------------------------------------------
        // Testing references and circular-references
        // ------------------------------------------

        [ApiMember]
        public ParallelApiClass GetMyself => null;

        [ApiMember]
        public NestedApiClass GetReference => null;

        [ApiMember]
        public Options GetEnums { get; private set; }
    }
}
