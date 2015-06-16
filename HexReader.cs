using System.Collections;
using System.IO;
using System;

namespace Arduino
{
    /**
     * Reads .hex file formats. 'getchar' will read subsequent characters from the file
     */
    public class HexReader : Stream
    {
        StreamReader stream;
        int length;
        int offset;
        HexRecord currentRecord;

        /**
         * a single record in a .hex file
         */
        class HexRecord
        {
            const bool LENDIAN = true;
            enum RecordType
            {
                DATA = 0,
                EOF = 1,
                EXTSEGADDR = 2,
                STARTSEGADDR = 3,
                EXTLINADDR = 4,
                STARTLINADDR = 5
            }

            int count; // byte count
            int address; // address of data
            RecordType type;
            byte[] data;
            byte chksum;

            int offset;

            static int hexToInt(char c) {
                if (c >= '0' && c <= '9') {
                    return c - '0';
                } else if (c >= 'a' && c <= 'f') {
                    return c - 'a' + 10;
                } else if (c >= 'A' && c <= 'F') {
                    return c - 'A' + 10;
                }

                throw new ArgumentException("Character must be hex character: " + c);
            }

            public static int RecordSize(string str) {
                if (!str.StartsWith(":")) {
                    //throw new InvalidDataException("file does not conform to HEX format");
                    throw new ArgumentException("file does not conform to HEX format");
                }

                return (hexToInt(str[1]) << 4) + hexToInt(str[2]);
            }

            public HexRecord(string str) {
                if (!str.StartsWith(":")) {
                    //throw new InvalidDataException("file does not conform to HEX format");
                    throw new ArgumentException("file does not conform to HEX format");
                }

                count = (hexToInt(str[1]) << 4) + hexToInt(str[2]);
                address = (hexToInt(str[3]) << 12) + (hexToInt(str[4]) << 8) + (hexToInt(str[5]) << 4) + hexToInt(str[6]);
                type = (RecordType)(hexToInt(str[7]) << 4) + hexToInt(str[8]);

                data = new byte[count];
                for (int i = 0; i < count * 2; i += 2) {
                    data[i / 2] = (byte)((hexToInt(str[9 + i]) << 4) + hexToInt(str[10 + i]));
                }

                chksum = (byte)((hexToInt(str[count * 2 + 9]) << 4) + hexToInt(str[count * 2 + 10]));

                offset = 0;
            }

            public int length() {
                return count;
            }

            public int tell() {
                return offset;
            }

            public byte GetByte() {
                if (LENDIAN) {
                    if (offset % 2 == 0) {
                        return data[++offset];
                    } else {
                        return data[offset++ - 1];
                    }
                } else {
                    return data[offset++];
                }
            }

            public byte peekbyte() {
                if (LENDIAN) {
                    if (offset % 2 == 0) {
                        return data[offset + 1];
                    } else {
                        return data[offset - 1];
                    }
                } else {
                    return data[offset];
                }
            }

            public bool eof() {
                return offset >= count;
            }
        }

        public HexReader(Stream f) {
            length = -1;
            stream = new StreamReader(f);
            currentRecord = new HexRecord(stream.ReadLine());
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override bool CanSeek {
            get { return true; }
        }

        public override void Flush() {
            // do nothing
        }

        public override long Length {
            get {
                if (length < 0) {
                    length = 0;
                    long set = stream.BaseStream.Position;
                    stream.BaseStream.Seek(0, SeekOrigin.Begin);
                    while (!stream.EndOfStream) {
                        length += HexRecord.RecordSize(stream.ReadLine());
                    }
                    stream.BaseStream.Seek(set, SeekOrigin.Begin);
                }
                return length;
            }
        }

        public override void SetLength(long value) {
            throw new NotSupportedException("HexReader is read only; cannot set length");
        }

        public override long Position {
            get {
                return offset + currentRecord.tell();
            }

            set {
                if (value > Length) {
                    throw new ArgumentOutOfRangeException("Attempt to seek past end of file");
                }
                long count = 0;
                stream.BaseStream.Seek(0, SeekOrigin.Begin);
                while (value < count) {
                    string line = stream.ReadLine();
                    long recordSz = HexRecord.RecordSize(line);
                    if (count + recordSz > value) {
                        currentRecord = new HexRecord(line);
                        for (int i = 0; i < (count + recordSz - value); i++) {
                            currentRecord.GetByte();
                        }
                        return;
                    }
                    count += recordSz;
                }
            }
        }

        public override long Seek(long off, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    Position = off;
                    break;
                case SeekOrigin.Current:
                    Position = this.offset + off;
                    break;
                case SeekOrigin.End:
                    Position = Length + off;
                    break;
            }
            return Position;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            for (int i = 0; i < count; i++) {
                int b = ReadByte();
                if (b < 0) return i;
                buffer[i + offset] = (byte)b;
            }
            return count;
        }

        public override int ReadByte() {
            while (currentRecord.eof()) {
                if (stream.EndOfStream) {
                    return -1;
                }
                currentRecord = new HexRecord(stream.ReadLine());
            }

            return (int)currentRecord.GetByte();
        }

        public override void Write(byte[] b, int off, int count) {
            throw new NotSupportedException("Writting on Hex Streams not supported");
        }

        public byte PeekByte() {
            return currentRecord.peekbyte();
        }

        public bool eof() {
            return stream.EndOfStream && currentRecord.eof();
        }
    }
}