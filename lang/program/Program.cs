using lang.assembler;
using lang.utils;
using System.Collections.Generic;

namespace lang.program
{
    class Program {
        public List<CodeBlock> codeBlocks = new List<CodeBlock>();
        public DataManager data = new DataManager();
        public ImportManager import = new ImportManager();

    }

    class CodeBlock {
        public Assembler assembler = new X64Assembler();
        public AddressReference addr = new AddressReference();
    }
}
