using LLVMSharp.Interop;
using LLVMWithGeneric.Generic;

namespace LLVMWithGeneric.Interface;

public interface TypeRegister
{
    public void RegisterInstanceFunc(LLVMTypeRef funcOwner, string funcName);
    
    public GenericValue GetInstanceFunc(IType funcOwner, string funcName);

    public bool CheckType(GenericBase generic, LLVMTypeRef type);
}