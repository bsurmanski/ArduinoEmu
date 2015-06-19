using System.Collections;
using System.IO;
using System.Text;

namespace Arduino
{
    public class Memory
    {
        bool littleEndian;
        byte[] data;

        public Memory(int size) {
            data = new byte[size];
            littleEndian = true;
        }

        public Memory(int size, Stream init) {
            data = new byte[size];
            Fill(init);
            littleEndian = true;
        }

        public void Fill(Stream init) {
            init.Read(data, 0, data.Length);
        }

        public void Fill(byte[] init) {
            init.CopyTo(data, 0);
        }

        public void Zero() {
            data = new byte[data.Length];
        }

        public byte readByte(uint offset) {
            return data[offset];
        }

        public ushort readWord(uint offset) {
            if (littleEndian) {
                return (ushort)((data[offset + 1] << 8) | (data[offset]));
            } else {
                return (ushort)((data[offset] << 8) | (data[offset + 1]));
            }
        }

        public void writeByte(uint offset, byte b) {
            data[offset] = b;
        }

        public void writeWord(uint offset, ushort val) {
            if (littleEndian) {
                data[offset + 1] = (byte)((val >> 8) & 0xff00);
                data[offset] = (byte)(val & 0x00ff);
            } else {
                data[offset] = (byte)((val >> 8) & 0xff00);
                data[offset + 1] = (byte)(val & 0x00ff);
            }
        }

        public override string ToString() {
            StringBuilder str = new StringBuilder();

            for (int i = 0; i < data.Length; i++) {
                if ((i % 16) == 0) {
                    string addr = "\n" + i.ToString("X4") + ": ";
                    str.Append(addr);
                }

                str.Append(data[i].ToString("X2"));
            }

            return str.ToString();
        }
    }
}
