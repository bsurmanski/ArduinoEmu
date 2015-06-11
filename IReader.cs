using System.Collections;

public interface IReader {
    long Length();
	byte getbyte();
	byte peekbyte();
	long tell();
	bool eof();
}
