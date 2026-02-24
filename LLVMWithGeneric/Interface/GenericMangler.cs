using LLVMSharp.Interop;

namespace LLVMWithGeneric.Interface;

/// <summary>
/// Provides name mangling for instantiated generic functions and types.
/// </summary>
public interface GenericMangler
{
    /// <summary>
    /// Mangles a function name given the concrete parameter types.
    /// </summary>
    /// <param name="funcName">Original generic function name.</param>
    /// <param name="typeNames">Concrete type arguments used for instantiation.</param>
    /// <returns>A unique concrete function symbol name.</returns>
    public abstract string MangleFunc(string funcName, LLVMTypeRef[] typeNames);
    
    /// <summary>
    /// Mangles a type name given the concrete type arguments.
    /// </summary>
    /// <param name="funcName">Original generic type name.</param>
    /// <param name="typeNames">Concrete type arguments used for instantiation.</param>
    /// <returns>A unique concrete type name.</returns>
    public abstract string MangleType(string funcName, LLVMTypeRef[] typeNames);

    /// <summary>
    /// Mangles a static variable name for an instantiated concrete owner type.
    /// </summary>
    /// <param name="typeName">Concrete owner type name.</param>
    /// <param name="varName">Declared static variable name.</param>
    /// <returns>A unique global variable symbol name.</returns>
    public string MangleStaticVariable(string typeName, string varName);
}
