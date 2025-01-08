using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ScriptRunner.Plugins.Attributes;
using ScriptRunner.Plugins.DatasetService.Interfaces;
using ScriptRunner.Plugins.Models;
using ScriptRunner.Plugins.Utilities;

namespace ScriptRunner.Plugins.DatasetService;

/// <summary>
///     A plugin that registers and provides ...
/// </summary>
/// <remarks>
///     This plugin demonstrates how to ...
/// </remarks>
[PluginMetadata(
    "DatasetService",
    "This plugin provides functionality for processing datasets based on schema definitions. It supports grouping, filtering, and aggregation operations on datasets.",
    "Peter van de Pas",
    "1.0.0",
    PluginSystemConstants.CurrentPluginSystemVersion,
    PluginSystemConstants.CurrentFrameworkVersion,
    ["IDatasetService"])]
public class Plugin : BaseAsyncServicePlugin
{
    /// <summary>
    ///     Gets the name of the plugin.
    /// </summary>
    public override string Name => "DatasetService";

    /// <summary>
    /// Asynchronously initializes the plugin using the provided configuration.
    /// </summary>
    /// <param name="configuration">A dictionary containing configuration key-value pairs for the plugin.</param>
    public override async Task InitializeAsync(IEnumerable<PluginSettingDefinition> configuration)
    {
        if (LocalStorage == null)
        {
            throw new InvalidOperationException(
                "LocalStorage has not been initialized. " +
                "Ensure the host injects LocalStorage before calling InitializeAsync.");
        }
        
        // Store settings into LocalStorage
        PluginSettingsHelper.StoreSettings(LocalStorage, configuration);

        // Optionally display the settings
        PluginSettingsHelper.DisplayStoredSettings(LocalStorage);
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Asynchronously registers the plugin's services into the application's dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    public override async Task RegisterServicesAsync(IServiceCollection services)
    {
        // Simulate async service registration (e.g., initializing an external resource)
        await Task.Delay(50);
        services.AddSingleton<IDatasetService, DatasetService>();
    }
    
    /// <summary>
    /// Asynchronously executes the plugin's main functionality.
    /// </summary>
    public override async Task ExecuteAsync()
    {
        // Example execution logic
        await Task.Delay(50);
        
        var storedSetting = PluginSettingsHelper.RetrieveSetting<string>(LocalStorage, "PluginName");
        Console.WriteLine($"Retrieved PluginName: {storedSetting}");
    }
}