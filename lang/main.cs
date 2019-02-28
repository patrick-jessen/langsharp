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
                new Call(p.Import("MessageBoxA", "user32.dll")),

                new Call    (testFn.addr),

                new Xor     (RCX, RCX),
                new Call(p.Import("ExitProcess", "kernel32.dll")),

                new Add     (RSP, (byte)0x20),
                new Pop     (RBP),
                new Retn    ()
            );

            var labelG = new Label("greater");
            var labelL = new Label("less");
            var labelAfter = new Label("after");

            var labelStart = new Label("start");
            var labelExit = new Label("exit");


            testFn.assembler.Add(
                new Push(RBP),
                new Mov(RBP, RSP),
                new Sub(RSP, 0x20 + 4),

                labelStart,

                // Print message
                new Mov(RCX, p.String("Enter a number between 0 and 100\n")),
                new Call(p.Import("printf", "msvcrt.dll")),

                // Read input
                new Call(p.Import("__iob_func", "msvcrt.dll")), // get stdin 
                new Mov(RCX, RBP),
                new Add(RCX, 4),
                new Mov(RDX, 3),
                new Mov(R8, RAX),
                new Call(p.Import("fgets", "msvcrt.dll")),

                // Parse int
                new Mov(RCX, RBP),
                new Add(RCX, 4),
                new Call(p.Import("atoi", "msvcrt.dll")),

                new Cmp(RAX, 42),
                new Jl(labelL),
                new Jg(labelG),

                new Mov(RCX, p.String("You guessed it!")),
                new Call(p.Import("printf", "msvcrt.dll")),
                new Jmp(labelExit),
                labelG,
                new Mov(RCX, p.String("Too high\n")),
                new Jmp(labelAfter),
                labelL,
                new Mov(RCX, p.String("Too low\n")),
                labelAfter,
                new Call(p.Import("printf", "msvcrt.dll")),
                new Jmp(labelStart),

                labelExit,

                new Add(RSP, 0x20 + 4),
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
