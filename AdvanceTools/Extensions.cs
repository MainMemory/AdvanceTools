using System;
using System.Collections.Generic;
using System.Drawing;

namespace AdvanceTools
{
	public static class Extensions
	{
		public static Rectangle Flip(this Rectangle rect, bool xflip, bool yflip)
		{
			if (xflip)
				rect.X = -rect.Right;
			if (yflip)
				rect.Y = -rect.Bottom;
			return rect;
		}

		public static bool ArrayEqual<T>(this T[] arr1, T[] arr2)
		{
			if (arr1 == arr2) return true;
			if (arr1.Length != arr2.Length) return false;
			for (int i = 0; i < arr1.Length; i++)
				if (!arr1[i].Equals(arr2[i]))
					return false;
			return true;
		}

		private static unsafe bool FastArrayEqualInternal(void* fp1, void* fp2, int length)
		{
			ulong* lp1 = (ulong*)fp1;
			ulong* lp2 = (ulong*)fp2;
			int longlen = length / 8;
			for (int i = 0; i < longlen; i++)
				if (*lp1++ != *lp2++) return false;
			if ((length & 7) != 0)
			{
				byte* bp1 = (byte*)lp1;
				byte* bp2 = (byte*)lp2;
				if ((length & 4) == 4)
					if (*(uint*)bp1 != *(uint*)bp2)
						return false;
					else
					{
						bp1 += 4;
						bp2 += 4;
					}
				if ((length & 2) == 2)
					if (*(ushort*)bp1 != *(ushort*)bp2)
						return false;
					else
					{
						bp1 += 2;
						bp2 += 2;
					}
				if ((length & 1) == 1)
					return *bp1 == *bp2;
			}
			return true;
		}

		public static unsafe bool FastArrayEqual(this byte[] arr1, byte[] arr2)
		{
			if (arr1 == arr2) return true;
			if (arr1.Length != arr2.Length) return false;
			fixed (byte* fp1 = arr1, fp2 = arr2)
				return FastArrayEqualInternal(fp1, fp2, arr1.Length);
		}

		public static unsafe bool FastArrayEqual(this sbyte[] arr1, sbyte[] arr2)
		{
			if (arr1 == arr2) return true;
			if (arr1.Length != arr2.Length) return false;
			fixed (sbyte* fp1 = arr1, fp2 = arr2)
				return FastArrayEqualInternal(fp1, fp2, arr1.Length);
		}

		public static unsafe bool FastArrayEqual(this ushort[] arr1, ushort[] arr2)
		{
			if (arr1 == arr2) return true;
			if (arr1.Length != arr2.Length) return false;
			fixed (ushort* fp1 = arr1, fp2 = arr2)
				return FastArrayEqualInternal(fp1, fp2, arr1.Length * 2);
		}

		public static unsafe bool FastArrayEqual(this short[] arr1, short[] arr2)
		{
			if (arr1 == arr2) return true;
			if (arr1.Length != arr2.Length) return false;
			fixed (short* fp1 = arr1, fp2 = arr2)
				return FastArrayEqualInternal(fp1, fp2, arr1.Length * 2);
		}

		public static unsafe bool FastArrayEqual(this uint[] arr1, uint[] arr2)
		{
			if (arr1 == arr2) return true;
			if (arr1.Length != arr2.Length) return false;
			fixed (uint* fp1 = arr1, fp2 = arr2)
				return FastArrayEqualInternal(fp1, fp2, arr1.Length * 4);
		}

		public static unsafe bool FastArrayEqual(this int[] arr1, int[] arr2)
		{
			if (arr1 == arr2) return true;
			if (arr1.Length != arr2.Length) return false;
			fixed (int* fp1 = arr1, fp2 = arr2)
				return FastArrayEqualInternal(fp1, fp2, arr1.Length * 4);
		}

		public static unsafe bool FastArrayEqual(this char[] arr1, char[] arr2)
		{
			if (arr1 == arr2) return true;
			if (arr1.Length != arr2.Length) return false;
			fixed (char* fp1 = arr1, fp2 = arr2)
				return FastArrayEqualInternal(fp1, fp2, arr1.Length * 2);
		}

		private static unsafe bool FastArrayEqualInternal(void* fp1, ulong value, int length)
		{
			ulong* lp1 = (ulong*)fp1;
			int longlen = length / 8;
			for (int i = 0; i < longlen; i++)
				if (*lp1++ != value) return false;
			if ((length & 7) != 0)
			{
				byte* bp1 = (byte*)lp1;
				if ((length & 4) == 4)
					if (*(uint*)bp1 != (uint)value)
						return false;
					else
						bp1 += 4;
				if ((length & 2) == 2)
					if (*(ushort*)bp1 != (ushort)value)
						return false;
					else
						bp1 += 2;
				if ((length & 1) == 1)
					return *bp1 == (byte)value;
			}
			return true;
		}

		public static unsafe bool FastArrayEqual(this byte[] arr1, byte value, int startIndex, int length)
		{
			if (startIndex < 0 || startIndex >= arr1.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length <= 0 || startIndex + length > arr1.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)value;
			longval |= longval << 8;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (byte* fp1 = arr1)
				return FastArrayEqualInternal(&fp1[startIndex], longval, length);
		}

		public static bool FastArrayEqual(this byte[] arr1, byte value) { return FastArrayEqual(arr1, value, 0, arr1.Length); }

		public static unsafe bool FastArrayEqual(this sbyte[] arr1, sbyte value, int startIndex, int length)
		{
			if (startIndex < 0 || startIndex >= arr1.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length <= 0 || startIndex + length > arr1.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)(byte)value;
			longval |= longval << 8;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (sbyte* fp1 = arr1)
				return FastArrayEqualInternal(&fp1[startIndex], longval, length);
		}

		public static bool FastArrayEqual(this sbyte[] arr1, sbyte value) { return FastArrayEqual(arr1, value, 0, arr1.Length); }

		public static unsafe bool FastArrayEqual(this ushort[] arr1, ushort value, int startIndex, int length)
		{
			if (startIndex < 0 || startIndex >= arr1.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length <= 0 || startIndex + length > arr1.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)value;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (ushort* fp1 = arr1)
				return FastArrayEqualInternal(&fp1[startIndex], longval, length * 2);
		}

		public static bool FastArrayEqual(this ushort[] arr1, ushort value) { return FastArrayEqual(arr1, value, 0, arr1.Length); }

		public static unsafe bool FastArrayEqual(this short[] arr1, short value, int startIndex, int length)
		{
			if (startIndex < 0 || startIndex >= arr1.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length <= 0 || startIndex + length > arr1.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)(ushort)value;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (short* fp1 = arr1)
				return FastArrayEqualInternal(&fp1[startIndex], longval, length * 2);
		}

		public static bool FastArrayEqual(this short[] arr1, short value) { return FastArrayEqual(arr1, value, 0, arr1.Length); }

		public static unsafe bool FastArrayEqual(this uint[] arr1, uint value, int startIndex, int length)
		{
			if (startIndex < 0 || startIndex >= arr1.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length <= 0 || startIndex + length > arr1.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)value;
			longval |= longval << 32;
			fixed (uint* fp1 = arr1)
				return FastArrayEqualInternal(&fp1[startIndex], longval, length * 4);
		}

		public static bool FastArrayEqual(this uint[] arr1, uint value) { return FastArrayEqual(arr1, value, 0, arr1.Length); }

		public static unsafe bool FastArrayEqual(this int[] arr1, int value, int startIndex, int length)
		{
			if (startIndex < 0 || startIndex >= arr1.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length <= 0 || startIndex + length > arr1.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)(uint)value;
			longval |= longval << 32;
			fixed (int* fp1 = arr1)
				return FastArrayEqualInternal(&fp1[startIndex], longval, length * 4);
		}

		public static bool FastArrayEqual(this int[] arr1, int value) { return FastArrayEqual(arr1, value, 0, arr1.Length); }

		public static unsafe bool FastArrayEqual(this char[] arr1, char value, int startIndex, int length)
		{
			if (startIndex < 0 || startIndex >= arr1.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length <= 0 || startIndex + length > arr1.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)value;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (char* fp1 = arr1)
				return FastArrayEqualInternal(&fp1[startIndex], longval, length * 2);
		}

		public static bool FastArrayEqual(this char[] arr1, char value) { return FastArrayEqual(arr1, value, 0, arr1.Length); }

		public static bool ListEqual<T>(this IList<T> lst1, IList<T> lst2)
		{
			if (lst1 == lst2) return true;
			if (lst1.Count != lst2.Count) return false;
			for (int i = 0; i < lst1.Count; i++)
				if (!lst1[i].Equals(lst2[i]))
					return false;
			return true;
		}

		public static void Fill<T>(this T[] arr, T item, int startIndex, int length)
		{
			if (length == 0) return;
			if (startIndex < 0 || startIndex >= arr.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length < 0 || startIndex + length > arr.Length) throw new ArgumentOutOfRangeException("length");
			for (int i = startIndex; i < startIndex + length; i++)
				arr[i] = item;
		}

		public static void Fill<T>(this T[] arr, T item)
		{
			for (int i = 0; i < arr.Length; i++)
				arr[i] = item;
		}

		private static unsafe void FastFillInternal(void* fp, ulong item, int length)
		{
			ulong* lp = (ulong*)fp;
			int longlen = length / 8;
			for (int i = 0; i < longlen; i++)
				*lp++ = item;
			if ((length & 7) != 0)
			{
				byte* bp = (byte*)lp;
				if ((length & 4) == 4)
				{
					*(uint*)bp = (uint)item;
					bp += 4;
				}
				if ((length & 2) == 2)
				{
					*(ushort*)bp = (ushort)item;
					bp += 2;
				}
				if ((length & 1) == 1)
					*bp = (byte)item;
			}
		}

		public static unsafe void FastFill(this byte[] arr, byte value, int startIndex, int length)
		{
			if (length == 0) return;
			if (startIndex < 0 || startIndex >= arr.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length < 0 || startIndex + length > arr.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)value;
			longval |= longval << 8;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (byte* fp = arr)
				FastFillInternal(fp + startIndex, longval, length);
		}

		public static unsafe void FastFill(this byte[] arr, byte value)
		{
			ulong longval = (ulong)value;
			longval |= longval << 8;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (byte* fp = arr)
				FastFillInternal(fp, longval, arr.Length);
		}

		public static unsafe void FastFill(this sbyte[] arr, sbyte value, int startIndex, int length)
		{
			if (length == 0) return;
			if (startIndex < 0 || startIndex >= arr.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length < 0 || startIndex + length > arr.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)(byte)value;
			longval |= longval << 8;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (sbyte* fp = arr)
				FastFillInternal(fp + startIndex, longval, length);
		}

		public static unsafe void FastFill(this sbyte[] arr, sbyte value)
		{
			ulong longval = (ulong)(byte)value;
			longval |= longval << 8;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (sbyte* fp = arr)
				FastFillInternal(fp, longval, arr.Length);
		}

		public static unsafe void FastFill(this ushort[] arr, ushort value, int startIndex, int length)
		{
			if (length == 0) return;
			if (startIndex < 0 || startIndex >= arr.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length < 0 || startIndex + length > arr.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)value;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (ushort* fp = arr)
				FastFillInternal(fp + startIndex, longval, length * 2);
		}

		public static unsafe void FastFill(this ushort[] arr, ushort value)
		{
			ulong longval = (ulong)value;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (ushort* fp = arr)
				FastFillInternal(fp, longval, arr.Length * 2);
		}

		public static unsafe void FastFill(this short[] arr, short value, int startIndex, int length)
		{
			if (length == 0) return;
			if (startIndex < 0 || startIndex >= arr.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length < 0 || startIndex + length > arr.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)(ushort)value;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (short* fp = arr)
				FastFillInternal(fp + startIndex, longval, length * 2);
		}

		public static unsafe void FastFill(this short[] arr, short value)
		{
			ulong longval = (ulong)(ushort)value;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (short* fp = arr)
				FastFillInternal(fp, longval, arr.Length * 2);
		}

		public static unsafe void FastFill(this uint[] arr, uint value, int startIndex, int length)
		{
			if (length == 0) return;
			if (startIndex < 0 || startIndex >= arr.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length < 0 || startIndex + length > arr.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)value;
			longval |= longval << 32;
			fixed (uint* fp = arr)
				FastFillInternal(fp + startIndex, longval, length * 4);
		}

		public static unsafe void FastFill(this uint[] arr, uint value)
		{
			ulong longval = (ulong)value;
			longval |= longval << 32;
			fixed (uint* fp = arr)
				FastFillInternal(fp, longval, arr.Length * 4);
		}

		public static unsafe void FastFill(this int[] arr, int value, int startIndex, int length)
		{
			if (length == 0) return;
			if (startIndex < 0 || startIndex >= arr.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length < 0 || startIndex + length > arr.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)(uint)value;
			longval |= longval << 32;
			fixed (int* fp = arr)
				FastFillInternal(fp + startIndex, longval, length * 4);
		}

		public static unsafe void FastFill(this int[] arr, int value)
		{
			ulong longval = (ulong)(uint)value;
			longval |= longval << 32;
			fixed (int* fp = arr)
				FastFillInternal(fp, longval, arr.Length * 4);
		}

		public static unsafe void FastFill(this char[] arr, char value, int startIndex, int length)
		{
			if (length == 0) return;
			if (startIndex < 0 || startIndex >= arr.Length) throw new ArgumentOutOfRangeException("startIndex");
			if (length < 0 || startIndex + length > arr.Length) throw new ArgumentOutOfRangeException("length");
			ulong longval = (ulong)value;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (char* fp = arr)
				FastFillInternal(fp + startIndex, longval, length * 2);
		}

		public static unsafe void FastFill(this char[] arr, char value)
		{
			ulong longval = (ulong)value;
			longval |= longval << 16;
			longval |= longval << 32;
			fixed (char* fp = arr)
				FastFillInternal(fp, longval, arr.Length * 2);
		}
	}
}
