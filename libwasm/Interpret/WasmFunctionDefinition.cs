using System;
using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Represents a WebAssembly function definition.
    /// </summary>
    public sealed class WasmFunctionDefinition : FunctionDefinition
    {
        /// <summary>
        /// Creates a WebAssembly function definition from the given signature,
        /// function body and declaring module.
        /// </summary>
        /// <param name="signature">The function's signature.</param>
        /// <param name="body">The function's body.</param>
        /// <param name="module">The declaring module.</param>
        public WasmFunctionDefinition(
            FunctionType signature,
            FunctionBody body,
            ModuleInstance module)
        {
            this.Signature = signature;
            this.body = body;
            this.Module = module;
        }

        /// <summary>
        /// Gets the function's signature.
        /// </summary>
        /// <returns>The function's signature.</returns>
        public FunctionType Signature { get; private set; }

        /// <summary>
        /// The function's body.
        /// </summary>
        private FunctionBody body;

        /// <summary>
        /// Gets the module that owns this function definition.
        /// </summary>
        /// <returns>The declaring module.</returns>
        public ModuleInstance Module { get; private set; }

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ParameterTypes => Signature.ParameterTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ReturnTypes => Signature.ReturnTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> arguments, uint callStackDepth = 0)
        {
            var locals = new List<Variable>();

            // Check argument types and create parameter variables.
            if (Signature.ParameterTypes.Count != arguments.Count)
            {
                throw new WasmException(
                    "Function arity mismatch: function has " + Signature.ParameterTypes.Count +
                    " parameters and is given " + arguments.Count + " arguments.");
            }

            // Turn each argument into a variable.
            for (int i = 0; i < Signature.ParameterTypes.Count; i++)
            {
                locals.Add(Variable.Create<object>(Signature.ParameterTypes[i], true, arguments[i]));
            }

            // Turn each local into a variable.
            foreach (var localEntry in body.Locals)
            {
                for (int i = 0; i < localEntry.LocalCount; i++)
                {
                    locals.Add(Variable.CreateDefault(localEntry.LocalType, true));
                }
            }

            // Interpret the function body.
            var context = new InterpreterContext(Module, ReturnTypes, locals, Module.Policy, callStackDepth + 1);
            var interpreter = Module.Interpreter;
            foreach (var instruction in body.BodyInstructions)
            {
                interpreter.Interpret(instruction, context);
                if (context.BreakRequested)
                {
                    // Functions can use a break to return. This acts exactly like
                    // a regular return.
                    OperatorImpls.Return(context);
                    break;
                }
            }
            context.Return();

            // Check return types.
            var retVals = context.ReturnValues;
            if (retVals.Count != Signature.ReturnTypes.Count)
            {
                throw new WasmException(
                    "Return value arity mismatch: function expects " + Signature.ReturnTypes.Count +
                    " return values but is given " + retVals.Count + " return values.");
            }

            for (int i = 0; i < retVals.Count; i++)
            {
                if (!Variable.IsInstanceOf<object>(retVals[i], Signature.ReturnTypes[i]))
                {
                    throw new WasmException(
                        "Return type mismatch: function has return type '" +
                        Signature.ReturnTypes[i].ToString() +
                        " but is given a return value of type '" +
                        retVals[i].GetType().Name + "'.");
                }
            }

            return retVals;
        }
    }
}