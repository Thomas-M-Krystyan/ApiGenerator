using System;
using System.Text.RegularExpressions;

namespace ApiGenerator.Annotations
{
    /// <inheritdoc cref="ApiMemberAttribute"/>
    [AttributeUsage(validOn: AttributeTargets.Class, Inherited = false)]
    public sealed class ApiClassAttribute : Attribute
    {
        private string m_customName = string.Empty;

        /// <summary>
        /// The custom name of a decorated member.
        /// </summary>
        public string CustomName
        {
            get => m_customName;
            private set => m_customName = ToValidName(value);
        }

        private Type[] m_derivedFrom = { };

        /// <summary>
        /// The dependencies of a decorated member.
        /// <para>
        ///   Used to move member's derivatives to the newly generated API interface.
        /// </para>
        /// </summary>
        public Type[] DerivedFrom
        {
            get => m_derivedFrom;
            private set => m_derivedFrom = value ?? Array.Empty<Type>();
        }

        /// <summary>
        /// Determines whether this API candidate should be registered as a coupled binging:
        /// <code>
        ///    r.Register&lt;interface, class&gt;();
        /// </code>
        /// </summary>
        public bool ToBeRegistered { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiMemberAttribute"/> class.
        /// </summary>
        /// <inheritdoc cref="ApiClassAttribute"/>
        public ApiClassAttribute(
            string customName = default,
            bool register = true,
            params Type[] derivedFrom)
        {
            CustomName = customName;
            ToBeRegistered = register;
            DerivedFrom = derivedFrom;
        }

        private static string ToValidName(string name)
        {
            // Null or whitespaces value is not allowed
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            // .NET is not allowing to put digits on the beginning of members names
            while (char.IsDigit(name[0]))
            {
                name = name.Substring(1);  // Skip first character and copy the rest of the string
            }

            return Regex.Replace(name, "[^a-zA-Z0-9_.]+", string.Empty, RegexOptions.Compiled);
        }
    }
}
