using pe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x64
{
    class X64 {
        public List<Instruction> ins = new List<Instruction>();
        public static Reg32 RAX = new Reg32(0);
        public static Reg32 RCX = new Reg32(1);
        public static Reg32 RDX = new Reg32(2);
        public static Reg32 RBX = new Reg32(3);
        public static Reg32 RSP = new Reg32(4);
        public static Reg32 RBP = new Reg32(5);
        public static Reg32 RSI = new Reg32(6);
        public static Reg32 RDI = new Reg32(7);
        public static Reg64 R8 = new Reg64(0);
        public static Reg64 R9 = new Reg64(1);
        public static Reg64 R10 = new Reg64(2);
        public static Reg64 R11 = new Reg64(3);
        public static Reg64 R12 = new Reg64(4);
        public static Reg64 R13 = new Reg64(5);
        public static Reg64 R14 = new Reg64(6);
        public static Reg64 R15 = new Reg64(7);

        public int Size {
            get {
                int s = 0;
                foreach(Instruction i in ins) {
                    s += i.size;
                }
                return s;
            }
        }

        public override String ToString() {
            String o = "";
            foreach (Instruction i in ins) {
                o += i.ToString() + "\r\n";
            }
            return o;
        }

        public void Write(ByteStream s) {
            foreach(Instruction i in ins) {
                i.Write(s);
            }
        }

        public static X64 operator +(X64 left, Instruction right) {
            left.ins.Add(right);
            return left;
        }
    }

    public class AddressReference {
        public int address = -1;
    }
}
