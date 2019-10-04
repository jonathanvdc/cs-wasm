using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// A type of section that defines names for debugging purposes.
    /// </summary>
    public sealed class NameSection : Section
    {
        /// <summary>
        /// Creates an empty name section.
        /// </summary>
        public NameSection()
        {
            this.Names = new List<NameEntry>();
        }

        /// <summary>
        /// Creates a name section from the given sequence of name entries.
        /// </summary>
        /// <param name="names">The name entries to initialize this section with.</param>
        public NameSection(IEnumerable<NameEntry> names)
        {
            this.Names = new List<NameEntry>(names);
        }

        /// <summary>
        /// The custom name used for name sections.
        /// </summary>
        public const string CustomName = "name";

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(CustomName);

        /// <summary>
        /// Gets the name entries in this section.
        /// </summary>
        /// <returns>The name entries.</returns>
        public List<NameEntry> Names { get; private set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            foreach (var entry in Names)
            {
                entry.WriteTo(writer);
            }
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(Names.Count);
            writer.WriteLine();
            for (int i = 0; i < Names.Count; i++)
            {
                writer.Write("#");
                writer.Write(i);
                writer.Write(" -> ");
                Names[i].Dump(writer);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Reads the name section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">The WebAssembly file reader.</param>
        /// <returns>The parsed section.</returns>
        public static NameSection ReadSectionPayload(SectionHeader header, BinaryWasmReader reader)
        {
            var section = new NameSection();
            long startPos = reader.Position;
            while (reader.Position - startPos < header.PayloadLength)
            {
                // Read entries until we've read the entire section.
                section.Names.Add(NameEntry.Read(reader));
            }
            return section;
        }
    }

    /// <summary>
    /// An enumeration of encodings for name section entries.
    /// </summary>
    public enum NameEntryKind : byte
    {
        /// <summary>
        /// The name entry code for a module name entry.
        /// </summary>
        Module = 0,

        /// <summary>
        /// The name entry code for a function name entry.
        /// </summary>
        Function = 1,

        /// <summary>
        /// The name entry code for a local name entry.
        /// </summary>
        Local = 2
    }

    /// <summary>
    /// A base class for entries in the name section.
    /// </summary>
    public abstract class NameEntry
    {
        /// <summary>
        /// Gets this name entry's kind.
        /// </summary>
        /// <returns>The name entry kind.</returns>
        public abstract NameEntryKind Kind { get; }

        /// <summary>
        /// Writes this name entry's payload to the given writer.
        /// </summary>
        /// <param name="writer">The writer to write the payload to.</param>
        public abstract void WritePayloadTo(BinaryWasmWriter writer);

        /// <summary>
        /// Writes a textual representation of this name entry to the given writer.
        /// </summary>
        /// <param name="writer">The text writer.</param>
        public virtual void Dump(TextWriter writer)
        {
            using (var memStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memStream))
                {
                    WritePayloadTo(new BinaryWasmWriter(binaryWriter));
                    memStream.Seek(0, SeekOrigin.Begin);
                    writer.WriteLine("entry kind '{0}', payload size: {1}", Kind, memStream.Length);
                    var instructionWriter = DumpHelpers.CreateIndentedTextWriter(writer);
                    DumpHelpers.DumpStream(memStream, writer);
                }
            }
        }

        /// <summary>
        /// Writes this name entry's header and payload to the given writer.
        /// </summary>
        /// <param name="writer">The writer to write the header and payload to.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt7((byte)Kind);
            writer.WriteLengthPrefixed(WritePayloadTo);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            Dump(new StringWriter(builder));
            return builder.ToString();
        }

        /// <summary>
        /// Reads a name entry's header and payload from the given binary
        /// WebAssembly reader.
        /// </summary>
        /// <param name="reader">The reader to read the name entry from.</param>
        /// <returns>A name entry.</returns>
        public static NameEntry Read(BinaryWasmReader reader)
        {
            NameEntryKind kind = (NameEntryKind)reader.ReadVarUInt7();
            uint length = reader.ReadVarUInt32();
            switch (kind)
            {
                case NameEntryKind.Module:
                    return ModuleNameEntry.ReadPayload(reader, length);
                default:
                    return UnknownNameEntry.ReadPayload(reader, kind, length);
            }
        }
    }

    /// <summary>
    /// Describes a name section entry with an unknown entry kind code.
    /// </summary>
    public sealed class UnknownNameEntry : NameEntry
    {
        /// <summary>
        /// Creates an unknown name entry from the given entry kind and payload.
        /// </summary>
        public UnknownNameEntry(NameEntryKind kind, byte[] payload)
        {
            this.entryKind = kind;
            this.Payload = payload;
        }

        private NameEntryKind entryKind;

        /// <summary>
        /// Gets the payload for this unknown name entry.
        /// </summary>
        /// <returns>The payload.</returns>
        public byte[] Payload { get; set; }

        /// <inheritdoc/>
        public override NameEntryKind Kind => entryKind;

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.Writer.Write(Payload);
        }

        /// <summary>
        /// Reads an unknown name entry's payload.
        /// </summary>
        /// <param name="reader">The reader to read the name entry payload from.</param>
        /// <param name="kind">The kind of name entry to read.</param>
        /// <param name="length">The length of the name entry's payload, in bytes.</param>
        /// <returns>An unknown name entry.</returns>
        public static UnknownNameEntry ReadPayload(BinaryWasmReader reader, NameEntryKind kind, uint length)
        {
            return new UnknownNameEntry(kind, reader.ReadBytes((int)length));
        }
    }

    /// <summary>
    /// A name entry type that defines a module's name.
    /// </summary>
    public sealed class ModuleNameEntry : NameEntry
    {
        /// <summary>
        /// Creates a module name entry from the given name.
        /// </summary>
        public ModuleNameEntry(string moduleName)
        {
            this.ModuleName = moduleName;
        }

        /// <inheritdoc/>
        public override NameEntryKind Kind => NameEntryKind.Module;

        /// <summary>
        /// Gets or sets the module's name.
        /// </summary>
        /// <returns>The module's name.</returns>
        public string ModuleName { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteString(ModuleName);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write("module name: {0}", ModuleName);
        }

        /// <summary>
        /// Reads a module name entry's payload.
        /// </summary>
        /// <param name="reader">The reader to read the name entry payload from.</param>
        /// <param name="length">The length of the name entry's payload, in bytes.</param>
        /// <returns>A module name entry.</returns>
        public static ModuleNameEntry ReadPayload(BinaryWasmReader reader, uint length)
        {
            return new ModuleNameEntry(reader.ReadString());
        }
    }
}
