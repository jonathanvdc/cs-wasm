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

        [Test]
        public void AssembleModulesWithMemory()
        {
            var module = AssembleModule("(module (memory $mem (limits 10 40)))");
            Assert.AreEqual(1, module.Sections.Count);
            var memSection = module.GetFirstSectionOrNull<MemorySection>();
            Assert.IsNotNull(memSection);
            Assert.AreEqual(1, memSection.Memories.Count);
            var memory = memSection.Memories[0];
            Assert.AreEqual(10u, memory.Limits.Initial);
            Assert.IsTrue(memory.Limits.HasMaximum);
            Assert.AreEqual(40u, memory.Limits.Maximum);

            module = AssembleModule("(module (memory (limits 10)))");
            Assert.AreEqual(1, module.Sections.Count);
            memSection = module.GetFirstSectionOrNull<MemorySection>();
            Assert.IsNotNull(memSection);
            Assert.AreEqual(1, memSection.Memories.Count);
            memory = memSection.Memories[0];
            Assert.AreEqual(10u, memory.Limits.Initial);
            Assert.IsFalse(memory.Limits.HasMaximum);
        }

        [Test]
        public void AssembleBadMemoryModules()
        {
            AssertInvalidModule("(module (memory))");
            AssertInvalidModule("(module (memory (limits)))");
            AssertInvalidModule("(module (memory $mem (limits 78359126329586239865823 725357639275693276582334525)))");
            AssertInvalidModule("(module (memory $mem (limits 10e7 10e8)))");
            AssertInvalidModule("(module (memory (limits +10 +40)))");
            AssertInvalidModule("(module (memory $mem1 $mem2 (limits 10 40)))");
            AssertInvalidModule("(module (memory (limits 10 40) (limits 10 40)))");
        }

        private static void AssertInvalidModule(string text)
        {
            Assert.Throws(
                typeof(PixieException),
                () => AssembleModule(text));
        }

        private static WasmFile AssembleModule(string text)
        {
            var log = new TestLog(new[] { Severity.Error }, NullLog.Instance);
            var assembler = new Assembler(log);
            return assembler.AssembleModule(text);
        }
    }
}
