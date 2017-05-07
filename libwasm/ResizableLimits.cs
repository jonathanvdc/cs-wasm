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
    }
}