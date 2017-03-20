namespace Wasm
{
    /// <summary>
    /// /// Represents a section's header.
    /// </summary>
    public struct SectionName
    {
        /// <summary>
        /// Creates a section name for a non-custom section with the given section code.
        /// </summary>
        /// <param name="Code">The section code.</param>
        public SectionName(SectionCode Code)
        {
            this.Code = Code;
            this.CustomName = null;
        }

        /// <summary>
        /// Creates a section header for a custom section with the given name.
        /// </summary>
        /// <param name="CustomName">The name of the custom section.</param>
        public SectionName(string CustomName)
        {
            this.Code = SectionCode.Custom;
            this.CustomName = CustomName;
        }

        /// <summary>
        /// Gets the section's code.
        /// </summary>
        /// <returns>The section code.</returns>
        public SectionCode Code { get; private set; }

        /// <summary>
        /// Gets a Boolean value that tells if the section is a custom section.
        /// </summary>
        public bool IsCustom => Code == SectionCode.Custom;

        /// <summary>
        /// Gets the name of the section, as a byte string. This applies only to
        /// custom sections.
        /// </summary>
        /// <returns>The name of the section if is this a custom section; otherwise, null.</returns>
        public string CustomName { get; private set; }

        public override string ToString()
        {
            if (IsCustom)
                return "Custom section '" + CustomName + "'";
            else
                return ((object)Code).ToString();
        }
    }
}
