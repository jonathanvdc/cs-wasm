using System;

namespace Wasm
{
    /// <summary>
    /// /// Represents a section's header.
    /// </summary>
    public struct SectionName : IEquatable<SectionName>
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

        /// <summary>
        /// Checks if this section name is equal to the given section name.
        /// </summary>
        /// <param name="Other">The other section name.</param>
        /// <returns><c>true</c> if this section name is equal to the given section name; otherwise, <c>false</c>.</returns>
        public bool Equals(SectionName Other)
        {
            if (IsCustom)
            {
                return Other.IsCustom && CustomName == Other.CustomName;
            }
            else
            {
                return Code == Other.Code;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (IsCustom)
            {
                return CustomName.GetHashCode();
            }
            else
            {
                return (int)Code;
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object Other)
        {
            return Other is SectionName && Equals((SectionName)Other);
        }

        /// <summary>
        /// Checks if the given section names are the same.
        /// </summary>
        /// <param name="First">The first section name.</param>
        /// <param name="Second">The second section name.</param>
        /// <returns><c>true</c> if the given section names are the same; otherwise, <c>false</c>.</returns>
        public static bool operator==(SectionName First, SectionName Second)
        {
            return First.Equals(Second);
        }

        /// <summary>
        /// Checks if the given section names not are the same.
        /// </summary>
        /// <param name="First">The first section name.</param>
        /// <param name="Second">The second section name.</param>
        /// <returns><c>true</c> if the given section names are not the same; otherwise, <c>false</c>.</returns>
        public static bool operator!=(SectionName First, SectionName Second)
        {
            return !First.Equals(Second);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsCustom)
                return "Custom section '" + CustomName + "'";
            else
                return ((object)Code).ToString();
        }
    }
}
