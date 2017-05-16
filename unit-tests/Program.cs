using System;
using System.Collections.Generic;
using System.Diagnostics;
using Loyc;
using Loyc.MiniTest;
using Loyc.Syntax;
using Wasm.Interpret;

namespace Wasm.UnitTests
{
    // Test driver based on Loyc project: https://github.com/qwertie/ecsharp/blob/master/Core/Tests/Program.cs

    public static class Program
    {
        public static readonly List<Pair<string, Func<int>>> Menu = new List<Pair<string, Func<int>>>()
        {
            new Pair<string, Func<int>>("Run libwasm-interpret unit tests", LibwasmInterpret),
        };

        public static void Main(string[] args)
        {
            // Workaround for MS bug: Assert(false) will not fire in debugger
            Debug.Listeners.Clear();
            Debug.Listeners.Add(new DefaultTraceListener());
            if (RunMenu(Menu, args.Length > 0 ? args[0] : null) > 0)
                // Let the outside world know that something went wrong (e.g. Travis CI)
                Environment.ExitCode = 1;
        }

        private static IEnumerable<char> ConsoleChars()
        {
            for (ConsoleKeyInfo k; (k = Console.ReadKey(true)).Key != ConsoleKey.Escape
                && k.Key != ConsoleKey.Enter;)
                yield return k.KeyChar;
        }

        public static int RunMenu(IList<Pair<string, Func<int>>> menu, IEnumerable<char> input)
        {
            var reader = (input ?? ConsoleChars()).GetEnumerator();
            int errorCount = 0;
            for (;;)
            {
                Console.WriteLine();
                Console.WriteLine("What do you want to do? (Esc to quit)");
                for (int i = 0; i < menu.Count; i++)
                    Console.WriteLine(PrintHelpers.HexDigitChar(i + 1) + ". " + menu[i].Key);
                Console.WriteLine("Space. Run all tests");

                if (!reader.MoveNext())
                    break;

                char c = reader.Current;
                if (c == ' ')
                {
                    for (int i = 0; i < menu.Count; i++)
                    {
                        Console.WriteLine();
                        ConsoleMessageSink.WriteColoredMessage(ConsoleColor.White, i + 1, menu[i].Key);
                        errorCount += menu[i].Value();
                    }
                }
                else
                {
                    int i = ParseHelpers.HexDigitValue(c);
                    if (i > 0 && i <= menu.Count)
                        errorCount += menu[i - 1].Value();
                }
            }
            return errorCount;
        }

        public static int LibwasmInterpret()
        {
            return RunTests.RunMany(
                new LinearMemoryTests());
        }
    }
}
