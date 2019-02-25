using lang.assembler;
using lang.utils;
using System;
using System.IO;

namespace lang.assembler
{


    class Push : Instruction {
        private Reg reg;

        public Push(Reg32 reg) : base(1) {
            this.reg = reg;
        }
        public Push(Reg64 reg) : base(2) {
            this.reg = reg;
        }

        public override void Write(Writer stream) {
            if (this.reg is Reg32)
                stream.Write((byte)(0x50 + reg));
            else
                stream.Write(0x41, (byte)(0x50 + reg));
        }

        public override string ToString() {
            Writer s = new Writer();
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

        public override void Write(Writer stream) {
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
            Writer s = new Writer();
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

        public override void Write(Writer stream) {
            var tmp = BitConverter.GetBytes(addr.address);
            stream.Write(0xFF, 0x14, 0x25, tmp[0], tmp[1], tmp[2], tmp[3]);
        }
        public override string ToString() {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "call", Addr(addr));
        }
    }

    class Leave : Instruction
    {
        public Leave() : base(1) { }

        public override void Write(Writer stream) {
            stream.Write(0xC9);
        }
        public override string ToString() {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "leave");
        }
    }

    class NOP : Instruction
    {
        public NOP() : base(1) { }

        public override void Write(Writer stream) {
            stream.Write(0x90);
        }
        public override string ToString() { 
            return Format(new byte[]{0x90}, "NOP");
        }
    }

    class Sub : Instruction {
        private Reg dst;
        private int srcVal;

        public Sub(Reg reg, int val) : base(7) {
            this.dst = reg;
            this.srcVal = val;
        }

        public override void Write(Writer stream) {
            byte first = 0x48;
            if (dst is Reg64) first = 0x49;

            var tmp = BitConverter.GetBytes(srcVal);
            stream.Write(first, 0x81, (byte)(0xE8 + dst), tmp[0], tmp[1], tmp[2], tmp[3]);
        }

        public override string ToString()
        {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "sub", dst, Hex(srcVal));
        }
    }

    class Add : Instruction
    {
        private Reg dst;
        private int srcVal;

        public Add(Reg reg, int val) : base(7)
        {
            this.dst = reg;
            this.srcVal = val;
        }

        public override void Write(Writer stream)
        {
            byte first = 0x48;
            if (dst is Reg64) first = 0x49;

            var tmp = BitConverter.GetBytes(srcVal);
            stream.Write(first, 0x81, (byte)(0xC0 + dst), tmp[0], tmp[1], tmp[2], tmp[3]);
        }

        public override string ToString()
        {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "add", dst, Hex(srcVal));
        }
    }

    class Ret : Instruction
    {
        public Ret() : base(1) { }
        public override void Write(Writer stream) {
            stream.Write(0xC3);
        }
        public override string ToString() {
            return Format(new byte[] { 0xC3 }, "ret");
        }
    }

    class Pop: Instruction
    {
        private Reg dst;

        public Pop(Reg32 reg) : base(1) {
            this.dst = reg;
        }
        public Pop(Reg64 reg) : base(2) {
            this.dst = reg;
        }

        public override void Write(Writer stream) {
            if(dst is Reg64)
                stream.Write(0x41, (byte)(0x58 + dst));
            else
                stream.Write((byte)(0x58 + dst));
        }

        public override string ToString()
        {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "pop", dst);
        }
    }
}
