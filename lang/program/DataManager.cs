using lang.utils;
using System.Collections.Generic;

namespace lang.program
{
    class DataManager {
        public List<DataItem> itemList = new List<DataItem>();

        public AddressReference String(string str) {
            byte[] bytes = new byte[str.Length + 1];
            for (int i = 0; i < str.Length; i++)
                bytes[i] = (byte)str[i];
            return Bytes(bytes);
        }

        public AddressReference Bytes(byte[] bytes) {
            DataItem item = new DataItem(bytes);
            itemList.Add(item);
            return item.addr;
        }
    }

    public class DataItem {
        public AddressReference addr = new AddressReference();
        public byte[] data;

        public int Size {
            get { return data.Length; }
        }

        public DataItem(byte[] data) {
            this.data = data;
        }
    }
}
