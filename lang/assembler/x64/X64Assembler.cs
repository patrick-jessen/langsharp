using System;
using System.Collections.Generic;

namespace lang.assembler
{
    class X64Assembler : Assembler {
        // Register encodings
        // https://wiki.osdev.org/X86-64_Instruction_Encoding#Registers
        public static X64Reg RAX = new X64Reg(0);
        public static X64Reg RCX = new X64Reg(1);
        public static X64Reg RDX = new X64Reg(2);
        public static X64Reg RBX = new X64Reg(3);
        public static X64Reg RSP = new X64Reg(4);
        public static X64Reg RBP = new X64Reg(5);
        public static X64Reg RSI = new X64Reg(6);
        public static X64Reg RDI = new X64Reg(7);
        public static X64RegExt R8  = new X64RegExt(0);
        public static X64RegExt R9  = new X64RegExt(1);
        public static X64RegExt R10 = new X64RegExt(2);
        public static X64RegExt R11 = new X64RegExt(3);
        public static X64RegExt R12 = new X64RegExt(4);
        public static X64RegExt R13 = new X64RegExt(5);
        public static X64RegExt R14 = new X64RegExt(6);
        public static X64RegExt R15 = new X64RegExt(7);
    }
}
