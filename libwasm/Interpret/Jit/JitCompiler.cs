using System;
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
        private TypeBuilder wasmType;
        private IReadOnlyList<Func<IReadOnlyList<object>, IReadOnlyList<object>>> wrappers;

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
            this.wasmType = wasmModule.DefineType("CompiledWasm", TypeAttributes.Public | TypeAttributes.Sealed);
            var builderList = new List<MethodBuilder>();
            var wrapperList = new List<Func<IReadOnlyList<object>, IReadOnlyList<object>>>();
            foreach (var signature in types)
            {
                var methodDef = wasmType.DefineMethod(
                    $"func_{builderList.Count}",
                    MethodAttributes.Public | MethodAttributes.Static);
                methodDef.SetParameters(
                    new[] { typeof(int) }
                    .Concat(signature.ParameterTypes.Select(ValueHelpers.ToClrType))
                    .ToArray());
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
            this.wrappers = wrapperList;
        }

        /// <inheritdoc/>
        public override FunctionDefinition Compile(int index, FunctionBody body)
        {
            if (!TryCompile(body, builders[index].GetILGenerator()))
            {
                builders[index].CreateMethodBody(Array.Empty<byte>(), 0);
                MakeInterpreterThunk(index, builders[index].GetILGenerator());
            }
            return new CompiledFunctionDefinition(types[index], builders[index]);
        }

        private void MakeInterpreterThunk(int index, ILGenerator generator)
        {

            // To bridge the divide between JIT-compiled code and the interpreter,
            // we generate code that packs the parameter list of a JIT-compiled
            // function as an array of objects and feed that to the interpreter.
            // We then unpack the list of objects produced by the interpreter and
            // return.

            // TODO: generate code that actually does this.
            throw new NotImplementedException();
        }

        private bool TryCompile(FunctionBody body, ILGenerator generator)
        {
            return false;
        }
    }

    internal sealed class CompiledFunctionDefinition : FunctionDefinition
    {
        internal CompiledFunctionDefinition(FunctionType signature, MethodBuilder builder)
        {
            this.signature = signature;
            this.builder = builder;
        }

        private FunctionType signature;
        internal MethodBuilder builder;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ParameterTypes => signature.ParameterTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ReturnTypes => signature.ReturnTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> arguments, uint callStackDepth = 0)
        {
            var result = builder.Invoke(null, new object[] { callStackDepth }.Concat(arguments).ToArray());
            if (ReturnTypes.Count == 0)
            {
                return Array.Empty<object>();
            }
            else if (ReturnTypes.Count == 1)
            {
                return new[] { result };
            }
            else
            {
                throw new WasmException("Cannot compile functions with more than one return value.");
            }
        }
    }
}
