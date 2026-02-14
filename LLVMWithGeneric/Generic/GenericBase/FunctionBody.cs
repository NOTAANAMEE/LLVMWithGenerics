using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private readonly List<GenericFuncBlock> _blocks = [];
    
    private GenericFuncBlock? _currentBlock;

    /// <summary>
    /// Adds a basic block to this function.
    /// </summary>
    public GenericFuncBlock AddBlock(string name)
    {
        var blk = new GenericFuncBlock(name);
        _blocks.Add(blk);
        return blk;
    }

    /// <summary>
    /// Sets the current block for subsequent IR operations.
    /// </summary>
    public void PositionAtEnd(GenericFuncBlock block)
        => _currentBlock = block;
    
    /// <summary>
    /// Instantiates this generic function with concrete template bindings.
    /// </summary>
    public LLVMValueRef Instantiate(Dictionary<GenericTemplate, LLVMTypeRef> typeContext)
    {
        // 1. function type
        var parameters = InstantiateParam(typeContext);
        var functionType = LLVMTypeRef.CreateFunction(
            InstantiateType(typeContext, ReturnType),
            parameters);
        // 2. real function
        var function = module.Module.AddFunction(
            module.Mangler.MangleFunc(Name, parameters),
            functionType);
        //3. build block
        var blockDict = new Dictionary<GenericFuncBlock, LLVMBasicBlockRef>();
        foreach (var block in _blocks)
        {
            var basicBlk = module.Context.AppendBasicBlock(function, block.Name);
            blockDict.Add(block, basicBlk);
        }
        //4. implement function body
        foreach (var blk in _blocks)
        {
            var basicBlk = blockDict[blk];
            module.Builder.PositionAtEnd(basicBlk);
            Dictionary<ulong, LLVMValueRef> valueDict = [];
            foreach (var iop in blk.Operations)
                iop.Instantiate(
                    function,
                    typeContext, 
                    valueDict,
                    blockDict,
                    module);
        }
        
        return function;
    }

    private LLVMTypeRef[] InstantiateParam(
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext)
    {
        List<LLVMTypeRef> ret = [];
        ret.AddRange(this.Parameters.Select(type => InstantiateType(typeContext, type)));
        return [..ret];
    }

    private static LLVMTypeRef InstantiateType(
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
        IType targetType)
        => GenericType.InstantiateType(typeContext, targetType);
    
}

public class GenericFuncBlock(string name)
{
    /// <summary>
    /// Block name.
    /// </summary>
    public string Name { get; private set; } = name;

    /// <summary>
    /// Whether the block is terminated.
    /// </summary>
    public bool Terminated { get; private set; }

    /// <summary>
    /// Marks the block as terminated.
    /// </summary>
    public void Terminate() => Terminated = true;

    internal List<IOperation> Operations = [];
}

internal interface IOperation
{
    public void Instantiate(
        LLVMValueRef function,
        Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
        Dictionary<ulong, LLVMValueRef> valueContext,
        Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
        GenericModule module);
}
