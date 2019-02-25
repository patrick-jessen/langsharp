using lang.program;
using lang.utils;
using System;
using System.IO;

namespace lang.linker.pe
{
    class PE
    {
        public PE(Program prog) {
            codeSection = new PECodeSection(this, prog.codeBlocks);
            dataSection = new PEDataSection(this, prog.data);
            importSection = new PEImportSection(this, prog.import);
        }

        private Writer w = new Writer();
        public PECodeSection codeSection;
        public PEDataSection dataSection;
        public PEImportSection importSection;

        private const Byte majorLinkerVer = 0;
        private const Byte minorLinkerVer = 1;
        private const Int32 sectionAlignment        = 0x1000;   // Page size of x64 Windows
        private const Int32 fileAlignment           = 0x200;    // Default file alignment
        private const Int32 addressOfEntryPoint     = 0x1000;
        public const Int32 baseOfCode               = 0x1000;
        public const Int64 imageBase                = 0x400000; // Default base address of Win32 executables
        private const Int32 checksum                = 0;        // No applicable

        private Int16 NumSections {
            get { return (Int16)3; }
        }
        private Int32 TimeStamp {
            get { return (Int32)DateTimeOffset.Now.ToUnixTimeSeconds(); }
        }
        private Int32 SizeOfCode {
            get { return codeSection.header.sizeOfRawData; }
        }
        private Int32 SizeOfInitializedData {
            get { return dataSection.header.sizeOfRawData + importSection.header.sizeOfRawData; }
        }
        private Int32 SizeOfImage {
            get { return importSection.header.NextVirtualAddress(); }
        }
        public Int32 SizeOfHeaders {
            get { return AlignFile(GetActualSizeOfHeaders()); }
        }

        private Int32 sizeOfUninitializedData = 0;

        private Int32 GetActualSizeOfHeaders() {
            // DOS header: 0x40 bytes
            // PE header: 0x04 bytes
            // File header: 0x14 bytes
            // Optional header: 0xF0 bytes
            // Section size: 40 bytes * 3
            return (Int32)(0x40 + 0x04 + 0x14 + 0xF0 + 40 * 3);
        }
        
        private Int64 sizeOfStackReserve    = 0x200000;
        private Int64 sizeOfStackCommit     = 0x1000;
        private Int64 sizeOfHeapReserve     = 0x100000;
        private Int64 sizeOfHeapCommit      = 0x1000;

        public void WriteFile(string file) {
            WritePE();

            Directory.CreateDirectory(Path.GetDirectoryName(file));
            Stream stream = File.Create(file);
            byte[] bytes = w.GetBytes();
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            stream.Close();
        }

        private void WritePE() {
            WriteDOSHeader();
            WritePEHeader();
            WriteSectionsTable();
            w.Write(new byte[SizeOfHeaders - GetActualSizeOfHeaders()]); // Padding
            WriteSectionsData();
        }

        private void WriteDOSHeader() {
            w.Write(
                new byte[] {
                /*0x00*/ 0x4D, 0x5A, // signature
		        /*0x02*/ 0x00, 0x00, // lastsize
		        /*0x04*/ 0x00, 0x00, // nblocks
		        /*0x06*/ 0x00, 0x00, // nreloc
		        /*0x08*/ 0x00, 0x00, // hdrsize
		        /*0x0A*/ 0x00, 0x00, // minalloc
		        /*0x0C*/ 0x00, 0x00, // maxalloc
		        /*0x0E*/ 0x00, 0x00, // ss
		        /*0x10*/ 0x00, 0x00, // sp
		        /*0x12*/ 0x00, 0x00, // checksum
		        /*0x14*/ 0x00, 0x00, // ip
		        /*0x16*/ 0x00, 0x00, // cs
		        /*0x18*/ 0x00, 0x00, // relocpos
                /*0x1A*/ 0x00, 0x00, // noverlay 
                },

                /*0x1C*/ new byte[8],// reserved

                new byte[] {
                /*0x20*/ 0x00, 0x00, // oem_id
                /*0x22*/ 0x00, 0x00, // oem_info
                },

                /*0x24*/ new byte[20],// reserved

                new byte[] {
                /*0x38*/ 0x40, 0x00, 0x00, 0x00, // e_lfanew
                /*0x40 - end of DOS header*/
                }
            );
        }

        private void WritePEHeader() {
            w.Write(
                new byte[] {
                /*0x00*/ 0x50, 0x45, 0x00, 0x00, // Signature
                /*0x04*/
            });
            WriteFileHeader();
            WriteOptionalHeader();
        }

        private void WriteFileHeader() {
            w.Write(
                new byte[] {
                /*0x00*/ 0x64, 0x86, // Machine
                },
            
                /*0x02*/ BitConverter.GetBytes(NumSections),
                /*0x04*/ BitConverter.GetBytes(TimeStamp),

                new byte[] {
                /*0x08*/ 0x00, 0x00, 0x00, 0x00, // PointerToSymbolTable
                /*0x0C*/ 0x00, 0x00, 0x00, 0x00, // NumberOfSymbols
                /*0x10*/ 0xF0, 0x00, // SizeOfOptionalHeader
		        /*0x12*/ 0x22, 0x00, // Characteristics
                /*0x14 - end of file header*/
                }
            );
        }

        private void WriteOptionalHeader() {
            w.Write(
                new byte[] {
                /*0x00*/ 0x0B, 0x02, // Magic
		        /*0x02*/ majorLinkerVer, // MajorLinkerVersion
                /*0x03*/ minorLinkerVer, // MinorLinkerVersion
                },
            
                /*0x04*/ BitConverter.GetBytes(SizeOfCode),
                /*0x08*/ BitConverter.GetBytes(SizeOfInitializedData),
                /*0x0C*/ BitConverter.GetBytes(sizeOfUninitializedData),
                /*0x10*/ BitConverter.GetBytes(addressOfEntryPoint),
                /*0x14*/ BitConverter.GetBytes(baseOfCode),
                /*0x18*/ BitConverter.GetBytes(imageBase),
                /*0x20*/ BitConverter.GetBytes(sectionAlignment),
                /*0x24*/ BitConverter.GetBytes(fileAlignment),

                new byte[] {
		        /*0x28*/ 0x06, 0x00, // MajorOperatingSystemVersion
		        /*0x2A*/ 0x00, 0x00, // MinorOperatingSystemVersion
		        /*0x2C*/ 0x00, 0x00, // MajorImageVersion
		        /*0x2E*/ 0x00, 0x00, // MinorImageVersion
		        /*0x30*/ 0x06, 0x00, // MajorSubsystemVersion
		        /*0x32*/ 0x00, 0x00, // MinorSubsystemVersion
                /*0x34*/ 0x00, 0x00, 0x00, 0x00, // Win32VersionValue
                },

                /*0x38*/ BitConverter.GetBytes(SizeOfImage),
                /*0x3C*/ BitConverter.GetBytes(SizeOfHeaders),
                /*0x40*/ BitConverter.GetBytes(checksum),

                new byte[] {
                /*0x44*/ 0x03, 0x00, // Subsystem
                /*0x46*/ 0x00, 0x00, // DllCharacteristics
                },

                /*0x48*/ BitConverter.GetBytes(sizeOfStackReserve),
                /*0x50*/ BitConverter.GetBytes(sizeOfStackCommit),
                /*0x58*/ BitConverter.GetBytes(sizeOfHeapReserve),
                /*0x60*/ BitConverter.GetBytes(sizeOfHeapCommit),

                new byte[] {
                /*0x68*/ 0x00, 0x00, 0x00, 0x00, // LoaderFlags
		        /*0x6C*/ 0x10, 0x00, 0x00, 0x00, // NumberOfRvaAndSizes
                /*0x70*/
                }
            );
            WriteDataDirectories();
        }

        private void WriteDataDirectories() {
            WriteDataDirs(
            /*0x70*/ new DataDirectory(),   // exportTable
            /*0x78*/ new DataDirectory(importSection.header.virtualAddress, importSection.header.virtualSize),   // importTable
            /*0x80*/ new DataDirectory(),   // resourceTable
            /*0x88*/ new DataDirectory(),   // exceptionTable
            /*0x90*/ new DataDirectory(),   // certificateTable
            /*0x98*/ new DataDirectory(),   // baseRelocationTable
            /*0xA0*/ new DataDirectory(),   // debug
            /*0xA8*/ new DataDirectory(),   // architecture
            /*0xB0*/ new DataDirectory(),   // globalPtr
            /*0xB8*/ new DataDirectory(),   // TLSTable
            /*0xC0*/ new DataDirectory(),   // loadConfigTable
            /*0xC8*/ new DataDirectory(),   // boundImport
            /*0xD0*/ new DataDirectory(),   // IAT
            /*0xD8*/ new DataDirectory(),   // delayImportDescriptor
            /*0xE0*/ new DataDirectory(),   // CLRRuntimeHeader
            /*0xE8*/ new DataDirectory()    // reserved
            /*0xF0 * end of optional header*/
            );
        }

        private void WriteSectionsTable() {
            codeSection.WriteHeader(w);
            dataSection.WriteHeader(w);
            importSection.WriteHeader(w);
        }

        private void WriteSectionsData() {
            codeSection.WriteData(w);
            dataSection.WriteData(w);
            importSection.WriteData(w);
        }

        private void WriteDataDirs(params DataDirectory[] args) {
            foreach (DataDirectory dd in args) {
                w.Write(BitConverter.GetBytes(dd.address), BitConverter.GetBytes(dd.size));
            }
        }
       
        private class DataDirectory
        {
            public DataDirectory(Int32 address = 0, Int32 size = 0) {
                this.address = address;
                this.size = size;
            }
            public Int32 address;
            public Int32 size;
        }

        public static int AlignSection(int val) {
            return ((Int32)((float)val / PE.sectionAlignment) + 1) * PE.sectionAlignment;
        }
        public static int AlignFile(int val) {
            return ((Int32)((float)val / PE.fileAlignment) + 1) * PE.fileAlignment;
        }
    }

}
