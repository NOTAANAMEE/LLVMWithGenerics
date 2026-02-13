using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private class FuncCallOperation(
        ILType returnType,
        ILType[] parameterTypes,
        LLVMValueRef function,
        GenericValue[] arguments,
        string name
        ) : IOperation
    {
        private readonly ILType   _returnType = returnType;
        private readonly ILType[] _paramTypes = parameterTypes;
        private readonly LLVMValueRef _function = function;
        private readonly GenericValue[] _arguments = arguments;
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);
        
        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var builder = module.Builder;
            var retType = _returnType.Type;
            var paramTypes = _paramTypes.Select(
                t => t.Type).ToArray();
            var funcType = LLVMTypeRef.CreateFunction(retType, paramTypes);
            var ret = builder.BuildCall2(
                funcType,
                _function,
                _arguments.Select(v => GetLLVMValueRef(valueContext, v)).ToArray(),
                Return.Name
            );
            valueContext[Return.ID] = ret;
        }
    }

    private class GenericFuncCallOperation(
        IType returnType,
        IType[] parameterTypes,
        IType[] genericTypeList,
        GenericStaticFunc function,
        GenericValue[] arguments,
        string name
    ) : IOperation
    {
        private readonly IType _returnType = returnType;
        private readonly IType[] _paramTypes = parameterTypes;
        private readonly GenericStaticFunc _function = function;
        private readonly GenericValue[] _arguments = arguments;
        private readonly Dictionary<GenericTemplate, IType> _fnTypeContext =
            MakeTypeContext(function.GenericTemplates.ToArray(), genericTypeList);
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);
        
        public void Instantiate(
            LLVMValueRef function,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var builder = module.Builder;
            // 1. make function type context
            var fnFinalTypeContext = InstantiateTypeContext(typeContext, _fnTypeContext);
            // 2. instantiate function
            var fn = _function.Instantiate(fnFinalTypeContext);
            // 3. instantiate fn type
            var callerParams = _paramTypes.Select(a => InstantiateType(typeContext, a)).ToArray();
            var returnType = InstantiateType(typeContext, _returnType);
            var fnType = LLVMTypeRef.CreateFunction(returnType, callerParams);
            // 4. Final build
            var ret = builder.BuildCall2(
                fnType,
                fn,
                _arguments.Select(v => GetLLVMValueRef(valueContext, v)).ToArray(),
                Return.Name
            );
            valueContext[Return.ID] = ret;
        }

        public static Dictionary<GenericTemplate, IType> MakeTypeContext(
            GenericTemplate[] templates, IType[] types)
        {
            return templates
                .Zip(types, (k, v) => new {k, v})
                .ToDictionary(t => t.k, t => t.v);
        }

        public static Dictionary<GenericTemplate, LLVMTypeRef> InstantiateTypeContext(
                Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
                Dictionary<GenericTemplate, IType> fnTypeContext)
        {
            return fnTypeContext.Select(kv => new
                {
                    key = kv.Key,
                    value = InstantiateType(typeContext, kv.Value)
                })
                .ToDictionary(a => a.key, a => a.value
                );
        }
    }
    

    private class PolymorphismCallOperation(
        ILType returnType,
        ILType[] parameterTypes,
        IType funcOwner,
        string funcName,
        GenericValue[] arguments,
        string name
        ): IOperation
    {
        private readonly ILType _returnType = returnType;
        private readonly ILType[] _paramTypes = parameterTypes;
        private readonly IType _funcOwner = funcOwner;
        private readonly string _funcName = funcName;
        private readonly GenericValue[] _arguments = arguments;
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);

        public void Instantiate(
            LLVMValueRef baseFunction,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var function = module.Register.GetInstanceFunc(_funcOwner, _funcName);
            if (function is not GenericValueFromLLVM gvfl)  throw new Exception();
            var _function = gvfl.Value;
            var builder = module.Builder;
            var retType = _returnType.Type;
            var paramTypes = _paramTypes.Select(
                t => t.Type).ToArray();
            var funcType = LLVMTypeRef.CreateFunction(retType, paramTypes);
            var ret = builder.BuildCall2(
                funcType,
                _function,
                _arguments.Select(v => GetLLVMValueRef(valueContext, v)).ToArray(),
                Return.Name
            );
            valueContext[Return.ID] = ret;
        }
    }
    
    private class PolymorphismGenericCallOperation(
        IType returnType,
        IType[] parameterTypes,
        IType[] genericTypeList,
        IType funcOwner,
        string funcName,
        GenericValue[] arguments,
        string name
    ): IOperation
    {
        private readonly IType _returnType = returnType;
        private readonly IType[] _paramTypes = parameterTypes;
        private readonly IType _funcOwner = funcOwner;
        private readonly string _funcName = funcName;
        private readonly GenericValue[] _arguments = arguments;
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);

        public void Instantiate(
            LLVMValueRef baseFunction,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var function = module.Register.GetInstanceFunc(_funcOwner, _funcName);
            if (function is not GenericStaticFunc _function)  throw new Exception();
            var _fnTypeContext = 
                GenericFuncCallOperation.MakeTypeContext(_function.GenericTemplates.ToArray(), genericTypeList);
            var builder = module.Builder;
            // 1. make function type context
            var fnFinalTypeContext = 
                GenericFuncCallOperation.InstantiateTypeContext(typeContext, _fnTypeContext);
            // 2. instantiate function
            var fn = _function.Instantiate(fnFinalTypeContext);
            // 3. instantiate fn type
            var callerParams = _paramTypes.Select(a => InstantiateType(typeContext, a)).ToArray();
            var returnType = InstantiateType(typeContext, _returnType);
            var fnType = LLVMTypeRef.CreateFunction(returnType, callerParams);
            // 4. Final build
            var ret = builder.BuildCall2(
                fnType,
                fn,
                _arguments.Select(v => GetLLVMValueRef(valueContext, v)).ToArray(),
                Return.Name
            );
            valueContext[Return.ID] = ret;
        }
    }
    
    public GenericValue BuildCall2(
        ILType returnType,
        ILType[] paramTypes,
        LLVMValueRef function,
        GenericValue[] arguments,
        string name)
    {
        CheckBlock();
        var op = new FuncCallOperation(returnType, paramTypes, function, arguments, name);
        _currentBlock?.Operations.Add(op);
        return op.Return;
    }
    
    public GenericValue BuildGenericCall(
        IType returnType,
        IType[] parameterTypes,
        IType[] genericTypeList,
        GenericStaticFunc function,
        GenericValue[] arguments,
        string name)
    {
        CheckBlock();
        var op = new GenericFuncCallOperation(returnType, parameterTypes, genericTypeList, function, arguments, name);
        _currentBlock?.Operations.Add(op);
        return op.Return;
    }

    public GenericValue BuildPolymorphismCall(
        ILType returnType,
        ILType[] parameterTypes,
        IType funcOwner,
        string funcName,
        GenericValue[] arguments,
        string name)
    {
        CheckBlock();
        var op = new PolymorphismCallOperation(
            returnType, parameterTypes, funcOwner, funcName, arguments, name);
        _currentBlock?.Operations.Add(op);
        return op.Return;
    }
    
    public GenericValue BuildPolymorphismGenericCall(
        IType returnType,
        IType[] parameterTypes,
        IType[] genericTypeList,
        IType funcOwner,
        string funcName,
        GenericValue[] arguments,
        string name)
    {
        CheckBlock();
        var op = new PolymorphismGenericCallOperation(
            returnType, parameterTypes, genericTypeList, funcOwner, funcName, arguments, name);
        _currentBlock?.Operations.Add(op);
        return op.Return;
    }
}