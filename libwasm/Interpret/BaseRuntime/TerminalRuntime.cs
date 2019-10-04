using System.Collections.Generic;
using System.IO;

namespace Wasm.Interpret.BaseRuntime
{
    /// <summary>
    /// Defines terminal I/O operations for the base-runtime environment.
    /// </summary>
    public sealed class TerminalRuntime
    {
        /// <summary>
        /// Creates a base-runtime IO implementation from the given streams.
        /// </summary>
        /// <param name="inputStream">The runtime's standard input stream.</param>
        /// <param name="outputStream">The runtime's standard output stream.</param>
        /// <param name="errorStream">The runtime's standard error stream.</param>
        private TerminalRuntime(Stream inputStream, Stream outputStream, Stream errorStream)
        {
            this.stdinStream = inputStream;
            this.stdoutStream = outputStream;
            this.stderrStream = errorStream;
            this.importerVal = new PredefinedImporter();

            this.importerVal.DefineFunction(
                "stdin_read",
                new DelegateFunctionDefinition(
                    new WasmValueType[0],
                    new WasmValueType[] { WasmValueType.Int32 },
                    StdinReadByte));
            this.importerVal.DefineFunction(
                "stdout_write",
                new DelegateFunctionDefinition(
                    new WasmValueType[] { WasmValueType.Int32 },
                    new WasmValueType[0],
                    StdoutWriteByte));
            this.importerVal.DefineFunction(
                "stderr_write",
                new DelegateFunctionDefinition(
                    new WasmValueType[] { WasmValueType.Int32 },
                    new WasmValueType[0],
                    StderrWriteByte));

            this.importerVal.DefineFunction(
                "stdin_flush",
                new DelegateFunctionDefinition(
                    new WasmValueType[0],
                    new WasmValueType[0],
                    StdinFlush));
            this.importerVal.DefineFunction(
                "stdout_flush",
                new DelegateFunctionDefinition(
                    new WasmValueType[0],
                    new WasmValueType[0],
                    StdoutFlush));
            this.importerVal.DefineFunction(
                "stderr_flush",
                new DelegateFunctionDefinition(
                    new WasmValueType[0],
                    new WasmValueType[0],
                    StderrFlush));
        }

        private Stream stdinStream;
        private Stream stdoutStream;
        private Stream stderrStream;

        private readonly PredefinedImporter importerVal;

        /// <summary>
        /// Adds all definitions from this runtime to the given importer.
        /// </summary>
        /// <param name="Importer">The importer.</param>
        private void IncludeDefinitionsIn(PredefinedImporter Importer)
        {
            Importer.IncludeDefinitions(importerVal);
        }

        /// <summary>
        /// Creates a new terminal I/O runtime and adds all of its definitions to the given
        /// importer.
        /// </summary>
        /// <param name="inputStream">The runtime's standard input stream.</param>
        /// <param name="outputStream">The runtime's standard output stream.</param>
        /// <param name="errorStream">The runtime's standard error stream.</param>
        /// <param name="importer">The importer.</param>
        public static void IncludeDefinitionsIn(
            Stream inputStream,
            Stream outputStream,
            Stream errorStream,
            PredefinedImporter importer)
        {
            new TerminalRuntime(inputStream, outputStream, errorStream).IncludeDefinitionsIn(importer);
        }

        private IReadOnlyList<object> StdinReadByte(IReadOnlyList<object> args)
        {
            return new object[] { stdinStream.ReadByte() };
        }

        private IReadOnlyList<object> StdoutWriteByte(IReadOnlyList<object> args)
        {
            object data = args[0];
            stdoutStream.WriteByte((byte)(int)data);
            return new object[0];
        }

        private IReadOnlyList<object> StderrWriteByte(IReadOnlyList<object> args)
        {
            object data = args[0];
            stderrStream.WriteByte((byte)(int)data);
            return new object[0];
        }

        private IReadOnlyList<object> StdinFlush(IReadOnlyList<object> args)
        {
            stdinStream.Flush();
            return new object[0];
        }

        private IReadOnlyList<object> StdoutFlush(IReadOnlyList<object> args)
        {
            stdinStream.Flush();
            return new object[0];
        }

        private IReadOnlyList<object> StderrFlush(IReadOnlyList<object> args)
        {
            stdinStream.Flush();
            return new object[0];
        }
    }
}