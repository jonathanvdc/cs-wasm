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
        /// <param name="code">The section code.</param>
        public SectionName(SectionCode code)
        {
            this.Code = code;
            this.CustomName = null;
        }

        /// <summary>
        /// Creates a section header for a custom section with the given name.
        /// </summary>
        /// <param name="customName">The name of the custom section.</param>
        public SectionName(string customName)
        {
            this.Code = SectionCode.Custom;
            this.CustomName = customName;
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
        /// <param name="other">The other section name.</param>
        /// <returns><c>true</c> if this section name is equal to the given section name; otherwise, <c>false</c>.</returns>
        public bool Equals(SectionName other)
        {
            if (IsCustom)
            {
                return other.IsCustom && CustomName == other.CustomName;
            }
            else
            {
                return Code == other.Code;
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
        public override bool Equals(object other)
        {
            return other is SectionName && Equals((SectionName)other);
        }

        /// <summary>
        /// Checks if the given section names are the same.
        /// </summary>
        /// <param name="first">The first section name.</param>
        /// <param name="second">The second section name.</param>
        /// <returns><c>true</c> if the given section names are the same; otherwise, <c>false</c>.</returns>
        public static bool operator==(SectionName first, SectionName second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Checks if the given section names not are the same.
        /// </summary>
        /// <param name="first">The first section name.</param>
        /// <param name="second">The second section name.</param>
        /// <returns><c>true</c> if the given section names are not the same; otherwise, <c>false</c>.</returns>
        public static bool operator!=(SectionName first, SectionName second)
        {
            return !first.Equals(second);
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
