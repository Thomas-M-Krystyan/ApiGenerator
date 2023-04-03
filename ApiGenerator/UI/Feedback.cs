using System.Diagnostics.CodeAnalysis;

namespace ApiGenerator.Logic.UI
{
    /// <summary>
    /// User communication for Console application context.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class Feedback
    {
        /// <summary>
        /// Shows the result of API Generation.
        /// </summary>
        internal static void PrintResult(bool isSuccess)
        {
            Console.ForegroundColor = isSuccess ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
            Console.WriteLine(isSuccess ? @"API files generated properly" : @"API generation failure!");
            Console.ResetColor();
        }
    }
}
