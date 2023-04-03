using ApiGenerator.Logic.Configuration;
using ApiGenerator.Logic.UI;
using ApiGenerator.Logic.Workflow;

// Setup API Generation
var manager = new ApiGenerationManager(Config.SettingsToUse(true));

// Generating API
var isSuccess = manager.StartApiGeneration();

// Operation status
Feedback.PrintResult(isSuccess.Result);
