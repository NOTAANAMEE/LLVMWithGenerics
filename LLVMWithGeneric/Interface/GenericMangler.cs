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
    public abstract string MangleFunc(string funcName, LLVMTypeRef[] typeNames);
    
    /// <summary>
    /// Mangles a type name given the concrete type arguments.
    /// </summary>
    public abstract string MangleType(string funcName, LLVMTypeRef[] typeNames);

    public string MangleStaticVariable(string typeName, string varName);
}
