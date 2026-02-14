using System.Reflection.Metadata;
using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc(string name, GenericModule module) : 
    GenericValue, GenericBase
{
    public string Name { get; } = name;

    public List<GenericTemplate> GenericTemplates { get; } = [];

    public List<IType> Parameters = [];

    public IType ReturnType = new ILType(LLVMTypeRef.Void);

    internal void AddGenericTemplate(List<string> names)
    {
        foreach (var name in names)
        {
            GenericTemplates.Add(new GenericTemplate(name));
        }
    }

    public void SetParameter(List<IType> parameters)
    {
        this.Parameters = parameters;
    }

    public void SetReturnType(IType returnType)
    {
        ReturnType = returnType;
    }
}