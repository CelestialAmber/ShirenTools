using System;
using System.Collections.Generic;
using LunaPNG.Chunks;

namespace LunaPNG {
	/*
	PNG Format:

	Header: 89 50 4E 47 0D 0A 1A 0A (504E47 = "PNG")

	Chunk format:
	0x0: length (32 bit)
	0x4: chunk name (4 bytes)
	0x8: chunk data
	Checksum (4 bytes, calculated over name and data)

	Chunks:
	IHDR (first chunk):
	Format:
	0x0: width (32 bit)
	0x4: height (32 bit)
	0x8: bit depth (1 byte, can be 1/2/4/8/16)
	0x9: color type (1 byte, can be 0/2/3/4/6)
	0xA: compression type (1 byte, 0)
	0xB: filter method (1 byte, 0)
	0xC: interlace method (1 byte, 0: no interlace, 1: Adam7 interlace)

	Color types:
	0: Grayscale (bit depth: 1/2/4/8/16)
	2: RGB (bit depth: 8/16)
	3: indexed (requires a PLTE chunk, bit depth: 1/2/4/8)
	4: Grayscale + alpha (bit depth: 8/16)
	6: RGB + alpha (bit depth: 8/16)

	PLTE (palette):
	Format:
	0x0: color list (each color is a 24 bit RGB color, 256 colors max)

	IDAT (image data):
	-Image data can be split across multiple IDAT chunks in order

	IEND (image end):
	-Data field is empty

	Optional chunks:

	tRNS (transparency):
	-Not allowed for color types 4/6

	Format:
	Color type 3:
	0x0: palette alpha list (each entry is 1 byte, 1 entry per palette entry)

	Color type 0:
	0x0: gray (16 bit)

	Color type 2:
	0x0: red (16 bit)
	0x2: green (16 bit)
	0x4: blue (16 bit)

	gAMA (image gamma):
	Format:
	0x0: gamma (32 bit, encoded as gamma*100000)

	cHRM (primary chromaticities):
	-Each value is encoded as the value times 100000

	Format:
	0x0: white point x (32 bit)
	0x4: white point y (32 bit)
	0x8: red x (32 bit)
	0xC: red y (32 bit)
	0x10: green x (32 bit)
	0x14: green y (32 bit)
	0x18: blue x (32 bit)
	0x1C: blue y (32 bit)

	sRGB (standard RGB color space):
	-Overrides the cHRM chunk

	Format:
	0x0: rendering intent (8 bit)

	Rendering intent values:
	0: Perceptual
	1: Relative colorimetric
	2: Saturation
	3: Absolute colorimetric

	iCCP (embedded ICC profile):
	-overrides the cHRM chunk

	Format:
	profile name (1-79 bytes + terminator)
	compression method (8 bit, always 0 (zlib+deflate)
	compressed profile

	Text chunks:
	-Multiple of each type of text chunk are allowed

	tEXt (text data):
	Format:
	keyword (1-79 bytes + terminator)
	text (not null terminated)

	zTXt (compressed text data):
	Format:
	keyword (1-79 bytes + terminator)
	compression method (8 bit, always 0 (zlib+deflate))
	compressed text

	iTXt (international text data):
	Format:
	keyword (1-79 bytes + terminator)
	compression flag (8 bit, 0: uncompressed, 1: compressed)
	compression method (8 bit, always 0 (zlib+deflate))
	language tag (string)
	translated keyword (string)
	text (string)


	bKGD (background color):
	Format:
	Color type 3:
	0x0: palette index (8 bit)

	Color type 0/4:
	0x0: gray (16 bit)

	Color type 2/6:
	0x0: red (16 bit)
	0x2: green (16 bit)
	0x4: blue (16 bit)

	pHYs (physical pixel dimensions):
	Format:
	0x0: ppu, x axis (32 bit)
	0x4: ppu, y axis (32 bit)
	0x8: unit specifier (8 bit, 0: unknown, 1: meter)

	sBIT (significant bits):
	Format:
	Color type 0:
	0x0: significant bits (8 bit)

	Color type 2:
	0x0: red channel sig bits (8 bit)
	0x1: green channel (8 bit)
	0x2: blue channel (8 bit)

	Color type 3:
	0x0: red component sig bits (8 bit)
	0x1: green component (8 bit)
	0x2: blue component (8 bit)

	Color type 4:
	0x0: gray channel (8 bit)
	0x1: alpha channel (8 bit)

	Color type 6:
	0x0: red channel sig bits (8 bit)
	0x1: green channel (8 bit)
	0x2: blue channel (8 bit)
	0x3: alpha channel (8 bit)

	sPLT (suggested palette):
	Format:
	palette name (1-79 bytes + null terminator)
	sample depth (8 bit, can be 8/16)
	palette color list

	Palette color format:
	0x0: red (1/2 bytes)
	0x2: blue (1/2 bytes)
	0x4: green (1/2 bytes)
	0x6: frequency (2 bytes)

	hIST (palette histogram):
	Format:
	0x0: frequency list (each entry is 16 bit, one entry for each palette color)

	Can only appear is a PLTE chunk is present

	tIME (image last modification time):
	Format:
	0x0: year (16 bit)
	0x2: month (8 bit)
	0x3: day (8 bit)
	0x4: hour (8 bit)
	0x5: minute (8 bit)
	0x6: second (8 bit)

	Optional chunk order constraints:
	cHRM/gAMA/iCCP/sBIT/sRGB: before PLTE and IDAT
	bKGD/hIST/tRNS: between PLTE and IDAT
	pHYs/sPLT: before IDAT

	*/

	//Only supports 4bpp indexed images
	public class PNG {
		//Header data
		static byte[] header = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
		public IHDRChunk ihdrChunk;
		public PLTEChunk plteChunk;
		public List<IDATChunk> idatChunks;
		public IENDChunk iendChunk;



		public PNG() {
		}

		public byte[] ToByteArray() {
			List<byte> data = new List<byte>();

			//Add the header to the list
			data.AddRange(header);
			//Add the different chunks to the list in order

			//IHDR chunk
			data.AddRange(ihdrChunk.ToByteArray());
			//PLTE chunk
			data.AddRange(plteChunk.ToByteArray());
			//IDAT chunks
			foreach (IDATChunk chunk in idatChunks) {
				data.AddRange(chunk.ToByteArray());
			}
			//IEND chunk
			data.AddRange(iendChunk.ToByteArray());

			return data.ToArray();
		}
	}
}

