using System;

namespace LunaPNG.Utility {
	//Implementation of the Adler32 checksum algorithm.
	public class Adler32 {
		const uint MOD_ADLER = 65521;

		//Shamelessly stolen from Wikipedia :3
		public uint Compute(byte[] data) {
			uint a = 1, b = 0;

			// Process each byte of the data in order
			for (int index = 0; index < data.Length; index++) {
				a = (a + data[index]) % MOD_ADLER;
				b = (b + a) % MOD_ADLER;
			}

			return (b << 16) | a;
		}
	}
}

