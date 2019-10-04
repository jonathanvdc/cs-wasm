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
        /// <summary>
        /// Creates an empty code section.
        /// </summary>
        public CodeSection()
        {
            this.Bodies = new List<FunctionBody>();
            this.ExtraPayload = new byte[0];
        }

        /// <summary>
        /// Creates a code section from a sequence of function bodies.
        /// </summary>
        /// <param name="bodies">The code section's function codies.</param>
        public CodeSection(IEnumerable<FunctionBody> bodies)
            : this(bodies, new byte[0])
        {
        }

        /// <summary>
        /// Creates a code section from a sequence of function bodies and a
        /// trailing payload.
        /// </summary>
        /// <param name="bodies">The code section's function bodies.</param>
        /// <param name="extraPayload">
        /// A sequence of bytes that have no intrinsic meaning; they are part
        /// of the code section but are placed after the code section's actual contents.
        /// </param>
        public CodeSection(IEnumerable<FunctionBody> bodies, byte[] extraPayload)
        {
            this.Bodies = new List<FunctionBody>(bodies);
            this.ExtraPayload = extraPayload;
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
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)Bodies.Count);
            foreach (var body in Bodies)
            {
                body.WriteTo(writer);
            }
            writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the code section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">A reader for a binary WebAssembly file.</param>
        /// <returns>The parsed section.</returns>
        public static CodeSection ReadSectionPayload(
            SectionHeader header, BinaryWasmReader reader)
        {
            long startPos = reader.Position;
            // Read the function bodies.
            uint count = reader.ReadVarUInt32();
            var funcBodies = new List<FunctionBody>();
            for (uint i = 0; i < count; i++)
            {
                funcBodies.Add(FunctionBody.ReadFrom(reader));
            }

            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, header);
            return new CodeSection(funcBodies, extraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(Bodies.Count);
            writer.WriteLine();
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(writer);
            for (int i = 0; i < Bodies.Count; i++)
            {
                writer.Write("#{0}: ", i);
                indentedWriter.WriteLine();
                Bodies[i].Dump(indentedWriter);
            }
            if (ExtraPayload.Length > 0)
            {
                writer.Write("Extra payload size: ");
                writer.Write(ExtraPayload.Length);
                writer.WriteLine();
                DumpHelpers.DumpBytes(ExtraPayload, writer);
                writer.WriteLine();
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
        /// <param name="locals">The list of local entries.</param>
        /// <param name="body">The block instruction that serves as the function's body.</param>
        public FunctionBody(IEnumerable<LocalEntry> locals, IEnumerable<Instruction> body)
            : this(locals, body, new byte[0])
        { }

        /// <summary>
        /// Creates a function body from the given list of local entries,
        /// a list of instructions and the specified extra payload.
        /// </summary>
        /// <param name="locals">The list of local entries.</param>
        /// <param name="body">The list of instructions that serves as the function's body.</param>
        /// <param name="extraPayload">
        /// The function body's extra payload, which is placed right after the function body.
        /// </param>
        public FunctionBody(IEnumerable<LocalEntry> locals, IEnumerable<Instruction> body, byte[] extraPayload)
        {
            this.Locals = new List<LocalEntry>(locals);
            this.BodyInstructions = new List<Instruction>(body);
            this.ExtraPayload = extraPayload;
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
        /// <param name="writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteLengthPrefixed(WriteContentsTo);
        }

        private void WriteContentsTo(BinaryWasmWriter writer)
        {
            // Write the number of local entries to the file.
            writer.WriteVarUInt32((uint)Locals.Count);

            // Write the local variables to the file.
            foreach (var local in Locals)
            {
                local.WriteTo(writer);
            }

            // Write the body to the file.
            Operators.Block.Create(WasmType.Empty, BodyInstructions).WriteContentsTo(writer);

            if (HasExtraPayload)
            {
                // If we have at least one byte of additional payload,
                // then we should write it to the stream now.
                writer.Writer.Write(ExtraPayload);
            }
        }

        /// <summary>
        /// Reads a function body from the given WebAssembly file reader.
        /// </summary>
        /// <param name="reader">The WebAssembly file reader to use.</param>
        /// <returns>A function body.</returns>
        public static FunctionBody ReadFrom(BinaryWasmReader reader)
        {
            // Read the length of the function body definition.
            uint funcBodyLength = reader.ReadVarUInt32();

            // Save the function body's start position.
            long startPos = reader.Position;

            // Read the number of local entries.
            uint localEntryCount = reader.ReadVarUInt32();

            // Read local entries.
            var localEntries = new List<LocalEntry>((int)localEntryCount);
            for (uint i = 0; i < localEntryCount; i++)
            {
                localEntries.Add(LocalEntry.ReadFrom(reader));
            }

            // Read the function's body block.
            var body = Operators.Block.ReadBlockContents(WasmType.Empty, reader);

            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, funcBodyLength);

            return new FunctionBody(localEntries, body.Contents, extraPayload);
        }

        /// <summary>
        /// Writes a textual representation of this function body to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            if (Locals.Count > 0)
            {
                writer.Write("- Local entries:");
                var varEntryWriter = DumpHelpers.CreateIndentedTextWriter(writer);
                for (int i = 0; i < Locals.Count; i++)
                {
                    varEntryWriter.WriteLine();
                    varEntryWriter.Write("#{0}: ", i);
                    Locals[i].Dump(varEntryWriter);
                }
                writer.WriteLine();
            }
            else
            {
                writer.WriteLine("- No local entries");
            }

            if (BodyInstructions.Count > 0)
            {
                writer.Write("- Function body:");
                var instructionWriter = DumpHelpers.CreateIndentedTextWriter(writer);
                foreach (var instr in BodyInstructions)
                {
                    instructionWriter.WriteLine();
                    instr.Dump(instructionWriter);
                }
                writer.WriteLine();
            }
            else
            {
                writer.WriteLine("- Empty function body");
            }

            if (HasExtraPayload)
            {
                writer.Write("- Extra payload size: ");
                writer.Write(ExtraPayload.Length);
                writer.WriteLine();
                DumpHelpers.DumpBytes(ExtraPayload, writer);
                writer.WriteLine();
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
        /// <param name="localType">The type of the variables to define.</param>
        /// <param name="localCount">The number of local variables to define.</param>
        public LocalEntry(WasmValueType localType, uint localCount)
        {
            this.LocalType = localType;
            this.LocalCount = localCount;
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
        /// <param name="writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32(LocalCount);
            writer.WriteWasmValueType(LocalType);
        }

        /// <summary>
        /// Reads a local entry from the given WebAssembly file reader.
        /// </summary>
        /// <param name="reader">The WebAssembly file reader.</param>
        /// <returns>A local entry.</returns>
        public static LocalEntry ReadFrom(BinaryWasmReader reader)
        {
            var count = reader.ReadVarUInt32();
            var type = reader.ReadWasmValueType();
            return new LocalEntry(type, count);
        }

        /// <summary>
        /// Writes a textual representation of this local entry to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write(LocalCount);
            writer.Write(" x ");
            DumpHelpers.DumpWasmType(LocalType, writer);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            Dump(new StringWriter(builder));
            return builder.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is LocalEntry && Equals((LocalEntry)obj);
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
        /// <param name="other">The other local entry.</param>
        /// <returns>
        /// <c>true</c> if this local entry is the same as the given entry; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(LocalEntry other)
        {
            return LocalType == other.LocalType && LocalCount == other.LocalCount;
        }
    }
}
