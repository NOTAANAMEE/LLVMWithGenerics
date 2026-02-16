using System.Diagnostics.CodeAnalysis;

namespace LLVMWithGeneric.Generic;

/// <summary>
/// Extension methods for generic definitions.
/// </summary>
public static class GenericBaseExtensions
{
    /// <summary>
    /// Finds a template parameter by name.
    /// </summary>
    /// <param name="generic">Generic definition.</param>
    /// <param name="name">Template parameter name.</param>
    /// <returns>The matching template parameter.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the template is not found.</exception>
    public static GenericTemplate FindTemplate(this GenericBase generic, string name)
    {
        return generic.FindTemplate(name);
    }

    /// <summary>
    /// Attempts to find a template parameter by name.
    /// </summary>
    /// <param name="generic">Generic definition.</param>
    /// <param name="name">Template parameter name.</param>
    /// <param name="template">The matched template when found; otherwise null.</param>
    /// <returns>True if found; otherwise false.</returns>
    public static bool TryFindTemplate(
        this GenericBase generic,
        string name,
        [NotNullWhen(true)] out GenericTemplate? template)
    {
        return generic.TryFindTemplate(name, out template);
    }

    /// <summary>
    /// Attempts to find a template parameter by name.
    /// </summary>
    /// <param name="generic">Generic definition.</param>
    /// <param name="name">Template parameter name.</param>
    /// <param name="template">The matched template when found; otherwise null.</param>
    /// <returns>True if found; otherwise false.</returns>
    public static bool TryGetTemplate(
        this GenericBase generic,
        string name,
        [NotNullWhen(true)] out GenericTemplate? template)
    {
        return generic.TryFindTemplate(name, out template);
    }
}
