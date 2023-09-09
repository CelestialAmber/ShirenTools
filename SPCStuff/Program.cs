using System;
using System.Collections.Generic;

namespace SPCStuff {
	class Program {

		public static int[] offsets = {
0x320042,
0x3219B5,
0x322844,
0x3238AD,
0x3249BF,
0x325774,
0x3268E8,
0x327937,
0x3288CF,
0x329B45,
0x32AD2D,
0x32BF55,
0x32D001,
0x32E0E0,
0x32F2F3,
0x330434,
0x33158C,
0x332783,
0x333AF3,
0x334AD8,
0x335ACB,
0x336426,
0x33787D
};


		public static List<string> lines = new List<string>();

		public static void Main(string[] args) {
			byte[] fileBytes = File.ReadAllBytes("shiren.sfc");

			if (!Directory.Exists("out")) Directory.CreateDirectory("out");

			for (int i = 0; i < offsets.Length - 1; i++) {
				int length = offsets[i + 1] - offsets[i];
				int address = offsets[i];
				int snesAddress = address + 0xC00000;

				byte[] data = fileBytes.Skip(address).Take(length).ToArray();

				lines.Add(";" + snesAddress.ToString("x6"));
				lines.Add("Data_" + snesAddress.ToString("X6") + ":");

				string filename = snesAddress.ToString("x6") + ".bin";

				lines.Add(".incbin \"data/bank32/" + filename + "\"");
				lines.Add("\n");
				//PrintBytes(data);

				File.WriteAllBytes("out/" + filename, data);
				File.WriteAllLines("data.asm", lines.ToArray());
			}
		}

		public static ushort ReadUInt16(byte[] array, int offset) {
			byte lo = array[offset++];
			byte hi = array[offset];
			return (ushort)((hi << 8) + lo);
		}

		public static void PrintBytes(byte[] bytes) {
			int currentLineBytes = 0;
			int i = 0;
			string currentLine = ".db ";

			while (i < bytes.Length) {
				currentLine += "$" + bytes[i].ToString("X2");
				currentLineBytes++;

				if (i < bytes.Length - 1 && currentLineBytes < 16) {
					currentLine += ",";
				}

				if (i == bytes.Length - 1) {
					lines.Add(currentLine);
				} else {
					if (currentLineBytes == 16) {
						//Console.WriteLine(currentLine);
						lines.Add(currentLine);
						currentLine = ".db ";
						currentLineBytes = 0;
					}
				}

				i++;
			}

			//Console.WriteLine();
			lines.Add("\n");
		}
	}
}