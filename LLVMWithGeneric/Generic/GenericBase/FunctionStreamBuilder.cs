using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private class BrBuilder(GenericFuncBlock block) : IOperation
    {
        private readonly GenericFuncBlock _block = block;
        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            module.Builder.BuildBr(blockContext[_block]);
        }
    }
    
    private class CondBrBuilder(
        GenericValue cond, 
        GenericFuncBlock thenBlock,
        GenericFuncBlock elseBlock) : IOperation
    {
        private readonly GenericValue _cond = cond;
        private readonly GenericFuncBlock _thenBlock = thenBlock;
        private readonly GenericFuncBlock _elseBlock = elseBlock;
        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            module.Builder.BuildCondBr(
                GetLLVMValueRef(valueContext, _cond),
                blockContext[_thenBlock],
                blockContext[_elseBlock]);
        }
    }
    
    private class IndirectBrBuilder(
        GenericBlockValue blockValue,
        uint numJumps) : IOperation
    {
        private readonly GenericBlockValue _blockValue = blockValue;
        private readonly uint _numJumps = numJumps;
        public GenericIndirectBrValue Ret { get; } = new GenericIndirectBrValue();
        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var block = blockContext[_blockValue.Block];
            LLVMValueRef addrValue;
            unsafe
            {
                var addr = LLVM.BlockAddress(
                    (LLVMOpaqueValue*)function.Handle, 
                    (LLVMOpaqueBasicBlock*)block.Handle);
                addrValue = new LLVMValueRef
                {
                    Handle = (IntPtr)addr
                };
            }
            var p = module.Builder.BuildIndirectBr(addrValue, _numJumps);
            foreach (var gotoBlock in Ret.GotoBlocks) 
                unsafe
                {
                    var b = blockContext[gotoBlock];
                    LLVM.AddDestination(
                        (LLVMOpaqueValue*)p.Handle, 
                        (LLVMOpaqueBasicBlock*)b.Handle);
                }
            
        }
    }
    
    private class ReturnBuilder(GenericValue value) : IOperation
    {
        private readonly GenericValue _value = value;
        
        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildRet(GetLLVMValueRef(valueContext, _value));
        }
    }
    
    private class RetVoidBuilder : IOperation
    {
        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildRetVoid();
        }
    }

    private class UnreachableBuilder : IOperation
    {
        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildUnreachable();
        }
    }

    private class SwitchBuilder(
        GenericValue value,
        GenericFuncBlock defaultBlock,
        uint numJumps
        ) : IOperation
    {
        private readonly GenericValue _value = value;
        
        private readonly GenericFuncBlock _defaultBlock = defaultBlock;
        
        private readonly uint _numJumps = numJumps;
        
        public GenericSwitchValue Ret { get; } = new GenericSwitchValue();
        
        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildSwitch(
                GetLLVMValueRef(valueContext, _value),
                blockContext[_defaultBlock], 
                _numJumps);
        }
    }
    
    public void BuildBr(GenericFuncBlock block)
    {
        CheckBlock();
        this._currentBlock!.Operations.Add(new BrBuilder(block));
        this._currentBlock.Terminate();
    }
    
    public void BuildConditionBr(
        GenericValue cond, 
        GenericFuncBlock thenBlock,
        GenericFuncBlock elseBlock)
    {
        CheckBlock();
        this._currentBlock!.Operations.Add(
            new CondBrBuilder(cond, thenBlock, elseBlock));
        this._currentBlock.Terminate();
    }

    public GenericIndirectBrValue BuildIndirectBr(
        GenericBlockValue blockValue,
        uint numJumps)
    {
        CheckBlock();
        var tmp = new IndirectBrBuilder(blockValue, numJumps);
        this._currentBlock!.Operations.Add(tmp);
        this._currentBlock.Terminate();
        return tmp.Ret;
    }
    
    public void BuildRet(GenericValue value)
    {
        CheckBlock();
        this._currentBlock!.Operations.Add(new ReturnBuilder(value));
        this._currentBlock.Terminate();
    }

    public void BuildRetVoid()
    {
        CheckBlock();
        this._currentBlock!.Operations.Add(new RetVoidBuilder());
        this._currentBlock.Terminate();
    }

    public void BuildUnreachable()
    {
        CheckBlock();
        this._currentBlock!.Operations.Add(new UnreachableBuilder());
        this._currentBlock.Terminate();
    }

    public GenericSwitchValue BuildSwitch(
        GenericValue value,
        GenericFuncBlock defaultBlock,
        uint numJumps)
    {
        CheckBlock();
        var tmp = new SwitchBuilder(value, defaultBlock, numJumps);
        this._currentBlock!.Operations.Add(tmp);
        this._currentBlock.Terminate();
        return tmp.Ret;
    }
    
}