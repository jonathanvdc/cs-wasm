using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator, which consists of an opcode, a defining type and a mnemonic.
    /// </summary>
    public abstract class Operator
    {
        public Operator(byte OpCode, WasmType DeclaringType, string Mnemonic)
        {
            this.OpCode = OpCode;
            this.DeclaringType = DeclaringType;
            this.Mnemonic = Mnemonic;
        }

        /// <summary>
        /// Gets the opcode for this operator.
        /// </summary>
        /// <returns>The operator's opcode.</returns>
        public byte OpCode { get; private set; }

        /// <summary>
        /// Gets the type that defines this operator, if any.
        /// </summary>
        /// <returns>The type that defines this operator, if any; otherwise, <c>WasmType.Empty</c>.</returns>
        public WasmType DeclaringType { get; private set; }

        /// <summary>
        /// Gets the mnemonic for this operator.
        /// </summary>
        /// <returns>The operator's mnemonic.</returns>
        public string Mnemonic { get; private set; }

        /// <summary>
        /// Gets a Boolean that tells if this operator has a declaring type.
        /// </summary>
        public bool HasDeclaringType => DeclaringType != WasmType.Empty;

        /// <summary>
        /// Reads the immediates (not the opcode) of a WebAssembly instruction
        /// for this operator from the given reader and returns the result as an
        /// instruction.
        /// </summary>
        /// <param name="Reader">The WebAssembly file reader to read immediates from.</param>
        /// <returns>A WebAssembly instruction.</returns>
        public abstract Instruction ReadImmediates(BinaryWasmReader Reader);

        /// <summary>
        /// Writes a string representation of this operator to the given text writer.
        /// </summary>
        /// <param name="Writer">
        /// The writer to which a representation of this operator is written.
        /// </param>
        public virtual void Dump(TextWriter Writer)
        {
            if (HasDeclaringType)
            {
                DumpHelpers.DumpWasmType(DeclaringType, Writer);
                Writer.Write(".");
            }
            Writer.Write(Mnemonic);
        }

        /// <summary>
        /// Creates a string representation of this operator.
        /// </summary>
        /// <returns>The operator's string representation.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            Dump(new StringWriter(builder));
            return builder.ToString();
        }
    }
}