using System;
using System.Collections.Generic;
using System.Text;

namespace LunaPNG.Chunks {
	public class IENDChunk : Chunk {
		public const string name = "IEND";

		public IENDChunk() {
		}

		public override int GetLength() {
			return 0; //IEND has no data
		}

		public override byte[] ConvertMainDataToByteArray() {
			List<byte> data = new List<byte>();

			//Add the name to the list
			data.AddRange(Encoding.ASCII.GetBytes(name));

			return data.ToArray();
		}
	}
}

