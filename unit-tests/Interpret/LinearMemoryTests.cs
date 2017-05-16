using System;
using Loyc.MiniTest;

namespace Wasm.Interpret
{
    [TestFixture]
    public class LinearMemoryTests
    {
        [Test]
        public void GrowMemory()
        {
            var limits = new ResizableLimits(1, 2);
            var memory = new LinearMemory(limits);

            Assert.AreEqual(1, memory.Size);
            Assert.AreEqual(1, memory.Grow(1));
            Assert.AreEqual(2, memory.Size);
            Assert.AreEqual(-1, memory.Grow(1));
            Assert.AreEqual(2, memory.Size);
        }
    }
}

