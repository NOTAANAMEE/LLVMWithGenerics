using System.Drawing;
using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public interface IType
{
    public string Name { get; }
}

public class ILType(LLVMTypeRef type): IType
{
    public LLVMTypeRef Type { get; } = type;

    public string Name => Type.StructName;
}

public class GenericTemplate(string name): IType
{
    public string Name { get; } = name;
}

public class GenericTypeReference : IType
{
    public string Name { get; }
    
    public Dictionary<GenericTemplate, IType> GenericArguments { get; }

    public GenericType Type { get; }

    public GenericTypeReference(IType[] generics, GenericType type)
    {
        Name = type.Name + $"<{
            string.Join(",", generics.Select(g => g.Name))
        }>";
        if (generics.Length != type.GenericTemplates.Count) 
            throw new ArgumentException();
        GenericArguments = new Dictionary<GenericTemplate, IType>();
        for (var i = 0; i < type.GenericTemplates.Count; i++) 
            GenericArguments[type.GenericTemplates[i]] = generics[i];
        Type = type;
    }
}

public class PointerType(IType type, uint ramType = 0) : IType
{
    public string Name => Type.Name + "*";
    public IType Type { get; } = type;

    public uint ramType = ramType;
}
