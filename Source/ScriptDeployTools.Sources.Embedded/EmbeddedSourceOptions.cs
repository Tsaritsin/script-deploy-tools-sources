using System.Reflection;
using System.Text;

namespace ScriptDeployTools.Sources.Embedded;

/// <summary>
/// Represents configuration options for specifying and filtering embedded resources, such as assemblies, filters,
/// and encoding types.
/// </summary>
public class EmbeddedSourceOptions
{
    #region Assemblies

    private List<Assembly> _assemblies = [];

    /// <summary>
    /// Gets or sets the collection of assemblies used to locate embedded resources.
    /// </summary>
    public IReadOnlyCollection<Assembly> Assemblies
    {
        get => _assemblies;
        set => _assemblies = value.ToList();
    }

    #endregion

    #region Filter

    private Func<string, bool>? _filter;

    /// <summary>
    /// Gets or sets a predicate function used to filter embedded resource names.
    /// </summary>
    public Func<string, bool> Filter
    {
        get => _filter ?? (_ => true);
        set => _filter = value;
    }

    #endregion

    #region Encoding

    private Encoding? _encoding;

    /// <summary>
    /// Gets or sets the encoding used for reading embedded resources. Defaults to UTF-8 if not explicitly specified.
    /// </summary>
    public Encoding Encoding
    {
        get => _encoding ??= Encoding.UTF8;
        set => _encoding = value;
    }

    #endregion
}
