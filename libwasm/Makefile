files = CustomSection.cs \
		DumpHelpers.cs \
		ExportSection.cs \
		ExternalKind.cs \
		FunctionSection.cs \
		MemorySection.cs \
		ResizableLimits.cs \
		Section.cs \
		UnknownSection.cs \
		SectionCode.cs \
		SectionName.cs \
		TypeSection.cs \
		WasmException.cs \
		WasmFile.cs \
		WasmType.cs \
		Binary/BadHeaderException.cs \
		Binary/BinaryWasmReader.cs \
		Binary/BinaryWasmWriter.cs \
		Binary/SectionHeader.cs \
		Binary/VersionHeader.cs

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
