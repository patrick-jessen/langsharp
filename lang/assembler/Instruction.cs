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
        protected abstract string Mnemonic();
        protected Operand op1;
        protected Operand op2;
         
        public virtual void Resolve(long addr) { }
        public int Size() { return Bytes().Length; }

        public abstract byte[] Bytes(long addr = 0);
        public override string ToString()
        {
            String s = "";
            foreach (byte b in Bytes(0))
                s += String.Format("0x{0:X2} ", b);
            return String.Format("{0,-50}|{1,-5}{2,-5}{3}", s, Mnemonic(), op1, op2);
        }
    }

    class Label : Instruction
    {
        public AddressReference addr = new AddressReference(AddressReference.Type.Code);

        public Label(string name = "unnamed label")
        {
            addr.name = name;
        }

        public override byte[] Bytes(long addr)
        {
            return new byte[0];
        }

        protected override string Mnemonic() { return ""; }
        public override void Resolve(long addr)
        {
            this.addr.Resolve(addr);
        }

        public override string ToString()
        {
            return addr.name + ":";
        }
    }

    abstract class Reg
    {
        public byte idx = 0;
        public static implicit operator byte(Reg r)
        {
            return r.idx;
        }
        public static implicit operator String(Reg r)
        {
            return r.ToString();
        }
    }


    class Operand
    {
        enum Type { Reg, Addr, Byte, Word, Dword, Qword}
        private Type type;
        private Reg reg;
        private long val;
        private AddressReference addr;

        public Operand(Reg reg)
        {
            this.type = Type.Reg;
            this.reg = reg;
        }
        public Operand(AddressReference addr)
        {
            this.type = Type.Addr;
            this.addr = addr;
        }
        public Operand(byte val)
        {
            this.type = Type.Byte;
            this.val = val;
        }
        public Operand(short val)
        {
            this.type = Type.Word;
            this.val = val;
        }
        public Operand(int val)
        {
            this.type = Type.Dword;
            this.val = val;
        }
        public Operand(long val)
        {
            this.type = Type.Qword;
            this.val = val;
        }

        public byte Byte
        {
            get { return (byte)val; }
        }
        public byte[] Word
        {
            get
            {
                if (type == Type.Addr) val = addr.address;
                return BitConverter.GetBytes((short)val);
            }
        }
        public byte[] Dword
        {
            get
            {
                if (type == Type.Addr) val = addr.address;
                return BitConverter.GetBytes((int)val);
            }
        }
        public byte[] Qword
        {
            get
            {
                if (type == Type.Addr) val = addr.address;
                return BitConverter.GetBytes(val);
            }
        }
        public Reg Reg
        {
            get { return reg; }
        }
        public AddressReference Addr
        {
            get { return addr; }
        }

        public override string ToString()
        {
            switch(type)
            {
                case Type.Byte:     return String.Format("0x{0:X2}", (byte)val);
                case Type.Word:     return String.Format("0x{0:X4}", (short)val);
                case Type.Dword:    return String.Format("0x{0:X8}", (int)val);
                case Type.Qword:    return String.Format("0x{0:X16}", val);
                case Type.Reg:      return reg;
                case Type.Addr:     return String.Format("[{0}]", addr.name);
            }
            throw new Exception("Invalid operand type");
        }
    }
}
