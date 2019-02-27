using lang.utils;
using System;
using System.Collections.Generic;

namespace lang.program
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
                impSym = new ImportSymbol(impLib.name, symbol);
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
        public AddressReference addr = new AddressReference(AddressReference.Type.Import);

        public ImportSymbol(String lib, String name)
        {
            this.name = name;
            addr.name = lib + "." + name;
        }
    }
}
