using System;
using System.Collections.Generic;
using System.Text;

namespace PNGLib.Chunks
{
	public enum ColorType
	{
		Grayscale = 0,
		RGB = 2,
		Indexed = 3,
		GrayscaleAlpha = 4,
		RGBAlpha = 6
	}

	public class IHDRChunk : Chunk
	{
		public const string name = "IHDR";
        public int width;
		public int height;
		public byte bitDepth; //can be 1/2/4/8/16
        public ColorType colorType;
		public const byte compressionType = 0; //Always 0
		public const byte filterMethod = 0; //Always 0
		public byte interlaceMethod; //0: no interlacing, 1: Adam7 interlacing

        public IHDRChunk(int width, int height, byte bitDepth, ColorType colorType, byte interlaceMethod)
		{
			this.width = width;
			this.height = height;
			this.bitDepth = bitDepth;
			this.colorType = colorType;
			this.interlaceMethod = interlaceMethod;
		}

		public override int GetLength()
		{
			return 13; //IHDR is always 13 bytes long
		}

        public override byte[] ConvertMainDataToByteArray() {
			List<byte> data = new List<byte>();

            //Add the name to the list
            data.AddRange(Encoding.ASCII.GetBytes(name));
            //Add the main data to the list
            data.AddRange(width.ToByteArray()); //Width
            data.AddRange(height.ToByteArray()); //Height
            data.Add(bitDepth); //Bit depth
            data.Add((byte)(int)colorType); //Color type
            data.Add(compressionType); //Compression type
            data.Add(filterMethod); //Filter method
            data.Add(interlaceMethod); //Interlace method

			return data.ToArray();
        }

		public void PrintInfo()
		{
			Console.WriteLine("Width: " + width);
			Console.WriteLine("Height: " + height);
			Console.WriteLine("Bit depth: " + bitDepth);
			Console.WriteLine("Color type: " + colorType);
		}
    }
}

