using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LunaPNG.Utility;
using GraphicsDecompress.Utils;

namespace GraphicsDecompress {

	public class Program {
		public static List<Color[]> palettes = new List<Color[]>();

		public static Color[] grayscalePalette = {
			new Color(0,0,0), new Color(16,16,16), new Color(32,32,32), new Color(48,48,48),
			new Color(64,64,64), new Color(80,80,80), new Color(96,96,96), new Color(112,112,112),
			new Color(128,128,128), new Color(144,144,144), new Color(160,160,160), new Color(176,176,176),
			new Color(190,190,190), new Color(206,206,206), new Color(220,220,220), new Color(236,236,236)
		};

		public static Color[] grayscalePalette1bpp = {
			new Color(0,0,0), new Color(255, 255, 255)
		};

		public static Color[] grayscalePalette2bpp = {
			new Color(0,0,0), new Color(80,80,80), new Color(160,160,160), new Color(255, 255, 255)
		};

		public static bool noHeader = false;
		public static int optionWidth = -1;
		public static int optionBpp = -1;
		public static Pattern patternToUse = Pattern.Horizontal;

		public static void Main(string[] args) {

			if (args.Length < 2)
			{
				Console.WriteLine("Usage: GraphicsDecompress command path [-noheader] [-w[num]] [-v] [-bpp[1/2/4]");
				Console.WriteLine("Commands:\ncompress - Compresses a 4bpp file.\ndecompress - Decompresses a 4bpp.lz file\npng - Converts a 1/2/4bpp file to png");
				Console.WriteLine("Optional arguments:");
			}else{
				LoadSNESPaletteData();

				for(int i = 2; i < args.Length; i++){
					if (args[i] == "-noheader") noHeader = true;
					else if (args[i].StartsWith("-w")) optionWidth = int.Parse(args[i].Replace("-w", ""));
					else if (args[i] == "-v") patternToUse = Pattern.Vertical;
					else if (args[i].StartsWith("-bpp")) optionBpp = int.Parse(args[i].Replace("-bpp", ""));
				}

				int bpp = 4;

				if (optionBpp != -1) bpp = optionBpp;

				string command = args[0];

				switch (command) {
					case "compress":
						if (Directory.Exists(args[1]))
						{
							string[] extensions = { ".1bpp.lz", ".2bpp.lz", ".4bpp.lz" };
							foreach (string file in Directory.EnumerateFiles(args[1], "*.*", SearchOption.AllDirectories).Where(s => extensions.Any(ext => s.EndsWith(ext)))){
								CompressFile(file, !noHeader);
							}
						}
						else
						{
							CompressFile(args[1], !noHeader);
						}
					break;
					case "decompress":
						if (Directory.Exists(args[1])) {
							string[] extensions = { ".1bpp.lz", ".2bpp.lz", ".4bpp.lz" };
							foreach (string file in Directory.EnumerateFiles(args[1], "*.*", SearchOption.AllDirectories).Where(s => extensions.Any(ext => s.EndsWith(ext)))) {
								DecompressFile(file,!noHeader,patternToUse);
							}
						} else {
							DecompressFile(args[1], !noHeader, patternToUse);
						}
					break;
					case "png":
						if (Directory.Exists(args[1]))
						{
							foreach (string file in Directory.EnumerateFiles(args[1], "*.4bpp", SearchOption.AllDirectories))
							{
								ConvertToPng(file);
							}
						}
						else
						{
							ConvertToPng(args[1]);
						}
						break;
				}
			}

		}

		public static void ConvertToPng(string file)
		{
			byte[] data = File.ReadAllBytes(file);
			if (optionWidth == -1) throw new Exception("Error: Width not specified.");
			int height = (data.Length * 2) / optionWidth;
			int bitDepth = DetermineBPPFromExtension(file);
			string path = Path.ChangeExtension(file, ".png");
			ConvertToImage(data, optionWidth, patternToUse, bitDepth).Save(path);
		}

		public static void CompressFile(string file, bool hasHeaderByte)
		{
			byte[] data = File.ReadAllBytes(file);
			byte[] compressedData;
			int bitDepth = DetermineBPPFromExtension(file);

			if (hasHeaderByte) {
				string filename = file.Replace(".4bpp", "").Replace(".2bpp", "").Replace(".1bpp", "");
				ShirenImageHeader header = new ShirenImageHeader(filename, data.Length);
				compressedData = ShirenImage.Compress(data, bitDepth, true, header.CalculateHeaderByte());
			}else{
				compressedData = ShirenImage.Compress(data, bitDepth, false);
			}

			File.WriteAllBytes(file + ".lz", compressedData);
		}


		public static void DecompressFile(string file, bool hasHeaderByte, Pattern pattern, int defaultWidth = 32) {
			//Remove the file extension from the path
			string filename = Path.GetFileNameWithoutExtension(file).Replace(".4bpp", "").Replace(".2bpp", "").Replace(".1bpp", "");
			string? basePath = Path.GetDirectoryName(file);
			string newFilename = filename;
			byte[] data = File.ReadAllBytes(file);
			int width = defaultWidth, height = 0;
			int bpp = (optionBpp != -1) ? optionBpp : DetermineBPPFromExtension(file);

			if (hasHeaderByte)
			{
				if(bpp == 1) {
					//Kointai graphics
					byte headerByte = data[0];
					width = (headerByte + 1) * 8;
				} else if (bpp == 4) {
					byte headerByte = data[0];
					ShirenImageHeader header = new ShirenImageHeader(headerByte);

					//If the pixel offset isn't zero, save the pixel offset information to the file name
					if (header.pixelOffset != 0) newFilename += "." + (header.pixelOffsetDirection == 0 ? "right" : "left") + header.pixelOffset;
					if (header.unk1) newFilename += ".unkflag";

					width = header.width;
					height = header.height;
				}
			}

			byte[] decompressedData = ShirenImage.Decompress(data, bpp, hasHeaderByte);

			//If the file doesn't have a header byte or the file is 1bpp and has one, derive the height manually
			if (!hasHeaderByte || (hasHeaderByte && bpp == 1)) height = (decompressedData.Length*2)/width;

			if (basePath != "") basePath += "/";

			string extension = bpp == 1 ? ".1bpp" : bpp == 2 ? ".2bpp" : ".4bpp";

			File.WriteAllBytes(basePath + newFilename + extension, decompressedData);

			if (optionWidth != -1) width = optionWidth;

			//Tiles in most compressed 4bpp graphics are arranged vertically
			IndexedBitmap bitmap = ConvertToImage(decompressedData.ToArray(), width, pattern, bpp);
			bitmap.Save(basePath + newFilename + ".png");
			
		}

		public static int DetermineBPPFromExtension(string file) {
			if (file.Contains(".1bpp")) return 1;
			else if (file.Contains(".2bpp")) return 2;
			else if (file.Contains(".4bpp")) return 4;
			return 4;
		}

		public enum Pattern {
			Horizontal,
			Vertical
		}

		//Convert the decompressed graphics data to an image.
		public static IndexedBitmap ConvertToImage(byte[] data, int width, Pattern pattern, int bitDepth) {
			int bytesPerTile = (bitDepth * 8);
			int tiles = data.Length / bytesPerTile;
			int tileWidth = width / 8;
			int tileHeight = (int)Math.Ceiling((float)tiles / (float)tileWidth);
			int height = tileHeight * 8;

			Color[] palette = bitDepth == 1 ? grayscalePalette1bpp : bitDepth == 2 ? grayscalePalette2bpp : grayscalePalette;

			IndexedBitmap bitmap = new IndexedBitmap(width, height, bitDepth, palette);

			for (int i = 0; i < tiles; i++) {
				//Calculate the current tile offset
				int tileOffset = bytesPerTile * i;


				for (int x = 0; x < 8; x++) {
					for (int y = 0; y < 8; y++) {
						byte col = 0;

						if (bitDepth == 1) {
							int tileByteIndex = y;
							col = (byte)((data[tileOffset + tileByteIndex] >> (7 - x)) & 1);
						}else if(bitDepth == 2) {
							int tileByteIndex = y * 2;
							int bit0 = (data[tileOffset + tileByteIndex] >> (7 - x)) & 1;
							int bit1 = (data[tileOffset + tileByteIndex + 1] >> (7 - x)) & 1;
							col = (byte)(bit0 + (bit1 << 1));
						} else if (bitDepth == 4) {
							int tileByteIndex = y * 2;
							//Bottom 2bpp plane
							int bit0 = (data[tileOffset + tileByteIndex] >> (7 - x)) & 1;
							int bit1 = (data[tileOffset + tileByteIndex + 1] >> (7 - x)) & 1;
							//Top 2bpp plane
							int bit2 = (data[tileOffset + tileByteIndex + 16] >> (7 - x)) & 1;
							int bit3 = (data[tileOffset + tileByteIndex + 16 + 1] >> (7 - x)) & 1;
							col = (byte)(bit0 + (bit1 << 1) + (bit2 << 2) + (bit3 << 3));
						}


						if (pattern == Pattern.Vertical) { //Vertical
							int tileX = 8 * (i / tileHeight);
							int tileY = 8 * (i % tileHeight);
							bitmap.SetPixel(tileX + x, tileY + y, col);
						} else if (pattern == Pattern.Horizontal) { //Horizontal
							int tileX = 8 * (i % tileWidth);
							int tileY = 8 * (i / tileWidth);
							bitmap.SetPixel(tileX + x, tileY + y, col);
						}
					}
				}
			}

			return bitmap;
		}


		public static void LoadSNESPaletteData() {
			string applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
			byte[] data = File.ReadAllBytes(applicationDirectory + "/palettes.pal");

			for (int i = 0; i < 16; i++) {
				Color[] newPallete = new Color[16];
				for (int j = 0; j < 32; j += 2) {
					//SNES color format: -bbbbbgg gggrrrrr (reverse order)

					ushort colVal = (ushort)((data[(i * 32) + j + 1] << 8) + data[(i * 32) + j]);

					//Scale the values to be between 0-255
					byte r = (byte)(255f * ((float)(colVal & 0x1F) / 31f));
					byte g = (byte)(255f * ((float)((colVal >> 5) & 0x1F) / 31f));
					byte b = (byte)(255f * ((float)((colVal >> 10) & 0x1F) / 31f));
					//Add the color to the palette
					newPallete[j / 2] = new Color(r, g, b);
				}

				palettes.Add(newPallete);
			}
		}

	}

}

