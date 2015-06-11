using System.Collections;

public interface IReader {
	byte getbyte();
	byte peekbyte();
	long tell();
	bool eof();
}
