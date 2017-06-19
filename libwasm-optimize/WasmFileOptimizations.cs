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
        /// <param name="File">The file to optimize.</param>
        public static void Optimize(this WasmFile File)
        {
            File.CompressFunctionTypes();
            foreach (var section in File.Sections)
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
        /// <param name="Section">The code section to optimize.</param>
        public static void Optimize(this CodeSection Section)
        {
            var optimizer = PeepholeOptimizer.DefaultOptimizer;
            foreach (var body in Section.Bodies)
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

