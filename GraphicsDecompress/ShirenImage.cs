using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsDecompress
{

	/*Header format (1 byte, starting from leftmost bit):
	Bits 0-1: image type (2 bit, 0: 32x32 uncompressed, 1: 16x32 compressed, 2: 24x32 compressed, 3: 32x32 compressed)
	Bit 2: pixel offset direction (0: right, 1: left)
	Bit 3: unused
	Bits 4-7: pixel offset (4 bit)

	The direction is always 0 (right) if the pixel offset is 0
	*/

	public class ShirenImageHeader {
		public int headerByte;
		public int type;
		public int width, height;
		public bool compressed;
		public int pixelOffsetDirection;
		public int pixelOffset;

		public ShirenImageHeader(byte headerByte) {
			this.headerByte = headerByte;
			int type = headerByte >> 6;

			switch (type) {
				//32x32, uncompressed
				//Is this used?
				case 0:
					width = 32;
					height = 32;
					compressed = false;
					break;
				//16x32, compressed
				case 1:
					width = 16;
					height = 32;
					compressed = true;
					break;
				//24x32, compressed
				case 2:
					width = 24;
					height = 32;
					compressed = true;
					break;
				//32x32, compressed
				case 3:
					width = 32;
					height = 32;
					compressed = true;
					break;
			}

			pixelOffsetDirection = ((headerByte >> 5) & 1);
			pixelOffset = headerByte & 0xF;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Header information:\n");
			sb.Append("Size: " + width + "x" + height + "\n");
			sb.Append("Compressed: " + compressed + "\n");
			sb.Append("Offset direction: " + (pixelOffsetDirection == 0 ? "right" : "left") + "\n");
			sb.Append("Offset (pixels): " + pixelOffset + "\n");
			//sb.Append("Header byte: " + headerByte.ToString("X2") + "\n");
			sb.AppendLine();
			return sb.ToString();
		}
	}

	public class ShirenImage
	{

		public static int offset;

		/*
		Decompresses compressed graphics data (1bpp/4bpp)
		
		Format (1bpp):
		The graphics data starts with the header byte
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
		-for item graphics (weapons/shields/staffs), each block doesn't start with a header byte
		*/
		public static byte[] Decompress(byte[] data, int bitsPerPixel, bool hasHeaderByte) {
			offset = 0;
			bool debugInfo = false;

			if (hasHeaderByte) offset++;

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

								//In type 2, instead of following the standard SNES 4bpp format, the four bytes for each pixel are together
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
				Console.WriteLine("Failed to decompress file");
				return decompressedBytes.ToArray();
			}

			return decompressedBytes.ToArray();
		}

		public static byte ReadByte(byte[] array) {
			return array[offset++];
		}

		public static byte[] Compress(byte[] imageData, int bitDepth)
		{
			List<byte> compressedData = new List<byte>();
			return CompressMain(compressedData, imageData, bitDepth);
		}

		public static byte[] Compress(byte[] imageData, int bitDepth, byte headerByte) {
			List<byte> compressedData = new List<byte>();
			compressedData.Add(headerByte);
			return CompressMain(compressedData, imageData, bitDepth);
		}

		//TODO: add support for 1bpp images
		public static byte[] CompressMain(List<byte> compressedData, byte[] imageData, int bitDepth) {
			
			bool debugInfo = false;

			int tiles = imageData.Length / 32;

			if (bitDepth == 4) {
				//Compress tiles in groups of 4
				for (int i = 0; i < tiles; i += 4) {
					if (debugInfo) Console.WriteLine("Creating block " + (i / 4));
					byte groupByte = 0;
					List<byte> groupData = new List<byte>();

					//For each tile in the group, try each compression method and choose the best one
					for (int j = 0; j < 4; j++) {
						if (debugInfo) Console.WriteLine("Compressing tile " + j);

						byte[] tileData = imageData.Skip(32 * (i + j)).Take(32).ToArray();
						int minLength = 0;
						int type;
						List<byte> compressedTileData = new List<byte>();

						if (debugInfo) Print4bppTile(tileData);

						//Try all 4 methods, and choose the best

						//Check if the tile is blank (all 0 bytes)
						bool blank = true;

						foreach (byte b in tileData) {
							if (b != 0) {
								blank = false;
								break;
							}
						}

						//If the tile is blank, use type 1 (blank tile)
						if (blank) {
							if (debugInfo) Console.WriteLine("Tile is blank, using type 1");
							type = 1;
						} else {
							//Type 0 (uncompressed)
							compressedTileData = tileData.ToList();
							minLength = compressedTileData.Count;
							type = 0;

							if (debugInfo) Console.WriteLine("Type 0 size: " + tileData.Length);

							//Type 2 (both halves are combined, missing lines are blank)

							List<byte> tempTileData = new List<byte>();
							//Info value for types 2/3
							ushort tileInfoVal = 0;

							for (int k = 0; k < 8; k++) {
								int lineBit1; //1st line bit
								int lineBit2; //2nd line bit
								byte plane1Byte = tileData[k * 2];
								byte plane2Byte = tileData[k * 2 + 1];
								byte plane3Byte = tileData[k * 2 + 16];
								byte plane4Byte = tileData[k * 2 + 16 + 1];

								//Check the first/second half separately

								//If both bytes are 0, then the first line bit is 0 (blank line)
								if (plane1Byte == 0 && plane2Byte == 0) {
									lineBit1 = 0;
								} else {
									//Otherwise, add the first two bytes to the list
									lineBit1 = 1;
									tempTileData.Add(plane1Byte);
									tempTileData.Add(plane2Byte);
								}

								//If both bytes are 0, then the second line bit is 0 (blank line)
								if (plane3Byte == 0 && plane4Byte == 0) {
									lineBit2 = 0;
								} else {
									lineBit2 = 1;
									tempTileData.Add(plane3Byte);
									tempTileData.Add(plane4Byte);
								}

								//Write the two line bits to the info byte
								tileInfoVal |= (ushort)((lineBit2 << (k * 2 + 1)) | (lineBit1 << (k * 2)));
							}

							//Write the tile info value
							tempTileData.Insert(0, (byte)(tileInfoVal >> 8));
							tempTileData.Insert(0, (byte)(tileInfoVal & 0xFF));


							if (tempTileData.Count < minLength) {
								compressedTileData = tempTileData;
								minLength = tempTileData.Count;
								type = 2;
							}

							if (debugInfo) Console.WriteLine("Type 2 size: " + tempTileData.Count);

							//Type 3 (halves are kept separate, missing lines use last defined line (blank by default))

							List<byte> tempTileData1 = new List<byte>();
							tileInfoVal = 0;
							byte[] lastLineBytes = new byte[2];

							for (int k = 0; k < 16; k++) {
								int lineBit;
								byte plane1Byte = tileData[k * 2];
								byte plane2Byte = tileData[k * 2 + 1];

								//If both bytes are the same as the last bytes, then the line bit is 0
								if (plane1Byte == lastLineBytes[0] && plane2Byte == lastLineBytes[1]) {
									lineBit = 0;
								} else {
									//Otherwise, add the first two bytes to the list
									lineBit = 1;
									tempTileData1.Add(plane1Byte);
									tempTileData1.Add(plane2Byte);
								}

								lastLineBytes[0] = plane1Byte;
								lastLineBytes[1] = plane2Byte;

								//Write the line bit to the info byte
								tileInfoVal |= (ushort)(lineBit << k);
							}

							//Write the tile info value
							tempTileData1.Insert(0, (byte)(tileInfoVal >> 8));
							tempTileData1.Insert(0, (byte)(tileInfoVal & 0xFF));



							if (tempTileData1.Count < minLength) {
								compressedTileData = tempTileData1;
								minLength = tempTileData1.Count;
								type = 3;
							}

							if (debugInfo) Console.WriteLine("Type 3 size: " + tempTileData1.Count);
						}

						if (debugInfo) Console.WriteLine("Chose type " + type);
						groupData.AddRange(compressedTileData);
						//Write the current type to the group byte
						groupByte |= (byte)((type & 3) << (j * 2));
					}

					//Add the group byte
					groupData.Insert(0, groupByte);

					compressedData.AddRange(groupData);
				}
			} else {
				//handle 1bpp
			}

			return compressedData.ToArray();
		}

		static void Print4bppTile(byte[] data) {
			for (int y = 0; y < 8; y++) {
				for (int x = 0; x < 8; x++) {
					int bit0 = (data[y * 2] >> (7 - x)) & 1;
					int bit1 = (data[y * 2 + 1] >> (7 - x)) & 1;
					int bit2 = (data[y * 2 + 16] >> (7 - x)) & 1;
					int bit3 = (data[y * 2 + 16 + 1] >> (7 - x)) & 1;
					byte col = (byte)(bit0 + (bit1 << 1) + (bit2 << 2) + (bit3 << 3));
					Console.Write(col + " ");
				}
				Console.WriteLine();
			}
		}
	}
}

