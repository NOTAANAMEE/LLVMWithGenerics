using LLVMSharp.Interop;
using LLVMWithGeneric;
using LLVMWithGeneric.Generic;
using LLVMWithGeneric.Interface;

namespace Example;

internal sealed class SimpleMangler : GenericMangler
{
    public string MangleFunc(string funcName, LLVMTypeRef[] typeNames)
        => $"{funcName}<{string.Join(",", typeNames.Select(t => t.ToString()))}>";

    public string MangleType(string funcName, LLVMTypeRef[] typeNames)
        => $"{funcName}<{string.Join(",", typeNames.Select(t => t.ToString()))}>";
}

internal sealed class SimpleTypeRegister : TypeRegister
{
    public void RegisterInstanceFunc(LLVMTypeRef funcOwner, string funcName)
    {
        // No-op for this simple example.
    }

    public GenericValue GetInstanceFunc(IType funcOwner, string funcName)
        => throw new InvalidOperationException("Instance functions are not used in this example.");

    public bool CheckType(GenericBase generic, LLVMTypeRef type) => true;
}

class Program
{
    static void Main(string[] args)
    { 
        var context = LLVMContextRef.Create();
        var module = context.CreateModuleWithName("demo");
        var builder = context.CreateBuilder();

        var gm = new GenericModule(
            context,
            module,
            builder,
            new SimpleMangler(),
            new SimpleTypeRegister());

        // Define generic struct: Pair<T> { T a; T b; }
        var pair = gm.AddGenericType("Pair", ["T"], packed: false);
        var pairT = pair.FindTemplate("T");
        pair.SetField([pairT, pairT]);

        // Instantiate Pair<i32>
        var i32 = LLVMTypeRef.Int32;
        var pairI32 = gm.InstantiateType(
            pair, [i32]);

        // Define generic function: Add<T>(T, T) -> T
        var add = gm.AddFunction("Add", ["T"]);
        var funcT = add.FindTemplate("T");
        add.SetParameter([funcT, funcT]);
        add.SetReturnType(funcT);

        var entry = add.AddBlock("entry");
        add.PositionAtEnd(entry);

        var c1 = new GenericValueFromLLVM(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 1, false));
        var c2 = new GenericValueFromLLVM(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 2, false));
        var sum = add.BuildAdd(c1, c2, "sum");
        add.BuildRet(sum);

        var addI32 = gm.InstantiateFunction(add, [i32]);

        _ = pairI32;
        _ = addI32;

        // Print IR
        Console.WriteLine(module.PrintToString());
        
    }
}