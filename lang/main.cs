using static lang.assembler.X64Assembler;
using lang.assembler;
using lang.linker;
using lang.program;
using lang.utils;
using System;

namespace lang
{
    class main
    {
        static void Main(string[] args)
        {
            var p = new Program();
            var mainFn = new CodeBlock();
            p.codeBlocks.Add(mainFn);

            var testFn = new CodeBlock();
            p.codeBlocks.Add(testFn);

            mainFn.assembler.Add(
                new Push    (RBP),
                new Mov     (RBP, RSP),
                new Sub     (RSP, (byte)0x20),

                new Xor     (RCX, RCX),
                new Mov     (RDX, p.data.String("CAPTION")),
                new Mov     (R8,  p.data.String("This is a text")),
                new Xor     (R9,  R9),
                new Call    (p.import.Import("MessageBoxA", "user32.DLL")),

                new Call    (testFn.addr),

                new Xor     (RCX, RCX),
                new Call    (p.import.Import("ExitProcess", "kernel32.dll")),

                new Add     (RSP, (byte)0x20),
                new Pop     (RBP),
                new Retn    ()
            );

            testFn.assembler.Add(
                new Push(RBP),
                new Mov(RBP, RSP),
                new Sub(RSP, 0x20),

                new Mov(RCX, p.data.String("Hello")),
                new Call(p.import.Import("printf", "msvcrt.dll")),

                new Add(RSP, 0x20),
                new Pop(RBP),
                new Retn()
            );

            Console.WriteLine("main:");
            Console.WriteLine(mainFn.assembler);
            Console.WriteLine("test:");
            Console.WriteLine(testFn.assembler);
            Console.Read();

            Linker.Link(p, Linker.Platform.Windows, "../../output/test.exe");
        }
    }
}
