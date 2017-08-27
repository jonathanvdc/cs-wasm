files = \
	Peephole/PeepholeOptimization.cs \
	Peephole/TeeLocalOptimization.cs \
	Peephole/UnreachableCodeOptimization.cs \
	FunctionBodyOptimizations.cs \
	FunctionTypeOptimizations.cs \
	WasmFileOptimizations.cs

libraries = ../libwasm-core/bin/flo/libwasm-core.flo

common_options = -asm-name=libwasm-optimize -asm-version=0.1.5.0 -Wall -Wextra

.PHONY: all flo clean
all: flo

flo: bin/flo/libwasm-optimize.flo

clean:
	rm -rf bin/
	rm -rf obj/

include ../flame-make-scripts/use-ecsc.mk

bin/flo/libwasm-optimize.flo: $(files) $(libraries) | ecsc
	$(ECSC) $(files) -platform ir -runtime clr -o $@ $(common_options) --libs $(libraries)
