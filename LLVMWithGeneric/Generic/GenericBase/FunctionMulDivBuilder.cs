using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private class MulDivOperation(GenericValue LHS, GenericValue RHS,
        string name,
        MulDivType type) : IOperation
    {
        private readonly GenericValue _lhs = LHS;
        private readonly GenericValue _rhs = RHS;
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);
        private readonly MulDivType _type = type;

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
                // Mul
                MulDivType.MUL => builder.BuildMul(lhs, rhs, Return.Name),
                MulDivType.FMUL => builder.BuildFMul(lhs, rhs, Return.Name),
                MulDivType.NSWMUL => builder.BuildNSWMul(lhs, rhs, Return.Name),
                MulDivType.NUWMUL => builder.BuildNUWMul(lhs, rhs, Return.Name),

                // Div
                MulDivType.SDIV => builder.BuildSDiv(lhs, rhs, Return.Name),
                MulDivType.UDIV => builder.BuildUDiv(lhs, rhs, Return.Name),
                MulDivType.EXACTSDIV => builder.BuildExactSDiv(lhs, rhs, Return.Name),

                MulDivType.FDIV => builder.BuildFDiv(lhs, rhs, Return.Name),

                _ => throw new ArgumentOutOfRangeException()
            };

            valueContext[Return.ID] = value;
        }
    }

    private enum MulDivType
    {
        // Mul
        MUL,
        FMUL,
        NSWMUL,
        NUWMUL,

        // Div (int)
        SDIV,
        UDIV,
        EXACTSDIV,

        // Div (float)
        FDIV
    }

    private GenericValue BuildMulDiv(
        GenericValue lhs, GenericValue rhs, string name,
        MulDivType type)
    {
        CheckBlock();
        var tmp = new MulDivOperation(lhs, rhs, name, type);
        _currentBlock?.Operations.Add(tmp);
        return tmp.Return;
    }

    // ===== Mul =====

    public GenericValue BuildMul(GenericValue LHS, GenericValue RHS, string name)
        => BuildMulDiv(LHS, RHS, name, MulDivType.MUL);

    public GenericValue BuildFMul(GenericValue LHS, GenericValue RHS, string name)
        => BuildMulDiv(LHS, RHS, name, MulDivType.FMUL);

    public GenericValue BuildNSWMul(GenericValue LHS, GenericValue RHS, string name)
        => BuildMulDiv(LHS, RHS, name, MulDivType.NSWMUL);

    public GenericValue BuildNUWMul(GenericValue LHS, GenericValue RHS, string name)
        => BuildMulDiv(LHS, RHS, name, MulDivType.NUWMUL);

    // ===== Div =====

    public GenericValue BuildSDiv(GenericValue LHS, GenericValue RHS, string name)
        => BuildMulDiv(LHS, RHS, name, MulDivType.SDIV);

    public GenericValue BuildUDiv(GenericValue LHS, GenericValue RHS, string name)
        => BuildMulDiv(LHS, RHS, name, MulDivType.UDIV);

    /// <summary>
    /// 我保证整除（无余数），否则就是 UB（给优化器的承诺）
    /// </summary>
    public GenericValue BuildExactSDiv(GenericValue LHS, GenericValue RHS, string name)
        => BuildMulDiv(LHS, RHS, name, MulDivType.EXACTSDIV);

    public GenericValue BuildFDiv(GenericValue LHS, GenericValue RHS, string name)
        => BuildMulDiv(LHS, RHS, name, MulDivType.FDIV);
}