using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lang.assembler
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
