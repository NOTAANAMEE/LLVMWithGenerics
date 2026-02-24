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
        public GenericFuncValue Return { get; } = new GenericFuncValue(name);
        
        public void Instantiate(
            LLVMValueRef function1,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var builder = module.Builder;
            var retType = returnType.Type;
            var paramTypes = parameterTypes.Select(
                t => t.Type).ToArray();
            var funcType = LLVMTypeRef.CreateFunction(retType, paramTypes);
            var ret = builder.BuildCall2(
                funcType,
                function,
                arguments.Select(v => GetLLVMValueRef(valueContext, v)).ToArray(),
                Return.Name
            );
            valueContext[Return.ID] = ret;
        }
    }

    private class GenericFuncCallOperation(
        IType type,
        IType[] parameterTypes,
        IType[] genericTypeList,
        GenericStaticFunc function,
        GenericValue[] arguments,
        string name
    ) : IOperation
    {
        private readonly Dictionary<GenericTemplate, IType> _fnTypeContext =
            MakeTypeContext(function.GenericTemplates.ToArray(), genericTypeList);
        public GenericFuncValue Return { get; } = new GenericFuncValue(name);
        
        public void Instantiate(
            LLVMValueRef function1,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var builder = module.Builder;
            // 1. make function type context
            var fnFinalTypeContext = InstantiateTypeContext(typeContext, _fnTypeContext);
            // 2. instantiate function
            var fn = function.Instantiate(fnFinalTypeContext);
            // 3. instantiate fn type
            var callerParams = parameterTypes.Select(a => InstantiateType(typeContext, a)).ToArray();
            var returnType = InstantiateType(typeContext, type);
            var fnType = LLVMTypeRef.CreateFunction(returnType, callerParams);
            // 4. Final build
            var ret = builder.BuildCall2(
                fnType,
                fn,
                arguments.Select(v => GetLLVMValueRef(valueContext, v)).ToArray(),
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
        public GenericFuncValue Return { get; } = new GenericFuncValue(name);

        public void Instantiate(
            LLVMValueRef baseFunction,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var function = module.Register.GetInstanceFunc(funcOwner, funcName);
            if (function is not GenericValueFromLLVM gvfl)  
                throw new InvalidOperationException("function is not an LLVM function");
            var _function = gvfl.Value;
            var builder = module.Builder;
            var retType = returnType.Type;
            var paramTypes = parameterTypes.Select(
                t => t.Type).ToArray();
            var funcType = LLVMTypeRef.CreateFunction(retType, paramTypes);
            var ret = builder.BuildCall2(
                funcType,
                _function,
                arguments.Select(v => GetLLVMValueRef(valueContext, v)).ToArray(),
                Return.Name
            );
            valueContext[Return.ID] = ret;
        }
    }
    
    private class PolymorphismGenericCallOperation(
        IType type,
        IType[] parameterTypes,
        IType[] genericTypeList,
        IType funcOwner,
        string funcName,
        GenericValue[] arguments,
        string name
    ): IOperation
    {
        public GenericFuncValue Return { get; } = new GenericFuncValue(name);

        public void Instantiate(
            LLVMValueRef baseFunction,
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext, 
            GenericModule module)
        {
            var function = module.Register.GetInstanceFunc(funcOwner, funcName);
            if (function is not GenericStaticFunc _function)  
                throw new InvalidOperationException("function is not a generic static function");
            var _fnTypeContext = 
                GenericFuncCallOperation.MakeTypeContext(_function.GenericTemplates.ToArray(), genericTypeList);
            var builder = module.Builder;
            // 1. make function type context
            var fnFinalTypeContext = 
                GenericFuncCallOperation.InstantiateTypeContext(typeContext, _fnTypeContext);
            // 2. instantiate function
            var fn = _function.Instantiate(fnFinalTypeContext);
            // 3. instantiate fn type
            var callerParams = parameterTypes.Select(a => InstantiateType(typeContext, a)).ToArray();
            var returnType = InstantiateType(typeContext, type);
            var fnType = LLVMTypeRef.CreateFunction(returnType, callerParams);
            // 4. Final build
            var ret = builder.BuildCall2(
                fnType,
                fn,
                arguments.Select(v => GetLLVMValueRef(valueContext, v)).ToArray(),
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