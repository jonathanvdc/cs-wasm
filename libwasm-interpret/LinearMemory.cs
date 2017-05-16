using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Describes an instance of a linear memory specification.
    /// </summary>
    public sealed class LinearMemory
    {
        public LinearMemory(MemoryType Type)
        {
            this.Type = Type;
            this.memory = new List<byte>();
            GrowToSize(Type.Limits.Initial);
        }

        private List<byte> memory;

        /// <summary>
        /// Gets the specification for this linear memory.
        /// </summary>
        /// <returns>The specification for this linear memory/</returns>
        public MemoryType Type { get; private set; }

        /// <summary>
        /// Gets the size of the linear memory in units of pages.
        /// </summary>
        /// <returns>The size of the linear memory in units of pages.</returns>
        public uint Size => (uint)memory.Count / MemoryType.PageSize;

        /// <summary>
        /// Grows the memory to the given number of pages.
        /// Return the previous memory size in units of pages or -1 on failure.
        /// </summary>
        /// <param name="NewSize">The new number of pages in the linear memory.</param>
        /// <returns>The previous memory size in units of pages or -1 on failure.</returns>
        private int GrowToSize(uint NewSize)
        {
            if (Type.Limits.HasMaximum && NewSize > Type.Limits.Maximum.Value)
            {
                return -1;
            }

            int oldMemorySize = (int)Size;
            int newSizeInBytes = (int)(NewSize * MemoryType.PageSize);
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
        /// <param name="NumberOfPages">The number of pages to grow the linear memory by.</param>
        /// <returns>The previous memory size in units of pages or -1 on failure.</returns>
        public int Grow(uint NumberOfPages)
        {
            return GrowToSize(Size + NumberOfPages);
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
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 8-bit signed integers.
    /// </summary>
    public struct LinearMemoryAsInt8
    {
        internal LinearMemoryAsInt8(List<byte> Memory)
        {
            this.mem = Memory;
        }

        private List<byte> mem;

        public sbyte this[uint Offset]
        {
            get
            {
                return (sbyte)mem[(int)Offset];
            }
            set
            {
                mem[(int)Offset] = (byte)value;
            }
        }
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 16-bit signed integers.
    /// </summary>
    public struct LinearMemoryAsInt16
    {
        internal LinearMemoryAsInt16(List<byte> Memory)
        {
            this.mem = Memory;
        }

        private List<byte> mem;

        public short this[uint Offset]
        {
            get
            {
                return (short)(
                    (uint)mem[(int)Offset + 1] << 8 |
                    (uint)mem[(int)Offset]);
            }
            set
            {
                mem[(int)Offset + 1] = (byte)(value >> 8);
                mem[(int)Offset] = (byte)value;
            }
        }
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 32-bit signed integers.
    /// </summary>
    public struct LinearMemoryAsInt32
    {
        internal LinearMemoryAsInt32(List<byte> Memory)
        {
            this.mem = Memory;
        }

        private List<byte> mem;

        public int this[uint Offset]
        {
            get
            {
                return (int)mem[(int)Offset + 3] << 24
                    | (int)mem[(int)Offset + 2] << 16
                    | (int)mem[(int)Offset + 1] << 8
                    | (int)mem[(int)Offset];
            }
            set
            {
                mem[(int)Offset + 3] = (byte)(value >> 24);
                mem[(int)Offset + 2] = (byte)(value >> 16);
                mem[(int)Offset + 1] = (byte)(value >> 8);
                mem[(int)Offset] = (byte)value;
            }
        }
    }

    /// <summary>
    /// Accesses linear memory as a sequence of 32-bit signed integers.
    /// </summary>
    public struct LinearMemoryAsInt64
    {
        internal LinearMemoryAsInt64(List<byte> Memory)
        {
            this.mem = Memory;
        }

        private List<byte> mem;

        public long this[uint Offset]
        {
            get
            {
                return (long)mem[(int)Offset + 7] << 56
                    | (long)mem[(int)Offset + 6] << 48
                    | (long)mem[(int)Offset + 5] << 40
                    | (long)mem[(int)Offset + 4] << 32
                    | (long)mem[(int)Offset + 3] << 24
                    | (long)mem[(int)Offset + 2] << 16
                    | (long)mem[(int)Offset + 1] << 8
                    | (long)mem[(int)Offset];
            }
            set
            {
                mem[(int)Offset + 7] = (byte)(value >> 56);
                mem[(int)Offset + 6] = (byte)(value >> 48);
                mem[(int)Offset + 5] = (byte)(value >> 40);
                mem[(int)Offset + 4] = (byte)(value >> 32);
                mem[(int)Offset + 3] = (byte)(value >> 24);
                mem[(int)Offset + 2] = (byte)(value >> 16);
                mem[(int)Offset + 1] = (byte)(value >> 8);
                mem[(int)Offset] = (byte)value;
            }
        }
    }
}