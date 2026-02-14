using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private class AllocBuilder(
        IType type,
        string name,
        bool onMem): IOperation
    {
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);


        public void Instantiate(LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type1 = InstantiateType(typeContext, type);
            var value
             = onMem ? module.Builder.BuildMalloc(type1, Return.Name) : 
                module.Builder.BuildAlloca(type1, Return.Name);
            valueContext[Return.ID] = value;
        }
    }
    
    private class ArrayAllocBuilder(
        IType type,
        GenericValue value,
        string name,
        bool onMem): IOperation
    {
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);


        public void Instantiate(LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type1 = InstantiateType(typeContext, type);
            var numValue = GetLLVMValueRef(valueContext, value);
            var value1
                = onMem ? module.Builder.BuildArrayMalloc(type1, numValue, Return.Name) : 
                    module.Builder.BuildArrayAlloca(type1, numValue, Return.Name);
            valueContext[Return.ID] = value1;
        }
    }

    private class FreeBuilder(GenericValue pointer): IOperation
    {
        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildFree(
                GetLLVMValueRef(valueContext, pointer));
        }
    }

    private class LoadBuilder(
        IType type,
        GenericValue pointer,
        string name) : IOperation
    {
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);

        public void Instantiate(LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type1 = InstantiateType(typeContext, type);
            var ret = module.Builder.BuildLoad2(
                type1,
                GetLLVMValueRef(valueContext, pointer),
                Return.Name);
            valueContext[Return.ID] = ret;
        }
    }

    private class StoreBuilder(
        GenericValue value,
        GenericValue pointer): IOperation
    {
        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildStore(
                GetLLVMValueRef(valueContext, value),
                GetLLVMValueRef(valueContext, pointer)
            );
        }
    }

    private class GEPBuilder(
        IType type,
        GenericValue pointer,
        GenericValue[] idices,
        string name,
        bool inbound): IOperation
    {
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);


        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type1 = InstantiateType(typeContext, type);
            var idices1 = idices
                .Select(v => GetLLVMValueRef(valueContext, v))
                .ToArray();
            var pointer1 = GetLLVMValueRef(valueContext, pointer);
            var ret = inbound? 
                module.Builder.BuildInBoundsGEP2(type1, pointer1, idices1, Return.Name) : 
                module.Builder.BuildGEP2(type1, pointer1, idices1, Return.Name);
            valueContext[Return.ID] = ret;
        }
    }
    
    private class StructGEPBuilder(
        IType type,
        GenericValue pointer,
        uint idx,
        string name): IOperation
    {
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);


        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type1 = InstantiateType(typeContext, type);
            var pointer1 = GetLLVMValueRef(valueContext, pointer);
            var ret = module.Builder.BuildStructGEP2(
                type1, pointer1, idx, Return.Name);
            valueContext[Return.ID] = ret;
        }
    }
    
    
    
    public GenericValue BuildAlloca(IType type, string name)
    {
        CheckBlock();
        var tmp = new AllocBuilder(type, name, false);
        this._currentBlock!.Operations.Add(tmp);
        return tmp.Return;
    }

    public GenericValue BuildArrayAlloca(IType type,
        GenericValue value,
        string name)
    {
        CheckBlock();
        var tmp = new ArrayAllocBuilder(type, value, name, false);
        this._currentBlock!.Operations.Add(tmp);
        return tmp.Return;
    }

    public GenericValue BuildMalloc(IType type, string name)
    {
        CheckBlock();
        var tmp = new AllocBuilder(type, name, true);
        this._currentBlock!.Operations.Add(tmp);
        return tmp.Return;
    }

    public GenericValue BuildArrayMalloc(IType type,
        GenericValue value,
        string name)
    {
        CheckBlock();
        var tmp = new ArrayAllocBuilder(type, value, name, true);
        this._currentBlock!.Operations.Add(tmp);
        return tmp.Return;
    }

    public void BuildFree(GenericValue pointer)
    {
        CheckBlock();
        this._currentBlock!.Operations.Add(
            new FreeBuilder(pointer));
    }

    public void BuildStore(
        GenericValue value,
        GenericValue pointer)
    {
        CheckBlock();
        this._currentBlock!.Operations.Add(
            new StoreBuilder(value, pointer));
    }
    
    /// <summary>
    /// Same as BuildLoad2
    /// </summary>
    public GenericValue BuildLoad(
        IType type,
        GenericValue pointer,
        string name) 
        => BuildLoad2(type, pointer, name);

    public GenericValue BuildLoad2(
        IType type,
        GenericValue pointer,
        string name)
    {
        CheckBlock();
        var tmp = new LoadBuilder(type, pointer, name);
        this._currentBlock!.Operations.Add(tmp);
        return tmp.Return;
    }

    public GenericValue BuildGEP(
        IType type,
        GenericValue pointer,
        GenericValue[] idices,
        string name)
        => BuildGEP2(type, pointer, idices, name);

    public GenericValue BuildGEP2(
        IType type,
        GenericValue pointer,
        GenericValue[] idices,
        string name)
    {
        CheckBlock();
        var tmp = new GEPBuilder(type, pointer, idices, name, false);
        this._currentBlock!.Operations.Add(tmp);
        return tmp.Return;
        
    }
    
    public GenericValue BuildInBoundsGEP(
        IType type,
        GenericValue pointer,
        GenericValue[] idices,
        string name)
        => BuildInBoundsGEP2(type, pointer, idices, name);
    
    public GenericValue BuildInBoundsGEP2(
        IType type,
        GenericValue pointer,
        GenericValue[] idices,
        string name)
    {
        CheckBlock();
        var tmp = new GEPBuilder(type, pointer, idices, name, true);
        this._currentBlock!.Operations.Add(tmp);
        return tmp.Return;
        
    }
    
    public GenericValue BuildStructGEP(
        IType type,
        GenericValue pointer,
        uint idx,
        string name)
        => BuildStructGEP2(type, pointer, idx, name);
    
    public GenericValue BuildStructGEP2(
        IType type,
        GenericValue pointer,
        uint idx,
        string name)
    {
        CheckBlock();
        var tmp = new StructGEPBuilder(type, pointer,idx, name);
        this._currentBlock!.Operations.Add(tmp);
        return tmp.Return;
    }
}