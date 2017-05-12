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
        /// <param name="GlobalVariables">The global section's list of global variables.</param>
        public GlobalSection(IEnumerable<GlobalVariable> GlobalVariables)
            : this(GlobalVariables, new byte[0])
        {
        }

        /// <summary>
        /// Creates a global section from the given list of global variables and additional payload.
        /// </summary>
        /// <param name="GlobalVariables">The global section's list of global variables.</param>
        /// <param name="ExtraPayload">The global section's additional payload.</param>
        public GlobalSection(IEnumerable<GlobalVariable> GlobalVariables, byte[] ExtraPayload)
        {
            this.GlobalVariables = new List<GlobalVariable>(GlobalVariables);
            this.ExtraPayload = ExtraPayload;
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
        public byte[] ExtraPayload { get; private set; }

        /// <summary>
        /// Writes this WebAssembly section's payload to the given binary WebAssembly writer.
        /// </summary>
        /// <param name="Writer">The writer to which the payload is written.</param>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)GlobalVariables.Count);
            foreach (var index in GlobalVariables)
            {
                index.WriteTo(Writer);
            }
            Writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the global section with the given header.
        /// </summary>
        /// <param name="Header">The section header.</param>
        /// <param name="Reader">The WebAssembly file reader.</param>
        /// <returns>The parsed section.</returns>
        public static GlobalSection ReadSectionPayload(SectionHeader Header, BinaryWasmReader Reader)
        {
            long startPos = Reader.Position;
            // Read the global variable definitions.
            uint count = Reader.ReadVarUInt32();
            var globalVars = new List<GlobalVariable>();
            for (uint i = 0; i < count; i++)
            {
                globalVars.Add(GlobalVariable.ReadFrom(Reader));
            }

            // Skip any remaining bytes.
            var extraPayload = Reader.ReadRemainingPayload(startPos, Header);
            return new GlobalSection(globalVars, extraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; number of entries: ");
            Writer.Write(GlobalVariables.Count);
            Writer.WriteLine();
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
            for (int i = 0; i < GlobalVariables.Count; i++)
            {
                Writer.Write("#{0}:", i);
                indentedWriter.WriteLine();
                GlobalVariables[i].Dump(indentedWriter);
                Writer.WriteLine();
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
    /// Describes a global variable's type and mutability.
    /// </summary>
    public sealed class GlobalType
    {
        /// <summary>
        /// Creates a global type from the given content type and mutability.
        /// </summary>
        /// <param name="ContentType">The type of content in the global type.</param>
        /// <param name="IsMutable">The global type's mutability.</param>
        public GlobalType(WasmValueType ContentType, bool IsMutable)
        {
            this.ContentType = ContentType;
            this.IsMutable = IsMutable;
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
        /// <param name="Reader">The WebAssembly reader to use.</param>
        /// <returns>The global variable type that was read.</returns>
        public static GlobalType ReadFrom(BinaryWasmReader Reader)
        {
            return new GlobalType(Reader.ReadWasmValueType(), Reader.ReadVarUInt1());
        }

        /// <summary>
        /// Writes this global variable type to the given WebAssembly writer.
        /// </summary>
        /// <param name="Writer">The WebAssembly writer to use.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteWasmValueType(ContentType);
            Writer.WriteVarUInt1(IsMutable);
        }

        /// <summary>
        /// Writes a textual representation of this global variable type to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            Writer.Write("{type: ");
            DumpHelpers.DumpWasmType(ContentType, Writer);
            Writer.Write(", is_mutable: ");
            Writer.Write(IsMutable);
            Writer.Write("}");
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
        /// <param name="Type">The global variable definition's type.</param>
        /// <param name="InitialValue">The global variable definition's initial value.</param>
        public GlobalVariable(GlobalType Type, InitializerExpression InitialValue)
        {
            this.Type = Type;
            this.InitialValue = InitialValue;
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
        /// <param name="Reader">The WebAssembly reader to use.</param>
        /// <returns>The global variable definition that was read.</returns>
        public static GlobalVariable ReadFrom(BinaryWasmReader Reader)
        {
            return new GlobalVariable(
                GlobalType.ReadFrom(Reader),
                InitializerExpression.ReadFrom(Reader));
        }

        /// <summary>
        /// Writes this global variable definition to the given WebAssembly writer.
        /// </summary>
        /// <param name="Writer">The WebAssembly writer to use.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Type.WriteTo(Writer);
            InitialValue.WriteTo(Writer);
        }

        /// <summary>
        /// Writes a textual representation of this global variable definition to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            Writer.Write("- Type: ");
            Type.Dump(Writer);
            Writer.WriteLine();
            Writer.Write("- Initial value:");
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
            foreach (var instruction in InitialValue.BodyInstructions)
            {
                indentedWriter.WriteLine();
                instruction.Dump(indentedWriter);
            }
        }
    }
}