using lang.assembler;
using lang.utils;
using System;
using System.IO;

namespace lang.assembler
{
    abstract class X64Instruction : Instruction
    {
        // https://wiki.osdev.org/X86-64_Instruction_Encoding
        // VEX/XOP opcodes omitted
        // Instruction encoding
        // |=REX=======|=Opcode====|=ModR/M====|=SIB=======|=Displacement=====|=Immediate========|
        // | 0-1 bytes | 1-3 bytes | 0-1 bytes | 0-1 bytes | 0,1,2 or 4 bytes | 0,1,2 or 4 bytes |

        // REX is an optional prefix to opcodes (1 byte)
        protected byte[] REX(byte W, byte R, byte X, byte B)
        {
            // W = 64bit size
            // R = ModRM.reg extension
            // X = SIB.index extension
            // B = ModRM.rm extension

            byte rex = 0x40; // fixed bit pattern
            rex |= (byte)((W & 1) << 3);
            rex |= (byte)((R & 1) << 2);
            rex |= (byte)((X & 1) << 1);
            rex |= (byte)(B & 1);
            return new byte[] { rex };
        }
        // 1 byte opcode
        protected byte[] Opcode(byte po)
        {
            return new byte[] { po };
        }
        // 2 byte opcode
        protected byte[] Opcode2(byte po)
        {
            return new byte[] { 0x0F, po };
        }
        // 3 byte opcode
        protected byte[] OpCode3(byte po, byte so)
        {
            return new byte[] { 0x0F, po, so };
        }
        // ModRM encodes operands (1 byte)
        protected byte[] ModRM(byte mod, byte reg, byte rm)
        {
            // mod (note: combines with rm):
            // 00 = address
            // 01 = 1byte displacement follows
            // 10 = 4byte displacement follows
            // 11 = register

            // reg:
            // if 1 operand: opcode extension
            // if 2 operands: register no. of operand #2 (can be combined with REX.R)

            // rm: (can be combined with REX.B) (note: combines with mod)
            // register no. of operand #1

            // Special combinations:
            // mod=00|01|10 && rm==100:
            //   SIB follows
            // mod=00 && rm==101:
            //   Relative to instruction ptr. 32bit offset follows
            return new byte[] { (byte)((mod & 3) << 6 | (reg & 7) << 3 | rm & 7) };
        }
        // SIB describes an address offset
        protected byte[] SIB(byte scale, byte index, byte _base)
        {
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
    }

    class Push : X64Instruction
    {
        private Reg srcReg;
        private int srcVal;

        public Push(Reg reg)
        {
            this.srcReg = reg;
            this.size = 1;
            if (reg is Reg64)
                this.size = 2;
        }
        public Push(int val)
        {
            this.srcVal = val;
            if (Math.Abs(val) < Byte.MaxValue / 2)
                this.size = 2;
            else if (Math.Abs(val) < int.MaxValue)
                this.size = 5;
            else throw new Exception("immediate push value must fit in a DWORD");
        }


        public override void Write(Writer w)
        {
            if(this.srcReg == null)
            {
                if (this.size == 2)
                    w.Write(
                        Opcode(0x6A),
                        new byte[] { (byte)this.srcVal }
                    );
                else
                    w.Write(
                        Opcode(0x68),
                        BitConverter.GetBytes(this.srcVal)
                    );
            }
            else if (this.srcReg is Reg32)
            {
                w.Write(
                    Opcode((byte)(0x50 + srcReg))
                );
            }
            else
            {
                w.Write(
                    REX(0, 0, 0, 1),
                    Opcode((byte)(0x50 + srcReg))
                );
            }
        }

        public override string ToString()
        {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "push", srcReg);
        }
    }
    class Pop : X64Instruction
    {
        private Reg dst;

        public Pop(Reg reg)
        { 
            this.dst = reg;
            this.size = 1;
            if (dst is Reg64)
                this.size = 2;
        }

        public override void Write(Writer w)
        {
            if (this.dst is Reg32)
            {
                w.Write(
                    Opcode((byte)(0x58 + dst))
                );
            }
            else
            {
                w.Write(
                    REX(0, 0, 0, 1),
                    Opcode((byte)(0x58 + dst))
                );
            }
        }

        public override string ToString()
        {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "pop", dst);
        }
    }
    class Mov : X64Instruction
    {
        private Reg dst;
        private Reg srcReg;
        private int srcVal;
        private AddressReference srcAddr;

        public Mov(Reg dst, Reg src) {
            this.dst = dst;
            this.srcReg = src;
            this.size = 3;
        }
        public Mov(Reg dst, int val) {
            this.dst = dst;
            this.srcVal = val;
            this.size = 7;
        }
        public Mov(Reg dst, AddressReference addr) {
            this.dst = dst;
            this.srcAddr = addr;
            this.size = 7;
        }

        public override void Write(Writer w) {
            byte dstExt = 0;
            if (dst is Reg64) dstExt = 1;

            if (srcReg != null) {
                byte srcExt = 0;
                if (srcReg is Reg64) srcExt = 1;

                w.Write(
                    REX(1, srcExt, 0, dstExt),
                    Opcode(0x89),
                    ModRM(11, srcReg, dst)
                );
            }
            else {
                int val = srcVal;
                if (srcAddr != null) val = srcAddr.address;

                w.Write(
                    REX(1, 0, 0, dstExt),
                    Opcode(0xC7),
                    ModRM(11, 0, dst),
                    BitConverter.GetBytes(val)
                );
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
    class Call : X64Instruction
    {
        private AddressReference addr;
        private bool near;

        public Call(AddressReference addr, bool near)
        {
            this.addr = addr;
            this.near = near;

            this.size = 5;
            if (!near) this.size = 7;
        }

        public override void Write(Writer w)
        {
            if (near)
                w.Write(
                    Opcode(0xE8),
                    BitConverter.GetBytes(addr.imageAddress - (w.currAddress + 5))
                );
            else
                w.Write(
                    Opcode(0xFF),
                    ModRM(0, 2, 4),
                    SIB(0, 4, 5),
                    BitConverter.GetBytes(addr.address)
                );
        }

        public override string ToString()
        {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "call", Addr(addr));
        }
    }

    class Leave : X64Instruction
    {
        public Leave() {
            this.size = 1;
        }

        public override void Write(Writer w)
        {
            w.Write(Opcode(0xC9));
        }
        public override string ToString()
        {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "leave");
        }
    }

    class Sub : X64Instruction {
        private Reg dst;
        private int srcVal;

        public Sub(Reg reg, int val) {
            this.dst = reg;
            this.srcVal = val;
            this.size = 7;
        }

        public override void Write(Writer w) {
            byte dstExt = 0;
            if (dst is Reg64) dstExt = 1;

            w.Write(
                REX(1, 0, 0, dstExt),
                Opcode(0x81),
                ModRM(3, 5, dst),
                BitConverter.GetBytes(this.srcVal)
            );
        }

        public override string ToString()
        {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "sub", dst, Hex(srcVal));
        }
    }

    class Add : X64Instruction
    {
        private Reg dst;
        private int srcVal;

        public Add(Reg reg, int val)
        {
            this.dst = reg;
            this.srcVal = val;
            this.size = 7;
        }

        public override void Write(Writer w)
        {
            byte dstExt = 0;
            if (dst is Reg64) dstExt = 1;

            w.Write(
                REX(1, 0, 0, dstExt),
                Opcode(0x81),
                ModRM(3, 0, dst),
                BitConverter.GetBytes(this.srcVal)
            );
        }

        public override string ToString()
        {
            Writer s = new Writer();
            Write(s);
            return Format(s.GetBytes(), "add", dst, Hex(srcVal));
        }
    }

    class Retn : X64Instruction
    {
        public Retn()
        {
            this.size = 1;
        }
        public override void Write(Writer w)
        {
            w.Write(Opcode(0xC3));
        }
        public override string ToString()
        {
            return Format(new byte[] { 0xC3 }, "ret");
        }
    }

    
}
