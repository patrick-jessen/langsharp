using pe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x64;

namespace lang
{
    class main
    {
        static void Main(string[] args)
        {
            var dataRef1 = new AddressReference();
            var dataRef2 = new AddressReference();
            var fnRef1 = new AddressReference();
            var fnRef2 = new AddressReference();

            var a = new X64();
            a += new Push(X64.RBP);
            a += new Mov(X64.RBP, X64.RSP);
            a += new Mov(X64.RCX, 0);
            a += new Mov(X64.RDX, dataRef1);
            a += new Mov(X64.R8, dataRef2);
            a += new Mov(X64.R9, 0);
            a += new Call(fnRef1);
            a += new Mov(X64.RCX, 0);
            a += new Call(fnRef2);
            a += new Leave();

            Console.WriteLine(a.Size);
            Console.ReadKey();

            dataRef1.address = 0x402005;
            dataRef2.address = 0x402000;
            fnRef1.address = 0x404088;
            fnRef2.address = 0x404078;


            Console.WriteLine(a.ToString());
            //Console.ReadKey();

            ByteStream bs = new ByteStream();
            a.Write(bs);

            byte[] code = bs.GetBytes();

            Importer i = new Importer();
            //i.Import("printf", "MSVCRT.dll");
            i.Import("ExitProcess", "kernel32.dll");
            i.Import("MessageBoxA", "user32.DLL");



            byte[] data = new byte[] {
                (byte)'t',(byte)'e',(byte)'x',(byte)'t',0x00,
                (byte)'c',(byte)'a',(byte)'p',0x00,
            };
            byte[] rdata = new byte[1];


            PE pe = new PE(code, data, rdata, i);

            pe.WriteFile("../../output/test.exe");
        }
    }
}
