using System;
using System.Collections.Generic;
using System.Linq;

namespace Wasm.Optimize
{
    /// <summary>
    /// Defines function type optimizations.
    /// </summary>
    public static class FunctionTypeOptimizations
    {
        /// <summary>
        /// Takes a sequence of function types as input and produces
        /// a list of equivalent, distinct function types and a map
        /// that maps type indices from the old type sequence to
        /// indices of equivalent types in the new function type list.
        /// </summary>
        /// <param name="types">The sequence of types to make distinct.</param>
        /// <param name="newTypes">The list of distinct function types.</param>
        /// <param name="typeMapping">
        /// A map from function types that occur in <c>Types</c> to their equivalents in <c>NewTypes</c>.
        /// </param>
        public static void MakeFunctionTypesDistinct(
            IEnumerable<FunctionType> types,
            out IReadOnlyList<FunctionType> newTypes,
            out IReadOnlyDictionary<uint, uint> typeMapping)
        {
            var newTypeList = new List<FunctionType>();
            var structuralOldToNewTypeMap = new Dictionary<FunctionType, uint>(
                ConstFunctionTypeComparer.Instance);
            var referentialOldToNewTypeMap = new Dictionary<uint, uint>();
            uint i = 0;
            foreach (var oldType in types)
            {
                uint newTypeIndex;
                if (structuralOldToNewTypeMap.TryGetValue(oldType, out newTypeIndex))
                {
                    referentialOldToNewTypeMap[i] = newTypeIndex;
                }
                else
                {
                    newTypeIndex = (uint)newTypeList.Count;
                    structuralOldToNewTypeMap[oldType] = newTypeIndex;
                    referentialOldToNewTypeMap[i] = newTypeIndex;
                    newTypeList.Add(oldType);
                }
                i++;
            }
            newTypes = newTypeList;
            typeMapping = referentialOldToNewTypeMap;
        }

        /// <summary>
        /// Rewrites function type references in the given WebAssembly file
        /// by replacing keys from the rewrite map with their corresponding
        /// values.
        /// </summary>
        /// <param name="file">The WebAssembly file to rewrite.</param>
        /// <param name="rewriteMap">A mapping of original type indices to new type indices.</param>
        public static void RewriteFunctionTypeReferences(
            this WasmFile file,
            IReadOnlyDictionary<uint, uint> rewriteMap)
        {
            // Type references occur only in the import and function sections.
            var importSections = file.GetSections<ImportSection>();
            for (int i = 0; i < importSections.Count; i++)
            {
                var importSec = importSections[i];
                for (int j = 0; j < importSec.Imports.Count; j++)
                {
                    var importDecl = importSec.Imports[j] as ImportedFunction;
                    uint newIndex;
                    if (importDecl != null && rewriteMap.TryGetValue(importDecl.TypeIndex, out newIndex))
                    {
                        importDecl.TypeIndex = newIndex;
                    }
                }
            }

            var funcSections = file.GetSections<FunctionSection>();
            for (int i = 0; i < funcSections.Count; i++)
            {
                var funcSec = funcSections[i];
                for (int j = 0; j < funcSec.FunctionTypes.Count; j++)
                {
                    uint newIndex;
                    if (rewriteMap.TryGetValue(funcSec.FunctionTypes[j], out newIndex))
                    {
                        funcSec.FunctionTypes[j] = newIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Compresses function types in the given WebAssembly file
        /// by including only unique function types.
        /// </summary>
        /// <param name="file">The WebAssembly file to modify.</param>
        public static void CompressFunctionTypes(
            this WasmFile file)
        {
            // Grab the first type section.
            var typeSection = file.GetFirstSectionOrNull<TypeSection>();
            if (typeSection == null)
            {
                return;
            }

            // Make all types from the first type section distinct.
            IReadOnlyList<FunctionType> newTypes;
            IReadOnlyDictionary<uint, uint> typeIndexMap;
            MakeFunctionTypesDistinct(typeSection.FunctionTypes, out newTypes, out typeIndexMap);

            // Rewrite the type section's function types.
            typeSection.FunctionTypes.Clear();
            typeSection.FunctionTypes.AddRange(newTypes);

            // Rewrite type indices.
            file.RewriteFunctionTypeReferences(typeIndexMap);
        }
    }

    /// <summary>
    /// An equality comparer for function types that assumes that the contents
    /// of the function types it is given remain constant over the course of
    /// its operation. 
    /// </summary>
    public sealed class ConstFunctionTypeComparer : IEqualityComparer<FunctionType>
    {
        private ConstFunctionTypeComparer() { }

        /// <summary>
        /// Gets the one and only instance of the constant function type comparer.
        /// </summary>
        public static readonly ConstFunctionTypeComparer Instance = new ConstFunctionTypeComparer();

        /// <inheritdoc/>
        public bool Equals(FunctionType x, FunctionType y)
        {
            return Enumerable.SequenceEqual<WasmValueType>(x.ParameterTypes, y.ParameterTypes);
        }

        private static int HashSequence(IEnumerable<WasmValueType> values, int seed)
        {
            // Based on Brendan's answer to this StackOverflow question:
            // https://stackoverflow.com/questions/16340/how-do-i-generate-a-hashcode-from-a-byte-array-in-c
            int result = seed;
            foreach (var item in values)
            {
                result = (result * 31) ^ (int)item;
            }
            return result;
        }

        /// <inheritdoc/>
        public int GetHashCode(FunctionType obj)
        {
            return HashSequence(obj.ReturnTypes, HashSequence(obj.ParameterTypes, 0));
        }
    }
}
