using System;

namespace lang.utils
{
    public class AddressReference
    {
        public String name;
        public int address = -1;

        public void Resolve(int addr) {
            this.address = addr;
        }
    }
}
