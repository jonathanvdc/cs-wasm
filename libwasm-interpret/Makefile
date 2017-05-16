files = LinearMemory.cs
libraries = ../libwasm-core/bin/flo/libwasm-core.flo

common_options = -asm-name=libwasm-interpret -asm-version=0.1.0.0 -Wall -Wextra

all: flo

flo: bin/flo/libwasm-interpret.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/flo/libwasm-interpret.flo: $(files) $(libraries)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options) --libs $(libraries)
