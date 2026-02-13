using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private readonly List<GenericFuncBlock> _blocks = [];
    
    private GenericFuncBlock? _currentBlock;

    public GenericFuncBlock AddBlock(string name)
    {
        var blk = new GenericFuncBlock(name);
        _blocks.Add(blk);
        return blk;
    }

    public void PositionAtEnd(GenericFuncBlock block)
    {
        _currentBlock = block;
    }

    public LLVMValueRef Instantiate(Dictionary<GenericTemplate, LLVMTypeRef> typeContext)
    {
        // 1. function type
        var parameters = InstantiateParam(typeContext);
        var functionType = LLVMTypeRef.CreateFunction(
            InstantiateType(typeContext, ReturnType),
            parameters);
        // 2. real function
        var function = _module.Module.AddFunction(
            _module.Mangler.MangleFunc(Name, parameters),
            functionType);
        //3. build block
        var blockDict = new Dictionary<GenericFuncBlock, LLVMBasicBlockRef>();
        foreach (var block in _blocks)
        {
            var basicBlk = _module.Context.AppendBasicBlock(function, block.Name);
            blockDict.Add(block, basicBlk);
        }
        //4. implement function body
        foreach (var blk in _blocks)
        {
            var basicBlk = blockDict[blk];
            _module.Builder.PositionAtEnd(basicBlk);
            Dictionary<ulong, LLVMValueRef> valueDict = [];
            foreach (var iop in blk.Operations)
                iop.Instantiate(
                    function,
                    typeContext, 
                    valueDict,
                    blockDict,
                    _module);
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
    public string Name { get; private set; } = name;
    
    private bool _terminated;
    
    public bool Terminated => _terminated;

    public void Terminate() => _terminated = true;

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