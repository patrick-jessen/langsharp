using System;

namespace lang.assembler
{
    class X64Reg : Reg
    {
        public X64Reg(byte idx)
        {
            this.idx = idx;
        }

        public override string ToString()
        {
            switch (idx)
            {
                case 0: return "rax";
                case 1: return "rcx";
                case 2: return "rdx";
                case 3: return "rbx";
                case 4: return "rsp";
                case 5: return "rbp";
                case 6: return "rsi";
                case 7: return "rdi";
                default: throw new Exception("invalid register");
            }
        }
    }
    class X64RegExt : Reg
    {
        public X64RegExt(byte idx)
        {
            this.idx = idx;
        }

        public override string ToString()
        {
            switch (idx)
            {
                case 0: return "r8";
                case 1: return "r9";
                case 2: return "r10";
                case 3: return "r11";
                case 4: return "r12";
                case 5: return "r13";
                case 6: return "r14";
                case 7: return "r15";
                default: throw new Exception("invalid register");
            }
        }
    }

}
