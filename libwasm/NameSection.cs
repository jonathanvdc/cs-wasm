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
        /// <param name="Names">The name entries to initialize this section with.</param>
        public NameSection(IEnumerable<NameEntry> Names)
        {
            this.Names = new List<NameEntry>(Names);
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
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            foreach (var entry in Names)
            {
                entry.WriteTo(Writer);
            }
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; number of entries: ");
            Writer.Write(Names.Count);
            Writer.WriteLine();
            for (int i = 0; i < Names.Count; i++)
            {
                Writer.Write("#");
                Writer.Write(i);
                Writer.Write(" -> ");
                Names[i].Dump(Writer);
                Writer.WriteLine();
            }
        }

        /// <summary>
        /// Reads the name section with the given header.
        /// </summary>
        /// <param name="Header">The section header.</param>
        /// <param name="Reader">The WebAssembly file reader.</param>
        /// <returns>The parsed section.</returns>
        public static NameSection ReadSectionPayload(SectionHeader Header, BinaryWasmReader Reader)
        {
            var section = new NameSection();
            long startPos = Reader.Position;
            while (Reader.Position - startPos < Header.PayloadLength)
            {
                // Read entries until we've read the entire section.
                section.Names.Add(NameEntry.Read(Reader));
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
        /// <param name="Writer">The writer to write the payload to.</param>
        public abstract void WritePayloadTo(BinaryWasmWriter Writer);

        /// <summary>
        /// Writes a textual representation of this name entry to the given writer.
        /// </summary>
        /// <param name="Writer">The text writer.</param>
        public virtual void Dump(TextWriter Writer)
        {
            using (var memStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memStream))
                {
                    WritePayloadTo(new BinaryWasmWriter(binaryWriter));
                    memStream.Seek(0, SeekOrigin.Begin);
                    Writer.WriteLine("entry kind '{0}', payload size: {1}", Kind, memStream.Length);
                    var instructionWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
                    DumpHelpers.DumpStream(memStream, Writer);
                }
            }
        }

        /// <summary>
        /// Writes this name entry's header and payload to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to write the header and payload to.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt7((byte)Kind);
            Writer.WriteLengthPrefixed(WritePayloadTo);
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
        /// <param name="Reader">The reader to read the name entry from.</param>
        /// <returns>A name entry.</returns>
        public static NameEntry Read(BinaryWasmReader Reader)
        {
            NameEntryKind kind = (NameEntryKind)Reader.ReadVarUInt7();
            uint length = Reader.ReadVarUInt32();
            switch (kind)
            {
                case NameEntryKind.Module:
                    return ModuleNameEntry.ReadPayload(Reader, length);
                default:
                    return UnknownNameEntry.ReadPayload(Reader, kind, length);
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
        public UnknownNameEntry(NameEntryKind Kind, byte[] Payload)
        {
            this.entryKind = Kind;
            this.Payload = Payload;
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
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.Writer.Write(Payload);
        }

        /// <summary>
        /// Reads an unknown name entry's payload.
        /// </summary>
        /// <param name="Reader">The reader to read the name entry payload from.</param>
        /// <param name="Kind">The kind of name entry to read.</param>
        /// <param name="Length">The length of the name entry's payload, in bytes.</param>
        /// <returns>An unknown name entry.</returns>
        public static UnknownNameEntry ReadPayload(BinaryWasmReader Reader, NameEntryKind Kind, uint Length)
        {
            return new UnknownNameEntry(Kind, Reader.ReadBytes((int)Length));
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
        public ModuleNameEntry(string ModuleName)
        {
            this.ModuleName = ModuleName;
        }

        /// <inheritdoc/>
        public override NameEntryKind Kind => NameEntryKind.Module;

        /// <summary>
        /// Gets or sets the module's name.
        /// </summary>
        /// <returns>The module's name.</returns>
        public string ModuleName { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteString(ModuleName);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write("module name: {0}", ModuleName);
        }

        /// <summary>
        /// Reads a module name entry's payload.
        /// </summary>
        /// <param name="Reader">The reader to read the name entry payload from.</param>
        /// <param name="Length">The length of the name entry's payload, in bytes.</param>
        /// <returns>A module name entry.</returns>
        public static ModuleNameEntry ReadPayload(BinaryWasmReader Reader, uint Length)
        {
            return new ModuleNameEntry(Reader.ReadString());
        }
    }
}