using static lang.assembler.X64Assembler;
using lang.assembler;
using lang.linker;

namespace lang
{
    class main
    {
        static void Main(string[] args)
        {
            var a = new X64Assembler();
            a.Add(
                new Push    (RBP),
                new Mov     (RBP, RSP),
                new Mov     (RCX, 0),
                new Mov     (RDX, a.data.String("CAPTION")),
                new Mov     (R8,  a.data.String("This is a text")),
                new Mov     (R9,  0),
                new Call    (a.import.Import("MessageBoxA", "user32.DLL")),

                new Mov     (RCX, a.data.String("Hello, %s")),
                new Mov     (RDX, a.data.String("PE")),
                new Call    (a.import.Import("printf", "msvcrt.dll")),

                new Mov     (RCX, 0),
                new Call    (a.import.Import("ExitProcess", "kernel32.dll")),
                new Leave   ()
            );

            Linker.Link(a, Linker.Platform.Windows, "../../output/test.exe");
        }
    }
}
