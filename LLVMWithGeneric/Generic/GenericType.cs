using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

/// <summary>
/// Generic struct type definition that can be instantiated to an LLVM named struct.
/// </summary>
public class GenericType(string name, GenericModule module, bool packed): 
    GenericBase
{
    /// <summary>
    /// Owning module used for mangling and instantiation.
    /// </summary>
    internal GenericModule Module { get; } = module;

    /// <summary>
    /// Base type name (before mangling).
    /// </summary>
    public string Name { get; } = name;
    
    /// <summary>
    /// Whether the struct is packed.
    /// </summary>
    public bool Packed { get; } = packed;

    /// <summary>
    /// Template parameters for this generic type.
    /// </summary>
    public List<GenericTemplate> GenericTemplates { get; } = [];

    /// <summary>
    /// Fields of this struct in declared order.
    /// </summary>
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

    /// <summary>
    /// Adds a template parameter to this type.
    /// </summary>
    public GenericTemplate AddTemplate(string templateName)
    {
        var template = new GenericTemplate(templateName);
        GenericTemplates.Add(template);
        return template;
    }

    /// <summary>
    /// Sets the fields for this struct type.
    /// </summary>
    public void SetField(IType[] fields)
    {
        Fields = fields;
    }
    
    /// <summary>
    /// Instantiates a generic type reference using the provided template bindings.
    /// </summary>
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

    /// <summary>
    /// Instantiates an IType into a concrete LLVM type using the provided bindings.
    /// </summary>
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
