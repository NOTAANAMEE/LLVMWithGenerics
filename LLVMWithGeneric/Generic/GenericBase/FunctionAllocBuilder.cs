using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public partial class GenericStaticFunc
{
    private class AllocBuilder(
        IType type,
        string name,
        bool onMem): IOperation
    {
        private readonly IType _type = type;
        private readonly bool _onMem = onMem;
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);


        public void Instantiate(LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type = InstantiateType(typeContext, _type);
            var value
             = _onMem ? module.Builder.BuildMalloc(type, Return.Name) : 
                module.Builder.BuildAlloca(type, Return.Name);
            valueContext[Return.ID] = value;
        }
    }
    
    private class ArrayAllocBuilder(
        IType type,
        GenericValue value,
        string name,
        bool onMem): IOperation
    {
        private readonly IType _type = type;
        private readonly bool _onMem = onMem;
        private readonly GenericValue _value = value;
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);


        public void Instantiate(LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type = InstantiateType(typeContext, _type);
            var numValue = GetLLVMValueRef(valueContext, _value);
            var value
                = _onMem ? module.Builder.BuildArrayMalloc(type, numValue, Return.Name) : 
                    module.Builder.BuildArrayAlloca(type, numValue, Return.Name);
            valueContext[Return.ID] = value;
        }
    }

    private class FreeBuilder(GenericValue pointer): IOperation
    {
        private readonly GenericValue _pointer = pointer;

        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildFree(
                GetLLVMValueRef(valueContext, _pointer));
        }
    }

    private class LoadBuilder(
        IType type,
        GenericValue pointer,
        string name) : IOperation
    {
        private readonly IType _type = type;
        private readonly GenericValue _pointer = pointer;
        
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);

        public void Instantiate(LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext,
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type = InstantiateType(typeContext, _type);
            var ret = module.Builder.BuildLoad2(
                type,
                GetLLVMValueRef(valueContext, _pointer),
                Return.Name);
            valueContext[Return.ID] = ret;
        }
    }

    private class StoreBuilder(
        GenericValue value,
        GenericValue pointer): IOperation
    {
        private readonly GenericValue _value = value;
        private readonly GenericValue _pointer = pointer;


        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            module.Builder.BuildStore(
                GetLLVMValueRef(valueContext, _value),
                GetLLVMValueRef(valueContext, _pointer)
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
        private readonly IType _type = type;
        private readonly GenericValue _pointer = pointer;
        private readonly GenericValue[] _idices = idices;
        private readonly bool _inbound = inbound;
        
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);


        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type = InstantiateType(typeContext, _type);
            var idices = _idices
                .Select(v => GetLLVMValueRef(valueContext, v))
                .ToArray();
            var pointer = GetLLVMValueRef(valueContext, _pointer);
            var ret = _inbound? 
                module.Builder.BuildInBoundsGEP2(type, pointer, idices, Return.Name) : 
                module.Builder.BuildGEP2(type, pointer, idices, Return.Name);
            valueContext[Return.ID] = ret;
        }
    }
    
    private class StructGEPBuilder(
        IType type,
        GenericValue pointer,
        uint idx,
        string name): IOperation
    {
        private readonly IType _type = type;
        private readonly GenericValue _pointer = pointer;
        private readonly uint _idx = idx;
        
        public GenericFuncVariable Return { get; } = new GenericFuncVariable(name);


        public void Instantiate(
            LLVMValueRef function, 
            Dictionary<GenericTemplate, LLVMTypeRef> typeContext, 
            Dictionary<ulong, LLVMValueRef> valueContext, 
            Dictionary<GenericFuncBlock, LLVMBasicBlockRef> blockContext,
            GenericModule module)
        {
            var type = InstantiateType(typeContext, _type);
            var pointer = GetLLVMValueRef(valueContext, _pointer);
            var ret = module.Builder.BuildStructGEP2(
                type, pointer, _idx, Return.Name);
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