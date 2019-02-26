using lang.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lang.assembler
{
    abstract class Instruction
    {
        public int size = -1;

        protected string Format(byte[] bytes, String i, String o1 = "", String o2 = "") {
            String s = "";
            foreach (byte b in bytes) {
                s += String.Format("0x{0:X2} ", b);
            }
            return String.Format("{0,-40}{1,-5}{2,-5}{3}{4,5} bytes", s, i, o1, o2, size);
        }
        protected static string Hex(int val) {
            return String.Format("0x{0:X2}", val);
        }
        protected static string Addr(AddressReference a) {
            return String.Format("<0x{0:X}>", a.address);
        }

        public abstract void Write(Writer w);
    }
}
