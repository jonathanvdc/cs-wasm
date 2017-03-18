files = Binary/BadHeaderException.cs \
		Binary/BinaryWasmReader.cs \
		Binary/ByteString.cs \
		Binary/SectionCode.cs \
		Binary/SectionHeader.cs \
		Binary/VersionHeader.cs

common_options = -Wall -Wextra -force-write

all: dll flo

dll: bin/clr/libwasm.dll
flo: bin/flo/libwasm.flo

bin/clr/libwasm.dll: $(files)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options)

bin/flo/libwasm.flo: $(files)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options)
