namespace Wasm.Binary
{
    /// <summary>
    /// Represents a section's header.
    /// </summary>
    public struct SectionHeader
    {
        /// <summary>
        /// Creates a section header for a non-custom section with the given section
        /// name and payload length.
        /// </summary>
        /// <param name="Name">The section name.</param>
        /// <param name="PayloadLength">The length of the payload.</param>
        public SectionHeader(SectionName Name, uint PayloadLength)
        {
            this.Name = Name;
            this.PayloadLength = PayloadLength;
        }

        /// <summary>
        /// Gets the section's name.
        /// </summary>
        /// <returns>The section's name.</returns>
        public SectionName Name { get; private set; }

        /// <summary>
        /// Gets the length of the payload, in bytes.
        /// </summary>
        /// <returns>The length of the payload, in bytes.</returns>
        public uint PayloadLength { get; private set; }

        public override string ToString()
        {
            return Name + ", payload size: " + PayloadLength;
        }
    }
}
