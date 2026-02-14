using LLVMSharp.Interop;

namespace LLVMWithGeneric.Generic;

/// <summary>
/// Base class for values used during generic IR construction.
/// </summary>
public abstract class GenericValue
{
    private static ulong _currentID;
    
    /// <summary>
    /// Unique ID for this value instance.
    /// </summary>
    public ulong ID { get; private set; }

    protected GenericValue()
    {
        ID = _currentID++;
    }
}

/// <summary>
/// Wrapper for an existing LLVM value.
/// </summary>
public class GenericValueFromLLVM(LLVMValueRef constant) : GenericValue
{
    /// <summary>
    /// Underlying LLVM value.
    /// </summary>
    public LLVMValueRef Value { get; } = constant;
}

/// <summary>
/// Named variable used within a generic function.
/// </summary>
public class GenericFuncVariable(string name): GenericValue
{
    /// <summary>
    /// Variable name.
    /// </summary>
    public string Name { get; } = name;
}

/// <summary>
/// Reference to a basic block within a generic function.
/// </summary>
public class GenericBlockValue(GenericStaticFunc func, GenericFuncBlock block): GenericValue
{
    /// <summary>
    /// Target block.
    /// </summary>
    public GenericFuncBlock Block { get; } = block;
    
    /// <summary>
    /// Owning function.
    /// </summary>
    public GenericStaticFunc StaticFunc { get; } = func;
}

/// <summary>
/// Value used for indirect branch operations.
/// </summary>
public class GenericIndirectBrValue: GenericValue
{
    /// <summary>
    /// Target blocks for the indirect branch.
    /// </summary>
    public List<GenericFuncBlock> GotoBlocks { get; private set; } = [];

    public void AddDestination(GenericFuncBlock block)
    {
        GotoBlocks.Add(block);
    }
}

/// <summary>
/// Value used for switch operations.
/// </summary>
public class GenericSwitchValue: GenericValue
{
    /// <summary>
    /// Switch destinations and their match values.
    /// </summary>
    public List<(GenericValue, GenericFuncBlock)> GotoBlocks 
    { get; private set; } = [];

    public void AddDestination(GenericValue value, GenericFuncBlock block)
    {
        GotoBlocks.Add((value, block));
    }
}


