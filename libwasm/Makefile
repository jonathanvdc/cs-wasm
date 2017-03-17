files = Binary/BadHeaderException.cs \
		Binary/BinaryWasmReader.cs \
		Binary/ByteString.cs \
		Binary/SectionCode.cs \
		Binary/SectionHeader.cs \
		Binary/VersionHeader.cs

warning_options = -Wall -Wextra -Wno-up-to-date -Wno-unused-option

all: dll flo

dll:
	ecsc $(files) -platform clr -o bin/clr/libwasm.dll -O3 $(warning_options)

flo:
	ecsc $(files) -platform ir -runtime clr -o bin/flo/libwasm.flo $(warning_options)
