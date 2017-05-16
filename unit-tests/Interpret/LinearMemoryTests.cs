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

        [Test]
        public void RoundTripInt8()
        {
            var limits = new ResizableLimits(1, 2);
            var memory = new LinearMemory(limits);

            uint offset = MemoryType.PageSize / 2;
            sbyte data = 0x7F;
            var int8Mem = memory.Int8;
            int8Mem[offset] = data;
            Assert.AreEqual((int)data, (int)int8Mem[offset]);
        }

        [Test]
        public void RoundTripInt16()
        {
            var limits = new ResizableLimits(1, 2);
            var memory = new LinearMemory(limits);

            uint offset = MemoryType.PageSize / 2;
            short data = 0x1F2E;
            var int16Mem = memory.Int16;
            int16Mem[offset] = data;
            Assert.AreEqual((int)data, (int)int16Mem[offset]);
        }

        [Test]
        public void RoundTripInt32()
        {
            var limits = new ResizableLimits(1, 2);
            var memory = new LinearMemory(limits);

            uint offset = MemoryType.PageSize / 2;
            int data = 0x1F2E3D4C;
            var int32Mem = memory.Int32;
            int32Mem[offset] = data;
            Assert.AreEqual((int)data, (int)int32Mem[offset]);
        }

        [Test]
        public void RoundTripInt64()
        {
            var limits = new ResizableLimits(1, 2);
            var memory = new LinearMemory(limits);

            uint offset = MemoryType.PageSize / 2;
            long data = 0x1F2E3D4C5B6A7988;
            var int64Mem = memory.Int64;
            int64Mem[offset] = data;
            Assert.AreEqual((long)data, (long)int64Mem[offset]);
        }
    }
}

