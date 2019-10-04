using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// Represents a global section.
    /// </summary>
    public sealed class GlobalSection : Section
    {
        /// <summary>
        /// Creates an empty global section.
        /// </summary>
        public GlobalSection()
        {
            this.GlobalVariables = new List<GlobalVariable>();
        }

        /// <summary>
        /// Creates a global from the given list of global variables.
        /// </summary>
        /// <param name="globalVariables">The global section's list of global variables.</param>
        public GlobalSection(IEnumerable<GlobalVariable> globalVariables)
            : this(globalVariables, new byte[0])
        {
        }

        /// <summary>
        /// Creates a global section from the given list of global variables and additional payload.
        /// </summary>
        /// <param name="globalVariables">The global section's list of global variables.</param>
        /// <param name="extraPayload">The global section's additional payload.</param>
        public GlobalSection(IEnumerable<GlobalVariable> globalVariables, byte[] extraPayload)
        {
            this.GlobalVariables = new List<GlobalVariable>(globalVariables);
            this.ExtraPayload = extraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Global);

        /// <summary>
        /// Gets this global section's list of global variables.
        /// </summary>
        /// <returns>A list of global variable definitions.</returns>
        public List<GlobalVariable> GlobalVariables { get; private set; }

        /// <summary>
        /// This global section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <summary>
        /// Writes this WebAssembly section's payload to the given binary WebAssembly writer.
        /// </summary>
        /// <param name="writer">The writer to which the payload is written.</param>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)GlobalVariables.Count);
            foreach (var index in GlobalVariables)
            {
                index.WriteTo(writer);
            }
            writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the global section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">The WebAssembly file reader.</param>
        /// <returns>The parsed section.</returns>
        public static GlobalSection ReadSectionPayload(SectionHeader header, BinaryWasmReader reader)
        {
            long startPos = reader.Position;
            // Read the global variable definitions.
            uint count = reader.ReadVarUInt32();
            var globalVars = new List<GlobalVariable>();
            for (uint i = 0; i < count; i++)
            {
                globalVars.Add(GlobalVariable.ReadFrom(reader));
            }

            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, header);
            return new GlobalSection(globalVars, extraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(GlobalVariables.Count);
            writer.WriteLine();
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(writer);
            for (int i = 0; i < GlobalVariables.Count; i++)
            {
                writer.Write("#{0}:", i);
                indentedWriter.WriteLine();
                GlobalVariables[i].Dump(indentedWriter);
                writer.WriteLine();
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
    /// Describes a global variable's type and mutability.
    /// </summary>
    public sealed class GlobalType
    {
        /// <summary>
        /// Creates a global type from the given content type and mutability.
        /// </summary>
        /// <param name="contentType">The type of content in the global type.</param>
        /// <param name="isMutable">The global type's mutability.</param>
        public GlobalType(WasmValueType contentType, bool isMutable)
        {
            this.ContentType = contentType;
            this.IsMutable = isMutable;
        }

        /// <summary>
        /// Gets or sets the type of content stored in globals of this type.
        /// </summary>
        /// <returns>The type of content stored in globals of this type.</returns>
        public WasmValueType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the mutability of globals of this type.
        /// </summary>
        /// <returns>The mutability of globals of this type.</returns>
        public bool IsMutable { get; set; }

        /// <summary>
        /// Reads a global variable type from the given WebAssembly reader.
        /// </summary>
        /// <param name="reader">The WebAssembly reader to use.</param>
        /// <returns>The global variable type that was read.</returns>
        public static GlobalType ReadFrom(BinaryWasmReader reader)
        {
            return new GlobalType(reader.ReadWasmValueType(), reader.ReadVarUInt1());
        }

        /// <summary>
        /// Writes this global variable type to the given WebAssembly writer.
        /// </summary>
        /// <param name="writer">The WebAssembly writer to use.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteWasmValueType(ContentType);
            writer.WriteVarUInt1(IsMutable);
        }

        /// <summary>
        /// Writes a textual representation of this global variable type to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write("{type: ");
            DumpHelpers.DumpWasmType(ContentType, writer);
            writer.Write(", is_mutable: ");
            writer.Write(IsMutable);
            writer.Write("}");
        }
    }

    /// <summary>
    /// Describes a global variable's type and initial value.
    /// </summary>
    public sealed class GlobalVariable
    {
        /// <summary>
        /// Creates a global variable definition from the given type and initial value.
        /// </summary>
        /// <param name="type">The global variable definition's type.</param>
        /// <param name="initialValue">The global variable definition's initial value.</param>
        public GlobalVariable(GlobalType type, InitializerExpression initialValue)
        {
            this.Type = type;
            this.InitialValue = initialValue;
        }

        /// <summary>
        /// Gets or sets a description of this global variable.
        /// </summary>
        /// <returns>The global variable's description.</returns>
        public GlobalType Type { get; set; }

        /// <summary>
        /// Gets or sets this global variable's initial value.
        /// </summary>
        /// <returns>The initial value.</returns>
        public InitializerExpression InitialValue { get; set; }

        /// <summary>
        /// Reads a global variable definition from the given WebAssembly reader.
        /// </summary>
        /// <param name="reader">The WebAssembly reader to use.</param>
        /// <returns>The global variable definition that was read.</returns>
        public static GlobalVariable ReadFrom(BinaryWasmReader reader)
        {
            return new GlobalVariable(
                GlobalType.ReadFrom(reader),
                InitializerExpression.ReadFrom(reader));
        }

        /// <summary>
        /// Writes this global variable definition to the given WebAssembly writer.
        /// </summary>
        /// <param name="writer">The WebAssembly writer to use.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            Type.WriteTo(writer);
            InitialValue.WriteTo(writer);
        }

        /// <summary>
        /// Writes a textual representation of this global variable definition to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write("- Type: ");
            Type.Dump(writer);
            writer.WriteLine();
            writer.Write("- Initial value:");
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(writer);
            foreach (var instruction in InitialValue.BodyInstructions)
            {
                indentedWriter.WriteLine();
                instruction.Dump(indentedWriter);
            }
        }
    }
}
