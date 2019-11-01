using System.IO;
using Loyc.MiniTest;
using Pixie;
using Pixie.Terminal;
using Wasm.Text;

namespace Wasm.Interpret
{
    [TestFixture]
    public class ScriptTests
    {
        [Test]
        public void RunSpecScripts()
        {
            RunSpecScript("nop.wast");
        }

        private void RunSpecScript(string scriptName)
        {
            var log = new TestLog(new[] { Severity.Error }, NullLog.Instance);
            var runner = new ScriptRunner(log);
            var path = Path.Combine("spec", "test", "core", scriptName);
            var scriptText = File.ReadAllText(path);
            runner.Run(scriptText, path);
        }
    }
}
