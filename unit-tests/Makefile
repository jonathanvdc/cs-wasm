files = \
	Program.cs \
	Interpret/DefaultInterpreterTests.cs \
	Interpret/LinearMemoryTests.cs \
	Optimize/FunctionBodyOptimizationTests.cs \
	../libwasm/bin/flo/libwasm.flo
libraries = \
	../packages/Loyc.Essentials.24.3.0/lib/net45/Loyc.Essentials.dll \
	../packages/Loyc.Collections.24.3.0/lib/net45/Loyc.Collections.dll \
	../packages/Loyc.Syntax.24.3.0/lib/net45/Loyc.Syntax.dll

common_options = -asm-name=unit-tests -asm-version=0.1.5.0 -Wall -Wextra -fno-whole-program

.PHONY: all exe flo clean
all: exe flo

exe: bin/clr/unit-tests.exe
flo: bin/flo/unit-tests.flo

clean:
	rm -rf bin/
	rm -rf obj/

include ../flame-make-scripts/use-ecsc.mk

bin/clr/unit-tests.exe: $(files) $(libraries) | ecsc
	$(ECSC) $(files) -platform clr -o $@ -O2 $(common_options) --libs $(libraries)

bin/flo/unit-tests.flo: $(files) $(libraries) | ecsc
	$(ECSC) $(files) -platform ir -runtime clr -o $@ $(common_options) --libs $(libraries)
