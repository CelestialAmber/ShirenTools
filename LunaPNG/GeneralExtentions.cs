using System;
using System.Linq;

namespace PNGLib {
	public static class GeneralExtentions {
		//Converts an int to a byte array in big endian order
		public static byte[] ToByteArray(this int val) {
			byte[] bytes = BitConverter.GetBytes(val);

			//If the computer uses little endian, reverse the bytes
			if (BitConverter.IsLittleEndian) {
				bytes = bytes.Reverse().ToArray();
			}

			return bytes;
		}

		//Converts an uint to a byte array in big endian order
		public static byte[] ToByteArray(this uint val) {
			byte[] bytes = BitConverter.GetBytes(val);

			//If the computer uses little endian, reverse the bytes
			if (BitConverter.IsLittleEndian) {
				bytes = bytes.Reverse().ToArray();
			}

			return bytes;
		}
	}
}

