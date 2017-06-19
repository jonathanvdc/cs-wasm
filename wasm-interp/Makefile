files = Program.cs \
		../libwasm/bin/flo/libwasm.flo

common_options = -asm-name=wasm-interp -asm-version=0.1.4.0 -Wall -Wextra

all: exe flo

exe: bin/clr/wasm-interp.exe
flo: bin/flo/wasm-interp.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/wasm-interp.exe: $(files)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options)

bin/flo/wasm-interp.flo: $(files)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options)
