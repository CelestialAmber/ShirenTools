using System;

namespace LunaPNG.Utility {

	public struct Color {
		public byte r = 0;
		public byte g = 0;
		public byte b = 0;
		public byte a = 255;

		public Color() {
		}

		public Color(byte r, byte g, byte b) {
			this.r = r;
			this.g = g;
			this.b = b;
			a = 255;
		}

		public Color(byte r, byte g, byte b, byte a) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
	}
}

