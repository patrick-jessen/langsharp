using lang.assembler;
using lang.utils;
using System;
using System.Collections.Generic;
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

        // REX is an optional prefix to opcodes
        protected byte REX(byte W, byte R, byte X, byte B)
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
            return rex;
        }

        // Indicates a two or three byte opcode
        protected const byte OpcodeExt = 0x0F;

        // ModRM encodes operands
        protected byte ModRM(byte mod, byte reg, byte rm)
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
            return (byte)((mod & 3) << 6 | (reg & 7) << 3 | rm & 7);
        }

        // SIB describes an address offset
        protected byte SIB(byte scale, byte index, byte _base)
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
            return (byte)((scale & 3) << 6 | (index & 7) << 3 | _base & 7);
        }
        
        // Returns 0 or 1 depending on whether register is x64 extended (r8-r15)
        protected byte IsExt(Reg r)
        {
            if (r is X64RegExt) return 1;
            return 0;
        }
    }

    internal class InstructionWriter
    {
        private List<byte> bytes = new List<byte>();

        public InstructionWriter Bytes(params byte[] bytes)
        {
            foreach(byte b in bytes)
                this.bytes.Add(b);
            return this;
        }
        public InstructionWriter Bytes(params byte[][] bytes)
        {
            foreach (byte[] b in bytes)
                Bytes(b);
            return this;
        }

        public static implicit operator byte[](InstructionWriter w)
        {
            return w.bytes.ToArray();
        }
    }

    class Push : X64Instruction
    {
        enum Variant { r64, imm8, imm32 };
        private Variant variant;
        private Operand src;

        public Push(Reg src)
        {
            this.variant = Variant.r64;
            this.src = new Operand(src);
        }
        public Push(byte src)
        {
            this.variant = Variant.imm8;
            this.src = new Operand(src);
        }
        public Push(int src)
        {
            this.variant = Variant.imm32;
            this.src = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.imm8:  return w.Bytes(0x6A, src.Byte);
                case Variant.imm32: return w.Bytes(0x68).Bytes(src.Dword);
                case Variant.r64:
                    if (src.Reg is X64RegExt)
                        return w.Bytes(REX(0, 0, 0, 1), (byte)(0x50 + src.Reg));
                    else
                        return w.Bytes((byte)(0x50 + src.Reg));
            }
            throw new Exception("Invalid variant");
        }

        public override string ToString()
        {
            return Format("push", src);
        }
    }
    class Pop : X64Instruction
    {
        enum Variant { r64 };
        private Variant variant;
        private Operand dst;

        public Pop(Reg dst)
        {
            this.variant = Variant.r64;
            this.dst = new Operand(dst);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.r64:
                    if (dst.Reg is X64RegExt)
                        return w.Bytes(REX(0, 0, 0, 1), (byte)(0x58 + dst.Reg));
                    else
                        return w.Bytes((byte)(0x58 + dst.Reg));
            }
            throw new Exception("Invalid variant");
        }

        public override string ToString()
        {
            return Format("pop", dst);
        }
    }
    class Mov : X64Instruction
    {
        enum Variant { r64, imm32, imm64 };
        private Variant variant;
        private Operand dst;
        private Operand src;

        public Mov(Reg dst, Reg src)
        {
            this.variant = Variant.r64;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }
        public Mov(Reg dst, int src)
        {
            this.variant = Variant.imm32;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }
        public Mov(Reg dst, long src)
        {
            this.variant = Variant.imm64;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }
        public Mov(Reg dst, AddressReference src)
        {
            this.variant = Variant.imm64;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch (variant)
            {
                case Variant.imm32: return w.Bytes(REX(1, 0, 0, IsExt(dst.Reg)), 0xC7, ModRM(11, 0, dst.Reg)).Bytes(src.Dword);
                case Variant.imm64: return w.Bytes(REX(1, 0, 0, IsExt(dst.Reg)), (byte)(0xB8 + dst.Reg)).Bytes(src.Qword);
                case Variant.r64:   return w.Bytes(REX(1, IsExt(src.Reg), 0, IsExt(dst.Reg)), 0x89, ModRM(11, src.Reg, dst.Reg));
            }
            throw new Exception("Invalid variant");
        }

        public override string ToString()
        {
            return Format("mov", dst, src);
        }
    }
    class Call : X64Instruction
    {
        enum Variant { Near, Far };
        private Variant variant;
        private Operand addr;

        public Call(AddressReference addr)
        {
            if (addr.type == AddressReference.Type.Code)
                this.variant = Variant.Near;
            else if (addr.type == AddressReference.Type.Import)
                this.variant = Variant.Far;
            else
                throw new Exception("Address must point to code or import");

            this.addr = new Operand(addr);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.Near: return w.Bytes(0xE8).Bytes(BitConverter.GetBytes((int)(this.addr.Addr.address - addr - 5)));
                case Variant.Far:  return w.Bytes(0xFF, ModRM(0, 2, 4), SIB(0, 4, 5)).Bytes(this.addr.Dword);
            }
            throw new Exception("Invalid variant");
        }

        public override string ToString()
        {
            return Format("call", addr);
        }
    }
    class Sub : X64Instruction
    {
        enum Variant { r64, imm32, imm8 };
        private Variant variant;
        private Operand dst;
        private Operand src;

        public Sub(Reg dst, Reg src)
        {
            this.variant = Variant.r64;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }
        public Sub(Reg dst, int src)
        {
            this.variant = Variant.imm32;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }
        public Sub(Reg dst, byte src)
        {
            this.variant = Variant.imm8;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.r64:   return w.Bytes(REX(1, IsExt(src.Reg), 0, IsExt(dst.Reg)), 0x2B, ModRM(11, src.Reg, dst.Reg));
                case Variant.imm32: return w.Bytes(REX(1, 0, 0, IsExt(dst.Reg)), 0x81, ModRM(11, 5, dst.Reg)).Bytes(src.Dword);
                case Variant.imm8:  return w.Bytes(REX(1, 0, 0, IsExt(dst.Reg)), 0x83, ModRM(11, 5, dst.Reg), src.Byte);
            }
            throw new Exception("Invalid variant");
        }

        public override string ToString()
        {
            return Format("sub", dst, src);
        }
    }
    class Add : X64Instruction
    {
        enum Variant { r64, imm32, imm8 };
        private Variant variant;
        private Operand dst;
        private Operand src;

        public Add(Reg dst, Reg src)
        {
            this.variant = Variant.r64;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }
        public Add(Reg dst, byte src)
        {
            this.variant = Variant.imm8;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }
        public Add(Reg dst, int src)
        {
            this.variant = Variant.imm32;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.r64:   return w.Bytes(REX(1, IsExt(src.Reg), 0, IsExt(dst.Reg)), 0x01, ModRM(11, src.Reg, dst.Reg));
                case Variant.imm32: return w.Bytes(REX(1, 0, 0, IsExt(dst.Reg)), 0x81, ModRM(11, 0, dst.Reg)).Bytes(src.Dword);
                case Variant.imm8: return w.Bytes(REX(1, 0, 0, IsExt(dst.Reg)), 0x83, ModRM(11, 0, dst.Reg), src.Byte);
            }
            throw new Exception("Invalid variant");
        }

        public override string ToString()
        {
            return Format("add", dst, src);
        }
    }
    class Xor : X64Instruction
    {
        enum Variant { r64 };
        private Variant variant;
        private Operand dst;
        private Operand src;

        public Xor(Reg dst, Reg src)
        {
            this.variant = Variant.r64;
            this.dst = new Operand(dst);
            this.src = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.r64: return w.Bytes(REX(1, IsExt(src.Reg), 0, IsExt(dst.Reg)), 0x31, ModRM(11, src.Reg, dst.Reg));
            }
            throw new Exception("Invalid variant");
        }

        public override string ToString()
        {
            return Format("xor", dst, src);
        }
    }
    class Retn : X64Instruction
    {
        public override byte[] Bytes(long addr)
        {
            return new byte[] { 0xC3 };
        }

        public override string ToString()
        {
            return Format("ret");
        }
    }


}
