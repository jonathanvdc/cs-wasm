using System.Collections.Generic;
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
        /// <param name="instructions">
        /// The instructions to match against the peephole optimization.
        /// </param>
        /// <returns>The number of instructions to rewrite.</returns>
        public override uint Match(IReadOnlyList<Instruction> instructions)
        {
            if (instructions.Count <= 1)
                return 0;

            var head = instructions[0];
            if (blockTerminatingInstructions.Contains(head.Op))
                return (uint)instructions.Count;
            else
                return 0;
        }

        /// <summary>
        /// Rewrites the given sequence of instructions.
        /// </summary>
        /// <param name="matched">
        /// A list of instructions that has been matched and will all be replaced.
        /// </param>
        /// <returns>The rewritten instructions.</returns>
        public override IReadOnlyList<Instruction> Rewrite(IReadOnlyList<Instruction> matched)
        {
            // Return only the first instruction, as no instruction in the linear
            // sequence of instructions will be executed after it is run.
            return new Instruction[] { matched[0] };
        }
    }
}
