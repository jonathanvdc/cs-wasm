namespace Wasm
{
    /// <summary>
    /// Represents a custom section.
    /// </summary>
    public sealed class CustomSection : Section
    {
        /// <summary>
        /// Creates a custom section from the given section name and payload.
        /// </summary>
        /// <param name="Name">The custom section's name.</param>
        /// <param name="Payload">The custom section's payload.</param>
        public CustomSection(string Name, byte[] Payload)
        {
            this.Name = Name;
            this.Payload = Payload;
        }

        /// <summary>
        /// Gets this custom section's name.
        /// </summary>
        /// <returns>The name of the custom section.</returns>
        public string Name { get; private set; }

        /// <summary>
        /// Gets this custom section's payload, as an array of bytes.
        /// </summary>
        /// <returns>A byte array that defines the custom section's payload.</returns>
        public byte[] Payload { get; private set; }
    }
}