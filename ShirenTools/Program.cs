using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ShirenTools {
    class Program {
        public static List<byte[]> tilesetAtlasData = new List<byte[]>();

        static void Main(string[] args) {
            ReadTilesetAtlas();
            foreach (string file in Directory.EnumerateFiles("tilesets","*.bin")) {
                ReadMapTilesetData(file);
            }
        }

        public static void ReadTilesetAtlas() {
            byte[] data = File.ReadAllBytes("atlas.4bpp");

            for(int i = 0; i < data.Length; i += 32) {
                tilesetAtlasData.Add(data.Skip(i).Take(32).ToArray());
            }
        }


        public static void ReadMapTilesetData(string file) {
            byte[] data = File.ReadAllBytes(file);
            int offset = 0;

            /*
            Referred to as qpflag in the shiren 1 hacking notes ruby script.
            What could q stand for?
            */
            byte flagByte1 = data[offset++];
            int palettesNum = (flagByte1 & 7) + 1;
            int qNum = flagByte1 >> 4;
            offset += palettesNum * 2;

            if(qNum > 0) {
                for (int i = 0; i < qNum; i++) {
                    int entries = (data[offset++] & 0xF) + 1;
                    offset += entries * 2;
                }
            }

            Console.WriteLine("Tileset flag offset: " + offset.ToString("X"));

            //Read the tileset data
            ushort tilesetFlag = ReadUInt16(data, ref offset);
            int tilesetEntries = tilesetFlag & 0x3FF;
            List<byte> mapTilesetData = new List<byte>();

            for(int i = 0; i < tilesetEntries; i++) {
                int tileIndex = ReadUInt16(data, ref offset);
                mapTilesetData.AddRange(tilesetAtlasData[tileIndex - 1]);
            }

            //Name from shiren 1 hacking notes script
            ushort bFlag = ReadUInt16(data, ref offset);

            //Rest of stuff


            File.WriteAllBytes(file + "_tileset.4bpp", mapTilesetData.ToArray());
        }

        public static ushort ReadUInt16(byte[] data, ref int offset) {
            ushort val = (ushort)(data[offset] + (data[offset + 1] << 8));
            offset += 2;
            return val;
        }
    }
}