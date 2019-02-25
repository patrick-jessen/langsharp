﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lang.assembler
{
    public class ImportManager {
        public Dictionary<String, ImportLibrary> libraries = new Dictionary<String, ImportLibrary>();

        public AddressReference Import(String symbol, String lib) {
            ImportLibrary impLib;
            if(!libraries.TryGetValue(lib, out impLib)) {
                impLib = new ImportLibrary(lib);
                libraries.Add(lib, impLib);
            }

            ImportSymbol impSym;
            if (!impLib.symbols.TryGetValue(symbol, out impSym)) {
                impSym = new ImportSymbol(symbol);
                impLib.symbols.Add(symbol, impSym);
            }

            return impSym.addr;
        }
    }
    public class ImportLibrary
    {
        public String name;
        public Dictionary<String, ImportSymbol> symbols = new Dictionary<String, ImportSymbol>();

        public ImportLibrary(String name)
        {
            this.name = name;
        }
    }

    public class ImportSymbol
    {
        String name;
        public AddressReference addr = new AddressReference();

        public ImportSymbol(String name)
        {
            this.name = name;
        }
    }
}
