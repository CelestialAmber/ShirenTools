using System;
using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using LunaPNG.Utility;
using LunaPNG.Chunks;
using LunaPNG.Filters;

namespace LunaPNG {

	public class PNGEncoder {
		//Converts an indexed image to PNG, and returns the data.
		//Only 4bpp indexed images are supported.
		public static byte[] EncodeIndexedImageToPNG(int width, int height, byte bitDepth, byte[,] imageData, Color[] palette) {
			//Check if the image data is valid
			switch (bitDepth) {
				case 4:
					if (palette.Length != 16) throw new Exception("Error: Palette size should be 16 for 4bpp images");
					break;
				default:
					throw new NotImplementedException("Bit depths other than 4 aren't supported");
			}

			PNG png = new PNG();
			IHDRChunk ihdrChunk = new IHDRChunk(width, height, bitDepth, ColorType.Indexed, 0);
			PLTEChunk plteChunk = new PLTEChunk(palette);
			//Filter and compress the image data before putting it into an IDAT chunk


			NONEFilter filter = new NONEFilter(imageData, bitDepth);
			//Apply the filter to the image
			byte[] filteredData = filter.Encode();
			List<byte> compressedData = new List<byte>();

			compressedData.AddRange(new byte[] { 0x78, 0xDA }); //Add the Zlib compression header
			compressedData.AddRange(Compress(filteredData)); //Compress the filtered image data, and add it to the list
															 //Calculate the adler32 checksum of the image data
			Adler32 adler = new Adler32();
			uint checksum = adler.Compute(filteredData);
			compressedData.AddRange(checksum.ToByteArray());

			IDATChunk idatChunk = new IDATChunk(compressedData.ToArray());
			IENDChunk iendChunk = new IENDChunk();

			png.ihdrChunk = ihdrChunk;
			png.plteChunk = plteChunk;
			png.idatChunks = new List<IDATChunk>() { idatChunk };
			png.iendChunk = iendChunk;

			//Get the PNG image bytes and return them
			return png.ToByteArray();
		}

		static byte[] Compress(byte[] data) {
			MemoryStream ms = new MemoryStream();
			using (DeflateStream deflateStream = new DeflateStream(ms, CompressionMode.Compress)) {
				deflateStream.Write(data, 0, data.Length);
			}

			//Return the compressed data from the memory stream
			return ms.ToArray();
		}

		static byte[] Decompress(byte[] data) {
			MemoryStream compressedStream = new MemoryStream(data);
			MemoryStream decompressedStream = new MemoryStream();
			using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress)) {
				deflateStream.CopyTo(decompressedStream);
			}

			//Return the compressed data from the memory stream
			return decompressedStream.ToArray();
		}
	}

}

