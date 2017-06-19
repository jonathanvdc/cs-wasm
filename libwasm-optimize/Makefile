files = \
	Peephole/PeepholeOptimization.cs \
	Peephole/TeeLocalOptimization.cs \
	FunctionBodyOptimizations.cs \
	FunctionTypeOptimizations.cs \
	WasmFileOptimizations.cs

libraries = ../libwasm-core/bin/flo/libwasm-core.flo

common_options = -asm-name=libwasm-optimize -asm-version=0.1.3.0 -Wall -Wextra

all: flo

flo: bin/flo/libwasm-optimize.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/flo/libwasm-optimize.flo: $(files) $(libraries)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options) --libs $(libraries)
