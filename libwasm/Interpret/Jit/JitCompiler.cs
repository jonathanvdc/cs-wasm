using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Wasm.Interpret.Jit
{
    /// <summary>
    /// A module compiler that compiles WebAssembly instructions to CIL.
    /// </summary>
    public class JitCompiler : ModuleCompiler
    {
        private ModuleInstance module;
        private int offset;
        private IReadOnlyList<FunctionType> types;
        private AssemblyBuilder assembly;
        private IReadOnlyList<MethodBuilder> builders;

        /// <inheritdoc/>
        public override void Initialize(ModuleInstance module, int offset, IReadOnlyList<FunctionType> types)
        {
            this.module = module;
            this.offset = offset;
            this.types = types;

            this.assembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("wasm"),
                AssemblyBuilderAccess.RunAndCollect);
            var wasmModule = assembly.DefineDynamicModule("main");
            var wasmType = wasmModule.DefineType("CompiledWasm", TypeAttributes.Public | TypeAttributes.Sealed);
            var builderList = new List<MethodBuilder>();
            foreach (var signature in types)
            {
                var methodDef = wasmType.DefineMethod(
                    $"func_{builderList.Count}",
                    MethodAttributes.Public | MethodAttributes.Static);
                methodDef.SetParameters(signature.ParameterTypes.Select(ValueHelpers.ToClrType).ToArray());
                if (signature.ReturnTypes.Count == 0)
                {
                    methodDef.SetReturnType(typeof(void));
                }
                else if (signature.ReturnTypes.Count == 1)
                {
                    methodDef.SetReturnType(ValueHelpers.ToClrType(signature.ReturnTypes[0]));
                }
                else
                {
                    throw new WasmException("Cannot compile functions with more than one return value.");
                }
                builderList.Add(methodDef);
            }
            this.builders = builderList;
        }

        /// <inheritdoc/>
        public override FunctionDefinition Compile(int index, FunctionBody body)
        {
            throw new System.NotImplementedException();
        }
    }
}
