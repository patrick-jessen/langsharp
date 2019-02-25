﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lang.utils
{
    public class Writer
    {
        public MemoryStream stream = new MemoryStream();

        public void Write(params byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }
        public void Write(params byte[][] bytes)
        {
            foreach(byte[] chunk in bytes)
                stream.Write(chunk, 0, chunk.Length);
        }
        public byte[] GetBytes()
        {
            return stream.ToArray();
        }
    }
}
