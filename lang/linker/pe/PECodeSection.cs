using lang.assembler;
using lang.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lang.linker.pe
{
    class PECodeSection : PESection {
        private const uint characteristics = 0x60000020; // IMAGE_SCN_MEM_READ  | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_CNT_CODE
        private Assembler assembler;

        public PECodeSection(PE pe, Assembler a) {
            this.assembler = a;
            this.header.Init(".text", a.Size, PE.baseOfCode, pe.SizeOfHeaders, characteristics);
        }

        public override void WriteData(Writer w) {
            foreach (Instruction i in assembler.instructions)
                i.Write(w);
            w.Write(new byte[header.sizeOfRawData - assembler.Size]);
        }
    }
}
