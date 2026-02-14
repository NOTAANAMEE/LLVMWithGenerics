using LLVMSharp.Interop;
using LLVMWithGeneric.Generic;

namespace LLVMWithGeneric.Interface;

/// <summary>
/// Registry for instance functions on concrete types.
/// </summary>
public interface TypeRegister
{
    /// <summary>
    /// Registers an instance function for a concrete owner type.
    /// </summary>
    public void RegisterInstanceFunc(LLVMTypeRef funcOwner, string funcName);
    
    /// <summary>
    /// Gets a previously registered instance function.
    /// </summary>
    public GenericValue GetInstanceFunc(IType funcOwner, string funcName);

    /// <summary>
    /// Validates whether a concrete LLVM type matches a generic definition.
    /// </summary>
    public bool CheckType(GenericBase generic, LLVMTypeRef type);
}
