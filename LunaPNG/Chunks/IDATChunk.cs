using System;
using System.Collections.Generic;
using System.Text;

namespace LunaPNG.Chunks {
	public class IDATChunk : Chunk {
		public const string name = "IDAT";
		public byte[] imageData;

		public IDATChunk(byte[] imageData) {
			this.imageData = imageData;
		}

		public override int GetLength() {
			return imageData.Length;
		}

		public override byte[] ConvertMainDataToByteArray() {
			List<byte> data = new List<byte>();

			//Add the name to the list
			data.AddRange(Encoding.ASCII.GetBytes(name));
			//Add the image data
			data.AddRange(imageData);

			return data.ToArray();
		}
	}
}

