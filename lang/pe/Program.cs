using System;
using System.IO;
using System.Linq;

namespace pe
{
    class PE
    {
        public PE(byte[] code, byte[] data, byte[] rdata, Importer i) {
            sections = new Section[] {
                new Section(".text",  0x60000020, code),    // IMAGE_SCN_MEM_READ  | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_CNT_CODE
                new Section(".data",  0xC0000040, data),    // IMAGE_SCN_MEM_WRITE | IMAGE_SCN_MEM_READ    | IMAGE_SCN_CNT_INITIALIZED_DATA
                new Section(".rdata", 0x40000040, rdata),   // IMAGE_SCN_MEM_READ  | IMAGE_SCN_CNT_INITIALIZED_DATA
                new Section(".idata", 0xC0000040, null),    // IMAGE_SCN_MEM_WRITE | IMAGE_SCN_MEM_READ    | IMAGE_SCN_CNT_INITIALIZED_DATA
            };

            Int32 va = baseOfCode;
            Int32 ra = SizeOfHeaders;
            foreach (Section s in sections) {
                s.header.virtualAddress = va;
                s.header.pointerToRawData = ra;

                va += ((Int32)((float)s.header.virtualSize / sectionAlignment) + 1) * sectionAlignment;
                ra += s.header.sizeOfRawData;
            }

            MemoryStream ms = new MemoryStream();
            i.Update(sections[3].header.virtualAddress);
            i.Write(ms);
            sections[3].SetData(ms.ToArray());
        }

        private Stream stream;
        private Section[] sections;
        private const Byte majorLinkerVer = 0;
        private const Byte minorLinkerVer = 1;
        private const Int32 sectionAlignment        = 0x1000;   // Page size of x64 Windows
        private const Int32 fileAlignment           = 0x200;    // Default file alignment
        private const Int32 addressOfEntryPoint     = 0x1000;
        private const Int32 baseOfCode              = 0x1000;
        private const Int64 imageBase               = 0x400000; // Default base address of Win32 executables
        private const Int32 checksum                = 0;        // No applicable

        private Int16 NumSections {
            get { return (Int16)sections.Length; }
        }
        private Int32 TimeStamp {
            get { return (Int32)DateTimeOffset.Now.ToUnixTimeSeconds(); }
        }
        private Int32 SizeOfCode {
            get { return sections[0].header.sizeOfRawData; }
        }
        private Int32 SizeOfInitializedData {
            get { return sections[1].header.sizeOfRawData + sections[2].header.sizeOfRawData; /*+ sections[3].header.sizeOfRawData;*/ }
        }
        private Int32 SizeOfImage {
            get { return sections.Last().header.virtualAddress + sections.Last().header.virtualSize; }
        }
        private Int32 SizeOfHeaders {
            get { return ((Int32)((float)GetActualSizeOfHeaders() / fileAlignment) + 1) * fileAlignment; }
        }

        private Int32 sizeOfUninitializedData = 0;

        private Int32 GetActualSizeOfHeaders() {
            // DOS header: 0x40 bytes
            // PE header: 0x04 bytes
            // File header: 0x14 bytes
            // Optional header: 0xF0 bytes
            // Section size: 40 bytes
            return (Int32)(0x40 + 0x04 + 0x14 + 0xF0 + 40 * sections.Length);
        }

        
        private Int64 sizeOfStackReserve    = 0x200000;
        private Int64 sizeOfStackCommit     = 0x1000;
        private Int64 sizeOfHeapReserve     = 0x100000;
        private Int64 sizeOfHeapCommit      = 0x1000;

        public void WriteFile(string file) {
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            stream = File.Create(file);
            WritePE();
            stream.Flush();
            stream.Close();
        }

        private void WritePE() {
            WriteDOSHeader();
            WritePEHeader();
            WriteSectionsTable();
            Write(new byte[SizeOfHeaders - GetActualSizeOfHeaders()]); // Padding
            WriteSectionsData();
        }

        private void WriteDOSHeader() {
            Write(
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
            Write(
                new byte[] {
                /*0x00*/ 0x50, 0x45, 0x00, 0x00, // Signature
                /*0x04*/
            });
            WriteFileHeader();
            WriteOptionalHeader();
        }

        private void WriteFileHeader() {
            Write(
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
            Write(
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
            Write(
            /*0x70*/ new DataDirectory(),   // exportTable
            /*0x78*/ new DataDirectory(sections[3].header.virtualAddress, sections[3].header.virtualSize),   // importTable
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
            foreach(Section s in sections) {
                Write(s.header);
            }
        }

        private void WriteSectionsData() {
            foreach (Section s in sections) {
                Write(s.data);
                Write(new byte[s.header.sizeOfRawData - s.data.Length]);
            }
        }

        public void Write(params byte[][] args) {
            foreach(byte[] bytes in args)
                stream.Write(bytes, 0, bytes.Length);
        }
        private void Write(params DataDirectory[] args) {
            foreach (DataDirectory dd in args) {
                Write(BitConverter.GetBytes(dd.address), BitConverter.GetBytes(dd.size));
            }
        }
        private void Write(SectionHeader h) {
            Write(
                h.name,
                BitConverter.GetBytes(h.virtualSize),
                BitConverter.GetBytes(h.virtualAddress),
                BitConverter.GetBytes(h.sizeOfRawData),
                BitConverter.GetBytes(h.pointerToRawData),
                BitConverter.GetBytes(SectionHeader.pointerToRelocations),
                BitConverter.GetBytes(SectionHeader.pointerToLinenumbers),
                BitConverter.GetBytes(SectionHeader.numberOfRelocations),
                BitConverter.GetBytes(SectionHeader.numberOfLinenumbers),
                BitConverter.GetBytes(h.characteristics)
            );
        }

        class SectionHeader
        {
            public SectionHeader(String name, UInt32 characteristics)
            {
                if (name.Length >= 8) throw new Exception("section name must be at most 8 characters");

                for (int i = 0; i < name.Length; i++)
                    this.name[i] = (byte)name[i];

                this.characteristics = characteristics;
            }

            public byte[] name = new byte[8];               /*0x00*/
            public Int32 virtualSize;                       /*0x08*/
            public Int32 virtualAddress;                    /*0x0C*/
            public Int32 sizeOfRawData;                     /*0x10*/
            public Int32 pointerToRawData;                  /*0x14*/
            public const Int32 pointerToRelocations = 0;    /*0x18*/
            public const Int32 pointerToLinenumbers = 0;    /*0x1C*/
            public const Int16 numberOfRelocations = 0;     /*0x20*/
            public const Int16 numberOfLinenumbers = 0;     /*0x22*/
            public UInt32 characteristics;                  /*0x24*/
            /*0x28 - size of section header*/
        }
        class Section
        {
            public SectionHeader header;
            public byte[] data;

            public Section(String name, UInt32 characteristics, byte[] data) {
                header = new SectionHeader(name, characteristics);
                if(data != null)
                    SetData(data);
            }

            public void SetData(byte[] bytes) {
                if (bytes == null || bytes.Length == 0) throw new Exception("length of data must be > 0");

                Int32 alignedSize = ((Int32)((float)bytes.Length / PE.fileAlignment) + 1) * PE.fileAlignment;
                data = bytes;
                header.sizeOfRawData = alignedSize;
                header.virtualSize = bytes.Length;
            }
        }

        class DataDirectory
        {
            public DataDirectory(Int32 address = 0, Int32 size = 0) {
                this.address = address;
                this.size = size;
            }
            public Int32 address;
            public Int32 size;
        }
    }

}
