using System;
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
        private static readonly string[] whitelist = new[] {
            "address.wast",
            "align.wast",
            "binary-leb128.wast",
            "block.wast",
            "break-drop.wast",
            "br_if.wast",
            // "br_table.wast",
            "br.wast",
            "call_indirect.wast",
            "call.wast",
            "comments.wast",
            "const.wast",
            // "conversions.wast",
            "custom.wast",
            "data.wast",
            "endianness.wast",
            "f32.wast",
            "f32_cmp.wast",
            "f64.wast",
            "f64_cmp.wast",
            "fac.wast",
            "forward.wast",
            "inline-module.wast",
            "int_exprs.wast",
            "int_literals.wast",
            "local_get.wast",
            "local_set.wast",
            "nop.wast",
            "skip-stack-guard-page.wast",
            "store.wast",
            "token.wast",
            "traps.wast",
            "type.wast",
            "unreached-invalid.wast",
            "utf8-custom-section-id.wast",
            "utf8-import-field.wast",
            "utf8-import-module.wast",
            "utf8-invalid-encoding.wast"
        };

        [Test]
        public void RunSpecScripts()
        {
            foreach (var name in Directory.EnumerateFiles(Path.Combine("spec", "test", "core")))
            {
                if (whitelist.Any(x => name.EndsWith(x)))
                {
                    Console.WriteLine($" - {name}");
                    RunSpecScript(name);
                }
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
