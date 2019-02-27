using System;

namespace lang.utils
{
    public class AddressReference
    {
        public enum Type { Code, Data, Import }
        public Type type;
        public String name;
        public long address = -1;

        public AddressReference(Type t)
        {
            this.type = t;
        }

        public void Resolve(long addr)
        {
            this.address = addr;
        }
    }
}
