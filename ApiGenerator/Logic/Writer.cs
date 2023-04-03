using System.Text.RegularExpressions;
using ApiGenerator.Annotations;
using ApiGenerator.Logic.Constants;

namespace ApiGenerator.Logic.Logic
{
    /// <summary>
    /// Writes a specific content to the given file path.
    /// </summary>
    internal static class Writer
    {
        internal static readonly string[] CSharpDelimiters = { @" ", @"(", @")", @",", @"<", @">" };  // "public (ClassA, T) Get<ClassC, T>(ClassB referenceB, T referenceD) where T : ClassD"

        /// <summary>
        /// Process all registered <see langword="interface"/>s to be appended.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if the all <see langword="interface"/>s were appended; otherwise: <see langword="false"/>.
        /// </returns>
        internal static bool AppendNewInterfaces()
        {
            foreach (var record in Register.NewInterfaces)
            {
                var (interfaceFullName, sourceClassName, sourceClassFilePath) = record.Value;

                // Updating source class with using new interface name
                if (!UpdateSourceClass(interfaceFullName, sourceClassName, sourceClassFilePath))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Writes the given content to a file with given path and name.
        /// </summary>
        /// <param name="targetPath">The target file path.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileContent">The content to be stored into a file.</param>
        /// <returns>
        ///   <see langword="true"/> if writing was successful; otherwise: an exception will occur.
        /// </returns>
        internal static async Task<bool> SaveFile(string targetPath, string fileName, params string[] fileContent)
        {
            // Prevents an attempt of creating two classes with the same names
            if (Register.NewInterfaces.ContainsKey(fileName))
            {
                return false;
            }

            // Create specified directory unless it's already existing
            Directory.CreateDirectory(targetPath);
            
            // Creating or overwriting an existing file
            using var writer = new StreamWriter(Path.Combine(targetPath, $@"{fileName}.cs"));

            // Saving content to the file
            foreach (var line in fileContent)
            {
                // 1. Add new line to the file
                // 2. Enforce "Windows CRLF" newlines
                await writer.WriteLineAsync(
                    RegexPatterns.NewlineRegex.Replace(line, RegexPatterns.GroupWindowsCrLfNewline));
            }

            return true;  // NOTE: False is not necessary. Failed saving will end up as a system exception
        }

        /// <summary>
        /// Cleans all of the source files where [<see cref="ApiClassAttribute"/>] or [<see cref="ApiMemberAttribute"/>]
        /// annotations were previously found.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if cleanup was successful; otherwise: an exception will occur.
        /// </returns>
        internal static async Task FinalCleanup()
        {
            try
            {
                // Prepare file paths with line numbers where API annotations were previously detected
                var pathsWithAnnotationLines = Register.GetFilesWithAnnotations();

                foreach (var (filePath, linesNumbers) in pathsWithAnnotationLines)
                {
                    if (linesNumbers.Length > 0)
                    {
                        // Get the current file content
                        var contentLines = Reader.GetFileContent_Lines(filePath);
                        CleanApiAttributes(linesNumbers, contentLines);  // Collections are changed by reference (no need to return it)

                        // Save to the current file cleaned up content
                        using var writer = new StreamWriter(filePath);

                        foreach (var contentLine in contentLines)
                        {
                            await writer.WriteLineAsync(contentLine);
                        }
                    }
                }
            }
            finally
            {
                Register.ClearAnnotationsPositions();
            }
        }

        #region Update source class file
        /// <summary>
        /// Updates the list of inheritances and usages for a specified <see langword="class"/>
        /// by adding already existing/newly generated API <see langword="interface"/>s to it.
        /// </summary>
        /// <param name="targetInterfaceName">The name of a newly generated API <see langword="interface"/>.</param>
        /// <param name="sourceClassName">The name of the source <see langword="class"/>.</param>
        /// <param name="sourceClassFilePath">The path to the original source file.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see langword="class"/> was updated; otherwise: <see langword="false"/>.
        /// </returns>
        private static bool UpdateSourceClass(string targetInterfaceName, string sourceClassName, string sourceClassFilePath)
        {
            var isRecognized = false;
            var isModified = false;

            string[] fileContent = File.ReadAllLines(sourceClassFilePath);

            for (var index = 0; index < fileContent.Length; index++)
            {
                var currentLine = fileContent[index];

                // Remember where API annotations where placed in the source class
                if (currentLine.ContainsApiAnnotation())
                {
                    Register.TryAddAnnotationPosition(sourceClassFilePath, lineNumber: index);
                }

                // Updating references and inheritances
                var classDefinitionMatch = RegexPatterns.ClassDeclarationRegex.Match(currentLine);

                // Line with class definition was found
                if (classDefinitionMatch.Success)
                {
                    var inheritances = classDefinitionMatch.Groups[RegexPatterns.GroupClassInheritance].Value;
                    var inheritancesCollection = inheritances != string.Empty
                        // Case #1 => public class Example : OldA, OldB
                        ? Regex.Split(inheritances, @",", RegexOptions.Compiled)  // Split ": A, B" into { "A", "B" }, or "A" => { "A" }
                               .Select(inheritance => inheritance.Trim())  // Do not preserve surrounding spaces, to not concatenate later as: "A , B"
                               .ToArray()
                        // Case #2 => public class Example
                        : Array.Empty<string>();

                    // Check only interfaces (without possible "where" constraints)
                    var trimmedInterfaces = RemoveConstraints(inheritancesCollection);

                    // Check if the class is not implementing the given interface already
                    if (!trimmedInterfaces.Contains(targetInterfaceName))
                    {
                        // Add new interface to the list of inheritances
                        var newInheritances = inheritancesCollection.Length > 0
                            // Case #1 => OldA, OldB, INewInterface
                            ? string.Join(@", ", new List<string>(inheritancesCollection) { targetInterfaceName })
                            // Case #2 => INewInterface
                            : targetInterfaceName;

                        // Restore class specification line with new interfaces
                        var newCurrentLine =
                            classDefinitionMatch.Groups[RegexPatterns.GroupClassDeclaration].Value +
                            classDefinitionMatch.Groups[RegexPatterns.GroupClassName].Value +
                            GenericsAndInheritances(classDefinitionMatch, newInheritances);

                        // Replace old class specification line
                        fileContent[index] = newCurrentLine;

                        // Success: File will be overriden
                        isModified = true;
                    }

                    // Success: No need to change
                    isRecognized = true;
                }
                // Update return types and parameters
                else
                {
                    fileContent[index] = UpdateReferences(currentLine, sourceClassName, CSharpDelimiters);
                }
            }

            // Saving new content to the file
            TryToSave(isModified, isRecognized, sourceClassFilePath, fileContent);

            return isRecognized;
        }

        private static IEnumerable<string> RemoveConstraints(IEnumerable<string> originalInheritances)
        {
            return originalInheritances.Select(inheritance =>
            {
                var wherePosition = inheritance.IndexOf(@" where ");

                return wherePosition != -1
                    ? inheritance.Substring(0, wherePosition)
                    : inheritance;
            });
        }

        private static void TryToSave(bool isModified, bool isRecognized, string sourceFilePath, IEnumerable<string> fileContent)
        {
            if (isModified)
            {
                File.WriteAllLines(sourceFilePath, fileContent);
            }

            if (!isRecognized)
            {
                Console.WriteLine($@"Regular Expression could not find the class name in ""{sourceFilePath}"" file");
            }
        }

        // ------------------------
        // Get new API inheritances
        // ------------------------
        private static string GenericsAndInheritances(Match classDefinitionMatch, string inheritances)
        {
            // 1st attempt to get generic constraint from the source class declaration
            var genericConstraint = classDefinitionMatch.Groups[RegexPatterns.GroupGenericConstraint].Value;
            var noGenericConstraints = string.IsNullOrEmpty(genericConstraint);

            // 2nd attempt to get generic constraint from matched inheritances
            if (noGenericConstraints)
            {
                genericConstraint = RegexPatterns.GenericConstraintRegex.Match(inheritances)
                    .Groups[RegexPatterns.GroupGenericConstraint].Value;

                noGenericConstraints = string.IsNullOrEmpty(genericConstraint);
            }

            // Case #1 (only inheritances)                  => " : IBehaviorA, IBehaviorB"
            // Case #2 (inheritances and generic constraint => " : IBehaviorA<T>, IBehaviorB where T : IValue"
            return $@" : {inheritances} {(noGenericConstraints ? string.Empty : genericConstraint.Trim())}";
        }

        // ----------------------
        // Use new API references
        // ----------------------
        /// <summary>
        /// Replaces occurrences of concrete source <see langword="class"/>es with their vis à vis
        /// target <see langword="interface"/>s created during API generation.
        /// </summary>
        /// <param name="sentence">The line from the parsed .cs file.</param>
        /// <param name="sourceClassName">The name of the source <see langword="class"/> to be filtered out.</param>
        /// <param name="separators">The list of C#-specific separators.</param>
        /// <returns>Replaced content of a single line from a given file.</returns>
        internal static string UpdateReferences(string sentence, string sourceClassName, IReadOnlyList<string> separators)
        {
            if (sentence.Contains($@" class {sourceClassName}") ||  // Ignore class declaration
                sentence.Contains($@" {sourceClassName}("))         // Ignore constructors
            {
                return sentence;
            }

            // Do not replace references in cases:  "ICat Cat { get; set; } = new Cat();" / "ICat GetCat() => new Cat();", etc.
            var match = RegexPatterns.ObjectInitializationRegex.Match(sentence);

            // Case #1: There is object initialization assignment or lambda expression in this line
            if (match.Success)
            {
                var lineToBeUpdated = match.Groups[RegexPatterns.GroupBeforeAssignmentOrLambda].Value;
                var lineToStayUnchanged = match.Groups[RegexPatterns.GroupWithAssignmentOrLambda].Value;

                return $@"{ReplaceOldReference(lineToBeUpdated, separators)}{lineToStayUnchanged}";
            }

            // Case #2: There is nothing unusual in this line
            return ReplaceOldReference(sentence, separators);
        }
        
        private static string ReplaceOldReference(string sentence, IReadOnlyList<string> separators, int separatorIndex = 0)
        {
            var words = sentence.Split(separators[separatorIndex].ToCharArray());
		
            for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
            {
                var currentWord = words[wordIndex];

                if (Register.TryGetGeneratedInterface(currentWord, out var interfaceFullName))
                {
                    words[wordIndex] = interfaceFullName;
                }
                else
                {
                    // Recursive loop if word after breakout still contains other separators
                    const int nextSeparatorAfterSpace = 1;

                    for (int nextSeparatorIndex = nextSeparatorAfterSpace; nextSeparatorIndex < separators.Count; nextSeparatorIndex++)
                    {
                        if (currentWord.Contains(separators[nextSeparatorIndex]))
                        {
                            words[wordIndex] = ReplaceOldReference(currentWord, separators, nextSeparatorIndex);
                        }
                    }
                }
            }
		
            return string.Join(separators[separatorIndex], words);
        }
        #endregion

        #region Remove API annotations
        private static bool ContainsApiAnnotation(this string line)
        {
            return RegexPatterns.ApiAttributeRegex.Match(line).Success;
        }

        /// <summary>
        /// Removes the API attributes from the source class in a smart way.
        /// </summary>
        internal static void CleanApiAttributes(IEnumerable<int> linesNumbers, IList<string> contentLines)
        {
            foreach (var lineNumber in linesNumbers)
            {
                var apiAttributeMatch = RegexPatterns.ApiAttributeRegex.Match(contentLines[lineNumber]);

                // Case #1: No attributes were found
                if (!apiAttributeMatch.Success)
                {
                    continue;
                }
                
                // Case #2: Line contains only [Api...] attribute. Entire line can be safely removed
                if (apiAttributeMatch.Groups[RegexPatterns.GroupBeforeApiAttribute].Value == @"[" &&
                    apiAttributeMatch.Groups[RegexPatterns.GroupAfterApiAttribute].Value == @"]")
                {
                    // Remove lines with API annotations
                    contentLines.RemoveAt(lineNumber);
                }
                // Case #3: Combined attributes: e.g., "[Obsolete, ApiClass, DataMember]" or "[Obsolete][ApiClass]" or "[ApiClass, Obsolete]", and similar
                else
                {
                    // Cut out the API attributes
                    var withoutApiAttribute =
                        apiAttributeMatch.Groups[RegexPatterns.GroupBeforeApiAttribute].Value +
                        apiAttributeMatch.Groups[RegexPatterns.GroupAfterApiAttribute].Value;

                    // ------------------
                    // Clean up leftovers
                    // ------------------

                    // Inner spaces
                    var cleanedLine =
                        Regex.Replace(withoutApiAttribute, @"\s+\]", @"]", RegexOptions.Compiled);
                    cleanedLine =
                        Regex.Replace(cleanedLine, @"\[\s+", @"[", RegexOptions.Compiled);

                    // Multi spaces
                    cleanedLine = Regex.Replace(cleanedLine, @"\]\s+\[", @"][", RegexOptions.Compiled);

                    // Artifacts surrounding square brackets
                    cleanedLine = cleanedLine.Replace(@"[]", string.Empty);
                    cleanedLine = Regex.Replace(cleanedLine, @"\s*\,*\s*\]", @"]", RegexOptions.Compiled);
                    cleanedLine = Regex.Replace(cleanedLine, @"\[\s*\,*\s*", @"[", RegexOptions.Compiled);

                    // Commas
                    cleanedLine = cleanedLine.Replace(@",,", @", ");
                    cleanedLine = Regex.Replace(cleanedLine, @"\s*\,\s*\,*\s*", @", ", RegexOptions.Compiled);

                    // Remove lines that are empty (after cleaning up) or where only comment backslashes are present
                    if (string.IsNullOrWhiteSpace(cleanedLine) ||
                        Regex.Match(cleanedLine, @".*//\s*$", RegexOptions.Compiled).Success)
                    {
                        contentLines.RemoveAt(lineNumber);
                    }
                    // Replaces old line with the cleaned up one
                    else
                    {
                        contentLines[lineNumber] = cleanedLine;
                    }
                }
            }
        }
        #endregion
    }
}
