using System.Diagnostics;
using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public class GenericType(string name, GenericModule module, bool packed): GenericBase
{
    internal GenericModule Module { get; } = module;

    public string Name { get; } = name;
    
    public bool Packed { get; } = packed;
    
    public List<GenericTemplate> GenericTemplates { get; } = [];

    public IType[] Fields { get; private set; } = [];

    internal LLVMTypeRef Instantiate(
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext)
    {
        var instantiatedTypes = GenericTemplates
            .Select(t => typeContext[t])
            .ToArray();
        var finalName = Module.Mangler.MangleType(
            Name, instantiatedTypes);
        if (Module.InstantiatedTypes.TryGetValue(finalName, out var inst))
            return inst;
        var typeRef = Module.Context.CreateNamedStruct(finalName);
        
        Module.InstantiatedTypes[finalName] = typeRef;
        
        var fields = Fields.Select(t => InstantiateType(typeContext, t)).ToArray();
        typeRef.StructSetBody(fields, Packed);
        
        return typeRef;
    }

    public GenericTemplate AddTemplate(string templateName)
    {
        var template = new GenericTemplate(templateName);
        GenericTemplates.Add(template);
        return template;
    }

    public void SetField(IType[] fields)
    {
        Fields = fields;
    }
    
    public static LLVMTypeRef InstantiateGenericType(
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
        GenericTypeReference generic)
    {
        var curDict = generic.GenericArguments;
        var finDict = new Dictionary<GenericTemplate, LLVMTypeRef>();
        foreach (var cur in curDict)
            finDict[cur.Key] = InstantiateType(typeContext, cur.Value);
        return generic.Type.Instantiate(finDict);
    }

    public static LLVMTypeRef InstantiateType(
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
        IType targetType)
    {
        return targetType switch
        {
            ILType ilType => ilType.Type,
            GenericTemplate temp => typeContext[temp],
            GenericTypeReference genType => InstantiateGenericType(typeContext, genType),
            PointerType ptrType => InstantiatePointerType(typeContext, ptrType),
            _ => throw new UnreachableException()
        };
    }

    private static LLVMTypeRef InstantiatePointerType(
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
        PointerType pointerType
    )
    {
        var baseType = InstantiateType(typeContext, pointerType.Type);
        return LLVMTypeRef.CreatePointer(baseType, pointerType.ramType);
    }
    
    internal void AddGenericTemplate(List<string> names)
    {
        foreach (var name in names)
        {
            GenericTemplates.Add(new GenericTemplate(name));
        }
    }
}
