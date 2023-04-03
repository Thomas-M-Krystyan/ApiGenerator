using ApiGenerator.Annotations;
using ApiGenerator.Logic.Logic;
using ApiGenerator.Logic.Logic.FluentNamesBuilder;
using ApiGenerator.Logic.Workflow.Models;

namespace ApiGenerator.Logic.Workflow
{
    /// <summary>
    /// Manages creation of multiple context-based API outputs.
    /// </summary>
    internal sealed class ApiGenerationManager
    {
        private readonly IEnumerable<GenerationSettings> m_tasksSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiGenerationManager"/> class.
        /// </summary>
        /// <param name="tasksSettings">The settings for API generation tasks to be picked up.</param>
        public ApiGenerationManager(IEnumerable<GenerationSettings> tasksSettings)
        {
            m_tasksSettings = tasksSettings;
        }

        internal async Task<bool> StartApiGeneration()
        {
            try
            {
                // Processing multiple tasks
                foreach (var taskSettings in m_tasksSettings)
                {
                    // Loads XML documentation for the specified project
                    Reader.LoadXmlDocumentation(taskSettings.SourceProjectName, taskSettings.SourceProjectPath);

                    // Retrieve C# files from the source directory
                    var sourceFilesPaths = Pathfinder.GetCSharpFilesPaths(taskSettings.SourceCatalogPath);

                    foreach (var sourceFilePath in sourceFilesPaths)
                    {
                        // Preparations
                        var type = sourceFilePath.GetTypeFromFile(taskSettings);

                        // Generating API
                        var generator = new Generator(type, taskSettings);

                        if (await generator.CreateApi())
                        {
                            // Match concrete class with its API interface equivalent
                            Register.TryAddGeneratedPair(generator);

                            // Preserves a given interface and path to the source class to be appended later
                            Register.TryAddApiInterface(generator, sourceFilePath);

                            // Preserves (if desired) a given <interface, class> binding pair to be registered later
                            Register.TryAddBinding(generator);
                        }
                        else
                        {
                            Console.WriteLine($@"API for ""{type.Name.TrimGenerics()}.cs"" wasn't generated");
                        }

                        // NOTE: It's normal to not generate an API if class is not an API candidate
                    }

                    // Updating source classes by adding newly generated API interfaces and replacing references
                    if (!Writer.AppendNewInterfaces())
                    {
                        return false;
                    }

                    // Generate file of registration bindings for the specific project
                    await Generator.CreateRegister(taskSettings);

                    if (Generator.NothingWasRegistered)
                    {
                        Console.WriteLine(
                            $@"{Environment.NewLine}Nothing was registered. Check [{nameof(ApiClassAttribute)}] settings if that wasn't intended");

                        return false;
                    }
                }

                // Purge
                Console.Write($@"{Environment.NewLine}-----------------------------------------------------------------------------" +
                              $@"{Environment.NewLine}WARNING: type ""clean"" to remove all API annotations from all source classes" +
                              $@"{Environment.NewLine}or press any key to continue without removing anything: ");

                var userInput = Console.ReadLine();
                if (userInput == @"clean")
                {
                    await Writer.FinalCleanup();

                    Console.WriteLine($@"{Environment.NewLine}API attributes were removed!");
                }
                
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(@"-----------------------------------------------------------------------------" +
                                 $@"{Environment.NewLine}{exception.Message}");

                return false;
            }
            finally
            {
                Console.WriteLine(@"-----------------------------------------------------------------------------");
            }
        }
    }
}
