using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaPNG.Filters {
	public class NONEFilter : Filter {

		public NONEFilter(byte[,] channel, int bitDepth) : base(channel, bitDepth) {
		}

		public override byte[] Encode() {
			List<byte> filteredImageData = new List<byte>();
			int height = channel.GetLength(1);

			for (int i = 0; i < height; i++) {
				//For the NONE filter, the data isn't modified at all, except for the filter type byte added at the start
				List<byte> lineData = PackLine(channel.GetRow(i));
				lineData.Insert(0, 0); //Add the filter type byte at the start of the line data
									   //Add the data to the filtered image data list
				filteredImageData.AddRange(lineData);
			}

			return filteredImageData.ToArray();
		}


	}
}

