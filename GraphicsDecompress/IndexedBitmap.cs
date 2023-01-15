using System;
using System.IO;
using PNGLib;
using PNGLib.Utility;

namespace GraphicsDecompress {
	public class IndexedBitmap {
		Color[] palette;
		byte[,] pixels;
		public int width;
		public int height;
		public int bitDepth;

		public IndexedBitmap(int width, int height, int bitDepth, Color[] palette) {
			pixels = new byte[width, height];
			this.palette = palette;
			this.width = width;
			this.height = height;
			this.bitDepth = bitDepth;
		}


		public void SetPixel(int x, int y, byte index) {
			pixels[x, y] = index;
		}

		public void Save(string path) {
			File.WriteAllBytes(path, EncodeAsPNG());
		}

		public byte[] EncodeAsPNG() {
			return PNGEncoder.EncodeIndexedImageToPNG(width, height, (byte)bitDepth, pixels, palette);
		}
	}
}

