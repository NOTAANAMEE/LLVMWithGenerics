using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

/// <summary>
/// Common interface for all types used by the generic IR builder.
/// </summary>
public interface IType
{
    /// <summary>
    /// Human-readable name of the type.
    /// </summary>
    public string Name { get; }
}

/// <summary>
/// Wrapper for a concrete LLVM type.
/// </summary>
public class ILType(LLVMTypeRef type): IType
{
    /// <summary>
    /// Underlying LLVM type reference.
    /// </summary>
    public LLVMTypeRef Type { get; } = type;

    /// <summary>
    /// Name of the LLVM struct type.
    /// </summary>
    public string Name => Type.StructName;
}

/// <summary>
/// Template type parameter placeholder.
/// </summary>
public class GenericTemplate(string name): IType
{
    /// <summary>
    /// Template parameter name.
    /// </summary>
    public string Name { get; } = name;
}

/// <summary>
/// Reference to a constructed generic type, e.g. B&lt;T&gt;.
/// </summary>
public class GenericTypeReference : IType
{
    /// <summary>
    /// Rendered name of the constructed generic type.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Mapping from template parameters to concrete argument types.
    /// </summary>
    public Dictionary<GenericTemplate, IType> GenericArguments { get; }

    /// <summary>
    /// Generic type definition.
    /// </summary>
    public GenericType Type { get; }

    /// <summary>
    /// Creates a constructed generic type reference.
    /// </summary>
    /// <param name="generics">Concrete argument types.</param>
    /// <param name="type">Generic type definition.</param>
    public GenericTypeReference(IType[] generics, GenericType type)
    {
        Name = type.Name + $"<{
            string.Join(",", generics.Select(g => g.Name))
        }>";
        if (generics.Length != type.GenericTemplates.Count) 
            throw new ArgumentException(
                $"Length of generics must match. Expected {type.GenericTemplates.Count}, got {generics.Length}"
                );
        GenericArguments = new Dictionary<GenericTemplate, IType>();
        for (var i = 0; i < type.GenericTemplates.Count; i++) 
            GenericArguments[type.GenericTemplates[i]] = generics[i];
        Type = type;
    }
}

/// <summary>
/// Pointer type wrapper.
/// </summary>
public class PointerType(IType type, uint ramType = 0) : IType
{
    /// <summary>
    /// Name of the pointer type.
    /// </summary>
    public string Name => Type.Name + "*";
    /// <summary>
    /// Pointee type.
    /// </summary>
    public IType Type { get; } = type;

    /// <summary>
    /// Address space index.
    /// </summary>
    public uint ramType = ramType;
}
