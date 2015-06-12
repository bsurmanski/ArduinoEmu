using System.Collections;
using System.IO;

public class Memory {
	byte[] data;

	public Memory(int size) {
		data = new byte[size];
	}

    public Memory(int size, Stream init) {
        data = new byte[size];
        init.Read(data, 0, size);
    }
}
