files = Program.cs \
		../../libwasm/bin/flo/libwasm.flo

common_options = -asm-name=readme-example -asm-version=0.1.2.0 -Wall -Wextra

all: exe flo

exe: bin/clr/readme-example.exe
flo: bin/flo/readme-example.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/readme-example.exe: $(files)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options)

bin/flo/readme-example.flo: $(files)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options)
