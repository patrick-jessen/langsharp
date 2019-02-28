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

        public Push(Reg src)
        {
            variant = Variant.r64;
            op1 = new Operand(src);
        }
        public Push(byte src)
        {
            variant = Variant.imm8;
            op1 = new Operand(src);
        }
        public Push(int src)
        {
            variant = Variant.imm32;
            op1 = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.imm8:  return w.Bytes(0x6A, op1.Byte);
                case Variant.imm32: return w.Bytes(0x68).Bytes(op1.Dword);
                case Variant.r64:
                    if (op1.Reg is X64RegExt)
                        return w.Bytes(REX(0, 0, 0, 1), (byte)(0x50 + op1.Reg));
                    else
                        return w.Bytes((byte)(0x50 + op1.Reg));
            }
            throw new Exception("Invalid variant");
        }

        protected override string Mnemonic() { return "push"; }
    }
    class Pop : X64Instruction
    {
        enum Variant { r64 };
        private Variant variant;

        public Pop(Reg dst)
        {
            variant = Variant.r64;
            op1 = new Operand(dst);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.r64:
                    if (op1.Reg is X64RegExt)
                        return w.Bytes(REX(0, 0, 0, 1), (byte)(0x58 + op1.Reg));
                    else
                        return w.Bytes((byte)(0x58 + op1.Reg));
            }
            throw new Exception("Invalid variant");
        }

        protected override string Mnemonic() { return "pop"; }
    }
    class Mov : X64Instruction
    {
        enum Variant { r64, imm32, imm64 };
        private Variant variant;

        public Mov(Reg dst, Reg src)
        {
            variant = Variant.r64;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }
        public Mov(Reg dst, int src)
        {
            variant = Variant.imm32;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }
        public Mov(Reg dst, long src)
        {
            variant = Variant.imm64;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }
        public Mov(Reg dst, AddressReference src)
        {
            variant = Variant.imm64;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch (variant)
            {
                case Variant.imm32: return w.Bytes(REX(1, 0, 0, IsExt(op1.Reg)), 0xC7, ModRM(0b11, 0, op1.Reg)).Bytes(op2.Dword);
                case Variant.imm64: return w.Bytes(REX(1, 0, 0, IsExt(op1.Reg)), (byte)(0xB8 + op1.Reg)).Bytes(op2.Qword);
                case Variant.r64:   return w.Bytes(REX(1, IsExt(op2.Reg), 0, IsExt(op1.Reg)), 0x89, ModRM(0b11, op2.Reg, op1.Reg));
            }
            throw new Exception("Invalid variant");
        }

        protected override string Mnemonic() { return "mov"; }
    }
    class Call : X64Instruction
    {
        enum Variant { Near, Far };
        private Variant variant;

        public Call(AddressReference addr)
        {
            if (addr.type == AddressReference.Type.Code)
                variant = Variant.Near;
            else if (addr.type == AddressReference.Type.Import)
                variant = Variant.Far;
            else
                throw new Exception("Address must point to code or import");

            op1 = new Operand(addr);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.Near: return w.Bytes(0xE8).Bytes(BitConverter.GetBytes((int)(op1.Addr.address - addr - 5)));
                case Variant.Far:  return w.Bytes(0xFF, ModRM(0, 2, 4), SIB(0, 4, 5)).Bytes(op1.Dword);
            }
            throw new Exception("Invalid variant");
        }

        protected override string Mnemonic() { return "call"; }
    }
    class Sub : X64Instruction
    {
        enum Variant { r64, imm32, imm8 };
        private Variant variant;

        public Sub(Reg dst, Reg src)
        {
            variant = Variant.r64;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }
        public Sub(Reg dst, int src)
        {
            variant = Variant.imm32;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }
        public Sub(Reg dst, byte src)
        {
            variant = Variant.imm8;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.r64:   return w.Bytes(REX(1, IsExt(op2.Reg), 0, IsExt(op1.Reg)), 0x2B, ModRM(0b11, op2.Reg, op1.Reg));
                case Variant.imm32: return w.Bytes(REX(1, 0, 0, IsExt(op1.Reg)), 0x81, ModRM(0b11, 5, op1.Reg)).Bytes(op2.Dword);
                case Variant.imm8:  return w.Bytes(REX(1, 0, 0, IsExt(op1.Reg)), 0x83, ModRM(0b11, 5, op1.Reg), op2.Byte);
            }
            throw new Exception("Invalid variant");
        }

        protected override string Mnemonic() { return "sub"; }
    }
    class Add : X64Instruction
    {
        enum Variant { r64, imm32, imm8 };
        private Variant variant;

        public Add(Reg dst, Reg src)
        {
            variant = Variant.r64;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }
        public Add(Reg dst, byte src)
        {
            variant = Variant.imm8;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }
        public Add(Reg dst, int src)
        {
            variant = Variant.imm32;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.r64:   return w.Bytes(REX(1, IsExt(op2.Reg), 0, IsExt(op1.Reg)), 0x01, ModRM(0b11, op2.Reg, op1.Reg));
                case Variant.imm32: return w.Bytes(REX(1, 0, 0, IsExt(op1.Reg)), 0x81, ModRM(0b11, 0, op1.Reg)).Bytes(op2.Dword);
                case Variant.imm8: return w.Bytes(REX(1, 0, 0, IsExt(op1.Reg)), 0x83, ModRM(0b11, 0, op1.Reg), op2.Byte);
            }
            throw new Exception("Invalid variant");
        }

        protected override string Mnemonic() { return "add"; }
    }
    class Xor : X64Instruction
    {
        enum Variant { r64 };
        private Variant variant;
        
        public Xor(Reg dst, Reg src)
        {
            variant = Variant.r64;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.r64: return w.Bytes(REX(1, IsExt(op2.Reg), 0, IsExt(op1.Reg)), 0x31, ModRM(0b11, op2.Reg, op1.Reg));
            }
            throw new Exception("Invalid variant");
        }

        protected override string Mnemonic() { return "xor"; }
    }
    class Retn : X64Instruction
    {
        public override byte[] Bytes(long addr)
        {
            return new byte[] { 0xC3 };
        }

        protected override string Mnemonic() { return "retn"; }
    }
    class Cmp : X64Instruction
    {
        enum Variant { imm32 };
        private Variant variant;

        public Cmp(Reg dst, int src)
        {
            variant = Variant.imm32;
            op1 = new Operand(dst);
            op2 = new Operand(src);
        }

        public override byte[] Bytes(long addr = 0)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.imm32: return w.Bytes(REX(1, 0, 0, IsExt(op1.Reg)), 0x81, ModRM(0b11, 7, op1.Reg)).Bytes(op2.Dword);
            }
            throw new Exception("Invalid variant");
        }

        protected override string Mnemonic() { return "cmp"; }
    }
    class Jmp : X64Instruction
    {
        enum Variant { rel32 };
        private Variant variant;

        public Jmp(Label label)
        {
            variant = Variant.rel32;
            op1 = new Operand(label.addr);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            switch(variant)
            {
                case Variant.rel32: return w.Bytes(0xE9).Bytes(BitConverter.GetBytes((int)(op1.Addr.address - addr - 5)));
            }
            throw new Exception("Invalid variant");
        }

        protected override string Mnemonic() { return "jmp"; }
    }
    class Jl : X64Instruction
    {
        public Jl(Label label)
        {
            op1 = new Operand(label.addr);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            return w.Bytes(OpcodeExt, 0x8C).Bytes(BitConverter.GetBytes((int)(op1.Addr.address - addr - 6)));
        }

        protected override string Mnemonic() { return "jl"; }
    }
    class Jg : X64Instruction
    {
        public Jg(Label label)
        {
            op1 = new Operand(label.addr);
        }

        public override byte[] Bytes(long addr)
        {
            var w = new InstructionWriter();
            return w.Bytes(OpcodeExt, 0x8F).Bytes(BitConverter.GetBytes((int)(op1.Addr.address - addr - 6)));
        }

        protected override string Mnemonic() { return "jg"; }
    }
}
