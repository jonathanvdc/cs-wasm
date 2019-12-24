using System;
using System.Reflection.Emit;
using Wasm.Instructions;

namespace Wasm.Interpret.Jit
{
    using InstructionImpl = Action<CompilerContext, ILGenerator>;

    /// <summary>
    /// A collection of methods that compiler WebAssembly instructions to IL.
    /// </summary>
    public static class JitOperatorImpls
    {
        private static InstructionImpl ImplementAsOpCode(OpCode op)
        {
            return (context, gen) =>
            {
                gen.Emit(op);
            };
        }

        /// <summary>
        /// Compiles an 'i32.add' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Add(Instruction instruction)
        {
            return ImplementAsOpCode(OpCodes.Add);
        }

        /// <summary>
        /// Compiles an 'i32.sub' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Sub(Instruction instruction)
        {
            return ImplementAsOpCode(OpCodes.Sub);
        }

        /// <summary>
        /// Compiles an 'i32.mul' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Mul(Instruction instruction)
        {
            return ImplementAsOpCode(OpCodes.Mul);
        }

        /// <summary>
        /// Compiles an 'i32.const' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Const(Instruction instruction)
        {
            var immediate = Operators.Int32Const.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Ldc_I4, immediate);
            };
        }

        /// <summary>
        /// Compiles an 'i64.const' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Const(Instruction instruction)
        {
            var immediate = Operators.Int64Const.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Ldc_I8, immediate);
            };
        }

        /// <summary>
        /// Compiles an 'f32.const' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Float32Const(Instruction instruction)
        {
            var immediate = Operators.Float32Const.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Ldc_R4, immediate);
            };
        }

        /// <summary>
        /// Compiles an 'f64.const' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Float64Const(Instruction instruction)
        {
            var immediate = Operators.Float64Const.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Ldc_R8, immediate);
            };
        }

        /// <summary>
        /// Compiles a 'get_local' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl GetLocal(Instruction instruction)
        {
            var index = Operators.GetLocal.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                if (index < context.ParameterTypes.Count)
                {
                    gen.Emit(OpCodes.Ldarg, (int)index + 1);
                }
                else
                {
                    gen.Emit(OpCodes.Ldloc, context.Locals[index]);
                }
            };
        }

        /// <summary>
        /// Compiles a 'set_local' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl SetLocal(Instruction instruction)
        {
            var index = Operators.SetLocal.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                if (index < context.ParameterTypes.Count)
                {
                    gen.Emit(OpCodes.Starg, (int)index + 1);
                }
                else
                {
                    gen.Emit(OpCodes.Stloc, context.Locals[index]);
                }
            };
        }

        /// <summary>
        /// Compiles a 'tee_local' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl TeeLocal(Instruction instruction)
        {
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Dup);
                SetLocal(instruction)(context, gen);
            };
        }
    }
}
