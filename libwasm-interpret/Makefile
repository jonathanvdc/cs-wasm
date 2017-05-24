files = \
	DefaultInstructionInterpreter.cs \
	DelegateFunctionDefinition.cs \
	FunctionDefinition.cs \
	FunctionTable.cs \
	IImporter.cs \
	InstructionInterpreter.cs \
	InterpreterContext.cs \
	LinearMemory.cs \
	ModuleInstance.cs \
	OperatorImpls.cs \
	SpecTestImporter.cs \
	ThrowFunctionDefinition.cs \
	ValueHelpers.cs \
	Variable.cs \
	WasmFunctionDefinition.cs

libraries = ../libwasm-core/bin/flo/libwasm-core.flo

common_options = -asm-name=libwasm-interpret -asm-version=0.1.0.0 -Wall -Wextra

all: flo

flo: bin/flo/libwasm-interpret.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/flo/libwasm-interpret.flo: $(files) $(libraries)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options) --libs $(libraries)
