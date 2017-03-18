using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// A base class for sections.
    /// </summary>
    public abstract class Section
    {
        public Section()
        {

        }

        /// <summary>
        /// Gets this section's name.
        /// </summary>
        /// <returns>The section name.</returns>
        public abstract SectionName Name { get; }
    }
}