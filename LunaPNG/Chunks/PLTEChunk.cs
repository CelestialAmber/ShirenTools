using System;
using System.Collections.Generic;
using System.Text;
using PNGLib.Utility;

namespace PNGLib.Chunks
{
    public class PLTEChunk : Chunk
	{

        public const string name = "PLTE";
		public Color[] palette;

        public PLTEChunk(Color[] palette)
		{
			this.palette = palette;
		}

        public override int GetLength() {
            return palette.Length * 3; //3 bytes per color
        }

        public override byte[] ConvertMainDataToByteArray() {
            List<byte> data = new List<byte>();

            //Add the name to the list
            data.AddRange(Encoding.ASCII.GetBytes(name));

            //Add the color palette data
            foreach(Color col in palette)
            {
                data.Add(col.r);
                data.Add(col.g);
                data.Add(col.b);
            }

            return data.ToArray();
        }
    }
}

