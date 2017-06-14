using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wasm.Binary;
using Wasm.Instructions;

namespace Wasm
{
    /// <summary>
    /// A type of section that contains a body for every function in the module.
    /// </summary>
    public sealed class CodeSection : Section
    {
        public CodeSection()
        {
            this.Bodies = new List<FunctionBody>();
            this.ExtraPayload = new byte[0];
        }

        public CodeSection(IEnumerable<FunctionBody> Bodies)
            : this(Bodies, new byte[0])
        {
        }

        public CodeSection(IEnumerable<FunctionBody> Bodies, byte[] ExtraPayload)
        {
            this.Bodies = new List<FunctionBody>(Bodies);
            this.ExtraPayload = ExtraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Code);

        /// <summary>
        /// Gets the list of all values that are exported by this section.
        /// </summary>
        /// <returns>A list of all values exported by this section.</returns>
        public List<FunctionBody> Bodies { get; private set; }

        /// <summary>
        /// Gets this function section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)Bodies.Count);
            foreach (var body in Bodies)
            {
                body.WriteTo(Writer);
            }
            Writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the code section with the given header.
        /// </summary>
        /// <param name="Header">The section header.</param>
        /// <param name="Reader">A reader for a binary WebAssembly file.</param>
        /// <returns>The parsed section.</returns>
        public static CodeSection ReadSectionPayload(
            SectionHeader Header, BinaryWasmReader Reader)
        {
            long startPos = Reader.Position;
            // Read the function bodies.
            uint count = Reader.ReadVarUInt32();
            var funcBodies = new List<FunctionBody>();
            for (uint i = 0; i < count; i++)
            {
                funcBodies.Add(FunctionBody.ReadFrom(Reader));
            }

            // Skip any remaining bytes.
            var extraPayload = Reader.ReadRemainingPayload(startPos, Header);
            return new CodeSection(funcBodies, extraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; number of entries: ");
            Writer.Write(Bodies.Count);
            Writer.WriteLine();
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
            for (int i = 0; i < Bodies.Count; i++)
            {
                Writer.Write("#{0}: ", i);
                indentedWriter.WriteLine();
                Bodies[i].Dump(indentedWriter);
            }
            if (ExtraPayload.Length > 0)
            {
                Writer.Write("Extra payload size: ");
                Writer.Write(ExtraPayload.Length);
                Writer.WriteLine();
                DumpHelpers.DumpBytes(ExtraPayload, Writer);
                Writer.WriteLine();
            }
        }
    }

    /// <summary>
    /// An entry in a code section; defines a function body.
    /// </summary>
    public sealed class FunctionBody
    {
        /// <summary>
        /// Creates a function body from the given list of local entries
        /// and a block instruction.
        /// </summary>
        /// <param name="Locals">The list of local entries.</param>
        /// <param name="Body">The block instruction that serves as the function's body.</param>
        public FunctionBody(IEnumerable<LocalEntry> Locals, IEnumerable<Instruction> Body)
            : this(Locals, Body, new byte[0])
        { }

        /// <summary>
        /// Creates a function body from the given list of local entries,
        /// a list of instructions and the specified extra payload.
        /// </summary>
        /// <param name="Locals">The list of local entries.</param>
        /// <param name="Body">The list of instructions that serves as the function's body.</param>
        /// <param name="ExtraPayload">
        /// The function body's extra payload, which is placed right after the function body.
        /// </param>
        public FunctionBody(IEnumerable<LocalEntry> Locals, IEnumerable<Instruction> Body, byte[] ExtraPayload)
        {
            this.Locals = new List<LocalEntry>(Locals);
            this.BodyInstructions = new List<Instruction>(Body);
            this.ExtraPayload = ExtraPayload;
        }

        /// <summary>
        /// Gets the list of local entries for this function body.
        /// </summary>
        /// <returns>The list of local entries.</returns>
        public List<LocalEntry> Locals { get; private set; }

        /// <summary>
        /// Gets the function body's list of instructions.
        /// </summary>
        /// <returns>The list of function body instructions.</returns>
        public List<Instruction> BodyInstructions { get; private set; }

        /// <summary>
        /// Gets this function body's additional payload.
        /// </summary>
        /// <returns>
        /// The additional payload, as an array of bytes.
        /// <c>null</c> indicates an empty additional payload.
        /// </returns>
        public byte[] ExtraPayload { get; set; }

        /// <summary>
        /// Checks if this function body has at least one byte of additional payload.
        /// </summary>
        public bool HasExtraPayload => ExtraPayload != null && ExtraPayload.Length > 0;

        /// <summary>
        /// Writes this function body to the given WebAssembly file writer.
        /// </summary>
        /// <param name="Writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteLengthPrefixed(WriteContentsTo);
        }

        private void WriteContentsTo(BinaryWasmWriter Writer)
        {
            // Write the number of local entries to the file.
            Writer.WriteVarUInt32((uint)Locals.Count);

            // Write the local variables to the file.
            foreach (var local in Locals)
            {
                local.WriteTo(Writer);
            }

            // Write the body to the file.
            Operators.Block.Create(WasmType.Empty, BodyInstructions).WriteContentsTo(Writer);

            if (HasExtraPayload)
            {
                // If we have at least one byte of additional payload,
                // then we should write it to the stream now.
                Writer.Writer.Write(ExtraPayload);
            }
        }

        /// <summary>
        /// Reads a function body from the given WebAssembly file reader.
        /// </summary>
        /// <param name="Reader">The WebAssembly file reader to use.</param>
        /// <returns>A function body.</returns>
        public static FunctionBody ReadFrom(BinaryWasmReader Reader)
        {
            // Read the length of the function body definition.
            uint funcBodyLength = Reader.ReadVarUInt32();

            // Save the function body's start position.
            long startPos = Reader.Position;

            // Read the number of local entries.
            uint localEntryCount = Reader.ReadVarUInt32();

            // Read local entries.
            var localEntries = new List<LocalEntry>((int)localEntryCount);
            for (uint i = 0; i < localEntryCount; i++)
            {
                localEntries.Add(LocalEntry.ReadFrom(Reader));
            }

            // Read the function's body block.
            var body = Operators.Block.ReadBlockContents(WasmType.Empty, Reader);

            // Skip any remaining bytes.
            var extraPayload = Reader.ReadRemainingPayload(startPos, funcBodyLength);

            return new FunctionBody(localEntries, body.Contents, extraPayload);
        }

        /// <summary>
        /// Writes a textual representation of this function body to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            if (Locals.Count > 0)
            {
                Writer.Write("- Local entries:");
                var varEntryWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
                for (int i = 0; i < Locals.Count; i++)
                {
                    varEntryWriter.WriteLine();
                    varEntryWriter.Write("#{0}: ", i);
                    Locals[i].Dump(varEntryWriter);
                }
                Writer.WriteLine();
            }
            else
            {
                Writer.WriteLine("- No local entries");
            }

            if (BodyInstructions.Count > 0)
            {
                Writer.Write("- Function body:");
                var instructionWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
                foreach (var instr in BodyInstructions)
                {
                    instructionWriter.WriteLine();
                    instr.Dump(instructionWriter);
                }
                Writer.WriteLine();
            }
            else
            {
                Writer.WriteLine("- Empty function body");
            }

            if (HasExtraPayload)
            {
                Writer.Write("- Extra payload size: ");
                Writer.Write(ExtraPayload.Length);
                Writer.WriteLine();
                DumpHelpers.DumpBytes(ExtraPayload, Writer);
                Writer.WriteLine();
            }
        }
    }

    /// <summary>
    /// Describes a local entry. Each local entry declares a number of local variables
    /// of a given type. It is legal to have several entries with the same type.
    /// </summary>
    public struct LocalEntry : IEquatable<LocalEntry>
    {
        /// <summary>
        /// Creates a new local entry that defines <c>LocalCount</c> variables of type
        /// <c>LocalType</c>.
        /// </summary>
        /// <param name="LocalType">The type of the variables to define.</param>
        /// <param name="LocalCount">The number of local variables to define.</param>
        public LocalEntry(WasmValueType LocalType, uint LocalCount)
        {
            this.LocalType = LocalType;
            this.LocalCount = LocalCount;
        }

        /// <summary>
        /// Gets the type of the local variables declared by this local entry.
        /// </summary>
        /// <returns>The type of the local variables declared by this local entry.</returns>
        public WasmValueType LocalType { get; private set; }

        /// <summary>
        /// Gets the number of local variables defined by this local entry.
        /// </summary>
        /// <returns>The number of local variables defined by this local entry.</returns>
        public uint LocalCount { get; private set; }

        /// <summary>
        /// Writes this local entry to the given WebAssembly file writer.
        /// </summary>
        /// <param name="Writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32(LocalCount);
            Writer.WriteWasmValueType(LocalType);
        }

        /// <summary>
        /// Reads a local entry from the given WebAssembly file reader.
        /// </summary>
        /// <param name="Reader">The WebAssembly file reader.</param>
        /// <returns>A local entry.</returns>
        public static LocalEntry ReadFrom(BinaryWasmReader Reader)
        {
            var count = Reader.ReadVarUInt32();
            var type = Reader.ReadWasmValueType();
            return new LocalEntry(type, count);
        }

        /// <summary>
        /// Writes a textual representation of this local entry to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            Writer.Write(LocalCount);
            Writer.Write(" x ");
            DumpHelpers.DumpWasmType(LocalType, Writer);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            Dump(new StringWriter(builder));
            return builder.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object Obj)
        {
            return Obj is LocalEntry && Equals((LocalEntry)Obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ((int)LocalType << 16) | (int)LocalCount;
        }

        /// <summary>
        /// Checks if this local entry declares the same type and
        /// number of locals as the given local entry.
        /// </summary>
        /// <param name="Other">The other local entry.</param>
        /// <returns>
        /// <c>true</c> if this local entry is the same as the given entry; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(LocalEntry Other)
        {
            return LocalType == Other.LocalType && LocalCount == Other.LocalCount;
        }
    }
}