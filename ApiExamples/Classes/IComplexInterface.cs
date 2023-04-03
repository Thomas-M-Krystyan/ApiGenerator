using ApiGenerator.Examples.Classes.Dependencies;

namespace ApiGenerator.Examples.Classes
{
    #region Suppressing warnings in test Example class
    // ReSharper disable UnusedTypeParameter
    #endregion

    /// <summary>
    /// Testing various cases of complex interface definitions.
    /// </summary>
    internal interface IComplexInterface<TValue, out TModel> where TValue : Item where TModel : NestedApiClass, ISimpleInterface
    {
    }
}
