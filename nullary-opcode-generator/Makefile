files = Program.cs \
		../libwasm/bin/flo/libwasm.flo

common_options = -asm-name=nullary-opcode-generator -asm-version=0.1.2.0 -Wall -Wextra

all: exe flo

exe: bin/clr/nullary-opcode-generator.exe
flo: bin/flo/nullary-opcode-generator.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/nullary-opcode-generator.exe: $(files)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options)

bin/flo/nullary-opcode-generator.flo: $(files)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options)
