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
}
