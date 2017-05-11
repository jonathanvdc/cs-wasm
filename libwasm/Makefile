files = BadHeaderException.cs \
		CodeSection.cs \
		CustomSection.cs \
		DumpHelpers.cs \
		ExportSection.cs \
		ExternalKind.cs \
		FunctionSection.cs \
		MemorySection.cs \
		ResizableLimits.cs \
		Section.cs \
		UnknownSection.cs \
		VersionHeader.cs \
		SectionCode.cs \
		SectionName.cs \
		StartSection.cs \
		TableSection.cs \
		TypeSection.cs \
		WasmException.cs \
		WasmFile.cs \
		WasmType.cs \
		Binary/BinaryWasmReader.cs \
		Binary/BinaryWasmWriter.cs \
		Binary/SectionHeader.cs \
		Instructions/BlockInstruction.cs \
		Instructions/BlockOperator.cs \
		Instructions/BrTableInstruction.cs \
		Instructions/BrTableOperator.cs \
		Instructions/CallIndirectInstruction.cs \
		Instructions/CallIndirectOperator.cs \
		Instructions/Float32Instruction.cs \
		Instructions/Float32Operator.cs \
		Instructions/Float64Instruction.cs \
		Instructions/Float64Operator.cs \
		Instructions/IfElseInstruction.cs \
		Instructions/IfElseOperator.cs \
		Instructions/Instruction.cs \
		Instructions/MemoryInstruction.cs \
		Instructions/MemoryOperator.cs \
		Instructions/NullaryInstruction.cs \
		Instructions/NullaryOperator.cs \
		Instructions/Operator.cs \
		Instructions/Operators.cs \
		Instructions/VarInt32Instruction.cs \
		Instructions/VarInt32Operator.cs \
		Instructions/VarInt64Instruction.cs \
		Instructions/VarInt64Operator.cs \
		Instructions/VarUInt32Instruction.cs \
		Instructions/VarUInt32Operator.cs

common_options = -asm-name=libwasm -asm-version=0.1.0.0 -Wall -Wextra -force-write

all: dll flo

dll: bin/clr/libwasm.dll
flo: bin/flo/libwasm.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/libwasm.dll: $(files)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options)

bin/flo/libwasm.flo: $(files)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options)
