using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Arduino
{
    public class HexadecimalStream : Stream
    {
        Stream stream;
        long offset;

        public HexadecimalStream(Stream f) {
            stream = f;
            offset = 0;
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override bool CanSeek {
            get { return true; }
        }

        public override void Flush() {
            // do nothing
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override long Position {
            get {
                return offset;
            }
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override long Length {
            get { return stream.Length / 2; }
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            for (int i = 0; i < count; i++) {
                int b = ReadByte();
                if (b < 0) return i;
                buffer[offset + i] = (byte)b;
            }
            return count;
        }

        int convertHexChar(byte b) {
            if (b >= '0' && b <= '9') {
                return b - '0';
            }

            if (b >= 'a' && b <= 'f') {
                return b - 'a' + 10;
            }

            if (b >= 'A' && b <= 'F') {
                return b - 'A' + 10;
            }

            throw new ArgumentException("hex character must be between 0-9a-fA-F");
        }

        public override int ReadByte() {
            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            if (b1 < 0) return b1;
            if (b2 < 0) return b2;

            offset++;
            return (convertHexChar((byte) b1) << 4) | convertHexChar((byte) b2);
        }
    }
}
