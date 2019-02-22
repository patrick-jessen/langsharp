using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace pe
{
    class Importer
    {
        private int baseOffset;
        private int lookupTablesOffset;
        private int addressTablesOffset;
        private int dllNamesOffset;
        private int namesOffset;

        private List<Directory> directories = new List<Directory>();
        private List<Table> lookupTables = new List<Table>();
        private List<Table> addressTables = new List<Table>();
        private List<String> dllNames = new List<string>();
        private List<Name> names = new List<Name>();

        private Dictionary<String, int> dlls = new Dictionary<string, int>();

        public void Import(String symbol, String dll)
        {
            int descIdx;
            if (!dlls.TryGetValue(dll, out descIdx))
            {
                descIdx = directories.Count;
                dlls[dll] = descIdx;

                directories.Add(new Directory());
                lookupTables.Add(new Table());
                addressTables.Add(new Table());
                dllNames.Add(dll);
            }

            Int64 nameIdx = (Int64)names.Count;
            names.Add(new Name(symbol));

            lookupTables[descIdx].entries.Add(nameIdx);
            addressTables[descIdx].entries.Add(nameIdx);
        }

        private class Directory
        {
            Int32 importLookupTableRVA; // RVA to ImportLookupTable
            Int32 timeStamp;            // reserved
            Int32 forwarderChain;       // Not used
            Int32 nameRVA;              // RVA to null-terminated DLL string
            Int32 importAddressTableRVA;// Identical to importLookupTableRVA until the image is bound

            public static int Size()
            {
                return 20;
            }

            public void Update(Importer imp, int index)
            {
                int entryOffset = 0;
                for (int i = 0; i < index; i++)
                {
                    entryOffset += imp.lookupTables[i].Size();
                }

                importLookupTableRVA = imp.baseOffset + imp.lookupTablesOffset + entryOffset;


                entryOffset = 0;
                for (int i = 0; i < index; i++)
                {
                    entryOffset += imp.addressTables[i].Size();
                }
                importAddressTableRVA = imp.baseOffset + imp.addressTablesOffset + entryOffset;

                entryOffset = 0;
                for (int i = 0; i < index; i++)
                {
                    entryOffset += imp.dllNames[i].Length + 1; // 0-terminated string
                }
                nameRVA = imp.baseOffset + imp.dllNamesOffset + entryOffset;
            }

            public void Write(Stream stream)
            {
                WriteStream(stream, BitConverter.GetBytes(importLookupTableRVA));
                WriteStream(stream, BitConverter.GetBytes(timeStamp));
                WriteStream(stream, BitConverter.GetBytes(forwarderChain));
                WriteStream(stream, BitConverter.GetBytes(nameRVA));
                WriteStream(stream, BitConverter.GetBytes(importAddressTableRVA));
            }
        }
        private class Table
        {
            // only bits [0-30] are set
            // they are RVAs to a name table entry
            // last entry must be 0 to indicate end of table
            public List<Int64> entries = new List<Int64>();

            public int Size()
            {
                return (entries.Count + 1) * 8; // Last entry is 0
            }

            public void Update(Importer imp, int index)
            {
                int entryOffset = 0;
                for (int i = 0; i < index; i++)
                {
                    entryOffset += imp.names[i].Size();
                }

                entries[0] = imp.baseOffset + imp.namesOffset + entryOffset;
            }

            public void Write(Stream stream)
            {
                foreach (Int64 e in entries)
                {
                    WriteStream(stream, BitConverter.GetBytes(e));
                }
                WriteStream(stream, new byte[8]); // Last entry must be 0
            }
        }
        private class Name
        {
            public Name(String name)
            {
                this.name = name;
            }

            private Int16 hint;     // An index into the export name pointer table
            private String name;    // The name to import

            public int Size()
            {
                return 2 + name.Length + 1; // hint is 2 bytes. string is 0-terminated.
            }

            public void Write(Stream stream)
            {
                WriteStream(stream, BitConverter.GetBytes(hint));
                WriteStream(stream, Encoding.ASCII.GetBytes(name));
                WriteStream(stream, new byte[1]); // hint + 0-terminated string
            }
        }

        public void Write(Stream stream)
        {
            foreach (Directory d in directories)
            {
                d.Write(stream);
            }
            new Directory().Write(stream); // Null directory entry

            foreach (Table t in lookupTables)
            {
                t.Write(stream);
            }

            foreach (Name n in names)
            {
                n.Write(stream);
            }

            foreach (Table t in addressTables)
            {
                t.Write(stream);
            }

            foreach (String s in dllNames)
            {
                WriteStream(stream, Encoding.ASCII.GetBytes(s));
                WriteStream(stream, new byte[1]); // 0-terminated string
            }

        }

        public void Update(int offset)
        {
            this.baseOffset = offset;
            this.lookupTablesOffset = (directories.Count + 1) * Directory.Size(); // Each directory is 20bytes. Last one is null

            this.namesOffset = lookupTablesOffset;
            foreach (Table t in lookupTables)
            {
                this.namesOffset += t.Size();
            }

            this.addressTablesOffset = namesOffset;
            foreach (Name n in names)
            {
                this.addressTablesOffset += n.Size();
            }

            this.dllNamesOffset = addressTablesOffset;
            foreach (Table t in addressTables)
            {
                this.dllNamesOffset += t.Size();
            }

            // UPDATE
            for (int i = 0; i < directories.Count; i++)
            {
                directories[i].Update(this, i);
            }
            for (int i = 0; i < lookupTables.Count; i++)
            {
                lookupTables[i].Update(this, i);
            }
            for (int i = 0; i < addressTables.Count; i++)
            {
                addressTables[i].Update(this, i);
            }
        }

        public static void WriteStream(Stream s, byte[] data)
        {
            s.Write(data, 0, data.Length);
        }
    }

}
