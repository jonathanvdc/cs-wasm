using System;

namespace Wasm.Optimize
{
    /// <summary>
    /// Defines convenience methods for WebAssembly file optimization.
    /// </summary>
    public static class WasmFileOptimizations
    {
        /// <summary>
        /// Applies all known optimizations to the given WebAssembly file.
        /// </summary>
        /// <param name="file">The file to optimize.</param>
        public static void Optimize(this WasmFile file)
        {
            file.CompressFunctionTypes();
            foreach (var section in file.Sections)
            {
                if (section is CodeSection)
                {
                    ((CodeSection)section).Optimize();
                }
            }
        }

        /// <summary>
        /// Applies all known optimizations to the given code section.
        /// </summary>
        /// <param name="section">The code section to optimize.</param>
        public static void Optimize(this CodeSection section)
        {
            var optimizer = PeepholeOptimizer.DefaultOptimizer;
            foreach (var body in section.Bodies)
            {
                // Compress local entries.
                body.CompressLocalEntries();

                // Apply peephole optimizations.
                var optInstructions = optimizer.Optimize(body.BodyInstructions);
                body.BodyInstructions.Clear();
                body.BodyInstructions.AddRange(optInstructions);
            }
        }
    }
}
