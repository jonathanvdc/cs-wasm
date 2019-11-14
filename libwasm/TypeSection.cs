using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// Represents a type section in a WebAssembly file.
    /// </summary>
    public sealed class TypeSection : Section
    {
        /// <summary>
        /// Creates an empty type section.
        /// </summary>
        public TypeSection()
            : this(Enumerable.Empty<FunctionType>())
        {
        }

        /// <summary>
        /// Creates a type section from the given list of function types.
        /// </summary>
        /// <param name="functionTypes">The list of function types in this type section.</param>
        public TypeSection(IEnumerable<FunctionType> functionTypes)
            : this(functionTypes, new byte[0])
        {
        }

        /// <summary>
        /// Creates a type section from the given list of function types and an additional payload.
        /// </summary>
        /// <param name="functionTypes">The list of function types in this type section.</param>
        /// <param name="extraPayload">The additional payload for this section, as an array of bytes.</param>
        public TypeSection(IEnumerable<FunctionType> functionTypes, byte[] extraPayload)
        {
            this.FunctionTypes = new List<FunctionType>(functionTypes);
            this.ExtraPayload = extraPayload;
        }

        /// <summary>
        /// Gets this type section's list of function types.
        /// </summary>
        /// <returns>The list of function types in this type section.</returns>
        public List<FunctionType> FunctionTypes { get; private set; }

        /// <summary>
        /// This type section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Type);

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)FunctionTypes.Count);
            foreach (var type in FunctionTypes)
                type.WriteTo(writer);

            writer.Writer.Write(ExtraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(FunctionTypes.Count);
            writer.WriteLine();
            for (int i = 0; i < FunctionTypes.Count; i++)
            {
                writer.Write("#");
                writer.Write(i);
                writer.Write(" -> ");
                FunctionTypes[i].Dump(writer);
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

        /// <summary>
        /// Reads a type section's payload from the given binary WebAssembly reader.
        /// </summary>
        /// <param name="header">The type section's header.</param>
        /// <param name="reader">A reader for a binary WebAssembly file.</param>
        /// <returns>A parsed type section.</returns>
        public static TypeSection ReadSectionPayload(SectionHeader header, BinaryWasmReader reader)
        {
            long initPos = reader.Position;
            uint typeCount = reader.ReadVarUInt32();
            var types = new List<FunctionType>((int)typeCount);
            for (uint i = 0; i < typeCount; i++)
            {
                types.Add(FunctionType.ReadFrom(reader));
            }
            var extraBytes = reader.ReadRemainingPayload(initPos, header);
            return new TypeSection(types, extraBytes);
        }
    }

    /// <summary>
    /// Represents a function type entry in a type section.
    /// </summary>
    public sealed class FunctionType
    {
        /// <summary>
        /// Creates a function type.
        /// </summary>
        public FunctionType()
        {
            this.ParameterTypes = new List<WasmValueType>();
            this.ReturnTypes = new List<WasmValueType>();
        }

        /// <summary>
        /// Creates a function type from the given parameter types and return types.
        /// </summary>
        /// <param name="parameterTypes">This function type's list of parameter types.</param>
        /// <param name="returnTypes">This function type's list of return types.</param>
        public FunctionType(
            IEnumerable<WasmValueType> parameterTypes,
            IEnumerable<WasmValueType> returnTypes)
        {
            this.ParameterTypes = new List<WasmValueType>(parameterTypes);
            this.ReturnTypes = new List<WasmValueType>(returnTypes);
        }

        /// <summary>
        /// Creates a function type that takes ownership of the given parameter types and return types.
        /// </summary>
        /// <param name="parameterTypes">This function type's list of parameter types.</param>
        /// <param name="returnTypes">This function type's list of return types.</param>
        private FunctionType(
            List<WasmValueType> parameterTypes,
            List<WasmValueType> returnTypes)
        {
            this.ParameterTypes = parameterTypes;
            this.ReturnTypes = returnTypes;
        }

        /// <summary>
        /// Gets this function type's form, which is always WasmType.Func.
        /// </summary>
        public WasmType Form => WasmType.Func;

        /// <summary>
        /// Gets this function type's list of parameter types.
        /// </summary>
        /// <returns>The list of parameter types for this function.</returns>
        public List<WasmValueType> ParameterTypes { get; private set; }

        /// <summary>
        /// Gets this function type's list of return types.
        /// </summary>
        /// <returns>The list of return types for this function.</returns>
        public List<WasmValueType> ReturnTypes { get; private set; }

        /// <summary>
        /// Writes this function type to the given binary WebAssembly file.
        /// </summary>
        /// <param name="writer">The writer for a binary WebAssembly file.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteWasmType(Form);
            writer.WriteVarUInt32((uint)ParameterTypes.Count);
            foreach (var item in ParameterTypes)
                writer.WriteWasmValueType(item);

            writer.WriteVarUInt32((uint)ReturnTypes.Count);
            foreach (var item in ReturnTypes)
                writer.WriteWasmValueType(item);
        }

        /// <summary>
        /// Writes a textual representation of this exported value to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write("func(");
            for (int i = 0; i < ParameterTypes.Count; i++)
            {
                if (i > 0)
                    writer.Write(", ");

                DumpHelpers.DumpWasmType(ParameterTypes[i], writer);
            }
            writer.Write(") returns (");
            for (int i = 0; i < ReturnTypes.Count; i++)
            {
                if (i > 0)
                    writer.Write(", ");

                DumpHelpers.DumpWasmType(ReturnTypes[i], writer);
            }
            writer.Write(")");
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var writer = new StringWriter();
            Dump(writer);
            return writer.ToString();
        }

        /// <summary>
        /// Reads a single function type from the given reader.
        /// </summary>
        /// <returns>The function type.</returns>
        public static FunctionType ReadFrom(BinaryWasmReader reader)
        {
            WasmType form = (WasmType)reader.ReadWasmType();
            if (form != WasmType.Func)
                throw new WasmException("Invalid 'form' value ('" + form + "') for function type.");

            uint paramCount = reader.ReadVarUInt32();
            var paramTypes = new List<WasmValueType>((int)paramCount);
            for (uint i = 0; i < paramCount; i++)
            {
                paramTypes.Add(reader.ReadWasmValueType());
            }

            uint retCount = reader.ReadVarUInt32();
            var retTypes = new List<WasmValueType>((int)retCount);
            for (uint i = 0; i < retCount; i++)
            {
                retTypes.Add(reader.ReadWasmValueType());
            }

            return new FunctionType(paramTypes, retTypes);
        }
    }
}
