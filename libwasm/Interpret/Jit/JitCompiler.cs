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

        private int helperFieldIndex;

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
            var signature = types[index];
            var builder = builders[index];
            var ilGen = builder.GetILGenerator();
            if (TryCompile(body, ilGen))
            {
                return new CompiledFunctionDefinition(signature, builder);
            }
            else
            {
                return MakeInterpreterThunk(index, body, ilGen);
            }
        }

        private WasmFunctionDefinition MakeInterpreterThunk(int index, FunctionBody body, ILGenerator generator)
        {
            // To bridge the divide between JIT-compiled code and the interpreter,
            // we generate code that packs the parameter list of a JIT-compiled
            // function as an array of objects and feed that to the interpreter.
            // We then unpack the list of objects produced by the interpreter and
            // return.

            var signature = types[index];

            // Step one: create the arguments array.
            EmitNewArray<object>(
                generator,
                signature.ParameterTypes
                    .Select<WasmValueType, Action<ILGenerator>>(
                        (p, i) => gen => gen.Emit(OpCodes.Ldarg, i + 1))
                    .ToArray());

            // Load the call stack depth.
            generator.Emit(OpCodes.Ldarg_0);

            // Step two: call the interpreter.
            var func = new WasmFunctionDefinition(signature, body, module);
            var field = DefineConstHelperField(func);
            generator.Emit(OpCodes.Ldsfld, field);
            generator.Emit(
                OpCodes.Call,
                typeof(WasmFunctionDefinition)
                    .GetMethod("Invoke", new[] { typeof(IReadOnlyList<object>), typeof(uint) }));

            // Step three: unpack the interpreter's return values.
            EmitUnpackList(signature.ReturnTypes.Count, typeof(IReadOnlyList<object>));

            return func;
        }

        private void EmitUnpackList(int count, Type type)
        {
            throw new NotImplementedException();
        }

        private FieldBuilder DefineConstHelperField<T>(T value)
        {
            var field = DefineHelperField(typeof(T));
            field.SetValue(null, value);
            return field;
        }

        private FieldBuilder DefineHelperField(Type type)
        {
            return wasmType.DefineField($"helper_{helperFieldIndex++}", type, FieldAttributes.Public | FieldAttributes.Static);
        }

        private void EmitNewArray<T>(ILGenerator generator, IReadOnlyList<Action<ILGenerator>> valueGenerators)
        {
            generator.Emit(OpCodes.Ldc_I4, valueGenerators.Count);
            generator.Emit(OpCodes.Newarr, typeof(T));
            for (int i = 0; i < valueGenerators.Count; i++)
            {
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldc_I4, i);
                valueGenerators[i](generator);
                generator.Emit(OpCodes.Stelem);
            }
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
