files = Program.cs \
		../../libwasm/bin/flo/libwasm.flo

common_options = -asm-name=readme-example -asm-version=0.1.5.0 -Wall -Wextra

.PHONY: all exe flo clean
all: exe flo

exe: bin/clr/readme-example.exe
flo: bin/flo/readme-example.flo

clean:
	rm -rf bin/
	rm -rf obj/

include ../../flame-make-scripts/use-ecsc.mk

bin/clr/readme-example.exe: $(files) | ecsc
	$(ECSC) $(files) -platform clr -o $@ -O3 $(common_options)

bin/flo/readme-example.flo: $(files) | ecsc
	$(ECSC) $(files) -platform ir -runtime clr -o $@ $(common_options)
