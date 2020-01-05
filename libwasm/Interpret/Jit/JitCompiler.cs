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
            if (TryCompile(signature, body, ilGen))
            {
                var result = new CompiledFunctionDefinition(signature, builder, module.Policy.TranslateExceptions);
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

        private bool TryCompile(FunctionType signature, FunctionBody body, ILGenerator generator)
        {
            var impl = GetImplementationOrNull(body.BodyInstructions);
            if (impl == null)
            {
                return false;
            }
            else
            {
                var locals = new Dictionary<uint, LocalBuilder>();
                var localTypes = new List<WasmValueType>(signature.ParameterTypes);
                uint localIndex = (uint)signature.ParameterTypes.Count;
                foreach (var item in body.Locals)
                {
                    for (uint i = 0; i < item.LocalCount; i++)
                    {
                        locals[localIndex++] = generator.DeclareLocal(ValueHelpers.ToClrType(item.LocalType));
                        localTypes.Add(item.LocalType);
                    }
                }
                var context = new CompilerContext(this, localTypes, signature.ParameterTypes.Count, locals);
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
            { Operators.Nop, JitOperatorImpls.Nop },
            { Operators.Drop, JitOperatorImpls.Drop },

            { Operators.GetLocal, JitOperatorImpls.GetLocal },
            { Operators.SetLocal, JitOperatorImpls.SetLocal },
            { Operators.TeeLocal, JitOperatorImpls.TeeLocal },

            { Operators.Int32Const, JitOperatorImpls.Int32Const },
            { Operators.Int64Const, JitOperatorImpls.Int64Const },
            { Operators.Float32Const, JitOperatorImpls.Float32Const },
            { Operators.Float64Const, JitOperatorImpls.Float64Const },

            { Operators.Int32Add, JitOperatorImpls.Int32Add },
            { Operators.Int32And, JitOperatorImpls.Int32And },
            { Operators.Int32Clz, JitOperatorImpls.Int32Clz },
            { Operators.Int32Ctz, JitOperatorImpls.Int32Ctz },
            { Operators.Int32DivS, JitOperatorImpls.Int32DivS },
            { Operators.Int32DivU, JitOperatorImpls.Int32DivU },
            { Operators.Int32Eq, JitOperatorImpls.Int32Eq },
            { Operators.Int32Eqz, JitOperatorImpls.Int32Eqz },
            { Operators.Int32GeS, JitOperatorImpls.Int32GeS },
            { Operators.Int32GeU, JitOperatorImpls.Int32GeU },
            { Operators.Int32GtS, JitOperatorImpls.Int32GtS },
            { Operators.Int32GtU, JitOperatorImpls.Int32GtU },
            { Operators.Int32LeS, JitOperatorImpls.Int32LeS },
            { Operators.Int32LeU, JitOperatorImpls.Int32LeU },
            { Operators.Int32LtS, JitOperatorImpls.Int32LtS },
            { Operators.Int32LtU, JitOperatorImpls.Int32LtU },
            { Operators.Int32Mul, JitOperatorImpls.Int32Mul },
            { Operators.Int32Ne, JitOperatorImpls.Int32Ne },
            { Operators.Int32Or, JitOperatorImpls.Int32Or },
            { Operators.Int32Popcnt, JitOperatorImpls.Int32Popcnt },
            { Operators.Int32RemS, JitOperatorImpls.Int32RemS },
            { Operators.Int32RemU, JitOperatorImpls.Int32RemU },
            { Operators.Int32Rotl, JitOperatorImpls.Int32Rotl },
            { Operators.Int32Rotr, JitOperatorImpls.Int32Rotr },
            { Operators.Int32Shl, JitOperatorImpls.Int32Shl },
            { Operators.Int32ShrS, JitOperatorImpls.Int32ShrS },
            { Operators.Int32ShrU, JitOperatorImpls.Int32ShrU },
            { Operators.Int32Sub, JitOperatorImpls.Int32Sub },
            { Operators.Int32WrapInt64, JitOperatorImpls.Int32WrapInt64 },
            { Operators.Int32Xor, JitOperatorImpls.Int32Xor },

            { Operators.Int64Add, JitOperatorImpls.Int64Add },
            { Operators.Int64And, JitOperatorImpls.Int64And },
            { Operators.Int64Clz, JitOperatorImpls.Int64Clz },
            { Operators.Int64Ctz, JitOperatorImpls.Int64Ctz },
            { Operators.Int64DivS, JitOperatorImpls.Int64DivS },
            { Operators.Int64DivU, JitOperatorImpls.Int64DivU },
            { Operators.Int64Eq, JitOperatorImpls.Int64Eq },
            { Operators.Int64Eqz, JitOperatorImpls.Int64Eqz },
            { Operators.Int64ExtendSInt32, JitOperatorImpls.Int64ExtendSInt32 },
            { Operators.Int64ExtendUInt32, JitOperatorImpls.Int64ExtendUInt32 },
            { Operators.Int64GeS, JitOperatorImpls.Int64GeS },
            { Operators.Int64GeU, JitOperatorImpls.Int64GeU },
            { Operators.Int64GtS, JitOperatorImpls.Int64GtS },
            { Operators.Int64GtU, JitOperatorImpls.Int64GtU },
            { Operators.Int64LeS, JitOperatorImpls.Int64LeS },
            { Operators.Int64LeU, JitOperatorImpls.Int64LeU },
            { Operators.Int64LtS, JitOperatorImpls.Int64LtS },
            { Operators.Int64LtU, JitOperatorImpls.Int64LtU },
            { Operators.Int64Mul, JitOperatorImpls.Int64Mul },
            { Operators.Int64Ne, JitOperatorImpls.Int64Ne },
            { Operators.Int64Or, JitOperatorImpls.Int64Or },
            { Operators.Int64Popcnt, JitOperatorImpls.Int64Popcnt },
            { Operators.Int64RemS, JitOperatorImpls.Int64RemS },
            { Operators.Int64RemU, JitOperatorImpls.Int64RemU },
            { Operators.Int64Rotl, JitOperatorImpls.Int64Rotl },
            { Operators.Int64Rotr, JitOperatorImpls.Int64Rotr },
            { Operators.Int64Shl, JitOperatorImpls.Int64Shl },
            { Operators.Int64ShrS, JitOperatorImpls.Int64ShrS },
            { Operators.Int64ShrU, JitOperatorImpls.Int64ShrU },
            { Operators.Int64Sub, JitOperatorImpls.Int64Sub },
            { Operators.Int64Xor, JitOperatorImpls.Int64Xor }
        };
    }

    internal sealed class CompiledFunctionDefinition : FunctionDefinition
    {
        internal CompiledFunctionDefinition(FunctionType signature, MethodInfo method, bool translateExceptions)
        {
            this.signature = signature;
            this.method = method;
            this.translateExceptions = translateExceptions;
        }

        private FunctionType signature;
        internal MethodInfo method;
        private bool translateExceptions;

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
                var inner = ex.InnerException;
                if (translateExceptions && TryTranslateException(inner, out Exception translate))
                {
                    throw translate;
                }
                else
                {
                    throw inner;
                }
            }
            catch (Exception ex)
            {
                if (translateExceptions && TryTranslateException(ex, out Exception translate))
                {
                    throw translate;
                }
                else
                {
                    throw;
                }
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

        private static bool TryTranslateException(Exception original, out Exception translated)
        {
            if (original.GetType() == typeof(DivideByZeroException))
            {
                translated = new TrapException(original.Message, TrapException.SpecMessages.IntegerDivideByZero);
                return true;
            }
            else if (original.GetType() == typeof(OverflowException))
            {
                translated = new TrapException(original.Message, TrapException.SpecMessages.IntegerOverflow);
                return true;
            }
            else
            {
                translated = null;
                return false;
            }
        }
    }
}
