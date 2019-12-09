using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Describes an instance of a linear memory specification.
    /// </summary>
    public sealed class LinearMemory
    {
        /// <summary>
        /// Creates a linear memory from the given specification.
        /// </summary>
        /// <param name="limits">The specification for this linear memory: a range in units of pages.</param>
        public LinearMemory(ResizableLimits limits)
        {
            this.Limits = limits;
            this.memory = new List<byte>();
            GrowToSize(limits.Initial);
        }

        private List<byte> memory;

        /// <summary>
        /// Gets the specification for this linear memory.
        /// </summary>
        /// <returns>The specification for this linear memory/</returns>
        public ResizableLimits Limits { get; private set; }

        /// <summary>
        /// Gets the size of the linear memory in units of pages.
        /// </summary>
        /// <returns>The size of the linear memory in units of pages.</returns>
        public uint Size => (uint)memory.Count / MemoryType.PageSize;

        /// <summary>
        /// Grows the memory to the given number of pages.
        /// Return the previous memory size in units of pages or -1 on failure.
        /// </summary>
        /// <param name="newSize">The new number of pages in the linear memory.</param>
        /// <returns>The previous memory size in units of pages or -1 on failure.</returns>
        private int GrowToSize(uint newSize)
        {
            if (Limits.HasMaximum && newSize > Limits.Maximum.Value)
            {
                return -1;
            }

            int oldMemorySize = (int)Size;
            int newSizeInBytes = (int)(newSize * MemoryType.PageSize);
            while (memory.Count < newSizeInBytes)
            {
                memory.Add(0);
            }
            return oldMemorySize;
        }

        /// <summary>
        /// Grows linear memory by a given unsigned delta of pages.
        /// Return the previous memory size in units of pages or -1 on failure.
        /// </summary>
        /// <param name="numberOfPages">The number of pages to grow the linear memory by.</param>
        /// <returns>The previous memory size in units of pages or -1 on failure.</returns>
        public int Grow(uint numberOfPages)
        {
            return GrowToSize(Size + numberOfPages);
        }

        /// <summary>
        /// Accesses linear memory as a sequence of 8-bit signed integers.
        /// </summary>
        /// <returns>A view of this memory.</returns>
        public LinearMemoryAsInt8 Int8
        {
            get { return new LinearMemoryAsInt8(memory); }
        }

        /// <summary>
        /// Accesses linear memory as a sequence of 16-bit signed integers.
        /// </summary>
        /// <returns>A view of this memory.</returns>
        public LinearMemoryAsInt16 Int16
        {
            get { return new LinearMemoryAsInt16(memory); }
        }

        /// <summary>
        /// Accesses linear memory as a sequence of 32-bit signed integers.
        /// </summary>
        /// <returns>A view of this memory.</returns>
        public LinearMemoryAsInt32 Int32
        {
            get { return new LinearMemoryAsInt32(memory); }
        }

        /// <summary>
        /// Accesses linear memory as a sequence of 64-bit signed integers.
        /// </summary>
        /// <returns>A view of this memory.</returns>
        public LinearMemoryAsInt64 Int64
        {
            get { return new LinearMemoryAsInt64(memory); }
        }

        /// <summary>
        /// Accesses linear memory as a sequence of 32-bit floating-point numbers.
        /// </summary>
        /// <returns>A view of this memory.</returns>
        public LinearMemoryAsFloat32 Float32
        {
            get { return new LinearMemoryAsFloat32(memory); }
        }

        /// <summary>
        /// Accesses linear memory as a sequence of 64-bit floating-point numbers.
        /// </summary>
        /// <returns>A view of this memory.</returns>
        public LinearMemoryAsFloat64 Float64
        {
            get { return new LinearMemoryAsFloat64(memory); }
        }

        internal static void CheckBounds(List<byte> memory, uint offset, uint length)
        {
            if ((ulong)memory.Count < (ulong)offset + (ulong)length)
            {
                throw new WasmException("out of bounds memory access");
            }
        }
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 8-bit signed integers.
    /// </summary>
    public struct LinearMemoryAsInt8
    {
        internal LinearMemoryAsInt8(List<byte> memory)
        {
            this.mem = memory;
        }

        private List<byte> mem;

        /// <summary>
        /// Gets or sets a value in memory at a particular byte offset.
        /// </summary>
        /// <value>A value in memory.</value>
        public sbyte this[uint offset]
        {
            get
            {
                LinearMemory.CheckBounds(mem, offset, 1);
                return (sbyte)mem[(int)offset];
            }
            set
            {
                LinearMemory.CheckBounds(mem, offset, 1);
                mem[(int)offset] = (byte)value;
            }
        }
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 16-bit signed integers.
    /// </summary>
    public struct LinearMemoryAsInt16
    {
        internal LinearMemoryAsInt16(List<byte> memory)
        {
            this.mem = memory;
        }

        private List<byte> mem;

        /// <summary>
        /// Gets or sets a value in memory at a particular byte offset.
        /// </summary>
        /// <value>A value in memory.</value>
        public short this[uint offset]
        {
            get
            {
                LinearMemory.CheckBounds(mem, offset, 2);
                return (short)(
                    (uint)mem[(int)offset + 1] << 8 |
                    (uint)mem[(int)offset]);
            }
            set
            {
                LinearMemory.CheckBounds(mem, offset, 2);
                mem[(int)offset + 1] = (byte)(value >> 8);
                mem[(int)offset] = (byte)value;
            }
        }
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 32-bit signed integers.
    /// </summary>
    public struct LinearMemoryAsInt32
    {
        internal LinearMemoryAsInt32(List<byte> memory)
        {
            this.mem = memory;
        }

        private List<byte> mem;

        /// <summary>
        /// Gets or sets a value in memory at a particular byte offset.
        /// </summary>
        /// <value>A value in memory.</value>
        public int this[uint offset]
        {
            get
            {
                LinearMemory.CheckBounds(mem, offset, 4);
                return (int)mem[(int)offset + 3] << 24
                    | (int)mem[(int)offset + 2] << 16
                    | (int)mem[(int)offset + 1] << 8
                    | (int)mem[(int)offset];
            }
            set
            {
                LinearMemory.CheckBounds(mem, offset, 4);
                mem[(int)offset + 3] = (byte)(value >> 24);
                mem[(int)offset + 2] = (byte)(value >> 16);
                mem[(int)offset + 1] = (byte)(value >> 8);
                mem[(int)offset] = (byte)value;
            }
        }
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 64-bit signed integers.
    /// </summary>
    public struct LinearMemoryAsInt64
    {
        internal LinearMemoryAsInt64(List<byte> memory)
        {
            this.mem = memory;
        }

        private List<byte> mem;

        /// <summary>
        /// Gets or sets a value in memory at a particular byte offset.
        /// </summary>
        /// <value>A value in memory.</value>
        public long this[uint offset]
        {
            get
            {
                LinearMemory.CheckBounds(mem, offset, 8);
                return (long)mem[(int)offset + 7] << 56
                    | (long)mem[(int)offset + 6] << 48
                    | (long)mem[(int)offset + 5] << 40
                    | (long)mem[(int)offset + 4] << 32
                    | (long)mem[(int)offset + 3] << 24
                    | (long)mem[(int)offset + 2] << 16
                    | (long)mem[(int)offset + 1] << 8
                    | (long)mem[(int)offset];
            }
            set
            {
                LinearMemory.CheckBounds(mem, offset, 8);
                mem[(int)offset + 7] = (byte)(value >> 56);
                mem[(int)offset + 6] = (byte)(value >> 48);
                mem[(int)offset + 5] = (byte)(value >> 40);
                mem[(int)offset + 4] = (byte)(value >> 32);
                mem[(int)offset + 3] = (byte)(value >> 24);
                mem[(int)offset + 2] = (byte)(value >> 16);
                mem[(int)offset + 1] = (byte)(value >> 8);
                mem[(int)offset] = (byte)value;
            }
        }
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 32-bit floating-point numbers.
    /// </summary>
    public struct LinearMemoryAsFloat32
    {
        internal LinearMemoryAsFloat32(List<byte> memory)
        {
            this.mem = memory;
        }

        private List<byte> mem;

        /// <summary>
        /// Gets or sets a value in memory at a particular byte offset.
        /// </summary>
        /// <value>A value in memory.</value>
        public float this[uint offset]
        {
            get
            {
                return ValueHelpers.ReinterpretAsFloat32(new LinearMemoryAsInt32(mem)[offset]);
            }
            set
            {
                var uintView = new LinearMemoryAsInt32(mem);
                uintView[offset] = ValueHelpers.ReinterpretAsInt32(value);
            }
        }
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 64-bit floating-point numbers.
    /// </summary>
    public struct LinearMemoryAsFloat64
    {
        internal LinearMemoryAsFloat64(List<byte> memory)
        {
            this.mem = memory;
        }

        private List<byte> mem;

        /// <summary>
        /// Gets or sets a value in memory at a particular byte offset.
        /// </summary>
        /// <value>A value in memory.</value>
        public double this[uint offset]
        {
            get
            {
                return ValueHelpers.ReinterpretAsFloat64(new LinearMemoryAsInt64(mem)[offset]);
            }
            set
            {
                var uintView = new LinearMemoryAsInt64(mem);
                uintView[offset] = ValueHelpers.ReinterpretAsInt64(value);
            }
        }
    }
}
