files = \
	../libwasm-core/bin/flo/libwasm-core.flo \
	../libwasm-interpret/bin/flo/libwasm-interpret.flo \
	../libwasm-optimize/bin/flo/libwasm-optimize.flo

common_options = -asm-name=libwasm -asm-version=0.1.5.0

.PHONY: all dll flo clean
all: dll flo

dll: bin/clr/libwasm.dll
flo: bin/flo/libwasm.flo

clean:
	rm -rf bin/
	rm -rf obj/

include ../flame-make-scripts/use-ecsc.mk

bin/clr/libwasm.dll: $(files) | ecsc
	$(ECSC) $(files) -platform clr -o $@ -O3 $(common_options) --docs

bin/flo/libwasm.flo: $(files) | ecsc
	$(ECSC) $(files) -platform ir -runtime clr -o $@ $(common_options)
