using lang.program;
using lang.utils;

namespace lang.linker.pe
{
    class PEDataSection : PESection {
        private const uint characteristics = 0x40000040; // IMAGE_SCN_MEM_READ  | IMAGE_SCN_CNT_INITIALIZED_DATA
        private DataManager dataManager;
        private int size;

        public PEDataSection(PE pe, DataManager dm) { 
            this.dataManager = dm;

            PESectionHeader codeHeader = pe.codeSection.header;
            int virAddr = codeHeader.NextVirtualAddress();
            int rawAddr = codeHeader.NextRawAddress();

            foreach (DataItem i in dm.itemList) {
                i.addr.Resolve((int)PE.imageBase + virAddr + size);
                size += i.Size;
            }

            this.header.Init(".rdata", size, virAddr, rawAddr, characteristics);
        }

        public override void WriteData(Writer w) {
            foreach (DataItem i in dataManager.itemList)
                w.Write(i.data);
            w.Write(new byte[header.sizeOfRawData - size]);
        }
    }
}
