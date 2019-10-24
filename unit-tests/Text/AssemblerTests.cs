using System;
using System.Text;
using Loyc.MiniTest;
using Pixie;
using Wasm.Interpret;

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
            var module = AssembleModule("(module (memory $mem 10 40))");
            Assert.AreEqual(1, module.Sections.Count);
            var memSection = module.GetFirstSectionOrNull<MemorySection>();
            Assert.IsNotNull(memSection);
            Assert.AreEqual(1, memSection.Memories.Count);
            var memory = memSection.Memories[0];
            Assert.AreEqual(10u, memory.Limits.Initial);
            Assert.IsTrue(memory.Limits.HasMaximum);
            Assert.AreEqual(40u, memory.Limits.Maximum);

            module = AssembleModule("(module (memory 10))");
            Assert.AreEqual(1, module.Sections.Count);
            memSection = module.GetFirstSectionOrNull<MemorySection>();
            Assert.IsNotNull(memSection);
            Assert.AreEqual(1, memSection.Memories.Count);
            memory = memSection.Memories[0];
            Assert.AreEqual(10u, memory.Limits.Initial);
            Assert.IsFalse(memory.Limits.HasMaximum);

            module = AssembleModule("(module (memory (data \"hello world\")))");
            Assert.AreEqual(2, module.Sections.Count);
            memSection = module.GetFirstSectionOrNull<MemorySection>();
            Assert.IsNotNull(memSection);
            Assert.AreEqual(1, memSection.Memories.Count);
            memory = memSection.Memories[0];
            Assert.AreEqual(1u, memory.Limits.Initial);
            Assert.IsTrue(memory.Limits.HasMaximum);
            Assert.AreEqual(1u, memory.Limits.Maximum);
            var dataSection = module.GetFirstSectionOrNull<DataSection>();
            Assert.IsNotNull(dataSection);
            Assert.AreEqual(1, dataSection.Segments.Count);
            var segment = dataSection.Segments[0];
            Assert.AreEqual(0u, segment.MemoryIndex);
            Assert.AreEqual("hello world", Encoding.UTF8.GetString(segment.Data));

            module = AssembleModule("(module (memory (import \"mod\" \"mem\") 10 40))");
            Assert.AreEqual(1, module.Sections.Count);
            var importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            var import = importSection.Imports[0];
            Assert.AreEqual(ExternalKind.Memory, import.Kind);
            Assert.AreEqual("mod", import.ModuleName);
            Assert.AreEqual("mem", import.FieldName);
            memory = ((ImportedMemory)import).Memory;
            Assert.AreEqual(10u, memory.Limits.Initial);
            Assert.IsTrue(memory.Limits.HasMaximum);
            Assert.AreEqual(40u, memory.Limits.Maximum);

            module = AssembleModule("(module (memory (export \"mem\") (import \"mod\" \"mem\") 10 40))");
            Assert.AreEqual(2, module.Sections.Count);
            importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            import = importSection.Imports[0];
            Assert.AreEqual(ExternalKind.Memory, import.Kind);
            Assert.AreEqual("mod", import.ModuleName);
            Assert.AreEqual("mem", import.FieldName);
            memory = ((ImportedMemory)import).Memory;
            Assert.AreEqual(10u, memory.Limits.Initial);
            Assert.IsTrue(memory.Limits.HasMaximum);
            Assert.AreEqual(40u, memory.Limits.Maximum);
            var exportSection = module.GetFirstSectionOrNull<ExportSection>();
            Assert.IsNotNull(exportSection);
            Assert.AreEqual(1, exportSection.Exports.Count);
            var export = exportSection.Exports[0];
            Assert.AreEqual("mem", export.Name);
            Assert.AreEqual(0u, export.Index);
            Assert.AreEqual(ExternalKind.Memory, export.Kind);
        }

        [Test]
        public void AssembleModulesWithExports()
        {
            var module = AssembleModule("(module (memory $mem1 10 40) (memory $mem2 10 40) (export \"mem\" (memory $mem2)))");
            Assert.AreEqual(2, module.Sections.Count);
            var memSection = module.GetFirstSectionOrNull<MemorySection>();
            Assert.IsNotNull(memSection);
            Assert.AreEqual(2, memSection.Memories.Count);
            var memory = memSection.Memories[1];
            Assert.AreEqual(10u, memory.Limits.Initial);
            Assert.IsTrue(memory.Limits.HasMaximum);
            Assert.AreEqual(40u, memory.Limits.Maximum);
            var exportSection = module.GetFirstSectionOrNull<ExportSection>();
            Assert.IsNotNull(exportSection);
            Assert.AreEqual(1, exportSection.Exports.Count);
            var export = exportSection.Exports[0];
            Assert.AreEqual("mem", export.Name);
            Assert.AreEqual(1u, export.Index);
            Assert.AreEqual(ExternalKind.Memory, export.Kind);
        }

        [Test]
        public void AssembleModulesWithImports()
        {
            var module = AssembleModule("(module (import \"spectest\" \"memory\" (memory 1 2)))");
            Assert.AreEqual(1, module.Sections.Count);
            var importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            var memoryImport = (ImportedMemory)importSection.Imports[0];
            Assert.AreEqual("spectest", memoryImport.ModuleName);
            Assert.AreEqual("memory", memoryImport.FieldName);
            var memory = memoryImport.Memory;
            Assert.AreEqual(1u, memory.Limits.Initial);
            Assert.IsTrue(memory.Limits.HasMaximum);
            Assert.AreEqual(2u, memory.Limits.Maximum);

            module = AssembleModule("(module (import \"spectest\" \"memory\" (func)))");
            Assert.AreEqual(2, module.Sections.Count);
            importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            var funcImport = (ImportedFunction)importSection.Imports[0];
            Assert.AreEqual("spectest", funcImport.ModuleName);
            Assert.AreEqual("memory", funcImport.FieldName);
            var funcTypeIndex = funcImport.TypeIndex;
            Assert.AreEqual(0u, funcTypeIndex);
            var typeSection = module.GetFirstSectionOrNull<TypeSection>();
            Assert.AreEqual(1, typeSection.FunctionTypes.Count);
            var funcType = typeSection.FunctionTypes[0];
            Assert.AreEqual(0, funcType.ParameterTypes.Count);
            Assert.AreEqual(0, funcType.ReturnTypes.Count);

            module = AssembleModule("(module (import \"spectest\" \"memory\" (func (param) (result))))");
            Assert.AreEqual(2, module.Sections.Count);
            importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            funcImport = (ImportedFunction)importSection.Imports[0];
            Assert.AreEqual("spectest", funcImport.ModuleName);
            Assert.AreEqual("memory", funcImport.FieldName);
            funcTypeIndex = funcImport.TypeIndex;
            Assert.AreEqual(0u, funcTypeIndex);
            typeSection = module.GetFirstSectionOrNull<TypeSection>();
            Assert.AreEqual(1, typeSection.FunctionTypes.Count);
            funcType = typeSection.FunctionTypes[0];
            Assert.AreEqual(0, funcType.ParameterTypes.Count);
            Assert.AreEqual(0, funcType.ReturnTypes.Count);

            module = AssembleModule("(module (import \"spectest\" \"memory\" (func (param i32 i64 f32 f64) (result f64))))");
            Assert.AreEqual(2, module.Sections.Count);
            importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            funcImport = (ImportedFunction)importSection.Imports[0];
            Assert.AreEqual("spectest", funcImport.ModuleName);
            Assert.AreEqual("memory", funcImport.FieldName);
            funcTypeIndex = funcImport.TypeIndex;
            Assert.AreEqual(0u, funcTypeIndex);
            typeSection = module.GetFirstSectionOrNull<TypeSection>();
            Assert.AreEqual(1, typeSection.FunctionTypes.Count);
            funcType = typeSection.FunctionTypes[0];
            Assert.AreEqual(4, funcType.ParameterTypes.Count);
            Assert.AreEqual(WasmValueType.Int32, funcType.ParameterTypes[0]);
            Assert.AreEqual(WasmValueType.Int64, funcType.ParameterTypes[1]);
            Assert.AreEqual(WasmValueType.Float32, funcType.ParameterTypes[2]);
            Assert.AreEqual(WasmValueType.Float64, funcType.ParameterTypes[3]);
            Assert.AreEqual(1, funcType.ReturnTypes.Count);
            Assert.AreEqual(WasmValueType.Float64, funcType.ReturnTypes[0]);

            module = AssembleModule("(module (import \"spectest\" \"global_i32\" (global $x i32)))");
            Assert.AreEqual(1, module.Sections.Count);
            importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            var globalImport = (ImportedGlobal)importSection.Imports[0];
            Assert.AreEqual("spectest", globalImport.ModuleName);
            Assert.AreEqual("global_i32", globalImport.FieldName);
            Assert.AreEqual(WasmValueType.Int32, globalImport.Global.ContentType);
            Assert.IsFalse(globalImport.Global.IsMutable);

            module = AssembleModule("(module (import \"spectest\" \"global_i32\" (global (mut i32))))");
            Assert.AreEqual(1, module.Sections.Count);
            importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            globalImport = (ImportedGlobal)importSection.Imports[0];
            Assert.AreEqual("spectest", globalImport.ModuleName);
            Assert.AreEqual("global_i32", globalImport.FieldName);
            Assert.AreEqual(WasmValueType.Int32, globalImport.Global.ContentType);
            Assert.IsTrue(globalImport.Global.IsMutable);

            module = AssembleModule("(module (import \"spectest\" \"table\" (table 10 20 funcref)))");
            Assert.AreEqual(1, module.Sections.Count);
            importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            var tableImport = (ImportedTable)importSection.Imports[0];
            Assert.AreEqual("spectest", tableImport.ModuleName);
            Assert.AreEqual("table", tableImport.FieldName);
            Assert.AreEqual(WasmType.AnyFunc, tableImport.Table.ElementType);
            Assert.AreEqual(10u, tableImport.Table.Limits.Initial);
            Assert.IsTrue(tableImport.Table.Limits.HasMaximum);
            Assert.AreEqual(20u, tableImport.Table.Limits.Maximum);

            module = AssembleModule("(module " +
                "(type $g (param i32) (result f64)) " +
                "(type $f (param i32 i64 f32 f64) (result f64)) " +
                "(import \"spectest\" \"f\" (func (type $f) (param i32 i64 f32 f64) (result f64))) " +
                "(import \"spectest\" \"f\" (func (type 1) (param i32 i64 f32 f64) (result f64))) " +
                "(import \"spectest\" \"f\" (func (param i32 i64 f32 f64) (result f64))) " +
                "(import \"spectest\" \"f\" (func (type 1))) " +
                "(import \"spectest\" \"f\" (func (type $f))))");
            Assert.AreEqual(2, module.Sections.Count);
            importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(5, importSection.Imports.Count);
            for (int i = 0; i < importSection.Imports.Count; i++)
            {
                funcImport = (ImportedFunction)importSection.Imports[i];
                Assert.AreEqual("spectest", funcImport.ModuleName);
                Assert.AreEqual("f", funcImport.FieldName);
                funcTypeIndex = funcImport.TypeIndex;
                Assert.AreEqual(1u, funcTypeIndex);
            }
            typeSection = module.GetFirstSectionOrNull<TypeSection>();
            Assert.AreEqual(2, typeSection.FunctionTypes.Count);
            funcType = typeSection.FunctionTypes[1];
            Assert.AreEqual(4, funcType.ParameterTypes.Count);
            Assert.AreEqual(WasmValueType.Int32, funcType.ParameterTypes[0]);
            Assert.AreEqual(WasmValueType.Int64, funcType.ParameterTypes[1]);
            Assert.AreEqual(WasmValueType.Float32, funcType.ParameterTypes[2]);
            Assert.AreEqual(WasmValueType.Float64, funcType.ParameterTypes[3]);
            Assert.AreEqual(1, funcType.ReturnTypes.Count);
            Assert.AreEqual(WasmValueType.Float64, funcType.ReturnTypes[0]);
        }

        [Test]
        public void AssembleModulesWithTables()
        {
            var module = AssembleModule("(module (table 0 funcref))");
            Assert.AreEqual(1, module.Sections.Count);
            var tableSection = module.GetFirstSectionOrNull<TableSection>();
            Assert.IsNotNull(tableSection);
            Assert.AreEqual(1, tableSection.Tables.Count);
            var table = (TableType)tableSection.Tables[0];
            Assert.AreEqual(WasmType.AnyFunc, table.ElementType);
            Assert.AreEqual(0u, table.Limits.Initial);
            Assert.IsFalse(table.Limits.HasMaximum);

            module = AssembleModule("(module (table 0 1 funcref))");
            Assert.AreEqual(1, module.Sections.Count);
            tableSection = module.GetFirstSectionOrNull<TableSection>();
            Assert.IsNotNull(tableSection);
            Assert.AreEqual(1, tableSection.Tables.Count);
            table = (TableType)tableSection.Tables[0];
            Assert.AreEqual(WasmType.AnyFunc, table.ElementType);
            Assert.AreEqual(0u, table.Limits.Initial);
            Assert.IsTrue(table.Limits.HasMaximum);
            Assert.AreEqual(1u, table.Limits.Maximum);

            module = AssembleModule("(module (table (import \"spectest\" \"table\") 10 20 funcref))");
            Assert.AreEqual(1, module.Sections.Count);
            var importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            var tableImport = (ImportedTable)importSection.Imports[0];
            Assert.AreEqual("spectest", tableImport.ModuleName);
            Assert.AreEqual("table", tableImport.FieldName);
            Assert.AreEqual(WasmType.AnyFunc, tableImport.Table.ElementType);
            Assert.AreEqual(10u, tableImport.Table.Limits.Initial);
            Assert.IsTrue(tableImport.Table.Limits.HasMaximum);
            Assert.AreEqual(20u, tableImport.Table.Limits.Maximum);

            module = AssembleModule("(module (table (export \"table1\") (export \"table2\") (import \"spectest\" \"table\") 10 20 funcref))");
            Assert.AreEqual(2, module.Sections.Count);
            importSection = module.GetFirstSectionOrNull<ImportSection>();
            Assert.IsNotNull(importSection);
            Assert.AreEqual(1, importSection.Imports.Count);
            tableImport = (ImportedTable)importSection.Imports[0];
            Assert.AreEqual("spectest", tableImport.ModuleName);
            Assert.AreEqual("table", tableImport.FieldName);
            Assert.AreEqual(WasmType.AnyFunc, tableImport.Table.ElementType);
            Assert.AreEqual(10u, tableImport.Table.Limits.Initial);
            Assert.IsTrue(tableImport.Table.Limits.HasMaximum);
            Assert.AreEqual(20u, tableImport.Table.Limits.Maximum);
            var exportSection = module.GetFirstSectionOrNull<ExportSection>();
            Assert.IsNotNull(exportSection);
            Assert.AreEqual("table1", exportSection.Exports[0].Name);
            Assert.AreEqual(ExternalKind.Table, exportSection.Exports[0].Kind);
            Assert.AreEqual(0u, exportSection.Exports[0].Index);
            Assert.AreEqual("table2", exportSection.Exports[1].Name);
            Assert.AreEqual(ExternalKind.Table, exportSection.Exports[1].Kind);
            Assert.AreEqual(0u, exportSection.Exports[1].Index);
        }

        [Test]
        public void AssembleBadMemoryModules()
        {
            AssertInvalidModule("(module (memory))");
            AssertInvalidModule("(module (memory (limits 10 50)))");
            AssertInvalidModule("(module (memory $mem 78359126329586239865823 725357639275693276582334525))");
            AssertInvalidModule("(module (memory $mem 10e7 10e8))");
            AssertInvalidModule("(module (memory +10 +40))");
            AssertInvalidModule("(module (memory $mem1 $mem2 10 40))");
            AssertInvalidModule("(module (memory 10 40 10 40))");
            AssertInvalidModule("(module (memory (import \"mod\" \"mem\")))");
        }

        [Test]
        public void AssembleBadExportModules()
        {
            AssertInvalidModule("(module (export \"mem\" (memory $mem)))");
        }

        [Test]
        public void AssembleInstructions()
        {
            Assert.AreEqual(10, EvaluateConstExpr(WasmType.Int32, "i32.const 10"));
            Assert.AreEqual(15, EvaluateConstExpr(WasmType.Int32, "i32.const 10 i32.const 5 i32.add"));
            Assert.AreEqual(5, EvaluateConstExpr(WasmType.Int32, "i32.const 10 i32.const -5 i32.add"));
            Assert.AreEqual(15, EvaluateConstExpr(WasmType.Int32, "(i32.add (i32.const 10) (i32.const 5))"));
            Assert.AreEqual(5, EvaluateConstExpr(WasmType.Int32, "(block $block (result i32) i32.const 10 i32.const -5 i32.add)"));
            Assert.AreEqual(15, EvaluateConstExpr(WasmType.Int32, "block $block (result i32) i32.const 10 i32.const -5 i32.add end i32.const 10 i32.add"));
            Assert.AreEqual(5, EvaluateConstExpr(WasmType.Int32, "(if $block (result i32) i32.const 0 (then i32.const 10) (else i32.const 5))"));
            Assert.AreEqual(10, EvaluateConstExpr(WasmType.Int32, "(if $block (result i32) i32.const 0 (then i32.const 5) (else i32.const 10))"));
            Assert.AreEqual(5, EvaluateConstExpr(WasmType.Int32, "(if $block (result i32) i32.const 1 (then i32.const 5) (else i32.const 10))"));
            Assert.AreEqual(5, EvaluateConstExpr(WasmType.Int32, "i32.const 1 (if $block (result i32) (then i32.const 5) (else i32.const 10))"));
            Assert.AreEqual(5, EvaluateConstExpr(WasmType.Int32, "i32.const 1 (if (then)) i32.const 5"));
            Assert.AreEqual(5, EvaluateConstExpr(WasmType.Int32, "i32.const 1 if $block (result i32) i32.const 5 else i32.const 10 end"));
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

        private static object EvaluateConstExpr(WasmType resultType, string expr)
        {
            var asm = AssembleModule($"(module (func $f (result {DumpHelpers.WasmTypeToString(resultType)}) {expr}) (export \"f\" (func $f)))");
            var instance = ModuleInstance.Instantiate(asm, new PredefinedImporter());
            return instance.ExportedFunctions["f"].Invoke(Array.Empty<object>())[0];
        }
    }
}
