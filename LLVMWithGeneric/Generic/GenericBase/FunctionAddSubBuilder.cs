using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private class AddSubOperation(GenericValue LHS, GenericValue RHS, 
        string name,
        AddSubType type) : IOperation
    {
        private readonly GenericValue _lhs = LHS;
        private readonly GenericValue _rhs = RHS;
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);
        private readonly AddSubType _type = type;
        
        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var builder = module.Builder;
            var lhs = GetLLVMValueRef(valueContext, _lhs);
            var rhs = GetLLVMValueRef(valueContext, _rhs);
            var value = _type switch
            {
                AddSubType.ADD => builder.BuildAdd(lhs, rhs, Return.Name),
                AddSubType.FADD => builder.BuildFAdd(lhs, rhs, Return.Name),
                AddSubType.NSWADD => builder.BuildNSWAdd(lhs, rhs, Return.Name),
                AddSubType.NUWADD => builder.BuildNUWAdd(lhs, rhs, Return.Name),
                AddSubType.SUB => builder.BuildSub(lhs, rhs, Return.Name),
                AddSubType.FSUB => builder.BuildFSub(lhs, rhs, Return.Name),
                AddSubType.NSWSUB => builder.BuildNSWSub(lhs, rhs, Return.Name),
                AddSubType.NUWSUB => builder.BuildNUWSub(lhs, rhs, Return.Name),
                _ => throw new ArgumentOutOfRangeException()
            };
            valueContext[Return.ID] = value;
        }

        
    }
    
    private static LLVMValueRef GetLLVMValueRef(
        Dictionary<ulong, LLVMValueRef> valueContext,
        GenericValue value)
    {
        return value switch
        {
            GenericFuncVariable genVal => valueContext[genVal.ID],
            GenericValueFromLLVM llvmVal => llvmVal.Value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private enum AddSubType
    {
        ADD,
        SUB,
        FADD,
        FSUB,
        NSWADD,
        NSWSUB,
        NUWADD,
        NUWSUB
    }

    private GenericValue BuildAddSub(
        GenericValue lhs, GenericValue rhs, string name,
        AddSubType type)
    {
        CheckBlock();
        var tmp = new AddSubOperation(lhs, rhs, name, type);
        _currentBlock?.Operations.Add(tmp);
        return tmp.Return;
    }

    private void CheckBlock()
    {
        if (_currentBlock == null)
            throw new InvalidOperationException("Haven't set current block yet");
        if (_currentBlock.Terminated)
            throw new InvalidOperationException("Block terminated");
    }

    public GenericValue BuildAdd(GenericValue LHS, GenericValue RHS, 
        string name)
        => BuildAddSub(LHS, RHS, name, AddSubType.ADD);
    
    public GenericValue BuildFAdd(GenericValue LHS, GenericValue RHS, 
        string name)
        => BuildAddSub(LHS, RHS, name, AddSubType.FADD);
    
    public GenericValue BuildNSWAdd(GenericValue LHS, GenericValue RHS, 
        string name)
        => BuildAddSub(LHS, RHS, name, AddSubType.NSWADD);
    
    public GenericValue BuildNUWAdd(GenericValue LHS, GenericValue RHS, 
        string name)
        => BuildAddSub(LHS, RHS, name, AddSubType.NUWADD);
    
    public GenericValue BuildSub(GenericValue LHS, GenericValue RHS, 
        string name)
        => BuildAddSub(LHS, RHS, name, AddSubType.SUB);
    
    public GenericValue BuildFSub(GenericValue LHS, GenericValue RHS, 
        string name)
        => BuildAddSub(LHS, RHS, name, AddSubType.FSUB);
    
    public GenericValue BuildNSWSub(GenericValue LHS, GenericValue RHS, 
        string name)
        => BuildAddSub(LHS, RHS, name, AddSubType.NSWSUB);
    
    public GenericValue BuildNUWSub(GenericValue LHS, GenericValue RHS, 
        string name)
        => BuildAddSub(LHS, RHS, name, AddSubType.NUWSUB);
}
