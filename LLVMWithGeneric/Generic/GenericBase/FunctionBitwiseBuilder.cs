using System.Diagnostics;
using LLVMSharp.Interop;
namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private class BitwiseOperation(GenericValue LHS, GenericValue RHS,
        string name,
        BitwiseType type) : IOperation
    {
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);

        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
            Dictionary<ulong, LLVMValueRef> valueContext,
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var builder = module.Builder;
            var lhs = GetLLVMValueRef(valueContext, LHS);
            var rhs = GetLLVMValueRef(valueContext, RHS);

            var value = type switch
            {
                // Bitwise
                BitwiseType.AND  => builder.BuildAnd(lhs, rhs, Return.Name),
                BitwiseType.OR   => builder.BuildOr(lhs, rhs, Return.Name),
                BitwiseType.XOR  => builder.BuildXor(lhs, rhs, Return.Name),

                // Shifts
                BitwiseType.SHL  => builder.BuildShl(lhs, rhs, Return.Name),
                BitwiseType.LSHR => builder.BuildLShr(lhs, rhs, Return.Name),
                BitwiseType.ASHR => builder.BuildAShr(lhs, rhs, Return.Name),

                // Default, impossible to reach
                _ => throw new UnreachableException("Impossible exception")
            };

            valueContext[Return.ID] = value;
        }
    }

    private enum BitwiseType
    {
        // Bitwise
        AND,
        OR,
        XOR,

        // Shifts
        SHL,
        LSHR,
        ASHR
    }

    private GenericValue BuildBitwise(
        GenericValue lhs, GenericValue rhs, string name,
        BitwiseType type)
    {
        if (_currentBlock == null)
            throw new InvalidOperationException("Haven't set current block yet");

        var tmp = new BitwiseOperation(lhs, rhs, name, type);
        _currentBlock.Operations.Add(tmp);
        return tmp.Return;
    }

    // ===== Bitwise =====

    public GenericValue BuildAnd(GenericValue LHS, GenericValue RHS, string name)
        => BuildBitwise(LHS, RHS, name, BitwiseType.AND);

    public GenericValue BuildOr(GenericValue LHS, GenericValue RHS, string name)
        => BuildBitwise(LHS, RHS, name, BitwiseType.OR);

    public GenericValue BuildXor(GenericValue LHS, GenericValue RHS, string name)
        => BuildBitwise(LHS, RHS, name, BitwiseType.XOR);

    // ===== Shifts =====

    public GenericValue BuildShl(GenericValue LHS, GenericValue RHS, string name)
        => BuildBitwise(LHS, RHS, name, BitwiseType.SHL);

    public GenericValue BuildLShr(GenericValue LHS, GenericValue RHS, string name)
        => BuildBitwise(LHS, RHS, name, BitwiseType.LSHR);

    public GenericValue BuildAShr(GenericValue LHS, GenericValue RHS, string name)
        => BuildBitwise(LHS, RHS, name, BitwiseType.ASHR);
    

    private class NotOperation(GenericValue Value, string name) : IOperation
    {
        private readonly GenericValue _value = Value;
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);

        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
            Dictionary<ulong, LLVMValueRef> valueContext,
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var builder = module.Builder;
            var v = GetLLVMValueRef(valueContext, _value);

            // all-ones constant with same type as v
            var allOnes = LLVMValueRef.CreateConstAllOnes(v.TypeOf);

            var res = builder.BuildXor(v, allOnes, Return.Name);
            valueContext[Return.ID] = res;
        }
    }

    public GenericValue BuildNot(GenericValue Value, string name)
    {
        if (_currentBlock == null)
            throw new InvalidOperationException("Haven't set current block yet");

        var tmp = new NotOperation(Value, name);
        _currentBlock.Operations.Add(tmp);
        return tmp.Return;
    }
}