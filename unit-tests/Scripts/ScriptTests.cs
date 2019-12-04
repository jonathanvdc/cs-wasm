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
            foreach (var name in Directory.EnumerateFiles(Path.Combine("spec", "test", "core")).OrderBy(x => x))
            {
                if (name.EndsWith(".wast") && !blacklist.Any(x => name.EndsWith(x)))
                {
                    Console.WriteLine($" - {name}");
                    try
                    {
                        RunSpecScript(name);
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
        }

        private void RunSpecScript(string scriptPath)
        {
            var log = new TestLog(new[] { Severity.Error }, NullLog.Instance);
            var runner = new ScriptRunner(log);
            var scriptText = File.ReadAllText(scriptPath);
            runner.Run(scriptText, scriptPath);
        }
    }
}
