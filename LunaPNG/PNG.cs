using System;
using System.Collections.Generic;
using PNGLib.Chunks;

namespace PNGLib {
	/*
	PNG Format:

	Header: 89 50 4E 47 0D 0A 1A 0A (504E47 = "PNG")

	Chunk format:
	0x0: length (32 bit)
	0x4: chunk name (4 bytes)
	0x8: chunk data
	Checksum (4 bytes, calculated over name and data)

	Chunks:
	IHDR (first chunk):
	Format:
	0x0: width (32 bit)
	0x4: height (32 bit)
	0x8: bit depth (1 byte, can be 1/2/4/8/16)
	0x9: color type (1 byte, can be 0/2/3/4/6)
	0xA: compression type (1 byte, 0)
	0xB: filter method (1 byte, 0)
	0xC: interlace method (1 byte, 0: no interlace, 1: Adam7 interlace)

	Color types:
	0: Grayscale (bit depth: 1/2/4/8/16)
	2: RGB (bit depth: 8/16)
	3: indexed (requires a PLTE chunk, bit depth: 1/2/4/8)
	4: Grayscale + alpha (bit depth: 8/16)
	6: RGB + alpha (bit depth: 8/16)

	PLTE (palette):
	Format:
	0x0: color list (each color is a 24 bit RGB color, 256 colors max)

	IDAT (image data):
	-Image data can be split across multiple IDAT chunks in order

	IEND (image end):
	-Data field is empty
	*/

	//Only supports 4bpp indexed images
	public class PNG {
		//Header data
		static byte[] header = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
		public IHDRChunk ihdrChunk;
		public PLTEChunk plteChunk;
		public List<IDATChunk> idatChunks;
		public IENDChunk iendChunk;



		public PNG() {
		}

		public byte[] ToByteArray() {
			List<byte> data = new List<byte>();

			//Add the header to the list
			data.AddRange(header);
			//Add the different chunks to the list in order

			//IHDR chunk
			data.AddRange(ihdrChunk.ToByteArray());
			//PLTE chunk
			data.AddRange(plteChunk.ToByteArray());
			//IDAT chunks
			foreach (IDATChunk chunk in idatChunks) {
				data.AddRange(chunk.ToByteArray());
			}
			//IEND chunk
			data.AddRange(iendChunk.ToByteArray());

			return data.ToArray();
		}
	}
}

