files = Program.cs \
		../libwasm/bin/flo/libwasm.flo

common_options = -asm-name=wasm-opt -asm-version=0.1.4.0 -Wall -Wextra

all: exe flo

exe: bin/clr/wasm-opt.exe
flo: bin/flo/wasm-opt.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/wasm-opt.exe: $(files)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options)

bin/flo/wasm-opt.flo: $(files)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options)
