﻿using lang.assembler;
using lang.program;
using lang.utils;
using System.Collections.Generic;

namespace lang.linker.pe
{
    class PECodeSection : PESection {
        private const uint characteristics = 0x60000020; // IMAGE_SCN_MEM_READ  | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_CNT_CODE
        private List<CodeBlock> blocks;
        private int size;

        public PECodeSection(PE pe, List<CodeBlock> blocks) {
            this.blocks = blocks;
            foreach(CodeBlock b in blocks) {
                long addr = PE.imageBase + PE.baseOfCode + size;
                b.addr.Resolve(addr);
                size += b.assembler.ResolveAndGetSize(addr);
            }

            this.header.Init(".text", size, PE.baseOfCode, pe.SizeOfHeaders, characteristics);
        }

        public override void WriteData(Writer w) {
            w.currAddress = PE.imageBase + PE.baseOfCode;
            foreach (CodeBlock b in blocks) {
                foreach (Instruction i in b.assembler.instructions)
                    w.Write(i.Bytes(w.currAddress));
            }
            w.Write(new byte[header.sizeOfRawData - size]);
        }
    }
}
