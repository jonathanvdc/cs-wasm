using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction.
    /// </summary>
    public abstract class Instruction
    {
        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public abstract IOperator Operator { get; }

        /// <summary>
        /// Writes this instruction's data to the given WebAssembly file writer.
        /// </summary>
        /// <param name="Writer">The writer to write this instruction to.</param>
        public abstract void WriteTo(BinaryWasmWriter Writer);

        /// <summary>
        /// Writes a string representation of this instruction to the given text writer.
        /// </summary>
        /// <param name="Writer">
        /// The writer to which a representation of this instruction is written.
        /// </param>
        public virtual void Dump(TextWriter Writer)
        {
            Writer.WriteLine(Operator.Mnemonic);
        }

        /// <summary>
        /// Creates a string representation of this instruction.
        /// </summary>
        /// <returns>The instruction's string representation.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            Dump(new StringWriter(builder));
            return builder.ToString();
        }
    }
}