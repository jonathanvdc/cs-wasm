exe:
	make -C libwasm-core flo
	make -C libwasm-interpret flo
	make -C libwasm-optimize flo
	make -C libwasm flo
	make -C nullary-opcode-generator exe
	make -C wasm-cat exe
	make -C wasm-dump exe
	make -C wasm-interp exe
	make -C wasm-opt exe
	make -C unit-tests exe

all:
	make -C libwasm-core all
	make -C libwasm-interpret all
	make -C libwasm-optimize all
	make -C libwasm all
	make -C nullary-opcode-generator all
	make -C wasm-cat all
	make -C wasm-dump all
	make -C wasm-interp all
	make -C wasm-opt all
	make -C unit-tests all

dll:
	make -C libwasm-core flo
	make -C libwasm-interpret flo
	make -C libwasm-optimize flo
	make -C libwasm all
	make -C nullary-opcode-generator exe
	make -C wasm-cat exe
	make -C wasm-dump exe
	make -C wasm-interp exe
	make -C wasm-opt exe
	make -C unit-tests exe

flo:
	make -C libwasm-core flo
	make -C libwasm-interpret flo
	make -C libwasm-optimize flo
	make -C libwasm flo
	make -C nullary-opcode-generator flo
	make -C wasm-cat flo
	make -C wasm-dump flo
	make -C wasm-interp flo
	make -C wasm-opt flo
	make -C unit-tests flo

clean:
	make -C libwasm-core clean
	make -C libwasm-interpret clean
	make -C libwasm-optimize clean
	make -C libwasm clean
	make -C nullary-opcode-generator clean
	make -C wasm-cat clean
	make -C wasm-dump clean
	make -C wasm-interp clean
	make -C wasm-opt clean
	make -C unit-tests clean
	make -C examples clean

example-projects:
	make -C examples

test: exe example-projects
	compare-test run-tests.test