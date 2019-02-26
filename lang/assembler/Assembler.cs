using lang.utils;
using System;
using System.Collections.Generic;

namespace lang.assembler
{
    abstract class Assembler {
        public List<Instruction> instructions = new List<Instruction>();

        public int Size {
            get {
                int s = 0;
                foreach (Instruction i in instructions)
                {
                    if (i.size < 0) throw new Exception("Instruction size is not set for " + i.GetType().Name);
                    s += i.size;
                }
                return s;
            }
        }

        public void Add(params Instruction[] instructions) {
            foreach(Instruction i in instructions)
                this.instructions.Add(i);
        }

        public override String ToString() {
            String o = "";
            foreach (Instruction i in instructions)
                o += i.ToString() + "\r\n";
            return o;
        }
    }
}
