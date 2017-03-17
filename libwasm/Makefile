files = Binary/BadHeaderException.cs \
		Binary/BinaryWasmReader.cs \
		Binary/VersionHeader.cs

all: dll flo

dll:
	ecsc $(files) -platform clr -o bin/clr/libwasm.dll -O3 -Wall -Wextra

flo:
	ecsc $(files) -platform ir -runtime clr -o bin/flo/libwasm.flo -Wall -Wextra -Wno-up-to-date
