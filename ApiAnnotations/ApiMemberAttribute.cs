using System;

namespace ApiGenerator.Annotations
{
    /// <summary>
    /// Allows to recognize members to be considered during mapping into API interfaces.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(validOn: AttributeTargets.Property | AttributeTargets.Method, Inherited = false)]
    public sealed class ApiMemberAttribute : Attribute
    {
    }
}
