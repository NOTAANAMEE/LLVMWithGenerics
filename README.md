# LLVMWithGeneric

A small C# library that builds concrete LLVM IR from generic type/function definitions using LLVMSharp.

## What it does

- Define generic struct types and static functions.
- Instantiate them with concrete LLVM types.
- Generate LLVM IR with zero runtime overhead in the emitted IR (all generic info is resolved at instantiation time).

## Requirements

- .NET 9.0
- LLVMSharp 20.1.2
- LLVM

## Ownership model

`GenericModule` wraps existing `LLVMContextRef`, `LLVMModuleRef`, and `LLVMBuilderRef`. It does not own their lifetimes. Callers are responsible for creating and disposing those LLVM resources.

## Minimal usage

- See [Example](./Example)

## Project structure

- `LLVMWithGeneric/GenericModule.cs`: main entry point.
- `LLVMWithGeneric/Generic/*`: type system and values.
- `LLVMWithGeneric/Generic/GenericBase/*`: IR builder operations.
- `LLVMWithGeneric/Interface/*`: mangler and type registry interfaces.

## Notes

- The emitted IR contains only concrete types and instructions. Generic abstractions exist only at build time.
- Error messages are designed to be clear when template bindings are missing or invalid.
- On macOS, set DYLD_LIBRARY_PATH to your LLVM library directory.

## Acknowledgements

This project makes use of the following open-source libraries:

- [LLVMSharp](https://github.com/dotnet/LLVMSharp)
- [LLVM](https://github.com/llvm/llvm-project)

