using System;
using System.Diagnostics.CodeAnalysis;
using ApiGenerator.Annotations;
using ApiGenerator.Examples.Classes.Dependencies;

namespace ApiGenerator.Examples.Classes
{
    #region Suppressing warnings in test Example class
    #pragma warning disable
    // ReSharper disable UnusedType.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedMember.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    // ReSharper disable UnusedParameter.Global
    // ReSharper disable UnusedParameter.Local
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
    // ReSharper disable ReplaceAutoPropertyWithComputedProperty
    // ReSharper disable MemberInitializerValueIgnored
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable StringLiteralTypo
    // ReSharper disable ArrangeObjectCreationWhenTypeEvident
    #endregion

    /// <summary>
    /// Test summary for the entire class
    /// </summary>
    // NOTE: Missing dot in class summary => add "."
    [ApiClass(customName: "531renam3d Spike!",  // NOTE: Change the name using constructor + name is invalid (space, init digits, and "!")
              derivedFrom: typeof(ISimpleInterface))]
    [ExcludeFromCodeCoverage]
    internal sealed class Spike<T> where T : Item
    {
        // -------------------------------------------------------
        // Valid properties: public + API annotation
        // -------------------------------------------------------
        
        // NOTE: Missing summary => Placeholder
        [ApiMember]
        public string GetSetName { get; set; } = string.Empty;  // NOTE: Get + Set
        
        // NOTE: Missing summary => Placeholder
        [ApiMember]
        public string GetOnlySurname { get; } = string.Empty;  // NOTE: Only Get

        // NOTE: Missing summary => Placeholder
        [ApiMember]
        public string SetOnlyNick { private get; set; } = string.Empty;  // NOTE: Only Set

        /// <summary>
        /// Test summary for <see cref="Age"/> property
        /// </summary>
        /// <returns>The age.</returns>
        // NOTE: Missing dot in method summary => add "."
        [ApiMember]
        public int Age { get; set; }

        // -------------------------------------------------------
        // Invalid properties: no API annotation and/or non-public
        // -------------------------------------------------------
        
        public int Version { get; set; }

        [ApiMember]
        internal string Description { get; private set; } = string.Empty;

        [ApiMember]
        protected string Note { get; set; } = string.Empty;

        [ApiMember]
        private bool IsSomething { get; set; }

        private byte SecretProperty { get; set; }

        // -------------------------------------------------------
        // Constructor to be ignored
        // -------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Spike"/> class.
        /// </summary>
        public Spike()
        {
            GetSetName = @"Test";
        }
        
        // -------------------------------------------------------
        // Valid methods: public + API annotation
        // -------------------------------------------------------

        /// <summary>
        /// </summary>
        // NOTE: Missing content of summary => Placeholder
        [ApiMember]
        public T GetName() => null;  // "Item" class is not API => concrete class will be used but that's not preferable.
                                     // Developer probably forgot to add API annotation to the source class
        /// <summary>
        /// Test summary for <see cref="SetName(string)"/> method.
        /// </summary>
        /// <param name="name">Test description for parameter</param>
        // NOTE: Summary + parameter
        // NOTE: Missing dot in parameter => add "."
        [ApiMember]
        public void SetName(string name) => GetSetName = name;

        /// <summary>
        /// </summary>
        /// <param name="firstName">Name</param>
        /// <param name="age">Age .</param>
        /// <param name="nested">Nested .</param>
        // NOTE: Missing content of summary => Placeholder (but only for summary, keep param unchanged)
        [ApiMember]  // NOTE: Change the name using property + name is valid
        public void SetNames(string firstName, bool? age, NestedApiClass nested)  // This object should be mapped into API in "Components" nested folder
        {
        }

        /// <summary>
        /// Multiline
        /// summary
        /// </summary>
        // NOTE: Dot should be put only in the last sentence
        [ApiMember]
        public ParallelApiClass GetNick(NestedApiClass nick) => new ParallelApiClass();

        // -------------------------------------------------------
        // Invalid methods: no API annotation and/or non-public
        // -------------------------------------------------------

        public int GetValue() => 0;

        [ApiMember]
        internal string GetDescription() => Description;

        [ApiMember]
        protected void SetSomething(ISimpleInterface something) => IsSomething = typeof(IComplexInterface<,>).IsInterface;

        [ApiMember]
        private void PrintVersion() => Console.WriteLine(Version);

        private int SecretMethod() => -1;
    }
}
