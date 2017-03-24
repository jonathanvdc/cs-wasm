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
        /// <param name="FunctionTypes">The list of function types in this type section.</param>
        public TypeSection(IEnumerable<FunctionType> FunctionTypes)
            : this(FunctionTypes, new byte[0])
        {
        }

        /// <summary>
        /// Creates a type section from the given list of function types and an additional payload.
        /// </summary>
        /// <param name="FunctionTypes">The list of function types in this type section.</param>
        /// <param name="ExtraPayload">The additional payload for this section, as an array of bytes.</param>
        public TypeSection(IEnumerable<FunctionType> FunctionTypes, byte[] ExtraPayload)
        {
            this.FunctionTypes = new List<FunctionType>(FunctionTypes);
            this.ExtraPayload = ExtraPayload;
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
        public byte[] ExtraPayload { get; private set; }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Type);

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)FunctionTypes.Count);
            foreach (var type in FunctionTypes)
                type.WriteTo(Writer);

            Writer.Writer.Write(ExtraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; number of entries: ");
            Writer.Write(FunctionTypes.Count);
            Writer.WriteLine();
            for (int i = 0; i < FunctionTypes.Count; i++)
            {
                Writer.Write("#");
                Writer.Write(i);
                Writer.Write(" -> ");
                FunctionTypes[i].Dump(Writer);
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

        /// <summary>
        /// Reads a type section's payload from the given binary WebAssembly reader.
        /// </summary>
        /// <param name="Header">The type section's header.</param>
        /// <param name="Reader">A reader for a binary WebAssembly file.</param>
        /// <returns>A parsed type section.</returns>
        public static TypeSection ReadSectionPayload(SectionHeader Header, BinaryWasmReader Reader)
        {
            long initPos = Reader.Position;
            uint typeCount = Reader.ReadVarUInt32();
            var types = new List<FunctionType>((int)typeCount);
            for (uint i = 0; i < typeCount; i++)
            {
                types.Add(FunctionType.ReadFrom(Reader));
            }
            var extraBytes = Reader.ReadRemainingPayload(initPos, Header);
            return new TypeSection(types, extraBytes);
        }
    }

    /// <summary>
    /// Represents a function type entry in a type section.
    /// </summary>
    public struct FunctionType
    {
        /// <summary>
        /// Creates a function type from the given parameter types and return types.
        /// </summary>
        /// <param name="ParameterTypes">This function type's list of parameter types.</param>
        /// <param name="ReturnTypes">This function type's list of return types.</param>
        public FunctionType(
            IEnumerable<WasmValueType> ParameterTypes,
            IEnumerable<WasmValueType> ReturnTypes)
        {
            this.ParameterTypes = new List<WasmValueType>(ParameterTypes);
            this.ReturnTypes = new List<WasmValueType>(ReturnTypes);
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
        /// <param name="Writer">The writer for a binary WebAssembly file.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteWasmType(Form);
            Writer.WriteVarUInt32((uint)ParameterTypes.Count);
            foreach (var item in ParameterTypes)
                Writer.WriteWasmValueType(item);

            Writer.WriteVarUInt32((uint)ReturnTypes.Count);
            foreach (var item in ReturnTypes)
                Writer.WriteWasmValueType(item);
        }

        /// <summary>
        /// Writes a textual representation of this exported value to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            Writer.Write("func(");
            for (int i = 0; i < ParameterTypes.Count; i++)
            {
                if (i > 0)
                    Writer.Write(", ");

                DumpHelpers.DumpWasmType(ParameterTypes[i], Writer);
            }
            Writer.Write(") returns (");
            for (int i = 0; i < ReturnTypes.Count; i++)
            {
                if (i > 0)
                    Writer.Write(", ");

                DumpHelpers.DumpWasmType(ReturnTypes[i], Writer);
            }
            Writer.Write(")");
        }

        /// <summary>
        /// Reads a single function type from the given reader.
        /// </summary>
        /// <returns>The function type.</returns>
        public static FunctionType ReadFrom(BinaryWasmReader Reader)
        {
            WasmType form = (WasmType)Reader.ReadWasmType();
            if (form != WasmType.Func)
                throw new WasmException("Invalid 'form' value ('" + form + "') for function type.");

            uint paramCount = Reader.ReadVarUInt32();
            var paramTypes = new List<WasmValueType>((int)paramCount);
            for (uint i = 0; i < paramCount; i++)
            {
                paramTypes.Add(Reader.ReadWasmValueType());
            }

            uint retCount = Reader.ReadVarUInt32();
            var retTypes = new List<WasmValueType>((int)retCount);
            for (uint i = 0; i < retCount; i++)
            {
                retTypes.Add(Reader.ReadWasmValueType());
            }

            var result = default(FunctionType);
            result.ParameterTypes = paramTypes;
            result.ReturnTypes = retTypes;
            return result;
        }
    }
}