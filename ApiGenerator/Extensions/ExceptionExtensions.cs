namespace ApiGenerator.Logic.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Exception"/> class.
    /// </summary>
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Enriches the provided generic of <see cref="Exception"/> type with the message to append.
        /// </summary>
        internal static T Enrich<T>(this T exception, string message) where T : Exception
        {
            return (T)Activator.CreateInstance(typeof(T), $@"{exception.Message}{message}");
        }
    }
}
