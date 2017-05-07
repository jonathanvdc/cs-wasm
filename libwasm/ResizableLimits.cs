using System;
using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// A description of the limits of a table or memory.
    /// </summary>
    public struct ResizableLimits
    {
        /// <summary>
        /// Creates resizable limits with the given initial size and no maximal
        /// size.
        /// </summary>
        /// <param name="Initial">The initial size of the resizable limits.</param>
        public ResizableLimits(uint Initial)
        {
            this.Initial = Initial;
            this.Maximum = default(Nullable<uint>);
        }

        /// <summary>
        /// Creates resizable limits with the given initial and maximal sizes.
        /// </summary>
        /// <param name="Initial">The initial size of the resizable limits.</param>
        /// <param name="Maximum">The maximal size of the resizable limits.</param>
        public ResizableLimits(uint Initial, uint Maximum)
        {
            this.Initial = Initial;
            this.Maximum = new Nullable<uint>(Maximum);
        }

        /// <summary>
        /// Creates resizable limits with the given initial and maximal sizes.
        /// </summary>
        /// <param name="Initial">The initial size of the resizable limits.</param>
        /// <param name="Maximum">The optional maximal size of the resizable limits.</param>
        public ResizableLimits(uint Initial, Nullable<uint> Maximum)
        {
            this.Initial = Initial;
            this.Maximum = Maximum;
        }

        /// <summary>
        /// Gets a Boolean that tells if these resizable limits have a maximum size.
        /// </summary>
        public bool HasMaximum => Maximum.HasValue;

        /// <summary>
        /// Gets the initial length (in units of table elements or wasm pages).
        /// </summary>
        /// <returns>The initial length of the resizable limits.</returns>
        public uint Initial { get; private set; }

        /// <summary>
        /// Gets the maximal length (in units of table elements or wasm pages).
        /// This value may be <c>null</c> to signify that no maximum is specified.
        /// </summary>
        /// <returns>The maximum length of the resizable limits, if any.</returns>
        public Nullable<uint> Maximum { get; private set; }

        /// <summary>
        /// Writes these resizable limits to the given WebAssembly file writer.
        /// </summary>
        /// <param name="Writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt1(HasMaximum);
            Writer.WriteVarUInt32(Initial);
            if (HasMaximum)
            {
                Writer.WriteVarUInt32(Maximum.Value);
            }
        }

        /// <summary>
        /// Writes a textual representation of these resizable limits to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            Writer.Write("{initial: ");
            Writer.Write(Initial);
            if (HasMaximum)
            {
                Writer.Write(", max: ");
                Writer.Write(Maximum.Value);
            }
            Writer.Write("}");
        }
    }
}