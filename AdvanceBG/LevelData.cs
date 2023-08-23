using AdvanceTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceBG
{
	static class LevelData
	{
		public static GBAColor[] Palette;
		public static List<byte[]> Tiles;
		public static ColorPalette BmpPal;

		public static Bitmap Tile4bppToBmp(byte[] tile, int pal) => BitmapBits.FromTile4bpp(tile, 0).ToBitmap4bpp(Palette.Skip(pal * 16).Take(16).Select(a => a.RGBColor).ToArray());

		public static Bitmap Tile8bppToBmp(byte[] tile) => BitmapBits.FromTile8bpp(tile, 0).ToBitmap(BmpPal);

		public static byte[] FlipTile4bpp(byte[] tile, bool xflip, bool yflip)
		{
			int mode = (xflip ? 1 : 0) | (yflip ? 2 : 0);
			switch (mode)
			{
				default:
					return tile;
				case 1:
					byte[] tileh = new byte[32];
					for (int ty = 0; ty < 8; ty++)
						for (int tx = 0; tx < 4; tx++)
						{
							byte px = tile[(ty * 4) + tx];
							tileh[(ty * 4) + (3 - tx)] = (byte)((px >> 4) | (px << 4));
						}
					return tileh;
				case 2:
					byte[] tilev = new byte[32];
					for (int ty = 0; ty < 8; ty++)
						Array.Copy(tile, ty * 4, tilev, (7 - ty) * 4, 4);
					return tilev;
				case 3:
					byte[] tilehv = new byte[32];
					for (int ty = 0; ty < 8; ty++)
						for (int tx = 0; tx < 4; tx++)
						{
							byte px = tile[(ty * 4) + tx];
							tilehv[((7 - ty) * 4) + (3 - tx)] = (byte)((px >> 4) | (px << 4));
						}
					return tilehv;
			}
		}

		public static byte[] FlipTile8bpp(byte[] tile, bool xflip, bool yflip)
		{
			int mode = (xflip ? 1 : 0) | (yflip ? 2 : 0);
			switch (mode)
			{
				default:
					return tile;
				case 1:
					byte[] tileh = (byte[])tile.Clone();
					for (int ty = 0; ty < 8; ty++)
						Array.Reverse(tileh, ty * 8, 8);
					return tileh;
				case 2:
					byte[] tilev = new byte[64];
					for (int ty = 0; ty < 8; ty++)
						Array.Copy(tile, ty * 4, tilev, (7 - ty) * 4, 4);
					return tilev;
				case 3:
					byte[] tilehv = new byte[64];
					for (int ty = 0; ty < 8; ty++)
						for (int tx = 0; tx < 8; tx++)
							tilehv[((7 - ty) * 8) + (7 - tx)] = tile[(ty * 8) + tx];
					return tilehv;
			}
		}

		public static ImportResult BitmapToTiles(BitmapInfo bmpi, bool color256, byte? forcepal, List<byte[]> tiles, bool optimize, Action updateProgress = null)
		{
			int w = bmpi.Width / 16;
			int h = bmpi.Height / 16;
			ImportResult result = new ImportResult(w, h);
			for (int y = 0; y < h; y++)
				for (int x = 0; x < w; x++)
				{
					TileIndex map = new TileIndex();
					byte[] tile = BmpToTile(new BitmapInfo(bmpi, x * 8, y * 8, 8, 8), color256, forcepal, out int pal);
					map.Palette = (byte)pal;
					bool match = false;
					if (optimize)
					{
						byte[] tileh, tilev, tilehv;
						if (color256)
						{
							tileh = FlipTile8bpp(tile, true, false);
							tilev = FlipTile8bpp(tile, false, true);
							tilehv = FlipTile8bpp(tileh, false, true);
						}
						else
						{
							tileh = FlipTile4bpp(tile, true, false);
							tilev = FlipTile4bpp(tile, false, true);
							tilehv = FlipTile4bpp(tileh, false, true);
						}
						for (int i = 0; i < tiles.Count; i++)
						{
							if (tiles[i].FastArrayEqual(tile))
							{
								match = true;
								map.Tile = (ushort)i;
								break;
							}
							if (tiles[i].FastArrayEqual(tileh))
							{
								match = true;
								map.Tile = (ushort)i;
								map.XFlip = true;
								break;
							}
							if (tiles[i].FastArrayEqual(tilev))
							{
								match = true;
								map.Tile = (ushort)i;
								map.YFlip = true;
								break;
							}
							if (tiles[i].FastArrayEqual(tilehv))
							{
								match = true;
								map.Tile = (ushort)i;
								map.XFlip = true;
								map.YFlip = true;
								break;
							}
						}
					}
					if (!match)
					{
						tiles.Add(tile);
						result.Art.Add(tile);
						map.Tile = (ushort)(tiles.Count - 1);
					}
					result.Mappings[x, y] = map;
					updateProgress?.Invoke();
				}
			return result;
		}

		public static byte[] BmpToTile(BitmapInfo bmp, bool color256, byte? forcepal, out int palette)
		{
			BitmapBits bmpbits = new BitmapBits(8, 8);
			switch (bmp.PixelFormat)
			{
				case PixelFormat.Format1bppIndexed:
					LoadBitmap1BppIndexed(bmpbits, bmp.Pixels, bmp.Stride);
					palette = forcepal ?? 0;
					break;
				case PixelFormat.Format32bppArgb:
					Color[,] pixels = new Color[8, 8];
					for (int y = 0; y < bmp.Height; y++)
					{
						int srcaddr = y * Math.Abs(bmp.Stride);
						for (int x = 0; x < bmp.Width; x++)
							pixels[x, y] = Color.FromArgb(BitConverter.ToInt32(bmp.Pixels, srcaddr + (x * 4)));
					}
					palette = forcepal ?? 0;
					Color[] newpal;
					if (!color256)
					{
						newpal = new Color[16];
						if (!forcepal.HasValue)
						{
							long mindist = long.MaxValue;
							for (int i = 0; i < Palette.Length / 16; i++)
							{
								for (int j = 0; j < 16; j++)
									newpal[j] = Palette[i * 16 + j].RGBColor;
								long totdist = 0;
								for (int y = 0; y < 8; y++)
									for (int x = 0; x < 8; x++)
										if (pixels[x, y].A >= 128)
										{
											pixels[x, y].FindNearestMatch(out int dist, newpal);
											totdist += dist;
										}
								if (totdist < mindist)
								{
									palette = i;
									mindist = totdist;
								}
							}
						}
						for (int j = 0; j < 16; j++)
							newpal[j] = Palette[palette * 16 + j].RGBColor;
					}
					else
						newpal = Palette.Select(a => a.RGBColor).ToArray();
					for (int y = 0; y < 8; y++)
						for (int x = 0; x < 8; x++)
							if (pixels[x, y].A >= 128)
								bmpbits[x, y] = (byte)Array.IndexOf(newpal, pixels[x, y].FindNearestMatch(newpal));
					break;
				case PixelFormat.Format4bppIndexed:
					LoadBitmap4BppIndexed(bmpbits, bmp.Pixels, bmp.Stride);
					palette = forcepal ?? 0;
					break;
				case PixelFormat.Format8bppIndexed:
					LoadBitmap8BppIndexed(bmpbits, bmp.Pixels, bmp.Stride);
					palette = forcepal ?? 0;
					if (!forcepal.HasValue)
					{
						int[] palcnt = new int[4];
						for (int y = 0; y < 8; y++)
							for (int x = 0; x < 8; x++)
							{
								if ((bmpbits[x, y] & 15) > 0)
									palcnt[bmpbits[x, y] / 16]++;
								bmpbits[x, y] &= 15;
							}
						if (palcnt[1] > palcnt[palette])
							palette = 1;
						if (palcnt[2] > palcnt[palette])
							palette = 2;
						if (palcnt[3] > palcnt[palette])
							palette = 3;
					}
					break;
				default:
					throw new Exception("wat");
			}
			return color256 ? bmpbits.Bits : bmpbits.ToTile4bpp();
		}

		private static void LoadBitmap1BppIndexed(BitmapBits bmp, byte[] Bits, int Stride)
		{
			int dstaddr = 0;
			for (int y = 0; y < bmp.Height; y++)
			{
				int srcaddr = y * Math.Abs(Stride);
				for (int x = 0; x < bmp.Width; x += 8)
				{
					byte b = Bits[srcaddr++];
					bmp.Bits[dstaddr++] = (byte)(b >> 7 & 1);
					bmp.Bits[dstaddr++] = (byte)(b >> 6 & 1);
					bmp.Bits[dstaddr++] = (byte)(b >> 5 & 1);
					bmp.Bits[dstaddr++] = (byte)(b >> 4 & 1);
					bmp.Bits[dstaddr++] = (byte)(b >> 3 & 1);
					bmp.Bits[dstaddr++] = (byte)(b >> 2 & 1);
					bmp.Bits[dstaddr++] = (byte)(b >> 1 & 1);
					bmp.Bits[dstaddr++] = (byte)(b & 1);
				}
			}
		}

		private static void LoadBitmap4BppIndexed(BitmapBits bmp, byte[] Bits, int Stride)
		{
			int dstaddr = 0;
			for (int y = 0; y < bmp.Height; y++)
			{
				int srcaddr = y * Math.Abs(Stride);
				for (int x = 0; x < bmp.Width; x += 2)
				{
					byte b = Bits[srcaddr++];
					bmp.Bits[dstaddr++] = (byte)(b >> 4);
					bmp.Bits[dstaddr++] = (byte)(b & 0xF);
				}
			}
		}

		private static void LoadBitmap8BppIndexed(BitmapBits bmp, byte[] Bits, int Stride)
		{
			int dstaddr = 0;
			for (int y = 0; y < bmp.Height; y++)
			{
				int srcaddr = y * Math.Abs(Stride);
				for (int x = 0; x < bmp.Width; x++)
					bmp.Bits[dstaddr++] = Bits[srcaddr++];
			}
		}

		public static void IncrementIndexes(this BitmapBits bmp, int amount)
		{
			for (int i = 0; i < bmp.Bits.Length; i++)
				if (bmp.Bits[i] > 0) bmp.Bits[i] = (byte)(bmp.Bits[i] + amount);
		}

		public static Color FindNearestMatch(this Color col, out int distance, params Color[] palette)
		{
			if (Array.IndexOf(palette, col) != -1)
			{
				distance = 0;
				return col;
			}
			Color nearest_color = Color.Empty;
			distance = int.MaxValue;
			foreach (Color o in palette)
			{
				int test_red = o.R - col.R;
				test_red *= test_red;
				int test_green = o.G - col.G;
				test_green *= test_green;
				int test_blue = o.B - col.B;
				test_blue *= test_blue;
				int temp = test_blue + test_green + test_red;
				if (temp == 0)
					return o;
				else if (temp < distance)
				{
					distance = temp;
					nearest_color = o;
				}
			}
			return nearest_color;
		}

		public static Color FindNearestMatch(this Color col, params Color[] palette) => FindNearestMatch(col, out _, palette);
	}

	public class BitmapInfo
	{
		public int Width { get; }
		public int Height { get; }
		public Size Size => new Size(Width, Height);
		public PixelFormat PixelFormat { get; }
		public int Stride { get; }
		public byte[] Pixels { get; }

		public BitmapInfo(Bitmap bitmap)
		{
			Width = bitmap.Width;
			Height = bitmap.Height;
			switch (bitmap.PixelFormat)
			{
				case PixelFormat.Format1bppIndexed:
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format4bppIndexed:
				case PixelFormat.Format8bppIndexed:
					PixelFormat = bitmap.PixelFormat;
					break;
				default:
					bitmap = bitmap.To32bpp();
					PixelFormat = PixelFormat.Format32bppArgb;
					break;
			}
			BitmapData bmpd = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat);
			Stride = Math.Abs(bmpd.Stride);
			Pixels = new byte[Stride * Height];
			System.Runtime.InteropServices.Marshal.Copy(bmpd.Scan0, Pixels, 0, Pixels.Length);
			bitmap.UnlockBits(bmpd);
		}

		public BitmapInfo(BitmapInfo source, int x, int y, int width, int height)
		{
			switch (source.PixelFormat)
			{
				case PixelFormat.Format1bppIndexed:
					if (x % 8 != 0)
						throw new FormatException("X coordinate of 1bpp image section must be multiple of 8.");
					if (width % 8 != 0)
						throw new FormatException("Width of 1bpp image section must be multiple of 8.");
					break;
				case PixelFormat.Format4bppIndexed:
					if (x % 2 != 0)
						throw new FormatException("X coordinate of 4bpp image section must be multiple of 2.");
					if (width % 2 != 0)
						throw new FormatException("Width of 4bpp image section must be multiple of 2.");
					break;
			}
			Width = width;
			Height = height;
			PixelFormat = source.PixelFormat;
			switch (PixelFormat)
			{
				case PixelFormat.Format1bppIndexed:
					Stride = width / 8;
					x /= 8;
					break;
				case PixelFormat.Format4bppIndexed:
					Stride = width / 2;
					x /= 2;
					break;
				case PixelFormat.Format8bppIndexed:
					Stride = width;
					break;
				case PixelFormat.Format32bppArgb:
					Stride = width * 4;
					x *= 4;
					break;
			}
			Pixels = new byte[height * Stride];
			for (int v = 0; v < height; v++)
				Array.Copy(source.Pixels, ((y + v) * source.Stride) + x, Pixels, v * Stride, Stride);
		}

		public Bitmap ToBitmap()
		{
			Bitmap bitmap = new Bitmap(Width, Height, PixelFormat);
			BitmapData bmpd = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat);
			byte[] bmpbits = new byte[Height * Math.Abs(bmpd.Stride)];
			for (int y = 0; y < Height; y++)
				Array.Copy(Pixels, y * Stride, bmpbits, y * Math.Abs(bmpd.Stride), Width);
			System.Runtime.InteropServices.Marshal.Copy(bmpbits, 0, bmpd.Scan0, bmpbits.Length);
			bitmap.UnlockBits(bmpd);
			return bitmap;
		}
	}

	public class ImportResult
	{
		public TileIndex[,] Mappings { get; }
		public List<byte[]> Art { get; }

		public ImportResult(int width, int height)
		{
			Mappings = new TileIndex[width, height];
			Art = new List<byte[]>();
		}
	}

}
