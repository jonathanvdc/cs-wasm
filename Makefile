exe:
	make -C libwasm flo
	make -C wasm-dump exe

all:
	make -C libwasm all
	make -C wasm-dump all

dll:
	make -C libwasm dll
	make -C wasm-dump exe

flo:
	make -C libwasm flo
	make -C wasm-dump flo

clean:
	make -C libwasm clean
	make -C wasm-dump clean

test: exe
	mono wasm-dump/bin/clr/wasm-dump.exe test/factorial.wasm