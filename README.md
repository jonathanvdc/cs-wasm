# `cs-wasm`

[![Build status](https://travis-ci.org/jonathanvdc/cs-wasm.svg?branch=master)](https://travis-ci.org/jonathanvdc/cs-wasm)
[![Build status](https://ci.appveyor.com/api/projects/status/4lfpgydcssxvr56o?svg=true)](https://ci.appveyor.com/project/jonathanvdc/cs-wasm)
[![NuGet](https://img.shields.io/nuget/v/Wasm.svg)](https://www.nuget.org/packages/Wasm)

`cs-wasm` is a C# library that can read, write, interpret and optimize binary WebAssembly files.

It tries to represent WebAssembly files as faithfully as possible; reading a file into memory and writing it back to disk is byte-for-byte equivalent to a simple copy.

## Setting up `cs-wasm`

Installing `cs-wasm` is easy. Just add a dependency on the [**Wasm** NuGet package](https://www.nuget.org/packages/Wasm) and you'll be all set.

## Using `cs-wasm`

Here are some sample code fragments to give you a feel of what it's like to use `cs-wasm`.

### Reading a WebAssembly file

Reading binary WebAssembly files is pretty easy. All we have to do is call a `WasmFile.ReadBinaryFrom` overload, like so:

```cs
using Wasm;
// ...
WasmFile file = WasmFile.ReadBinaryFrom(path);
// - or -
file = WasmFile.ReadBinaryFrom(stream);
```

### Writing a `WasmFile` to disk

Given a `WasmFile` stored in a variable `file`, we can just:

```cs
using Wasm;
// ...
WasmFile file;
// file = ...;
file.WriteBinaryTo(path);
// - or -
file.WriteBinaryTo(stream);
```

### Inspecting a `WasmFile`

`WasmFile` is modeled after the binary encoding for WebAssembly files. It contains a `Sections` property that returns a `List<Section>` which can be iterated through.

If you're interested in a specific type of section, then you can use the `GetSections`/`GetFirstSectionOrNull` methods defined by `WasmFile`. (Or you can just throw LINQ at the `Sections` list &ndash; it's up to you.)

For example, the following example prints the size of a WebAssembly file's linear memory, assuming that `file` defines a memory section which contains exactly one linear memory definition.

```cs
using System.Linq;
using Wasm;
// ...
WasmFile file;
// file = ...;
Console.WriteLine(
    "Memory size: {0}",
    file.GetFirstSectionOrNull<MemorySection>()
        .Memories
        .Single<MemoryType>()
        .Limits);
```

### Modifying a `WasmFile`

Most reference types (aka `class` types) in `cs-wasm` are mutable. So, to change the size of a `WasmFile`'s linear memory to exactly one page, i.e., 64KiB, we can do the following.

```cs
using System.Linq;
using Wasm;
// ...
WasmFile file;
// file = ...;
var memSection = file.GetFirstSectionOrNull<MemorySection>();
if (memSection == null)
{
    // The file doesn't specify a memory section, so we'll
    // have to create one and add it to the file.
    memSection = new MemorySection();
    file.Sections.Add(memSection);
}
memSection.Memories.Clear();
// Memory sizes are specified in WebAssembly pages,
// which are regions of storage with size 64KiB.
// `new ResizableLimits(1, 1)` creates a memory description
// that is initially one page (first argument) in size and
// is capped at one page of memory (second argument), so
// there will always be exactly one page of linear memory.
memSection.Memories.Add(
    new MemoryType(new ResizableLimits(1, 1))); 
```

### Creating a `WasmFile` from scratch

This one's easy. `WasmFile`'s parameterless constructor creates the bare-minimum WebAssembly module: it has an eight-byte header and nothing more.

```cs
using Wasm;
// ...
// Create a WebAssembly file.
var file = new WasmFile();
```

We may want to add some sections to our newly-created file now. The example from the previous section adds a memory section to a file if it doesn't exist already. Other kinds of sections can be added similarly. For instance, the following code adds a type section to `file`.

```cs
// Define a type section.
var typeSection = new TypeSection();
file.Sections.Add(typeSection);
```

### Interpreting a `WasmFile`

To interpret a WebAssembly file, you first need to instantiate it.

```cs
using Wasm.Interpret;
// ...
ModuleInstance module = ModuleInstance.Instantiate(wasmFile, importer);
```

Notice the `importer` argument there? That's the object from which a `ModuleInstance` can import functions, memories, global variables and tables. An `importer` must implement `IImporter`. You don't have to implement `IImporter` yourself if you don't feel like it, though: `PredefinedImporter` is perfectly adequate for most purposes:

```cs
using System;
using Wasm.Interpret;
// ...
IReadOnlyList<object> Print(IReadOnlyList<object> Values)
{
    Console.WriteLine(Values[0]);
    return new object[0];
}
// ...
// Create an importer.
var importer = new PredefinedImporter();
// Define a function called 'print_i32' that takes an i32 and
// returns nothing.
importer.DefineFunction(
    "print_i32",
    new DelegateFunctionDefinition(
        new WasmValueType[] { WasmValueType.Int32 },
        new WasmValueType[] { },
        Print));
// Instantiate the module.
ModuleInstance module = ModuleInstance.Instantiate(wasmFile, importer);
```

Once instantiated, you can access the module's functions, memories, global variables and tables, exported or otherwise. To run an exported function named `factorial` and print the result:

```cs
FunctionDefinition funcDef = module.ExportedFunctions["factorial"];
IReadOnlyList<object> results = funcDef.Invoke(new object[] { 10L });
Console.WriteLine("Return value: {0}", results[0]);
```

Alternatively, you might want to run the WebAssembly file's entry point. `ModuleInstance` does not expose an entry point directly, but it's easy enough to find it by examining the 'start' section of the `WasmFile` on which the `ModuleInstance` is based.

```cs
var startSec = wasmFile.GetFirstSectionOrNull<StartSection>();
FunctionDefinition mainFunc = module.Functions[(int)startSec.StartFunctionIndex];
mainFunc.Invoke(new object[] { });
```

### Optimizing a `WasmFile`

`cs-wasm` offers some basic optimizations for WebAssembly files. These optimizations include:

  * removing duplicate type table entries,
  * merging function body local entries, and
  * peephole optimizations for function body instructions.

These optimizations are mainly intended for people who want to manipulate/generate decent-looking WebAssembly files without jumping through too many hoops.

Applying all optimizations that ship with `cs-wasm` is a one-liner (not counting the lines that were added for context):

```cs
using Wasm.Optimize;
// ...
WasmFile file;
// file = ...;
file.Optimize();
```

### Assembling a text format file

The `cs-wasm` project includes an additional library&mdash;`Wasm.Text`, available as [a separate NuGet package](https://www.nuget.org/packages/Wasm.Text)&mdash;that can assemble WebAssembly text format files and scripts. To assemble a string of textual WebAssembly, add a dependency on that library and do the following:
```cs
using Wasm.Text;
using Pixie.Terminal;
// ...
var log = TerminalLog.Acquire();
WasmFile file = new Assembler(log).AssembleModule("(module)");
```

The resulting `WasmFile` can be manipulated in all the same ways that a regular `WasmFile` parsed from a binary module can.

### Other fun stuff

There are lots of things you can do with `cs-wasm`. Drop me a GitHub issue if you'd like a chat.

## Contributing

Want to help out? Thanks! That's awesome! You're very welcome to open an issue or send a pull request.
