using System;
using System.Collections.Generic;
using System.Linq;
using Loyc.MiniTest;
using Wasm.Instructions;

namespace Wasm.Optimize
{
    [TestFixture]
    public class FunctionBodyOptimizationTests
    {
        private static WasmValueType GenerateWasmValueType(Random rand)
        {
            switch (rand.Next(4))
            {
                case 0:
                    return WasmValueType.Int32;
                case 1:
                    return WasmValueType.Int64;
                case 2:
                    return WasmValueType.Float32;
                default:
                    return WasmValueType.Float64;
            }
        }

        private static IEnumerable<LocalEntry> GenerateLocalEntries(Random rand, int entryCount, int maxEntrySize)
        {
            var results = new List<LocalEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                results.Add(new LocalEntry(GenerateWasmValueType(rand), (uint)rand.Next(maxEntrySize)));
            }
            return results;
        }

        [Test]
        public void CompressLocals()
        {
            var rand = new Random();
            // Generate one hundred local entries. Create a compressed function body
            // from them as well as an uncompressed function body. Then check that they
            // declare the same locals.
            int testCount = 100;
            for (int i = 0; i < testCount; i++)
            {
                var localEntries = GenerateLocalEntries(rand, rand.Next(50), 10);
                var compressed = new FunctionBody(localEntries, Enumerable.Empty<Instruction>());
                compressed.CompressLocalEntries();
                var uncompressed = new FunctionBody(localEntries, Enumerable.Empty<Instruction>());
                AssertEquivalentLocals(compressed, uncompressed);
            }
        }

        private static void AssertEquivalentLocals(FunctionBody first, FunctionBody second)
        {
            var firstCopy = new FunctionBody(first.Locals, first.BodyInstructions);
            var secondCopy = new FunctionBody(second.Locals, second.BodyInstructions);
            firstCopy.ExpandLocalEntries();
            secondCopy.ExpandLocalEntries();
            Assert.IsTrue(Enumerable.SequenceEqual<LocalEntry>(firstCopy.Locals, secondCopy.Locals));
        }
    }
}

