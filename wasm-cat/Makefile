files = Program.cs \
		../libwasm/bin/flo/libwasm.flo

common_options = -asm-name=wasm-cat -asm-version=0.1.5.0 -Wall -Wextra

all: exe flo

exe: bin/clr/wasm-cat.exe
flo: bin/flo/wasm-cat.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/wasm-cat.exe: $(files)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options)

bin/flo/wasm-cat.flo: $(files)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options)
