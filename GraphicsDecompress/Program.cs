using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LunaPNG.Utility;
using GraphicsDecompress.Utils;

namespace GraphicsDecompress {

	public class Program {
		public static int totalFiles, mismatchedFiles;
		public static Logger logger;


		public static List<Color[]> palettes = new List<Color[]>();

		public static Color[] grayscalePalette = {
			new Color(0,0,0), new Color(16,16,16), new Color(32,32,32), new Color(48,48,48),
			new Color(64,64,64), new Color(80,80,80), new Color(96,96,96), new Color(112,112,112),
			new Color(128,128,128), new Color(144,144,144), new Color(160,160,160), new Color(176,176,176),
			new Color(190,190,190), new Color(206,206,206), new Color(220,220,220), new Color(236,236,236)
		};



		public static void Main(string[] args) {
			DecompressFolders();
		}

		public static void DecompressFolders() {
			LoadSNESPaletteData();
			logger = new Logger("fileHeaders.txt");
			logger.ClearFile();

			/* //Convert uncompressed 4bpp files
			 foreach (string file in Directory.EnumerateFiles("charactersprites/shiren", "*.4bpp", SearchOption.AllDirectories)) {
				 byte[] data = File.ReadAllBytes(file);
				 string filename = file.Remove(file.IndexOf(".")); //remove the extension
				 //Uncompressed character graphics (Shiren) store tiles in a weird horizontal order instead of vertical
				 //The width is 32, but I don't feel like dealing with the dumb tile pattern so I'm treating it as 64 for now (matches how it is in VRAM anyway)
				 //Row order: 1 3 2 4
				 SKBitmap bitmap = ConvertToImage(data, 64, Pattern.Horizontal);
				 SaveImageAsPng(filename + ".png", bitmap);
			 }*/


			foreach (string file in Directory.EnumerateFiles("characters", "*.4bpp.lz", SearchOption.AllDirectories)) {
				DecompressFile(file, true, Pattern.Vertical, 32);
			}

			//Console.WriteLine("Non-matching files: " + mismatchedFiles + "/" + totalFiles);

			/*
			foreach (string file in Directory.EnumerateFiles("items", "*.4bpp.lz", SearchOption.AllDirectories)) {
				DecompressFile(file, false, Pattern.Horizontal, 128);
			}*/

		}

		public static void DecompressFile(string file, bool hasHeaderByte, Pattern pattern, int size) {
			string filename = file.Remove(file.IndexOf(".")); //remove the extension
			string newFilename = filename;

			if (!filename.Contains("_recompressed"))
			{
				byte[] data = File.ReadAllBytes(file);

				//Console.WriteLine("Decompressing " + file);

				byte headerByte = 0;

				ShirenImageHeader header;

				if (hasHeaderByte)
				{
					headerByte = data[0];
					header = new ShirenImageHeader(headerByte);

					//If the pixel offset isn't zero, save the pixel offset information to the file name
					//TODO: Find a better way of handling this
					if (header.pixelOffset != 0)
					{
						newFilename += "." + (header.pixelOffsetDirection == 0 ? "right" : "left") + header.pixelOffset;
					}
				}

				byte[] decompressedData = ShirenImage.Decompress(data, 4, hasHeaderByte);
				
				File.WriteAllBytes(newFilename + ".4bpp", decompressedData);

				/*
				//Recompress the data for testing
				byte[] recompressedData;
				if(hasHeaderByte) recompressedData = ShirenImage.Compress(decompressedData, 4, headerByte);
				else recompressedData = ShirenImage.Compress(decompressedData, 4);

				File.WriteAllBytes(filename + "_recompressed.4bpp.lz", recompressedData);

				for (int i = 0; i < data.Length; i++) {
					if (data[i] != recompressedData[i]) {
						Console.WriteLine("Recompressed data does not match");
						mismatchedFiles++;
						break;
					}
				}
				*/

				//Tiles in most compressed 4bpp graphics are arranged vertically
				IndexedBitmap bitmap = ConvertToImage(decompressedData.ToArray(), size, pattern);
				bitmap.Save(newFilename + ".png");

				//totalFiles++;
			}
		}

		public enum Pattern {
			Horizontal,
			Vertical
		}

		//Convert the decompressed graphics data to an image.
		//TODO: add support for 1bpp images
		public static IndexedBitmap ConvertToImage(byte[] data, int size, Pattern pattern) {
			int tiles = data.Length / 32;
			int tileWidth = 0;
			int tileHeight = 0;

			if (pattern == Pattern.Horizontal) {
				tileWidth = size / 8;
				tileHeight = (int)Math.Ceiling((float)tiles / (float)tileWidth);
			} else if (pattern == Pattern.Vertical) {
				tileHeight = size / 8;
				tileWidth = (int)Math.Ceiling((float)tiles / (float)tileHeight);
			}

			int width = tileWidth * 8;
			int height = tileHeight * 8;
			IndexedBitmap bitmap = new IndexedBitmap(width, height, 4, palettes[11]);

			for (int i = 0; i < tiles; i++) {
				//Calculate the current tile offset
				int tileOffset = 32 * i;


				for (int x = 0; x < 8; x++) {
					for (int y = 0; y < 8; y++) {
						int tileByteIndex = y * 2;
						//Bottom 2bpp plane
						int bit0 = (data[tileOffset + tileByteIndex] >> (7 - x)) & 1;
						int bit1 = (data[tileOffset + tileByteIndex + 1] >> (7 - x)) & 1;
						//Top 2bpp plane
						int bit2 = (data[tileOffset + tileByteIndex + 16] >> (7 - x)) & 1;
						int bit3 = (data[tileOffset + tileByteIndex + 16 + 1] >> (7 - x)) & 1;
						byte col = (byte)(bit0 + (bit1 << 1) + (bit2 << 2) + (bit3 << 3));


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
			byte[] data = File.ReadAllBytes("palettes.pal");


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

