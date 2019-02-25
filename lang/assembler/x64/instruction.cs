using System;
using System.IO;

namespace assembler
{
    class ByteStream {
        public MemoryStream stream = new MemoryStream();

        public void Write(params byte[] bytes) {
            stream.Write(bytes, 0, bytes.Length);
        }
        public byte[] GetBytes() {
            return stream.ToArray();
        }
    }

    abstract class Instruction {
        protected Instruction(int size) {
            this.size = size;
        }

        public int size;
        public abstract void Write(ByteStream stream);


        protected static string Format(byte[] bytes, String i, String o1 = "", String o2 = "")
        {
            String s = "";
            foreach (byte b in bytes) {
                s += String.Format("0x{0:X2} ", b);
            }
            return String.Format("{0,-40}{1,-5}{2,-5}{3}", s, i, o1, o2);
        }
        protected static string Hex(int val)
        {
            return String.Format("0x{0:X2}", val);
        }
        protected static string Addr(AddressReference a)
        {
            return String.Format("<0x{0:X}>", a.address);
        }
    }

    class Push : Instruction {
        private Reg reg;

        public Push(Reg32 reg) : base(1) {
            this.reg = reg;
        }
        public Push(Reg64 reg) : base(2) {
            this.reg = reg;
        }

        public override void Write(ByteStream stream) {
            if (this.reg is Reg32)
                stream.Write((byte)(0x50 + reg));
            else
                stream.Write(0x41, (byte)(0x50 + reg));
        }

        public override string ToString() {
            ByteStream s = new ByteStream();
            Write(s);
            return Format(s.GetBytes(), "push", reg);
        }
    }

    class Mov : Instruction
    {
        private Reg dst;
        private Reg srcReg;
        private int srcVal;
        private AddressReference srcAddr;

        public Mov(Reg dst, Reg src) : base(3) {
            this.dst = dst;
            this.srcReg = src;
        }
        public Mov(Reg dst, int val) : base(7) {
            this.dst = dst;
            this.srcVal = val;
        }
        public Mov(Reg dst, AddressReference addr) : base(7) {
            this.dst = dst;
            this.srcAddr = addr;
        }

        public override void Write(ByteStream stream) {
            if (srcReg != null) {
                byte first;
                if (dst is Reg64) {
                    if (srcReg is Reg64) first = 0x4D;
                    else                 first = 0x49;
                }
                else
                {
                    if (srcReg is Reg64) first = 0x4C;
                    else                 first = 0x48;
                }
                byte tmp = (byte)(0xC0 + dst);
                tmp |= (byte)((byte)srcReg << 3);

                stream.Write(first, 0x89, tmp);
            }
            else {
                int val = srcVal;
                if (srcAddr != null) val = srcAddr.address;

                byte first = 0x48;
                if (dst is Reg64) first = 0x49;

                var tmp = BitConverter.GetBytes(val);
                stream.Write(first, 0xC7, (byte)(0xC0 + dst), tmp[0], tmp[1], tmp[2], tmp[3]);
            }
        }

        public override string ToString() {
            ByteStream s = new ByteStream();
            Write(s);
            if(srcReg != null)
                return Format(s.GetBytes(), "mov", dst, srcReg);
            if (srcAddr != null)
                return Format(s.GetBytes(), "mov", dst, Addr(srcAddr));
            return Format(s.GetBytes(), "mov", dst, Hex(srcVal));
        }
    }

    class Call : Instruction
    {
        private AddressReference addr;

        public Call(AddressReference addr) : base(7) {
            this.addr = addr;
        }

        public override void Write(ByteStream stream) {
            var tmp = BitConverter.GetBytes(addr.address);
            stream.Write(0xFF, 0x14, 0x25, tmp[0], tmp[1], tmp[2], tmp[3]);
        }
        public override string ToString() {
            ByteStream s = new ByteStream();
            Write(s);
            return Format(s.GetBytes(), "call", Addr(addr));
        }
    }

    class Leave : Instruction
    {
        public Leave() : base(1) { }

        public override void Write(ByteStream stream) {
            stream.Write(0xC9);
        }
        public override string ToString() {
            ByteStream s = new ByteStream();
            Write(s);
            return Format(s.GetBytes(), "leave");
        }
    }

}
