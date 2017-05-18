using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Represents a WebAssembly function definition.
    /// </summary>
    public sealed class WasmFunctionDefinition : FunctionDefinition
    {
        public WasmFunctionDefinition(
            FunctionType Signature,
            FunctionBody Body,
            ModuleInstance Module)
        {
            this.Signature = Signature;
            this.body = Body;
            this.Module = Module;
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

        /// <summary>
        /// Invokes this function with the given argument list.
        /// </summary>
        /// <param name="Arguments">The list of arguments for this function's parameters.</param>
        /// <returns>The list of return values.</returns>
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> Arguments)
        {
            var locals = new List<Variable>();

            // Check argument types and create parameter variables.
            if (Signature.ParameterTypes.Count != Arguments.Count)
            {
                throw new WasmException(
                    "Function arity mismatch: function has " + Signature.ParameterTypes.Count +
                    " parameters and is given " + Arguments.Count + " arguments.");
            }

            // Turn each argument into a variable.
            for (int i = 0; i < Signature.ParameterTypes.Count; i++)
            {
                locals.Add(Variable.Create<object>(Signature.ParameterTypes[i], true, Arguments[i]));
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
            var context = new InterpreterContext(Module, locals);
            var interpreter = Module.Interpreter;
            foreach (var instruction in body.BodyInstructions)
            {
                interpreter.Interpret(instruction, context);
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
                        ((object)Signature.ReturnTypes[i]).ToString() +
                        " but is given a return value of type '" +
                        retVals[i].GetType().Name + "'.");
                }
            }

            return retVals;
        }
    }
}