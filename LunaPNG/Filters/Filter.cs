using System;
using System.Collections.Generic;

namespace PNGLib.Filters
{
	//Base class for PNG filters, which are applied before DEFLATE compression.
	public abstract class Filter
	{
		public byte[,] channel;
		public int bitDepth;

		public Filter(byte[,] channel, int bitDepth)
		{
			this.channel = channel;
			this.bitDepth = bitDepth;
		}
		//Applies the filter to the image data
		public abstract byte[] Encode();

		//Packs the given line into the minimum number of bytes.
		public List<byte> PackLine(byte[] lineData)
		{
			List<byte> packedLineData = new List<byte>();

			if (bitDepth == 4)
			{
				//Pack two pixels into every byte
				for (int i = 0; i < lineData.Length; i += 2)
				{
					//If there is only one pixel left, only pack that pixel
					byte currentByte = (byte)((lineData[i] << 4) + (i + 1 < lineData.Length ? lineData[i + 1] : 0));
					packedLineData.Add(currentByte);
				}
			}
			else
			{
                throw new Exception("Error: Only 4bpp is supported for now");
            }

			return packedLineData;
		}
	}
}

