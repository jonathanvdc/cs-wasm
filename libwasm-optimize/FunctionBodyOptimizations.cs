using System;
using System.Collections.Generic;

namespace Wasm.Optimize
{
    /// <summary>
    /// Defines function body optimizations.
    /// </summary>
    public static class FunctionBodyOptimizations
    {
        /// <summary>
        /// Merges adjacent local entries that have the same type and deletes empty
        /// local entries.
        /// </summary>
        /// <param name="Body">The function body whose locals are to be compressed.</param>
        public static void CompressLocalEntries(this FunctionBody Body)
        {
            var newLocals = new List<LocalEntry>();
            var aggregateEntry = new LocalEntry(WasmValueType.Int32, 0);
            for (int i = 0; i < Body.Locals.Count; i++)
            {
                var currentEntry = Body.Locals[i];
                if (currentEntry.LocalType == aggregateEntry.LocalType)
                {
                    // If two adjacent local entries have the same type, then
                    // we should merge them.
                    aggregateEntry = new LocalEntry(
                        aggregateEntry.LocalType,
                        aggregateEntry.LocalCount + currentEntry.LocalCount);
                }
                else
                {
                    // We can't merge `currentEntry` with `aggregateEntry`. But maybe
                    // we'll be able to merge `currentEntry` and its successor.
                    if (aggregateEntry.LocalCount > 0)
                    {
                        newLocals.Add(aggregateEntry);
                    }
                    aggregateEntry = currentEntry;
                }
            }

            // Append the final entry to the new list of locals.
            if (aggregateEntry.LocalCount > 0)
            {
                newLocals.Add(aggregateEntry);
            }

            // Clear the old local list and replace its contents with the new entries.
            Body.Locals.Clear();
            Body.Locals.AddRange(newLocals);
        }
    }
}

