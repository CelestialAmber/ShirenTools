using System;

namespace PNGLib {

	public static class ArrayExtensions {
		public static T[] GetRow<T>(this T[,] array, int rowIndex) {
			int length = array.GetLength(0);
			T[] row = new T[length];

			for (int i = 0; i < length; i++) {
				row[i] = array[i, rowIndex];
			}

			return row;
		}

	}
}

