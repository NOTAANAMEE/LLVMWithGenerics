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

    public GenericModule(
        LLVMContextRef context,
        LLVMModuleRef module,
        LLVMBuilderRef builder,
        GenericMangler mangler,
        TypeRegister register)
    {
        Context = context;
        Module = module;
        Builder = builder;
        Mangler = mangler;
        Register = register;
    }

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

    public LLVMValueRef InstantiateFunction(
        GenericStaticFunc func,
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext)
    {
        return func.Instantiate(typeContext);
    }

    public LLVMTypeRef InstantiateType(
        GenericType type,
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext)
    {
        return type.Instantiate(typeContext);
    }
}
