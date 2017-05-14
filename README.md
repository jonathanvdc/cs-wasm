# `cs-wasm`

[![Build status](https://travis-ci.org/jonathanvdc/cs-wasm.svg?branch=master)](https://travis-ci.org/jonathanvdc/cs-wasm)
[![Build status](https://ci.appveyor.com/api/projects/status/4lfpgydcssxvr56o?svg=true)](https://ci.appveyor.com/project/jonathanvdc/cs-wasm)

`cs-wasm` is a C# library that can read and write binary WebAssembly files. It tries to represent WebAssembly files as faithfully as possible; reading a file into memory and writing it back to disk is byte-for-byte equivalent to a simple copy.

## Using `cs-wasm`

Here are some sample code fragments for a couple of basic use cases to give you a feel of what it's like to use `cs-wasm`.

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

`WasmFile` is modeled after the binary encoding for WebAssembly files. It contains a `Sections` property that returns a `List<Section>` which you can iterate through.

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
        .Single<ResizableLimits>());
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
memSection.Memories.Add(new ResizableLimits(1, 1)); 
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

### Other fun stuff

There are lots of things you can do with `cs-wasm`. Drop me a GitHub issue if you'd like a chat.