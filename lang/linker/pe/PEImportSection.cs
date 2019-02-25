using lang.assembler;
using lang.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace lang.linker.pe
{
    class PEImportSection : PESection
    {
        private const uint characteristics = 0xC0000040; // IMAGE_SCN_MEM_WRITE | IMAGE_SCN_MEM_READ    | IMAGE_SCN_CNT_INITIALIZED_DATA

        private int baseOffset;
        private int lookupTablesOffset;
        private int namesOffset;
        private int addressTablesOffset;
        private int dllNamesOffset;
        private int size;

        private List<Directory> directories = new List<Directory>();
        private List<Table> lookupTables = new List<Table>();
        private List<Table> addressTables = new List<Table>();
        private List<String> dllNames = new List<string>();
        private List<Name> names = new List<Name>();

        private Dictionary<String, int> dlls = new Dictionary<string, int>();

        public PEImportSection(PE pe, ImportManager im) {
            PESectionHeader dataHeader = pe.dataSection.header;
            int virAddr = dataHeader.NextVirtualAddress();
            int rawAddr = dataHeader.NextRawAddress();

            foreach (KeyValuePair<String, ImportLibrary> lib in im.libraries) {
                foreach (KeyValuePair<String, ImportSymbol> sym in lib.Value.symbols) {
                    Import(sym.Key, lib.Key, sym.Value.addr);
                }
            }
            Update(virAddr);

            header.Init(".idata", size, virAddr, rawAddr, characteristics);
        }

        private void Import(String symbol, String dll, AddressReference ar)
        {
            int descIdx;
            if (!dlls.TryGetValue(dll, out descIdx))
            {
                descIdx = directories.Count;
                dlls[dll] = descIdx;

                directories.Add(new Directory(ar));
                lookupTables.Add(new Table());
                addressTables.Add(new Table());
                dllNames.Add(dll);
            }

            Int64 nameIdx = (Int64)names.Count;
            names.Add(new Name(symbol));

            lookupTables[descIdx].entries.Add(nameIdx);
            addressTables[descIdx].entries.Add(nameIdx);
        }

        private class Directory {
            Int32 importLookupTableRVA; // RVA to ImportLookupTable
            Int32 timeStamp;            // reserved
            Int32 forwarderChain;       // Not used
            Int32 nameRVA;              // RVA to null-terminated DLL string
            Int32 importAddressTableRVA;// Identical to importLookupTableRVA until the image is bound
            public AddressReference addr; // Not part of PE

            public Directory(AddressReference ar = null) {
                this.addr = ar;
            }

            public static int Size() {
                return 20;
            }

            public void Update(PEImportSection imp, int index) {
                int entryOffset = 0;
                for (int i = 0; i < index; i++)
                    entryOffset += imp.lookupTables[i].Size();

                importLookupTableRVA = imp.baseOffset + imp.lookupTablesOffset + entryOffset;


                entryOffset = 0;
                for (int i = 0; i < index; i++)
                    entryOffset += imp.addressTables[i].Size();

                importAddressTableRVA = imp.baseOffset + imp.addressTablesOffset + entryOffset;
                this.addr.Resolve((int)(PE.imageBase + importAddressTableRVA));

                entryOffset = 0;
                for (int i = 0; i < index; i++)
                    entryOffset += imp.dllNames[i].Length + 1; // 0-terminated string

                nameRVA = imp.baseOffset + imp.dllNamesOffset + entryOffset;
            }

            public void Write(Writer w) {
                w.Write(BitConverter.GetBytes(importLookupTableRVA));
                w.Write(BitConverter.GetBytes(timeStamp));
                w.Write(BitConverter.GetBytes(forwarderChain));
                w.Write(BitConverter.GetBytes(nameRVA));
                w.Write(BitConverter.GetBytes(importAddressTableRVA));
            }
        }
        private class Table {
            // only bits [0-30] are set
            // they are RVAs to a name table entry
            // last entry must be 0 to indicate end of table
            public List<Int64> entries = new List<Int64>();
            
            public int Size() {
                return (entries.Count + 1) * 8; // Last entry is 0
            }

            public void Update(PEImportSection imp, int index) {
                int entryOffset = 0;
                for (int i = 0; i < index; i++)
                    entryOffset += imp.names[i].Size();

                entries[0] = imp.baseOffset + imp.namesOffset + entryOffset;
            }

            public void Write(Writer w) {
                foreach (Int64 e in entries)
                    w.Write(BitConverter.GetBytes(e));
                w.Write(new byte[8]); // Last entry must be 0
            }
        }
        private class Name {
            private Int16 hint;     // An index into the export name pointer table
            private String name;    // The name to import

            public Name(String name) {
                this.name = name;
            }

            public int Size() {
                return 2 + name.Length + 1; // hint is 2 bytes. string is 0-terminated.
            }

            public void Write(Writer w) {
                w.Write(BitConverter.GetBytes(hint));
                w.Write(Encoding.ASCII.GetBytes(name));
                w.Write(new byte[1]); // hint + 0-terminated string
            }
        }

        public override void WriteData(Writer w) {
            foreach (Directory d in directories)
                d.Write(w);
            new Directory().Write(w); // Null directory entry

            foreach (Table t in lookupTables)
                t.Write(w);

            foreach (Name n in names)
                n.Write(w);

            foreach (Table t in addressTables)
                t.Write(w);

            foreach (String s in dllNames) {
                w.Write(Encoding.ASCII.GetBytes(s));
                w.Write(new byte[1]); // 0-terminated string
            }

            w.Write(new byte[header.sizeOfRawData - size]);
        }

        private void Update(int offset) {
            this.baseOffset = offset;
            this.lookupTablesOffset = (directories.Count + 1) * Directory.Size(); // Each directory is 20bytes. Last one is null

            this.namesOffset = lookupTablesOffset;
            foreach (Table t in lookupTables)
                this.namesOffset += t.Size();

            this.addressTablesOffset = namesOffset;
            foreach (Name n in names)
                this.addressTablesOffset += n.Size();

            this.dllNamesOffset = addressTablesOffset;
            foreach (Table t in addressTables)
                this.dllNamesOffset += t.Size();

            this.size = dllNamesOffset;
            foreach(String s in dllNames) {
                this.size += s.Length + 1;
            }

            // UPDATE
            for (int i = 0; i < directories.Count; i++)
                directories[i].Update(this, i);
            for (int i = 0; i < lookupTables.Count; i++)
                lookupTables[i].Update(this, i);
            for (int i = 0; i < addressTables.Count; i++)
                addressTables[i].Update(this, i);
        }

    }

}
