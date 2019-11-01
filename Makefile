.PHONY: all debug release clean test

RUN_EXE ?= mono

all: release

release:
	msbuild /p:Configuration=Release /verbosity:quiet /nologo cs-wasm.sln

debug:
	msbuild /p:Configuration=Debug /verbosity:quiet /nologo cs-wasm.sln

include flame-make-scripts/use-compare-test.mk
include flame-make-scripts/use-ecsc.mk

test: debug | compare-test
	$(RUN_EXE) ./unit-tests/bin/Debug/net47/unit-tests.exe 123
	$(COMPARE_TEST) run-tests.test
