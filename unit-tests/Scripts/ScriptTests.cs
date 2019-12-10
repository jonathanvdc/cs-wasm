using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Loyc.MiniTest;
using Pixie;
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
        public void RunSpecScripts()
        {
            var failed = new SortedSet<string>();
            var total = ScriptRunner.TestStatistics.Empty;
            foreach (var name in Directory.EnumerateFiles(Path.Combine("spec", "test", "core")).OrderBy(x => x))
            {
                if (name.EndsWith(".wast") && !blacklist.Any(x => name.EndsWith(x)))
                {
                    Console.WriteLine($" - {name}");
                    try
                    {
                        var tally = RunSpecScript(name);
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

        private ScriptRunner.TestStatistics RunSpecScript(string scriptPath)
        {
            var log = new TestLog(new[] { Severity.Error }, NullLog.Instance);
            var runner = new ScriptRunner(log);
            var scriptText = File.ReadAllText(scriptPath);
            return runner.Run(scriptText, scriptPath);
        }
    }
}
