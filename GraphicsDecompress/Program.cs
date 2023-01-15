using System;
using System.Collections.Generic;
using System.IO;
using PNGLib.Utility;

namespace GraphicsDecompress {
	public class Program {
		public static int offset;
		public static string currentFilename = "";


		public static List<Color[]> palettes = new List<Color[]>();

		public static Color[] grayscalePalette = {
			new Color(0,0,0), new Color(16,16,16), new Color(32,32,32), new Color(48,48,48),
			new Color(64,64,64), new Color(80,80,80), new Color(96,96,96), new Color(112,112,112),
			new Color(128,128,128), new Color(144,144,144), new Color(160,160,160), new Color(176,176,176),
			new Color(190,190,190), new Color(206,206,206), new Color(220,220,220), new Color(236,236,236)
		};



		public static void Main(string[] args) {
			LoadSNESPaletteData();

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



			DecompressFolders();
		}

		public static void DecompressFolders() {
			foreach (string file in Directory.EnumerateFiles("charactersprites", "*.4bpp.lz", SearchOption.AllDirectories)) {
				DecompressFile(file, true, Pattern.Vertical, 32);
			}

			/*
			foreach (string file in Directory.EnumerateFiles("items", "*.4bpp.lz", SearchOption.AllDirectories)) {
				DecompressFile(file, false, Pattern.Horizontal, 128);
			}*/

		}

		public static void DecompressFile(string file, bool hasWidthByte, Pattern pattern, int size) {
			byte[] data = File.ReadAllBytes(file);
			currentFilename = file;
			//Console.WriteLine("Decompressing " + file);

			int width = 0;

			if (hasWidthByte) {
				width = (data[0] + 1) * 8; //image width = (width + 1)*8
			}

			List<byte> decompressedData = Decode(data, 4, hasWidthByte);
			if (decompressedData != null) {
				string filename = file.Remove(file.IndexOf(".")); //remove the extension
				File.WriteAllBytes(filename + ".4bpp", decompressedData.ToArray());

				//Tiles in most compressed 4bpp graphics are arranged vertically
				IndexedBitmap bitmap = ConvertToImage(decompressedData.ToArray(), size, pattern);
				bitmap.Save(filename + ".png");
			}
		}

		/*
		Decodes compressed graphics data (1bpp/4bpp)
		
		Format (1bpp):
		The graphics data starts with a byte for width
		width = (value + 1) * 8
		The actual graphics data comes after
		The data is split into groups of 4 tiles(uncompressed/compressed)
		The tiles are ordered from left to right, top down
		
		Format:
		0x00: group block types(bits 0-1: block 1, bits 2-3: block 2, etc)
		ex: 11001010 -> block 1: 10, block 2: 10, block 3: 00, block 4: 11
		0x01-: group block data

		Block Format:
		Type 0: uncompressed(8 bytes for the tile)
		Type 1: black tile(eight 0 bytes)
		Type 2:
		Format:
		0: info byte (bits 0-7 correspond to lines 1-8 in the tile)
		For every “1” bit there will be a byte for the corresponding line
		Otherwise, there is no byte for the line, and the line is black
		Type 3: same as type 2, but missing lines repeat the last line(default is a black line)


		For 4bpp graphics, the format is mostly the same except for the following differences:
		-each block instead has 32 bytes (8*4 bytes for the 4 bit planes for each tile)
		-types 2 and 3 instead have 2 bytes representing which lines are specified/which aren’t
		-each bit represents 2 lines (bit 0 = lines 1-2, etc)
		-for item graphics (weapons/shields/staffs), each block doesn't start with a width byte
		*/
		public static List<byte> Decode(byte[] data, int bitsPerPixel, bool hasWidthByte) {
			offset = 0;
			bool debugInfo = false;

			if (hasWidthByte) offset++;

			List<byte> decompressedBytes = new List<byte>();

			try {

				while (offset < data.Length) {
					byte groupTypeByte = ReadByte(data);

					//Each group has 4 blocks (tiles) that can be stored in 4 ways
					for (int i = 0; i < 4; i++) {
						//Get the type of the current block
						int blockType = (groupTypeByte >> (i * 2)) & 0x3;

						if (debugInfo) Console.WriteLine("Block type:" + blockType + "(group byte = " + groupTypeByte.ToString("X1") + ")");

						if (blockType == 0) {
							//Uncompressed

							if (bitsPerPixel == 1) {
								for (int j = 0; j < 8; j++) {
									decompressedBytes.Add(ReadByte(data));
								}
							} else if (bitsPerPixel == 4) {
								for (int j = 0; j < 32; j++) {
									decompressedBytes.Add(ReadByte(data));
								}
							}
						} else if (blockType == 1) {
							//Black tile

							if (bitsPerPixel == 1) {
								//Add eight 0 bytes to the list
								for (int j = 0; j < 8; j++) {
									decompressedBytes.Add(0);
								}
							} else if (bitsPerPixel == 4) {
								//Add 32 0 bytes
								for (int j = 0; j < 32; j++) {
									decompressedBytes.Add(0);
								}
							}
						} else if (blockType == 2 || blockType == 3) {
							//Block types 2 and 3 have an extra byte at the start representing which lines are specified
							//The lowest bit represents line 1, and the highest represents line 8
							//For type 2, unspecified lines are black, whereas in type 3 it repeats the last used line (starts as a black line)
							//For 4bpp graphics, there are 2 bytes instead (little endian)

							if (bitsPerPixel == 1) {
								//1bpp
								byte tileByte = ReadByte(data);
								byte lastLineByte = 0; //Starts at 0 for type 3 (black line)

								for (int j = 0; j < 8; j++) {
									int bit = tileByte & 0x1; //Get the next bit
									if (bit == 0) {
										if (blockType == 2) {
											//If type 2, add a 0 byte (black line)
											decompressedBytes.Add(0);
										} else {
											//If type 3, repeat the last line byte
											decompressedBytes.Add(lastLineByte);
										}

									} else {
										//Read a byte for the current line
										byte b = ReadByte(data);
										if (debugInfo) Console.WriteLine("Byte: " + b.ToString("X1"));

										decompressedBytes.Add(b);

										if (blockType == 3) {
											//If type 3, update the last specified byte to the current one
											lastLineByte = b;
										}
									}

									tileByte >>= 1; //Go to the next bit
								}

							} else if (bitsPerPixel == 4) {
								//4bpp
								//Each bit represents 2 lines
								byte tileByte1 = ReadByte(data);
								byte tileByte2 = ReadByte(data);
								bool alternatingBytes = true; //whether the bytes in the block alternate between the two halves or not
								ushort tileLinesVal = (ushort)((tileByte2 << 8) + tileByte1);
								byte lastLineByte1 = 0, lastLineByte2 = 0; //Starts at 0 for type 3 (black line)

								if (debugInfo) Console.WriteLine("Tile lines value: " + tileLinesVal.ToString("X4"));

								//For some reason, in type 2, the bytes alternate between the first and last 16 bytes every 2 bytes
								if (blockType == 2) {
									int currentHalf = 0;
									int startIndex = decompressedBytes.Count; //used to keep track of where to add the first half bytes

									for (int j = 0; j < 16; j++) {
										int bit = tileLinesVal & 0x1; //Get the next bit

										if (bit == 0) {
											//Add a 0 byte for each line

											if (alternatingBytes) {
												//If the bytes in the block alternate between halves, add them at the right index
												if (currentHalf == 0) {
													//If the bytes are part of the first half, add them at the right index
													decompressedBytes.Insert(startIndex + j, 0);
													decompressedBytes.Insert(startIndex + j + 1, 0);
												} else {
													//If the bytes are part of the second half, directly add them
													decompressedBytes.Add(0);
													decompressedBytes.Add(0);
												}
											} else {
												//If not, just directly add the bytes to the list
												decompressedBytes.Add(0);
												decompressedBytes.Add(0);
											}
										} else {
											//Read 2 byte for the current 2 lines
											byte line1Byte = ReadByte(data), line2Byte = ReadByte(data);

											if (alternatingBytes) {
												//If the bytes in the block alternate between halves, add them at the right index
												if (currentHalf == 0) {
													//If the bytes are part of the first half, add them at the right index
													decompressedBytes.Insert(startIndex + j, line1Byte);
													decompressedBytes.Insert(startIndex + j + 1, line2Byte);
												} else {
													//If the bytes are part of the second half, directly add them
													decompressedBytes.Add(line1Byte);
													decompressedBytes.Add(line2Byte);
												}
											} else {
												//If not, just directly add the bytes to the list
												decompressedBytes.Add(line1Byte);
												decompressedBytes.Add(line2Byte);
											}
										}

										tileLinesVal >>= 1; //Go to the next bit
										currentHalf = 1 - currentHalf; //Switch to the other half
									}
								} else {
									for (int j = 0; j < 16; j++) {
										int bit = tileLinesVal & 0x1; //Get the next bit

										if (bit == 0) {
											//If type 3, repeat the last 2 line bytes
											decompressedBytes.Add(lastLineByte1);
											decompressedBytes.Add(lastLineByte2);
										} else {
											//Read 2 byte for the current 2 lines
											byte line1Byte = ReadByte(data), line2Byte = ReadByte(data);
											decompressedBytes.Add(line1Byte);
											decompressedBytes.Add(line2Byte);

											//update the last specified byte to the current one
											lastLineByte1 = line1Byte;
											lastLineByte2 = line2Byte;
										}

										tileLinesVal >>= 1; //Go to the next bit
									}
								}
							}


						}
					}
				}

			}
			catch (Exception e) {
				Console.WriteLine("Failed to decompress " + currentFilename);
				return decompressedBytes;
			}

			return decompressedBytes;
		}

		public static byte ReadByte(byte[] array) {
			return array[offset++];
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

