using lang.assembler;
using lang.utils;
using System.Collections.Generic;

namespace lang.program
{
    class Program {
        internal List<CodeBlock> codeBlocks = new List<CodeBlock>();
        internal DataManager data = new DataManager();
        internal ImportManager import = new ImportManager();

        public AddressReference String(string str)
        {
            return data.String(str);
        }
        public AddressReference Bytes(byte[] b)
        {
            return data.Bytes(b);
        }
        public AddressReference Import(string sym, string lib)
        {
            return import.Import(sym, lib);
        }
        public CodeBlock AddBlock(string name = "")
        {
            if (name.Length == 0) name = "block_" + codeBlocks.Count;
            CodeBlock b = new CodeBlock(name);
            codeBlocks.Add(b);
            return b;
        }

        public override string ToString()
        {
            string o = "====================== CODE ======================\n";
            foreach (CodeBlock b in codeBlocks)
                o += string.Format("{0}:\n{1}\n", b.addr.name, b.assembler);

            o += "====================== DATA ======================\n";
            foreach (DataItem d in data.itemList)
            {
                o += string.Format("{0,-50}|", d.addr.name);
                for (int i = 0; i < d.data.Length; i++)
                {
                    if (i != 0 && i % 8 == 0)
                        o += string.Format("\n{0,-50}|", "");

                    o += string.Format("0x{0:X2} ", d.data[i]);
                }
                o += "\n\n" +
                    "";
            }
            o += "===================== IMPORT =====================\n";
            foreach(KeyValuePair<string, ImportLibrary> l in import.libraries)
            {
                o += string.Format("{0}\n", l.Key);
                foreach(KeyValuePair<string, ImportSymbol> s in l.Value.symbols)
                {
                    o += string.Format("{0,-5}{1}\n", "", s.Key);
                }
                o += "\n";
            }

            return o;
        }
    }

    class CodeBlock {
        public Assembler assembler = new Assembler();
        public AddressReference addr = new AddressReference(AddressReference.Type.Code);

        internal CodeBlock(string name)
        {
            addr.name = name;
        }
    }
}
