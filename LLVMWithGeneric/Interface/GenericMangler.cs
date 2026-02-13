using LLVMSharp.Interop;

namespace LLVMWithGeneric.Interface;

public interface GenericMangler
{
    public abstract string MangleFunc(string funcName, LLVMTypeRef[] typeNames);
    
    public abstract string MangleType(string funcName, LLVMTypeRef[] typeNames);
}