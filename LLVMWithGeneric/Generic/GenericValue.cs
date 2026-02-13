using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

public abstract class GenericValue
{
    private static ulong _currentID;
    
    public ulong ID { get; private set; }

    protected GenericValue()
    {
        ID = _currentID++;
    }
}

public class GenericValueFromLLVM(LLVMValueRef constant) : GenericValue
{
    public LLVMValueRef Value { get; } = constant;
}

public class GenericFuncVariable(string name): GenericValue
{
    public string Name { get; } = name;
}

public class GenericBlockValue(GenericStaticFunc func, GenericFuncBlock block): GenericValue
{
    public GenericFuncBlock Block { get; } = block;
    
    public GenericStaticFunc StaticFunc { get; } = func;
}

public class GenericIndirectBrValue: GenericValue
{
    public List<GenericFuncBlock> GotoBlocks { get; private set; } = [];

    public void AddDestination(GenericFuncBlock block)
    {
        GotoBlocks.Add(block);
    }
}

public class GenericSwitchValue: GenericValue
{
    public List<(GenericValue, GenericFuncBlock)> GotoBlocks 
    { get; private set; } = [];

    public void AddDestination(GenericValue value, GenericFuncBlock block)
    {
        GotoBlocks.Add((value, block));
    }
}



