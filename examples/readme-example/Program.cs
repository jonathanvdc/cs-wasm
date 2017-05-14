using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wasm.ReadmeExample
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Create an empty WebAssembly file.
            var file = new WasmFile();

            // Define a type section.
            var typeSection = new TypeSection();
            file.Sections.Add(typeSection);

            // Write the file to a (memory) stream.
            var stream = new MemoryStream();
            file.WriteBinaryTo(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Read the file from a (memory) stream.
            file = WasmFile.ReadBinary(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Define a memory section if it doesn't exist already.
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

            // Print the memory size.
            List<MemoryType> memSections =
                file.GetFirstSectionOrNull<MemorySection>()
                    .Memories;
            Console.WriteLine(
                "Memory size: {0}",
                memSections
                    .Single<MemoryType>()
                    .Limits);

            // Save the file again.
            file.WriteBinaryTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
