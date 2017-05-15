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
    }
}