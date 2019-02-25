using lang.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lang.linker.pe
{
    public abstract class PESection {
        public PESectionHeader header = new PESectionHeader();

        public void WriteHeader(Writer w) {
            w.Write(
                header.name,
                BitConverter.GetBytes(header.virtualSize),
                BitConverter.GetBytes(header.virtualAddress),
                BitConverter.GetBytes(header.sizeOfRawData),
                BitConverter.GetBytes(header.pointerToRawData),
                BitConverter.GetBytes(PESectionHeader.pointerToRelocations),
                BitConverter.GetBytes(PESectionHeader.pointerToLinenumbers),
                BitConverter.GetBytes(PESectionHeader.numberOfRelocations),
                BitConverter.GetBytes(PESectionHeader.numberOfLinenumbers),
                BitConverter.GetBytes(header.characteristics)
            );
        }

        public abstract void WriteData(Writer w);
    }

    public class PESectionHeader
    {
        public void Init(String name, int size, int virAddr, int rawAddr, UInt32 characteristics) {
            if (name.Length >= 8) throw new Exception("section name must be at most 8 characters");
            for (int i = 0; i < name.Length; i++)
                this.name[i] = (byte)name[i];

            this.sizeOfRawData = PE.AlignFile(size);
            this.virtualSize = size;
            this.virtualAddress = virAddr;
            this.pointerToRawData = rawAddr;
            this.characteristics = characteristics;
        }

        public int NextVirtualAddress() {
            return virtualAddress + PE.AlignSection(virtualSize);
        }
        public int NextRawAddress() {
            return pointerToRawData + PE.AlignFile(virtualSize);
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
}
