using System;
using System.Collections.Generic;
using System.Linq;
using Pixie;
using Pixie.Markup;
using Wasm.Interpret;

namespace Wasm.Text
{
    /// <summary>
    /// Maintains state for and runs a single WebAssembly test script.
    /// </summary>
    public sealed class ScriptRunner
    {
        /// <summary>
        /// Creates a new script runner.
        /// </summary>
        /// <param name="log">A log to send diagnostics to.</param>
        public ScriptRunner(ILog log)
        {
            this.Log = log;
            this.Assembler = new Assembler(log);
            this.moduleInstances = new List<ModuleInstance>();
        }

        /// <summary>
        /// Gets a log to which this script runner sends diagnostics.
        /// </summary>
        /// <value>A log.</value>
        public ILog Log { get; private set; }

        /// <summary>
        /// Gets the assembler that assembles modules for this script runner.
        /// </summary>
        /// <value>A WebAssembly text format assembler.</value>
        public Assembler Assembler { get; private set; }

        private List<ModuleInstance> moduleInstances;

        /// <summary>
        /// Runs a script, encoded as a sequence of expressions.
        /// </summary>
        /// <param name="expressions">The script, parsed as a sequence of expressions.</param>
        public void Run(IEnumerable<SExpression> expressions)
        {
            foreach (var item in expressions)
            {
                Run(item);
            }
        }

        /// <summary>
        /// Runs a script, encoded as a sequence of tokens.
        /// </summary>
        /// <param name="tokens">The script, parsed as a sequence of tokens.</param>
        public void Run(IEnumerable<Lexer.Token> tokens)
        {
            Run(Parser.ParseAsSExpressions(tokens, Log));
        }

        /// <summary>
        /// Runs a script, encoded as a string.
        /// </summary>
        /// <param name="script">The text of the script to run.</param>
        /// <param name="scriptName">The file name of the script to run.</param>
        public void Run(string script, string scriptName = "<string>")
        {
            Run(Lexer.Tokenize(script, scriptName));
        }

        /// <summary>
        /// Runs a single expression in the script.
        /// </summary>
        /// <param name="expression">The expression to run.</param>
        public void Run(SExpression expression)
        {
            if (expression.IsCallTo("module"))
            {
                var module = Assembler.AssembleModule(expression);
                var instance = Wasm.Interpret.ModuleInstance.Instantiate(module, new SpecTestImporter());
                moduleInstances.Add(instance);
            }
            else if (expression.IsCallTo("invoke") || expression.IsCallTo("get"))
            {
                RunAction(expression);
            }
            else if (expression.IsCallTo("assert_return"))
            {
                var results = RunAction(expression.Tail[0]);
                var expected = expression.Tail
                    .Skip(1)
                    .Zip(results, (expr, val) => EvaluateConstExpr(expr, val.GetType()))
                    .ToArray();

                if (!results.SequenceEqual(expected))
                {
                    Log.Log(
                        new LogEntry(
                            Severity.Error,
                            "assertion failed",
                            "action produced result ",
                            string.Join(", ", results),
                            "; expected ",
                            string.Join(", ", expected),
                            ".",
                            Assembler.Highlight(expression)));
                }
            }
            else
            {
                Log.Log(
                    new LogEntry(
                        Severity.Warning,
                        "unknown script command",
                        Quotation.QuoteEvenInBold(
                            "expression ",
                            expression.Head.Span.Text,
                            " was not recognized as a known script command."),
                        Assembler.Highlight(expression)));
            }
        }

        private object EvaluateConstExpr(SExpression expression, WasmValueType resultType)
        {
            var anonModule = new WasmFile();
            var instructions = Assembler.AssembleInstructionExpression(expression, anonModule);
            var inst = ModuleInstance.Instantiate(anonModule, new SpecTestImporter());
            return inst.Evaluate(new InitializerExpression(instructions), resultType);
        }

        private object EvaluateConstExpr(SExpression expression, Type resultType)
        {
            return EvaluateConstExpr(expression, ValueHelpers.ToWasmValueType(resultType));
        }

        private IReadOnlyList<object> RunAction(SExpression expression)
        {
            if (expression.IsCallTo("invoke"))
            {
                var name = Assembler.AssembleString(expression.Tail[0], Log);
                foreach (var inst in Enumerable.Reverse(moduleInstances))
                {
                    if (inst.ExportedFunctions.TryGetValue(name, out FunctionDefinition def))
                    {
                        var args = expression.Tail
                            .Skip(1)
                            .Zip(def.ParameterTypes, (expr, type) => EvaluateConstExpr(expr, type))
                            .ToArray();
                        try
                        {
                            return def.Invoke(args);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(
                                new LogEntry(
                                    Severity.Error,
                                    "unhandled exception",
                                    $"assertion threw {ex.GetType().Name}",
                                    new Paragraph(ex.ToString()),
                                    Assembler.Highlight(expression)));
                            throw;
                        }
                    }
                }

                Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "undefined function",
                        Quotation.QuoteEvenInBold(
                            "no function named ",
                            name,
                            " is defined here."),
                        Assembler.Highlight(expression)));
                return Array.Empty<object>();
            }
            else if (expression.IsCallTo("get"))
            {
                var name = Assembler.AssembleString(expression.Tail[0], Log);
                foreach (var inst in moduleInstances)
                {
                    if (inst.ExportedGlobals.TryGetValue(name, out Variable def))
                    {
                        return new[] { def.Get<object>() };
                    }
                }

                Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "undefined global",
                        Quotation.QuoteEvenInBold(
                            "no global named ",
                            name,
                            " is defined here."),
                        Assembler.Highlight(expression)));
                return Array.Empty<object>();
            }
            else
            {
                Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "unknown action",
                        Quotation.QuoteEvenInBold(
                            "expression ",
                            expression.Head.Span.Text,
                            " was not recognized as a known script action."),
                        Assembler.Highlight(expression)));
                return Array.Empty<object>();
            }
        }
    }
}
