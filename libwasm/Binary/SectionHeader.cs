/// <summary>
/// Represents a section's header.
/// </summary>
public struct SectionHeader
{
    /// <summary>
    /// Creates a section header for a non-custom section with the given section
    /// code and payload length.
    /// </summary>
    /// <param name="Code">The section code</param>
    /// <param name="PayloadLength">The length of the payload.</param>
    public SectionHeader(SectionCode Code, uint PayloadLength)
    {
        this.Code = Code;
        this.PayloadLength = PayloadLength;
        this.SectionName = default(ByteString);
    }

    /// <summary>
    /// Creates a section header for a custom section with the given name and
    /// payload length.
    /// </summary>
    /// <param name="SectionName">The name of the custom section</param>
    /// <param name="PayloadLength">The length of the payload.</param>
    public SectionHeader(ByteString SectionName, uint PayloadLength)
    {
        this.Code = SectionCode.Custom;
        this.PayloadLength = PayloadLength;
        this.SectionName = SectionName;
    }

    /// <summary>
    /// Gets the section's code.
    /// </summary>
    /// <returns>The section code.</returns>
    public SectionCode Code { get; private set; }

    /// <summary>
    /// Gets a Boolean value that tells if
    /// </summary>
    public bool IsCustom => Code == SectionCode.Custom;

    /// <summary>
    /// Gets the length of the payload, in bytes.
    /// </summary>
    /// <returns>The length of the payload, in bytes.</returns>
    public uint PayloadLength { get; private set; }

    /// <summary>
    /// Gets the name of the section, as a byte string. This applies only to
    /// custom sections.
    /// </summary>
    /// <returns>The name of the section if is this a custom section;
    /// otherwise, the null byte string.</returns>
    public ByteString SectionName { get; private set; }
}