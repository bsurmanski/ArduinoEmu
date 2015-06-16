using System;
using System.Collections;

public class InvalidInstructionException : Exception {
	public InvalidInstructionException(string err) : base(err) {
	}
}
