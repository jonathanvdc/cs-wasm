using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Wasm.Instructions;

namespace Wasm.Interpret.Jit
{
    using InstructionImpl = Action<CompilerContext, ILGenerator>;

    /// <summary>
    /// A module compiler that compiles WebAssembly instructions to CIL.
    /// </summary>
    public class JitCompiler : ModuleCompiler
    {
        /// <summary>
        /// Creates a JIT compiler from the default operator implementations.
        /// </summary>
        public JitCompiler()
            : this(DefaultOperatorImplementations)
        { }

        /// <summary>
        /// Creates a JIT compiler from an operator implementation map.
        /// </summary>
        /// <param name="operatorImplementations">A mapping of operators to functions that compile instructions.</param>
        public JitCompiler(
            IReadOnlyDictionary<Operator, Func<Instruction, InstructionImpl>> operatorImplementations)
        {
            this.OperatorImplementations = operatorImplementations; 
        }

        /// <summary>
        /// Gets a mapping of operators to functions that compile instances of those operators
        /// to implementations. <c>null</c> implementations indicate that an operator instance
        /// cannot be compiled.
        /// </summary>
        /// <value>A mapping of operators to functions that compile instructions.</value>
        public IReadOnlyDictionary<Operator, Func<Instruction, InstructionImpl>> OperatorImplementations { get; private set; }

        private ModuleInstance module;
        private int offset;
        private IReadOnlyList<FunctionType> types;
        private AssemblyBuilder assembly;
        private IReadOnlyList<MethodBuilder> builders;
        private TypeBuilder wasmType;
        private IReadOnlyList<Func<IReadOnlyList<object>, IReadOnlyList<object>>> wrappers;
        private List<CompiledFunctionDefinition> functionDefinitions;

        private int helperFieldIndex;
        private Dictionary<FieldInfo, object> constFieldValues;

        /// <inheritdoc/>
        public override void Initialize(ModuleInstance module, int offset, IReadOnlyList<FunctionType> types)
        {
            this.module = module;
            this.offset = offset;
            this.types = types;
            this.helperFieldIndex = 0;
            this.constFieldValues = new Dictionary<FieldInfo, object>();
            this.functionDefinitions = new List<CompiledFunctionDefinition>();

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
                    new[] { typeof(uint) }
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
                var result = new CompiledFunctionDefinition(signature, builder);
                functionDefinitions.Add(result);
                return result;
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

            // Create an interpreted function definition and push it onto the stack.
            var func = new WasmFunctionDefinition(signature, body, module);
            var field = DefineConstHelperField(func);
            generator.Emit(OpCodes.Ldsfld, field);

            // Create the arguments array.
            EmitNewArray<object>(
                generator,
                signature.ParameterTypes
                    .Select<WasmValueType, Action<ILGenerator>>(
                        (p, i) => gen =>
                        {
                            gen.Emit(OpCodes.Ldarg, i + 1);
                            gen.Emit(OpCodes.Box, ValueHelpers.ToClrType(p));
                        })
                    .ToArray());

            // Load the call stack depth.
            generator.Emit(OpCodes.Ldarg_0);

            // Call the interpreter.
            generator.Emit(
                OpCodes.Call,
                typeof(WasmFunctionDefinition)
                    .GetMethod("Invoke", new[] { typeof(IReadOnlyList<object>), typeof(uint) }));

            // Unpack the interpreter's return values.
            EmitUnpackList(
                generator,
                signature.ReturnTypes.Select(ValueHelpers.ToClrType).ToArray(),
                typeof(IReadOnlyList<object>));

            // Finally, return.
            generator.Emit(OpCodes.Ret);

            return func;
        }

        private void EmitUnpackList(ILGenerator generator, IReadOnlyList<Type> elementTypes, Type type)
        {
            var itemGetter = type.GetProperties().First(x => x.GetIndexParameters().Length > 0).GetMethod;
            var local = generator.DeclareLocal(type);
            generator.Emit(OpCodes.Stloc, local);
            for (int i = 0; i < elementTypes.Count; i++)
            {
                generator.Emit(OpCodes.Ldloc, local);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, itemGetter);
                generator.Emit(OpCodes.Unbox_Any, elementTypes[i]);
            }
        }

        private FieldBuilder DefineConstHelperField<T>(T value)
        {
            var field = DefineHelperField(typeof(T));
            constFieldValues[field] = value;
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
                generator.Emit(OpCodes.Stelem, typeof(T));
            }
        }

        private bool TryCompile(FunctionBody body, ILGenerator generator)
        {
            var impl = GetImplementationOrNull(body.BodyInstructions);
            if (impl == null)
            {
                return false;
            }
            else
            {
                var context = new CompilerContext(this);
                impl(context, generator);
                generator.Emit(OpCodes.Ret);
                return true;
            }
        }

        private InstructionImpl GetImplementationOrNull(Instruction instruction)
        {
            Func<Instruction, InstructionImpl> impl;
            if (OperatorImplementations.TryGetValue(instruction.Op, out impl))
            {
                return impl(instruction);
            }
            else
            {
                return null;
            }
        }

        private InstructionImpl GetImplementationOrNull(IReadOnlyList<Instruction> instructions)
        {
            var impls = new List<InstructionImpl>();
            foreach (var instruction in instructions)
            {
                var instructionImpl = GetImplementationOrNull(instruction);
                if (instructionImpl == null)
                {
                    return null;
                }
                impls.Add(instructionImpl);
            }
            return (context, gen) =>
            {
                foreach (var impl in impls)
                {
                    impl(context, gen);
                }
            };
        }

        /// <inheritdoc/>
        public override void Finish()
        {
            // Create the type.
            var realType = wasmType.CreateType();

            // Populate its fields.
            foreach (var pair in constFieldValues)
            {
                realType.GetField(pair.Key.Name).SetValue(null, pair.Value);
            }
            constFieldValues = null;

            // Rewrite function definitions.
            foreach (var functionDef in functionDefinitions)
            {
                functionDef.method = realType.GetMethod(functionDef.method.Name);
            }
            functionDefinitions = null;
        }

        /// <summary>
        /// The default mapping of operators to their implementations.
        /// </summary>
        public static readonly IReadOnlyDictionary<Operator, Func<Instruction, InstructionImpl>> DefaultOperatorImplementations =
            new Dictionary<Operator, Func<Instruction, InstructionImpl>>()
        {
            { Operators.Int32Const, JitOperatorImpls.Int32Const },
            { Operators.Int64Const, JitOperatorImpls.Int64Const },
            { Operators.Float32Const, JitOperatorImpls.Float32Const },
            { Operators.Float64Const, JitOperatorImpls.Float64Const },
            { Operators.Int32Add, JitOperatorImpls.Int32Add },
            { Operators.Int32Sub, JitOperatorImpls.Int32Sub },
            { Operators.Int32Mul, JitOperatorImpls.Int32Mul }
        };
    }

    internal sealed class CompiledFunctionDefinition : FunctionDefinition
    {
        internal CompiledFunctionDefinition(FunctionType signature, MethodInfo method)
        {
            this.signature = signature;
            this.method = method;
        }

        private FunctionType signature;
        internal MethodInfo method;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ParameterTypes => signature.ParameterTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ReturnTypes => signature.ReturnTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> arguments, uint callStackDepth = 0)
        {
            object result;
            try
            {
                result = method.Invoke(null, new object[] { callStackDepth }.Concat(arguments).ToArray());
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
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
