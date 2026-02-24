using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

/// <summary>
/// Generic static function definition that can be instantiated into an LLVM function.
/// </summary>
public partial class GenericStaticFunc(string name, GenericModule module) : 
    GenericValue, GenericBase
{
    /// <summary>
    /// Base function name (before mangling).
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Template parameters for this function.
    /// </summary>
    public List<GenericTemplate> GenericTemplates { get; } = [];

    /// <summary>
    /// Function parameters in declared order.
    /// </summary>
    public List<IType> Parameters = [];

    /// <summary>
    /// Return type of the function.
    /// </summary>
    public IType ReturnType = new ILType(LLVMTypeRef.Void);

    private List<GenericValue> parameter = [];

    internal void AddGenericTemplate(List<string> names)
    {
        foreach (var name in names)
        {
            GenericTemplates.Add(new GenericTemplate(name));
        }
    }

    /// <summary>
    /// Sets the function parameters.
    /// </summary>
    public void SetParameter(List<IType> parameters)
    {
        this.Parameters = parameters;
        var i = 0;
        foreach (var name in parameters)
        {
            parameter.Add(new GenericFuncValue("param" + i++));
        }
    }

    public GenericValue GetParameter(int i)
    {
        return parameter[i];
    }

    /// <summary>
    /// Sets the return type of the function.
    /// </summary>
    public void SetReturnType(IType returnType)
    {
        ReturnType = returnType;
    }
}
