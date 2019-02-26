using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X64Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var i = new Instruction();
            i.Test();
            
        }
    }

    class Instruction {
        // https://wiki.osdev.org/X86-64_Instruction_Encoding
        // VEX/XOP opcodes omitted
        // Instruction encoding
        // |=REX=======|=Opcode====|=ModR/M====|=SIB=======|=Displacement=====|=Immediate========|
        // | 0-1 bytes | 1-3 bytes | 0-1 bytes | 0-1 bytes | 0,1,2 or 4 bytes | 0,1,2 or 4 bytes |

        // REX is an optional prefix to opcodes (1 byte)
        private byte[] REX(bool W = true, bool R = false, bool X = false, bool B = false) {
            // W = 64bit size
            // R = ModRM.reg extension
            // X = SIB.index extension
            // B = ModRM.rm extension

            byte rex = 0x40; // fixed bit pattern
            if (W) rex |= 0x1 << 3;
            if (R) rex |= 0x1 << 2;
            if (X) rex |= 0x1 << 1;
            if (B) rex |= 0x1;
            return new byte[] { rex };
        }
        // 1 byte opcode
        private byte[] Opcode(byte po) {
            return new byte[] { po };
        }
        // 2 byte opcode
        private byte[] Opcode2(byte po) {
            return new byte[] { 0x0F, po };
        }
        // 3 byte opcode
        private byte[] OpCode3(byte po, byte so) {
            return new byte[] { 0x0F, po, so };
        }
        // ModRM encodes operands (1 byte)
        private byte[] ModRM(byte mod, byte reg, byte rm) {
            // mod (note: combines with rm):
            // 00 = address
            // 01 = 1byte displacement follows
            // 10 = 4byte displacement follows
            // 11 = register

            // reg:
            // if 1 operand: opcode extension
            // if 2 operands: register no. of operand #1 (can be combined with REX.R)

            // rm: (can be combined with REX.B) (note: combines with mod)
            // if 1 operand: register no. of operand #1
            // if 2 operands: register no. of operand #2

            // Special combinations:
            // mod=00|01|10 && rm==100:
            //   SIB follows
            // mod=00 && rm==101:
            //   Relative to instruction ptr. 32bit offset follows
            return new byte[] { (byte)((mod & 3) << 6 | (reg & 7) << 3 | rm & 7) };
        }
        // SIB describes an address offset
        private byte[] SIB(byte scale, byte index, byte _base) {
            // [base+index*scale]

            // scale:
            // 00 = x1
            // 01 = x2
            // 10 = x4
            // 11 = x8

            // index (can be combined with REX.X):
            // register no. of index for calculation

            // base (can be combined with REX.B):
            // register no. of base for 
            return new byte[] { (byte)((scale & 3) << 6 | (index & 7) << 3 | _base & 7) };
        }

        public void Test() {

            Write(
                Opcode(0xFF),
                ModRM(3, 6, 0)
            );



            Print();
            Console.Read();
        }

        MemoryStream ms = new MemoryStream();
        void Write(params byte[][] data) {
            foreach (byte[] d in data)
                ms.Write(d, 0, d.Length);
        }
        void Print() {
            var bytes = ms.ToArray();
            foreach (byte b in bytes)
                Console.Write("{0:X2} ", b);
        }
    }
}
