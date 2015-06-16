using System.Collections;
using System.IO;

namespace Arduino
{
    public class Memory
    {
        byte[] data;

        public Memory(int size) {
            data = new byte[size];
        }

        public Memory(int size, Stream init) {
            data = new byte[size];
            init.Read(data, 0, size);
        }

        public ushort readWord(uint offset) {
            return (ushort)((data[offset] << 8) | (data[offset + 1]));
        }
    }
}
