using System;
using System.Collections.Generic;
using System.IO;
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
        public byte[] ExtraPayload { get; private set; }

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
                funcBodies.Add(FunctionBody.Read(Reader));
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
    public struct FunctionBody
    {
        /// <summary>
        /// Creates a function body from the given list of local entries
        /// and a block instruction.
        /// </summary>
        /// <param name="Locals">The list of local entries.</param>
        /// <param name="Body">The block instruction that serves as the function's body.</param>
        public FunctionBody(IEnumerable<LocalEntry> Locals, BlockInstruction Body)
            : this(Locals, Body, new byte[0])
        { }

        /// <summary>
        /// Creates a function body from the given list of local entries,
        /// a block instruction and the specified extra payload.
        /// </summary>
        /// <param name="Locals">The list of local entries.</param>
        /// <param name="Body">The block instruction that serves as the function's body.</param>
        /// <param name="ExtraPayload">
        /// The function body's extra payload, which is placed right after the function body.
        /// </param>
        public FunctionBody(IEnumerable<LocalEntry> Locals, BlockInstruction Body, byte[] ExtraPayload)
        {
            this.Locals = new List<LocalEntry>(Locals);
            this.Body = Body;
            this.ExtraPayload = ExtraPayload;
        }

        /// <summary>
        /// Gets the list of local entries for this function body.
        /// </summary>
        /// <returns>The list of local entries.</returns>
        public List<LocalEntry> Locals { get; private set; }

        /// <summary>
        /// Gets the function body's block instruction, which represents
        /// the actual function body.
        /// </summary>
        /// <returns>The function body block instruction.</returns>
        public BlockInstruction Body { get; private set; }

        /// <summary>
        /// Gets this function body's additional payload.
        /// </summary>
        /// <returns>
        /// The additional payload, as an array of bytes.
        /// <c>null</c> indicates an empty additional payload.
        /// </returns>
        public byte[] ExtraPayload { get; private set; }

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
            using (var memStream = new MemoryStream())
            {
                var innerWriter = new BinaryWasmWriter(
                    new BinaryWriter(memStream),
                    Writer.StringEncoding);

                // Write the number of local entries to the file.
                innerWriter.WriteVarUInt32((uint)Locals.Count);

                // Write the local variables to the file.
                foreach (var local in Locals)
                {
                    local.WriteTo(Writer);
                }

                // Write the body to the file.
                Body.WriteContentsTo(Writer);

                if (HasExtraPayload)
                {
                    // If we have at least one byte of additional payload,
                    // then we should write it to the stream now.
                    Writer.Writer.Write(ExtraPayload);
                }

                // Seek to the beginning of the memory stream.
                memStream.Seek(0, SeekOrigin.Begin);

                // Write the size of the function body to follow, in bytes.
                Writer.WriteVarUInt32((uint)memStream.Length);

                // Write the memory stream to the writer's stream.
                memStream.WriteTo(Writer.Writer.BaseStream);
            }
        }

        /// <summary>
        /// Reads a function body from the given WebAssembly file reader.
        /// </summary>
        /// <param name="Reader">The WebAssembly file reader to use.</param>
        /// <returns>A function body.</returns>
        public static FunctionBody Read(BinaryWasmReader Reader)
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
                localEntries.Add(LocalEntry.Read(Reader));
            }

            // Read the function's body block.
            var body = BlockOperator.ReadBlockContents(WasmType.Empty, Reader);

            // Skip any remaining bytes.
            var extraPayload = Reader.ReadRemainingPayload(startPos, funcBodyLength);

            return new FunctionBody(localEntries, body, extraPayload);
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

            if (Body.Contents.Count > 0)
            {
                Writer.Write("- Function body:");
                var instructionWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
                foreach (var instr in Body.Contents)
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
    public struct LocalEntry
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
        public static LocalEntry Read(BinaryWasmReader Reader)
        {
            return new LocalEntry(Reader.ReadWasmValueType(), Reader.ReadVarUInt32());
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
    }
}