files = LinearMemory.cs
libwasm-dll = ../libwasm/bin/clr/libwasm.dll
libwasm-flo = ../libwasm/bin/flo/libwasm.flo

common_options = -asm-name=libwasm-interpret -asm-version=0.1.0.0 -Wall -Wextra -force-write

all: dll flo

dll: bin/clr/libwasm-interpret.dll
flo: bin/flo/libwasm-interpret.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/libwasm-interpret.dll: $(files) $(libwasm-dll)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options) --docs --libs $(libwasm-dll)

bin/flo/libwasm-interpret.flo: $(files) $(libwasm-flo)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options) --libs $(libwasm-flo)
