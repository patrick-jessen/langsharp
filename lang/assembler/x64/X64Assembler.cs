using System;
using System.Collections.Generic;

namespace lang.assembler
{
    class X64Assembler : Assembler {
        public static Reg32 RAX = new Reg32(0);
        public static Reg32 RCX = new Reg32(1);
        public static Reg32 RDX = new Reg32(2);
        public static Reg32 RBX = new Reg32(3);
        public static Reg32 RSP = new Reg32(4);
        public static Reg32 RBP = new Reg32(5);
        public static Reg32 RSI = new Reg32(6);
        public static Reg32 RDI = new Reg32(7);
        public static Reg64 R8  = new Reg64(0);
        public static Reg64 R9  = new Reg64(1);
        public static Reg64 R10 = new Reg64(2);
        public static Reg64 R11 = new Reg64(3);
        public static Reg64 R12 = new Reg64(4);
        public static Reg64 R13 = new Reg64(5);
        public static Reg64 R14 = new Reg64(6);
        public static Reg64 R15 = new Reg64(7);
    }
}
