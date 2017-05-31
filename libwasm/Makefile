files = \
	../libwasm-core/bin/flo/libwasm-core.flo \
	../libwasm-interpret/bin/flo/libwasm-interpret.flo

common_options = -asm-name=libwasm -asm-version=0.1.2.0

all: dll flo

dll: bin/clr/libwasm.dll
flo: bin/flo/libwasm.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/libwasm.dll: $(files)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options) --docs

bin/flo/libwasm.flo: $(files)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options)
