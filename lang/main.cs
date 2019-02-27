using static lang.assembler.X64Assembler;
using lang.assembler;
using lang.linker;
using lang.program;
using lang.utils;
using System;
using System.IO;

namespace lang
{
    class main
    {
        const string exeFile = "../../output/test.exe";

        static void Main(string[] args)
        {
            var p = new Program();
            var mainFn = p.AddBlock("main");
            var testFn = p.AddBlock("testFn");

            mainFn.assembler.Add(
                new Push    (RBP),
                new Mov     (RBP, RSP),
                new Sub     (RSP, (byte)0x20),

                new Xor     (RCX, RCX),
                new Mov     (RDX, p.String("CAPTION")),
                new Mov     (R8,  p.String("This is a text")),
                new Xor     (R9,  R9),
                new Call    (p.Import("MessageBoxA", "user32.dll")),

                new Call    (testFn.addr),

                new Xor     (RCX, RCX),
                new Call    (p.Import("ExitProcess", "kernel32.dll")),

                new Add     (RSP, (byte)0x20),
                new Pop     (RBP),
                new Retn    ()
            );

            testFn.assembler.Add(
                new Push(RBP),
                new Mov(RBP, RSP),
                new Sub(RSP, 0x20),

                new Mov(RCX, p.String("Hello")),
                new Call(p.Import("printf", "msvcrt.dll")),

                new Add(RSP, 0x20),
                new Pop(RBP),
                new Retn()
            );

            Console.WriteLine(p);
            Console.WriteLine("Press Enter to run");
            Console.Read();

            Linker.Link(p, Linker.Platform.Windows, exeFile);
            System.Diagnostics.Process.Start(Path.GetFullPath(exeFile));
        }
    }
}
