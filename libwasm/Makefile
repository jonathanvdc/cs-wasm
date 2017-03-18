files = CustomSection.cs \
		Section.cs \
		UnknownSection.cs \
		WasmFile.cs \
		Binary/BadHeaderException.cs \
		Binary/BinaryWasmReader.cs \
		Binary/ByteString.cs \
		Binary/SectionCode.cs \
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
