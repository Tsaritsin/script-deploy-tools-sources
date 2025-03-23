using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ScriptDeployTools.Sources.Embedded;

/// <summary>
/// Represents a source of scripts embedded in assemblies, providing methods to retrieve and process them.
/// </summary>
internal class EmbeddedSource(
    ILogger logger,
    EmbeddedSourceOptions options) : IDeploySource

{
    #region Fields

    /// <summary>
    /// A dictionary containing loaded scripts, keyed by their unique identifiers.
    /// </summary>
    private readonly Dictionary<string, string> _scripts = new();

    #endregion

    #region Methods

    /// <summary>
    /// Retrieves and filters the resource names of the provided assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to extract resources from.</param>
    /// <returns>A collection of tuples containing assemblies and their filtered resource names.</returns>
    private IReadOnlyCollection<(Assembly Assembly, string[] ResourceNames)> GetResourcesByAssemblies(
        IReadOnlyCollection<Assembly> assemblies)
    {
        var resources = assemblies
            .Select(assembly => (
                Assembly: assembly,
                ResourceNames: assembly
                    .GetManifestResourceNames()
                    .Where(options.Filter)
                    .ToArray()))
            .ToArray();

        logger.LogDebug("Loaded resources from {count} assemblies", resources.Length);

        return resources;
    }

    /// <summary>
    /// Extracts the content of an embedded resource from an assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing the resource.</param>
    /// <param name="resourceName">The name of the resource to extract.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A tuple containing the resource's key and content.</returns>
    private async Task<(string Key, string Content)> GetResourceContent(
        Assembly assembly,
        string resourceName,
        CancellationToken cancellationToken)
    {
        await using var stream = assembly.GetManifestResourceStream(resourceName);

        ArgumentNullException.ThrowIfNull(stream);

        using var resourceStreamReader = new StreamReader(stream, options.Encoding, true);

        var resourceContent = await resourceStreamReader.ReadToEndAsync(cancellationToken);

        return (
            Key: resourceName,
            Content: resourceContent);
    }

    /// <summary>
    /// Parses the resources of an assembly, creating scripts from the available resources.
    /// </summary>
    /// <param name="assemblyResources">A tuple containing an assembly and its resource names.</param>
    /// <param name="cancellationToken"></param>
    private async Task ParseAssemblyResources((Assembly Assembly, string[] ResourceNames) assemblyResources,
                                              CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Found {count} resource(s) in {assembly}",
            assemblyResources.ResourceNames.Length,
            assemblyResources.Assembly.FullName);

        foreach (var resourceName in assemblyResources.ResourceNames)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var resourceContent = await GetResourceContent(assemblyResources.Assembly, resourceName, cancellationToken);

            _scripts.Add(resourceContent.Key, resourceContent.Content);
        } // foreach (var resourceName in assemblyResources.ResourceNames)
    }

    /// <summary>
    /// Loads scripts from the configured assemblies if they are not already loaded.
    /// </summary>
    private async ValueTask LoadScripts(CancellationToken cancellationToken)
    {
        if (_scripts.Count != 0)
            return;

        var resourcesByAssemblies = GetResourcesByAssemblies(options.Assemblies);

        foreach (var resourcesInAssembly in resourcesByAssemblies)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ParseAssemblyResources(resourcesInAssembly, cancellationToken);
        }
    }

    #endregion

    #region Implemented IDeploySource

    public async Task<string?> GetScriptContent(string scriptSource,
                                                CancellationToken cancellationToken)
    {
        await LoadScripts(cancellationToken);

        var scriptExists = _scripts.TryGetValue(scriptSource, out var scriptContent);

        return scriptExists ? scriptContent : null;
    }

    #endregion
}
