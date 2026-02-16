using System.Diagnostics.CodeAnalysis;

namespace LLVMWithGeneric.Generic;

/// <summary>
/// Base interface for generic definitions with template parameters.
/// </summary>
public interface GenericBase
{
    /// <summary>
    /// Template parameters attached to the generic definition.
    /// </summary>
    public List<GenericTemplate> GenericTemplates { get; }
    
    /// <summary>
    /// Finds a template parameter by name.
    /// </summary>
    /// <param name="name">Template parameter name.</param>
    /// <returns>The matching template parameter.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the template is not found.</exception>
    public GenericTemplate FindTemplate(string name)
    {
        var ret = GenericTemplates.Find(a => a.Name == name);
        if (ret is null)
            throw new KeyNotFoundException(
                $"Could not find the template with the name {name}");
        return ret;
    }

    /// <summary>
    /// Attempts to find a template parameter by name.
    /// </summary>
    /// <param name="name">Template parameter name.</param>
    /// <param name="template">The matched template when found; otherwise null.</param>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryFindTemplate(
        string name, 
        [NotNullWhen(true)]out GenericTemplate? template)
    {
        template = GenericTemplates.Find(a => a.Name == name);
        return template != null;
    }
}
