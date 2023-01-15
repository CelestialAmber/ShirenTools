﻿using System;
using System.Collections.Generic;

namespace SPCStuff {
	class Program {
		public static int[] offsets = { 0x1F214F, 0x1F215F, 0x1F2949, 0x1F2BA4, 0x1F2DB1, 0x1F2F87, 0x1F2F93, 0x1F32EF, 0x1F32FD, 0x1F38D1, 0x1F38E1, 0x1F400D, 0x1F479F, 0x1F4C67, 0x1F5287, 0x1F56FA, 0x1F59B0, 0x1F5CC5, 0x1F61E4, 0x1F61F0, 0x1F6C1D, 0x1F6C29, 0x1F6FA5, 0x1F6FB3, 0x1F7709, 0x1F7DC8, 0x1F8668, 0x1F8676, 0x1F8686, 0x1F8AFD, 0x1F8B0B, 0x1F9400, 0x1F940C, 0x1F979B, 0x1F99D5, 0x1FA880, 0x1FB329, 0x1FBA7A, 0x1FC9E2, 0x1FCAB5, 0x1FCAC1, 0x1FCC8B, 0x1FCC93, 0x1FCC9B, 0x1FCD75, 0x1FCEE4, 0x1FCFA2, 0x1FD019, 0x1FD14C, 0x1FD152, 0x1FD158, 0x1FD1F0, 0x1FD1FA, 0x1FD304, 0x1FD3BA, 0x1FD41D, 0x1FD4C0, 0x1FD580, 0x1FD586, 0x1FD590, 0x1FD671, 0x1FD677, 0x1FD67B, 0x1FD67F, 0x1FD683, 0x1FD687, 0x1FD68B, 0x1FD68F, 0x1FD693, 0x1FD697, 0x1FD69B, 0x1FD7CC, 0x1FD7D0, 0x1FD7D4, 0x1FD7D8, 0x1FD83D, 0x1FD841, 0x1FD845, 0x1FD849, 0x1FD84D, 0x1FD851, 0x1FD855, 0x1FD859, 0x1FD85D, 0x1FD861, 0x1FD865, 0x1FD869, 0x1FD86D, 0x1FD871, 0x1FD875, 0x1FDA98, 0x1FDA9C, 0x1FDAA0, 0x1FDAA4, 0x1FDAA8, 0x1FDAAC, 0x1FDAB0, 0x1FDAB4, 0x1FDAB8, 0x1FDABC, 0x1FDAC0, 0x1FDAC4, 0x1FDAC8, 0x1FDACC, 0x1FDAD0, 0x1FDAD4, 0x1FDAD8, 0x1FDD15, 0x1FDD19, 0x1FDD1D, 0x1FDD7F, 0x1FDD83, 0x1FDD87, 0x1FDD8B, 0x1FDD8F, 0x1FDD93, 0x1FDD97, 0x1FDD9B, 0x1FDD9F, 0x1FDDA3, 0x1FDDA7, 0x1FDDAB, 0x1FDDAF, 0x1FDDB3, 0x1FDDB9, 0x1FDDBD, 0x1FDDC1, 0x1FDDC5, 0x1FDDC9, 0x1FDDCD, 0x1FDDD1, 0x1FDDD5, 0x1FDDD9, 0x1FDDDF, 0x1FDDE3, 0x1FDDE7, 0x1FDDEB, 0x1FDDF1, 0x1FDDF5, 0x1FDDF9, 0x1FDDFD, 0x1FDE01, 0x1FE3A6, 0x1FE3AA, 0x1FE3AE, 0x1FE3B2, 0x1FE3B6, 0x1FE3BA, 0x1FE3BE, 0x1FE3C2, 0x1FE3C6, 0x1FE3CA, 0x1FE3CE, 0x1FE3D2, 0x1FE3D6, 0x1FE3DA, 0x1FE3DE, 0x1FE3E2, 0x1FE3E6, 0x1FE3EA, 0x1FE3EE, 0x1FE3F2, 0x1FE3F6, 0x1FE3FA, 0x1FE3FE, 0x1FE402, 0x1FE408, 0x1FE826, 0x1FE82A, 0x1FE82E, 0x1FE834, 0x1FE838, 0x1FE83C, 0x1FE840, 0x1FE844, 0x1FE84A, 0x1FE84E, 0x1FE852, 0x1FE856, 0x1FE85A, 0x1FE85E, 0x1FE862, 0x1FE866, 0x1FE86A, 0x1FE86E, 0x1FE872, 0x1FE876, 0x1FE87A, 0x1FE87E, 0x1FE882, 0x1FE886, 0x1FECC7, 0x1FECCB, 0x1FECCF, 0x1FECD5, 0x1FECDB, 0x1FECE1, 0x1FECE7, 0x1FECEB, 0x1FEE74, 0x1FEEDA, 0x1FEEDE, 0x1FEEE4, 0x1FEEE8, 0x1FEEEC, 0x1FEEF2, 0x1FEEF6, 0x1FEEFA, 0x1FEEFE, 0x1FEF02, 0x1FEF06, 0x1FEF0A, 0x1FEF0E, 0x1FEF14, 0x1FEF18, 0x1FEF1C, 0x1FEF20, 0x1FEF24, 0x1FEF2A, 0x1FEF30, 0x1FEF36, 0x1FEF3A, 0x1FEF3E, 0x1FEF42, 0x1FEF48, 0x1FEF4C, 0x1FEF50, 0x1FEF54, 0x1FEF58, 0x1FEF5C, 0x1FEF60, 0x1FEF64, 0x1FEF68, 0x1FEF6C, 0x1FEF70, 0x1FEF74, 0x1FEF7A, 0x1FEF7E, 0x1FEF82, 0x1FEF86, 0x1FEF8A, 0x1FEF8E, 0x1FEF94, 0x1FEF9A, 0x1FEF9E, 0x1FEFA4, 0x1FEFA8, 0x1FEFAC, 0x1FEFB0, 0x1FF781, 0x1FF785, 0x1FF789, 0x1FF78D, 0x1FF793, 0x1FF797, 0x1FF79B, 0x1FF79F, 0x1FF7A3, 0x1FF7A9, 0x1FF7AD, 0x1FF7B1, 0x1FF7B5, 0x1FF7B9, 0x1FF7BD, 0x1FF7C1, 0x1FF7C5, 0x1FF7C9, 0x1FF7CF, 0x1FF7D3, 0x1FF7D7, 0x1FF7DD, 0x1FF7E1, 0x1FF7E7, 0x1FF7ED, 0x1FF7F1, 0x1FF7F7, 0x1FF7FB, 0x1FF7FF, 0x1FF803, 0x1FF807, 0x1FF80B, 0x1FFDB2, 0x1FFE8D, 0x1FFEC9, 0x1FFEF6, 0x1FFEFC, 0x1FFF46, 0x1FFF4A, 0x1FFF50, 0x1FFF56, 0x200000 };
		public static int[] weaponStaffOffsets = { 0x0064, 0x034C, 0x085A, 0x0B6C, 0x106A, 0x1264, 0x16A4, 0x190E, 0x1E0C, 0x2198, 0x26B6, 0x2996, 0x2EB4, 0x3294, 0x375A, 0x3AB2, 0x3EF2, 0x4208, 0x45F0, 0x4920, 0x4E2A, 0x524A, 0x5754, 0x5AD2, 0x5FF0, 0x6470, 0x697A, 0x6D5E, 0x7268, 0x7656, 0x7B26, 0x807E, 0x8558, 0x88C2, 0x8BBA };
		public static int[] shieldOffsets = { 0x8D0E, 0x8EB8, 0x90F2, 0x93D6, 0x9624, 0x98AA, 0x9BCA, 0x9F28, 0xA1F6, 0xA480, 0xA6C6, 0xA9BE, 0xACF4, 0xAF34, 0xB29E, 0xB5A8, 0xB8D6 };
		public static List<string> lines = new List<string>();

		public static void Main(string[] args) {
			byte[] fileBytes = File.ReadAllBytes("shiren.sfc");

			if (!Directory.Exists("out")) Directory.CreateDirectory("out");

			for (int i = 0; i < weaponStaffOffsets.Length - 1; i++) {
				int offset = weaponStaffOffsets[i] + 0x3B0000;
				int length = weaponStaffOffsets[i + 1] - weaponStaffOffsets[i];

				//Skip the first 5 bytes before each sample (header info)
				//byte[] brrFileData = fileBytes.Skip(offset + 5).Take(length - 5).ToArray();
				//byte[] headerBytes = fileBytes.Skip(offset).Take(5).ToArray();

				int address = offset;
				int snesAddress = address + 0xC00000;

				byte[] data = fileBytes.Skip(offset).Take(length).ToArray();

				//Console.WriteLine(";" + snesAddress.ToString("x6"));
				//Console.WriteLine("Data_" + snesAddress.ToString("x6") + ":");

				lines.Add(";" + snesAddress.ToString("x6"));
				lines.Add("Data_" + snesAddress.ToString("x6") + ":");

				string filename = address.ToString("X6") + ".4bpp.lz";

				lines.Add(".incbin \"gfx/items/" + filename + "\"");
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