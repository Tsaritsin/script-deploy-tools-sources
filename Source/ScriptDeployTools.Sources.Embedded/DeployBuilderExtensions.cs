namespace ScriptDeployTools.Sources.Embedded;

/// <summary>
/// Provides extension methods for configuring and utilizing deployment builders with embedded resources.
/// </summary>
public static class DeployBuilderExtensions
{
    /// <summary>
    /// Configures the deployment builder to use embedded resources as the source for deployment scripts.
    /// </summary>
    /// <param name="builder">The deployment builder to configure.</param>
    /// <param name="applyOptions">An action that configures the embedded resource options.</param>
    /// <returns>The configured deployment builder.</returns>
    public static IDeployBuilder FromEmbeddedResources(this IDeployBuilder builder,
                                                       Action<EmbeddedSourceOptions> applyOptions)
    {
        var options = new EmbeddedSourceOptions();

        applyOptions(options);

        ArgumentNullException.ThrowIfNull(builder.Logger);

        builder.Source = new EmbeddedSource(builder.Logger, options);

        return builder;
    }
}
