using lang.utils;
using System;
using System.Collections.Generic;

namespace lang.program
{
    class DataManager {
        public List<DataItem> itemList = new List<DataItem>();

        public AddressReference String(string str) {
            byte[] bytes = new byte[str.Length + 1];
            for (int i = 0; i < str.Length; i++)
                bytes[i] = (byte)str[i];

            string name = str.Substring(0, Math.Min(str.Length, 30));
            DataItem item = new DataItem(bytes, "data:\"" + name + "\"");
            itemList.Add(item);
            return item.addr;
        }

        public AddressReference Bytes(byte[] bytes) {
            DataItem item = new DataItem(bytes, "data_" + itemList.Count);
            itemList.Add(item);
            return item.addr;
        }
    }

    public class DataItem {
        public AddressReference addr = new AddressReference(AddressReference.Type.Data);
        public byte[] data;

        public int Size {
            get { return data.Length; }
        }

        public DataItem(byte[] data, string name) {
            this.data = data;
            this.addr.name = name;
        }
    }
}
