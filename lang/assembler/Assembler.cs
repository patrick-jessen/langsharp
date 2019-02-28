using lang.utils;
using System;
using System.Collections.Generic;

namespace lang.assembler
{
    class Assembler
    {
        public List<Instruction> instructions = new List<Instruction>();

        public int ResolveAndGetSize(long addr)
        {
            int s = 0;
            foreach (Instruction i in instructions)
            {
                i.Resolve(addr + s);
                s += i.Size();
            }
            return s;
        }
        
        public void Add(params Instruction[] instructions)
        {
            foreach(Instruction i in instructions)
                this.instructions.Add(i);
        }

        public override String ToString()
        {
            String o = "";
            foreach (Instruction i in instructions)
                o += i.ToString() + "\r\n";
            return o;
        }
    }
}
