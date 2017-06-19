using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wasm.Instructions;

namespace Wasm.Optimize
{
    /// <summary>
    /// An optimization that removes unreachable code.
    /// </summary>
    public sealed class UnreachableCodeOptimization : PeepholeOptimization
    {
        private UnreachableCodeOptimization() { }

        /// <summary>
        /// The only instance of this optimization.
        /// </summary>
        public static readonly UnreachableCodeOptimization Instance = new UnreachableCodeOptimization();

        private static readonly HashSet<Operator> blockTerminatingInstructions =
            new HashSet<Operator>()
        {
            Operators.Br,
            Operators.Unreachable,
            Operators.Return
        };

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
            if (Instructions.Count <= 1)
                return 0;

            var head = Instructions[0];
            if (blockTerminatingInstructions.Contains(head.Op))
                return (uint)Instructions.Count;
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
            // Return only the first instruction, as no instruction in the linear
            // sequence of instructions will be executed after it is run.
            return new Instruction[] { Matched[0] };
        }
    }
}