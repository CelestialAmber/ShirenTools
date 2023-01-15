using System;
using System.Collections.Generic;
using System.IO.Hashing;

namespace PNGLib.Chunks {
	//Base class for a PNG chunk.
	public abstract class Chunk {
		//Gets the length of the chunk data section
		public abstract int GetLength();
		//Converts the main chunk data (chunk name + data) to a byte array
		public abstract byte[] ConvertMainDataToByteArray();

		//Converts the entire chunk to a byte array
		public byte[] ToByteArray() {
			List<byte> data = new List<byte>();

			data.AddRange(GetLength().ToByteArray());
			byte[] mainData = ConvertMainDataToByteArray();
			data.AddRange(mainData);
			data.AddRange(CalcCRCChecksum().ToByteArray());

			return data.ToArray();
		}

		public uint CalcCRCChecksum() {
			Crc32 crc = new Crc32();
			//Get the chunk bytes excluding the length and checksum, and calculate the checksum
			crc.Append(ConvertMainDataToByteArray());
			byte[] hashBytes = crc.GetCurrentHash();
			return BitConverter.ToUInt32(hashBytes);
		}
	}
}

