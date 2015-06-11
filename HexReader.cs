using System.Collections;
using System.IO;
using System;

/**
 * Reads .hex file formats. 'getchar' will read subsequent characters from the file
 */ 
public class HexReader : IReader {
	StreamReader stream;
	int offset;
	HexRecord currentRecord;

	/**
	 * a single record in a .hex file
	 */
	class HexRecord {
		const bool LENDIAN = true;
		enum RecordType {
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

		int hexToInt(char c) {
			if (c >= '0' && c <= '9') {
				return c - '0';
			} else if (c >= 'a' && c <= 'f') {
				return c - 'a' + 10;
			} else if (c >= 'A' && c <= 'F') {
				return c - 'A' + 10;
			}

			throw new ArgumentException ("Character must be hex character");
		}

		public HexRecord(string str) {
			if(!str.StartsWith(":")) {
				//throw new InvalidDataException("file does not conform to HEX format");
				throw new ArgumentException("file does not conform to HEX format");
			}

			count = (hexToInt(str[1]) << 4) + hexToInt(str[2]);
			address = (hexToInt (str[3]) << 12) + (hexToInt (str[4]) << 8) + (hexToInt (str[5]) << 4) + hexToInt (str[6]);
			type = (RecordType)(hexToInt(str[7]) << 4) + hexToInt (str[8]);

			data = new byte[count];
			for(int i = 0; i < count*2; i+=2) {
				data[i/2] = (byte) ((hexToInt(str[9+i]) << 4) + hexToInt (str[10+i]));
			}

			chksum = (byte)((hexToInt (str[count*2 + 9]) << 4) + hexToInt(str[count*2 + 10]));

			offset = 0;
		}

		public int length() {
			return count;
		}

		public int tell() {
			return offset;
		}

		public byte getbyte() {
			if (LENDIAN) {
				if (offset % 2 == 0) {
					return data [++offset];
				} else {
					return data [offset++ - 1];
				}
			} else {
				return data[offset++];
			}
		}

		public byte peekbyte() {
			if (LENDIAN) {
				if(offset % 2 == 0) {
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

	public HexReader(FileStream f) {
		stream = new StreamReader(f);
		currentRecord = new HexRecord(stream.ReadLine());
	}

	public byte getbyte() {
		if (currentRecord.eof ()) {
			if(stream.EndOfStream) {
				return 0;
			}
			currentRecord = new HexRecord(stream.ReadLine());
		}

		return currentRecord.getbyte ();
	}

	public byte peekbyte() {
		return currentRecord.peekbyte ();
	}

	public long tell() {
		return offset + currentRecord.tell ();
	}

	public bool eof() {
		return stream.EndOfStream && currentRecord.eof ();
	}
}
