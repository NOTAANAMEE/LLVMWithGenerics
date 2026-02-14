using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private class BrBuilder(GenericFuncBlock block) : IOperation
    {
        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            module.Builder.BuildBr(blockContext[block]);
        }
    }
    
    private class CondBrBuilder(
        GenericValue cond, 
        GenericFuncBlock thenBlock,
        GenericFuncBlock elseBlock) : IOperation
    {
        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            module.Builder.BuildCondBr(
                GetLLVMValueRef(valueContext, cond),
                blockContext[thenBlock],
                blockContext[elseBlock]);
        }
    }
    
    private class IndirectBrBuilder(
        GenericBlockValue blockValue,
        uint numJumps) : IOperation
    {
        public GenericIndirectBrValue Ret { get; } = new GenericIndirectBrValue();
        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var block = blockContext[blockValue.Block];
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
            var p = module.Builder.BuildIndirectBr(addrValue, numJumps);
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
        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildRet(GetLLVMValueRef(valueContext, value));
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
        public GenericSwitchValue Ret { get; } = new GenericSwitchValue();
        
        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildSwitch(
                GetLLVMValueRef(valueContext, value),
                blockContext[defaultBlock], 
                numJumps);
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