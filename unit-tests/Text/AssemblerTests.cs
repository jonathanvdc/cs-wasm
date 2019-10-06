using System.Linq;
using Loyc.MiniTest;
using Pixie;

namespace Wasm.Text
{
    [TestFixture]
    public class AssemblerTests
    {
        [Test]
        public void AssembleEmptyModule()
        {
            var module = AssembleModule("(module)");
            Assert.AreEqual(0, module.Sections.Count);
        }

        [Test]
        public void AssembleNamedEmptyModule()
        {
            var module = AssembleModule("(module $test_module)");
            Assert.AreEqual(1, module.Sections.Count);
            Assert.AreEqual(1, module.GetFirstSectionOrNull<NameSection>().Names.Count);
            Assert.AreEqual("test_module", module.ModuleName);
        }

        private WasmFile AssembleModule(string text)
        {
            var log = new TestLog(new[] { Severity.Error }, NullLog.Instance);
            var assembler = new Assembler(log);
            return assembler.AssembleModule(text);
        }
    }
}
