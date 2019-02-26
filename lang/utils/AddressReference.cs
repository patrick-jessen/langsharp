using System;

namespace lang.utils
{
    public class AddressReference
    {
        public String name;
        public int address = -1;
        public int imageAddress = -1;

        public void Resolve(int baseAddr, int imageAddr) {
            this.address = baseAddr + imageAddr;
            this.imageAddress = imageAddr;
        }
    }
}
