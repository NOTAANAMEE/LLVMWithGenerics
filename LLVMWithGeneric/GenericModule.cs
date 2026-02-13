using LLVMSharp.Interop;
using LLVMWithGeneric.Generic;
using LLVMWithGeneric.Interface;

namespace LLVMWithGeneric;

public class GenericModule
{
    public readonly GenericMangler Mangler;
    
    public readonly TypeRegister Register;
    
    public readonly LLVMModuleRef Module;
    
    public readonly LLVMBuilderRef Builder;
    
    public readonly LLVMContextRef Context;
    
    internal Dictionary<string, LLVMTypeRef> InstantiatedTypes = [];

    public GenericStaticFunc AddFunction(
        string name, List<string> genericTemplates)
    {
        var func = new GenericStaticFunc(name, this);
        func.AddGenericTemplate(genericTemplates);
        return func;
    }

    public GenericType AddGenericType(
        string name, List<string> genericTemplates, bool packed)
    {
        var type = new GenericType(name, this, packed);
        type.AddGenericTemplate(genericTemplates);
        return type;
    }
}