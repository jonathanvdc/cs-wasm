using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Loyc.MiniTest;
using Pixie;
using Wasm.Interpret;
using Wasm.Interpret.Jit;
using Wasm.Text;

namespace Wasm.Scripts
{
    [TestFixture]
    public class ScriptTests
    {
        private static readonly string[] blacklist = new[] {
            "const.wast",
            "float_exprs.wast",
            "float_literals.wast",
            "float_misc.wast",
            "linking.wast"
        };

        [Test]
        public void RunSpecScriptsWithInterpreter()
        {
            RunSpecScripts("interpreter", null);
        }

        [Test]
        public void RunSpecScriptsWithJit()
        {
            RunSpecScripts("jit", () => new JitCompiler());
        }

        public void RunSpecScripts(string compilerName, Func<ModuleCompiler> compiler)
        {
            var failed = new SortedSet<string>();
            var total = ScriptRunner.TestStatistics.Empty;
            foreach (var name in Directory.EnumerateFiles(Path.Combine("spec", "test", "core")).OrderBy(x => x))
            {
                if (name.EndsWith(".wast") && !blacklist.Any(x => name.EndsWith(x)))
                {
                    Console.WriteLine($" - {name} ({compilerName})");
                    try
                    {
                        var tally = RunSpecScript(name, compiler);
                        total += tally;
                        Console.WriteLine($"    -> {tally}");
                    }
                    catch
                    {
                        failed.Add(name.Split('/').Last());
                    }
                }
            }
            if (failed.Count > 0)
            {
                Console.WriteLine("Failed: " + string.Join(", ", failed.Select(x => $"\"{x}\"")));
                Assert.Fail();
            }
            Console.WriteLine($"Total: {total}");
        }

        private ScriptRunner.TestStatistics RunSpecScript(string scriptPath, Func<ModuleCompiler> compiler)
        {
            var log = new TestLog(new[] { Severity.Error }, NullLog.Instance);
            var runner = new ScriptRunner(log, compiler);
            var scriptText = File.ReadAllText(scriptPath);
            return runner.Run(scriptText, scriptPath);
        }
    }
}
