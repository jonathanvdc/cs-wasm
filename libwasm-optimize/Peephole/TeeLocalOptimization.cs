using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wasm.Instructions;

namespace Wasm.Optimize
{
    /// <summary>
    /// An optimization that rewrites `set_local x; get_local x` as `tee_local x`.
    /// </summary>
    public sealed class TeeLocalOptimization : PeepholeOptimization
    {
        private TeeLocalOptimization() { }

        /// <summary>
        /// The only instance of this optimization.
        /// </summary>
        public static readonly TeeLocalOptimization Instance = new TeeLocalOptimization();

        /// <summary>
        /// Tests if the items at the front of the given list of instructions
        /// match the peephole optimization; if a match occurs, a nonzero value
        /// is returned that indicates the number of instructions at the front
        /// of the list of instructions that should be rewritten.
        /// </summary>
        /// <param name="Instructions">
        /// The instructions to match against the peephole optimization.
        /// </param>
        /// <returns>The number of instructions to rewrite.</returns>
        public override uint Match(IReadOnlyList<Instruction> Instructions)
        {
            if (Instructions.Count < 2)
                return 0;

            var first = Instructions[0];
            if (first.Op != Operators.SetLocal)
                return 0;

            var second = Instructions[1];
            if (second.Op != Operators.GetLocal)
                return 0;

            var setLocal = Operators.SetLocal.CastInstruction(first);
            var getLocal = Operators.GetLocal.CastInstruction(second);
            if (setLocal.Immediate == getLocal.Immediate)
                return 2;
            else
                return 0;
        }

        /// <summary>
        /// Rewrites the given sequence of instructions.
        /// </summary>
        /// <param name="Matched">
        /// A list of instructions that has been matched and will all be replaced.
        /// </param>
        /// <returns>The rewritten instructions.</returns>
        public override IReadOnlyList<Instruction> Rewrite(IReadOnlyList<Instruction> Matched)
        {
            var setLocal = Operators.SetLocal.CastInstruction(Matched[0]);
            return new Instruction[] { Operators.TeeLocal.Create(setLocal.Immediate) };
        }
    }
}