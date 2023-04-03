using System.Reflection;
using System.Runtime.CompilerServices;

namespace ApiGenerator.Logic.Extensions
{
    /// <summary>
    /// Extension methods to provide validation on dedicated types of data.
    /// </summary>
    internal static class ValidationExtensions
    {
        /// <summary>
        /// Ensures that the provided value is not null, empty, or containing only whitespaces.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        internal static T GuardAgainstMissing<T>(this T value,
            // ReSharper disable once UnusedParameter.Global
            #pragma warning disable IDE0060  // Remove unused parameter
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            #pragma warning restore IDE0060
        )
        {
            #region Argument name on which extension method was called  // NOTE: Replace by "CallerArgumentExpression(nameof())" in C# 10
            var callerLineNumber = File.ReadAllLines(sourceFilePath)[sourceLineNumber - 1];
            var callerMethodName = MethodBase.GetCurrentMethod()!.Name;
            var callerMethodPosition = callerLineNumber.IndexOf(callerMethodName + @"()");
            var argumentNameChars = new Stack<char>();

            for (var index = callerMethodPosition - 2; index >= 0; --index)
            {
                if (char.IsWhiteSpace(callerLineNumber[index]))
                {
                    break;
                }

                if (char.IsLetterOrDigit(callerLineNumber[index]))
                {
                    argumentNameChars.Push(callerLineNumber[index]);
                }
            }

            var thisArgumentName = new string(argumentNameChars.ToArray());
            #endregion

            // Strings: null, empty or whitespace
            if (typeof(T) == typeof(string) &&
                string.IsNullOrWhiteSpace((string)Convert.ChangeType(value, typeof(string))))
            {
                throw new ArgumentException($@"The value of ""{thisArgumentName}"" is missing!");
            }

            // Objects: null
            if (value is null)
            {
                throw new ArgumentNullException($@"The value of ""{thisArgumentName}"" is null!");
            }

            return value;
        }
    }
}
