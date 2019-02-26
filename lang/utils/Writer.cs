using System.IO;

namespace lang.utils
{
    public class Writer
    {
        public MemoryStream stream = new MemoryStream();
        public int currAddress;

        public void Write(params byte[] bytes) {
            stream.Write(bytes, 0, bytes.Length);
            currAddress += bytes.Length;
        }
        public void Write(params byte[][] bytes) {
            foreach(byte[] chunk in bytes)
                Write(chunk);
        }
        public byte[] GetBytes() {
            return stream.ToArray();
        }
    }
}
