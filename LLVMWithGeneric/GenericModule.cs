using LLVMSharp.Interop;
using LLVMWithGeneric.Generic;
using LLVMWithGeneric.Interface;

namespace LLVMWithGeneric;

/// <summary>
/// Entry point for defining and instantiating generic types/functions over an LLVM module.
/// This class does not own the lifetime of Context/Module/Builder; callers manage disposal.
/// </summary>
public class GenericModule
{
    /// <summary>
    /// Name mangler used for generic instantiation.
    /// </summary>
    public readonly GenericMangler Mangler;
    
    /// <summary>
    /// External registry for instance methods.
    /// </summary>
    public readonly TypeRegister Register;
    
    /// <summary>
    /// Underlying LLVM module reference (owned by caller).
    /// </summary>
    public readonly LLVMModuleRef Module;
    
    /// <summary>
    /// Builder used during instantiation (owned by caller).
    /// </summary>
    public readonly LLVMBuilderRef Builder;
    
    /// <summary>
    /// LLVM context reference (owned by caller).
    /// </summary>
    public readonly LLVMContextRef Context;
    
    internal Dictionary<string, LLVMTypeRef> InstantiatedTypes = [];

    /// <summary>
    /// Creates a GenericModule wrapper over existing LLVM references.
    /// Lifetime of all LLVM references remains with the caller.
    /// </summary>
    /// <param name="context">LLVM context reference.</param>
    /// <param name="module">LLVM module reference.</param>
    /// <param name="builder">LLVM builder reference.</param>
    /// <param name="mangler">Name mangler for generic instantiation.</param>
    /// <param name="register">Instance function register.</param>
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

    /// <summary>
    /// Adds a generic static function definition with template parameters.
    /// </summary>
    /// <param name="name">Base function name.</param>
    /// <param name="genericTemplates">Template parameter names.</param>
    /// <returns>A function definition to be configured and instantiated.</returns>
    public GenericStaticFunc AddFunction(
        string name, List<string> genericTemplates)
    {
        var func = new GenericStaticFunc(name, this);
        func.AddGenericTemplate(genericTemplates);
        return func;
    }

    /// <summary>
    /// Adds a generic struct type definition.
    /// </summary>
    /// <param name="name">Base type name.</param>
    /// <param name="genericTemplates">Template parameter names.</param>
    /// <param name="packed">Whether the struct is packed.</param>
    /// <returns>A type definition to be configured and instantiated.</returns>
    public GenericType AddGenericType(
        string name, List<string> genericTemplates, bool packed)
    {
        var type = new GenericType(name, this, packed);
        type.AddGenericTemplate(genericTemplates);
        return type;
    }

    /// <summary>
    /// Instantiates a generic function with a concrete template binding.
    /// </summary>
    /// <param name="func">Generic function definition.</param>
    /// <param name="typeContext">Mapping from templates to concrete LLVM types.</param>
    /// <returns>LLVM function value reference.</returns>
    public LLVMValueRef InstantiateFunction(
        GenericStaticFunc func,
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext)
    {
        return func.Instantiate(typeContext);
    }

    /// <summary>
    /// Instantiates a generic type with a concrete template binding.
    /// </summary>
    /// <param name="type">Generic type definition.</param>
    /// <param name="typeContext">Mapping from templates to concrete LLVM types.</param>
    /// <returns>LLVM type reference.</returns>
    public LLVMTypeRef InstantiateType(
        GenericType type,
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext)
    {
        return type.Instantiate(typeContext);
    }
}
