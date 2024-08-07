using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdvanceTools
{
	public static class Utility
	{
		public static int GetPointer(byte[] file, int address)
		{
			var addr = BitConverter.ToInt32(file, address);
			if (addr != 0)
				return addr - 0x8000000;
			return 0;
		}

		public static byte[] DecompressRLData(byte[] file, int address)
		{
			var head = BitConverter.ToUInt32(file, address);
			address += 4;
			if ((head & 0xFF) != 0x30)
				return null;
			var size = head >> 8;
			var dst = new byte[size];
			var off = 0;
			while (off < size)
			{
				var flag = file[address++];
				if ((flag & 0x80) == 0x80)
				{
					var cnt = (flag & 0x7F) + 3;
					var val = file[address++];
					for (var i = 0; i < cnt; ++i)
						dst[off++] = val;
				}
				else
				{
					var cnt = flag + 1;
					for (var i = 0; i < cnt; ++i)
						dst[off++] = file[address++];
				}
			}
			return dst;
		}

		public static byte[] CompressRLData(byte[] data)
		{
			using (var ms = new System.IO.MemoryStream())
			{
				var bw = new System.IO.BinaryWriter(ms);
				bw.Write((data.Length << 8) | 0x30);
				var run = new List<byte>(128);
				for (int i = 0; i < data.Length; i++)
				{
					byte cur = data[i];
					byte cnt = 1;
					while (cnt < 130 && i + cnt < data.Length && data[i + cnt] == cur)
						++cnt;
					if (cnt >= 3)
					{
						if (run.Count > 0)
						{
							bw.Write((byte)(run.Count - 1));
							bw.Write(run.ToArray());
							run.Clear();
						}
						bw.Write((byte)(0x80 | (cnt - 3)));
						bw.Write(cur);
						i += cnt - 1;
					}
					else
					{
						run.Add(cur);
						if (run.Count == 128)
						{
							bw.Write((byte)127);
							bw.Write(run.ToArray());
							run.Clear();
						}
					}
				}
				if (run.Count > 0)
				{
					bw.Write((byte)(run.Count - 1));
					bw.Write(run.ToArray());
				}
				if (ms.Length % 4 != 0)
					bw.Write(new byte[4 - (ms.Length % 4)]);
				return ms.ToArray();
			}
		}

		static readonly System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
		public static string HashFile(string path) => HashBytes(System.IO.File.ReadAllBytes(path));

		public static string HashBytes(byte[] buffer) => string.Join(string.Empty, md5.ComputeHash(buffer).Select(a => a.ToString("X2")));
	}

	[Serializable]
	public struct GBAColor
	{
		public byte R { get; set; }
		public byte G { get; set; }
		public byte B { get; set; }

		public Color RGBColor
		{
			get
			{
				return Color.FromArgb(R, G, B);
			}
			set
			{
				R = value.R;
				G = value.G;
				B = value.B;
			}
		}

		public ushort Value
		{
			get
			{
				return (ushort)((R >> 3) | ((G >> 3) << 5) | ((B >> 3) << 10));
			}
			set
			{
				int tmp = value & 0x1F;
				R = (byte)((tmp >> 2) | (tmp << 3));
				tmp = (value >> 5) & 0x1F;
				G = (byte)((tmp >> 2) | (tmp << 3));
				tmp = (value >> 10) & 0x1F;
				B = (byte)((tmp >> 2) | (tmp << 3));
			}
		}

		public GBAColor(byte red, byte green, byte blue)
			: this()
		{
			R = red;
			G = green;
			B = blue;
		}

		public GBAColor(Color color)
			: this()
		{
			RGBColor = color;
		}

		public GBAColor(ushort color)
			: this()
		{
			Value = color;
		}

		public static GBAColor[] Load(byte[] file, int address, int length)
		{
			GBAColor[] palfile = new GBAColor[length];
			for (int pi = 0; pi < length; pi++)
				palfile[pi] = new GBAColor(BitConverter.ToUInt16(file, address + (pi * 2)));
			return palfile;
		}

		public static GBAColor[] Load(byte[] file)
		{
			return Load(file, 0, file.Length / 2);
		}

		public static GBAColor[] Load(string filename)
		{
			byte[] file = System.IO.File.ReadAllBytes(filename);
			return Load(file);
		}
	}

	[Serializable]
	public class TileIndex
	{
		private byte _pal;
		public byte Palette
		{
			get
			{
				return _pal;
			}
			set
			{
				_pal = (byte)(value & 0xF);
			}
		}
		public bool XFlip { get; set; }
		public bool YFlip { get; set; }

		private ushort _ind;
		public ushort Tile
		{
			get
			{
				return _ind;
			}
			set
			{
				_ind = (ushort)(value & 0x3FF);
			}
		}

		public static int Size { get { return 2; } }

		public TileIndex() { }

		public TileIndex(ushort data)
		{
			Palette = (byte)((data >> 12) & 0xF);
			YFlip = (data & 0x800) == 0x800;
			XFlip = (data & 0x400) == 0x400;
			_ind = (ushort)(data & 0x3FF);
		}

		public TileIndex(byte[] file, int address)
		: this(BitConverter.ToUInt16(file, address)) { }

		public TileIndex(ushort tile, bool yflip, bool xflip, byte pal)
		{
			Tile = tile;
			YFlip = yflip;
			XFlip = xflip;
			Palette = pal;
		}

		public ushort GetUShort()
		{
			ushort val = _ind;
			if (XFlip) val |= 0x400;
			if (YFlip) val |= 0x800;
			val |= (ushort)(Palette << 12);
			return val;
		}

		public byte[] GetBytes()
		{
			return BitConverter.GetBytes(GetUShort());
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TileIndex)) return false;
			TileIndex other = (TileIndex)obj;
			if (Palette != other.Palette) return false;
			if (XFlip != other.XFlip) return false;
			if (YFlip != other.YFlip) return false;
			if (Tile != other.Tile) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(TileIndex a, TileIndex b)
		{
			if (a is null)
				return b is null;
			return a.Equals(b);
		}

		public static bool operator !=(TileIndex a, TileIndex b)
		{
			if (a is null)
				return !(b is null);
			return !a.Equals(b);
		}

		public static TileIndex operator +(TileIndex a, TileIndex b)
		{
			return new TileIndex((ushort)(a.GetUShort() + b.GetUShort()));
		}

		public TileIndex Clone()
		{
			return (TileIndex)MemberwiseClone();
		}
	}

	public class LevelArtPointers
	{
		public int Address { get; }
		public int ForegroundHigh { get; set; }
		public int ForegroundLow { get; set; }
		public int Background1 { get; set; }
		public int Background2 { get; set; }

		public LevelArtPointers(byte[] file, int address, int game)
		{
			Address = address;
			ForegroundHigh = Utility.GetPointer(file, address);
			ForegroundLow = Utility.GetPointer(file, address + 4);
			Background1 = Utility.GetPointer(file, address + 8);
			if (game == 3)
				Background2 = Utility.GetPointer(file, address + 0xC);
		}

		public static int GetSize(int game) => game == 3 ? 0x10 : 0xC;

		public ForegroundLayer ReadForegroundHigh(byte[] file) => ForegroundHigh != 0 ? new ForegroundLayer(file, ForegroundHigh) : null;

		public ForegroundLayer ReadForegroundLow(byte[] file) => ForegroundLow != 0 ? new ForegroundLayer(file, ForegroundLow) : null;

		public BackgroundLayer ReadBackground1(byte[] file) => Background1 != 0 ? new BackgroundLayer(file, Background1) : null;

		public BackgroundLayer ReadBackground2(byte[] file) => Background2 != 0 ? new BackgroundLayer(file, Background2) : null;
	}

	public abstract class LayerBase
	{
		public int Address { get; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public ushort AniTilesSize { get; set; }
		public byte AnimFrameCount { get; set; }
		public byte AnimDelay { get; set; }
		public int Tiles { get; set; }
		public uint TilesSize { get; set; }
		public int Palette { get; set; }
		public ushort PalDest { get; set; }
		public ushort PalLength { get; set; }
		public int Layout { get; set; }

		public LayerBase(byte[] file, int address)
		{
			Address = address;
			AniTilesSize = BitConverter.ToUInt16(file, address + 4);
			AnimFrameCount = file[address + 6];
			AnimDelay = file[address + 7];
			Tiles = Utility.GetPointer(file, address + 8);
			TilesSize = BitConverter.ToUInt32(file, address + 0xC);
			Palette = Utility.GetPointer(file, address + 0x10);
			PalDest = BitConverter.ToUInt16(file, address + 0x14);
			PalLength = BitConverter.ToUInt16(file, address + 0x16);
		}

		public byte[] GetAniTiles(byte[] file)
		{
			if (Tiles == 0)
				return null;
			byte[] result = new byte[AniTilesSize * (AnimFrameCount - 1)];
			Array.Copy(file, Tiles + TilesSize, result, 0, AniTilesSize * (AnimFrameCount - 1));
			return result;
		}

		public byte[] GetTiles(byte[] file)
		{
			if (Tiles == 0)
				return null;
			byte[] result = new byte[TilesSize];
			Array.Copy(file, Tiles, result, 0, TilesSize);
			return result;
		}

		public GBAColor[] GetPalette(byte[] file)
		{
			if (Palette == 0)
				return null;
			return GBAColor.Load(file, Palette, PalLength);
		}

		public byte[] GetPaletteRaw(byte[] file)
		{
			if (Palette == 0)
				return null;
			byte[] result = new byte[PalLength * 2];
			Array.Copy(file, Palette, result, 0, PalLength * 2);
			return result;
		}
	}

	public class ForegroundLayer : LayerBase
	{
		public ushort ChunkWidth { get; set; }
		public ushort ChunkHeight { get; set; }
		public int Chunks { get; set; }

		public ForegroundLayer(byte[] file, int address) : base(file, address)
		{
			ChunkWidth = BitConverter.ToUInt16(file, address);
			ChunkHeight = BitConverter.ToUInt16(file, address + 2);
			Chunks = Utility.GetPointer(file, address + 0x18);
			Layout = Utility.GetPointer(file, address + 0x1C);
			Width = BitConverter.ToUInt16(file, address + 0x20);
			Height = BitConverter.ToUInt16(file, address + 0x22);
		}

		public ushort[,] GetLayout(byte[] file, int game)
		{
			if (Layout == 0)
				return null;
			ushort[,] result = new ushort[Width, Height];
			if (game == 1)
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = file[Layout + y * Width + x];
			}
			else
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = BitConverter.ToUInt16(file, Layout + (y * Width + x) * 2);
			}
			return result;
		}

		public byte[] GetLayoutRaw(byte[] file, int game)
		{
			if (Layout == 0)
				return null;
			int len;
			if (game == 1)
				len = Width * Height;
			else
				len = Width * Height * 2;
			byte[] result = new byte[len];
			Array.Copy(file, Layout, result, 0, len);
			return result;
		}
	}

	public class BackgroundLayer : LayerBase
	{
		public BGMode Mode { get; }

		public BackgroundLayer(byte[] file, int address, BGMode mode = BGMode.Normal) : base(file, address)
		{
			Mode = mode;
			Width = BitConverter.ToUInt16(file, address);
			Height = BitConverter.ToUInt16(file, address + 2);
			Layout = Utility.GetPointer(file, address + 0x18);
		}

		public TileIndex[,] GetLayout(byte[] file)
		{
			if (Layout == 0)
				return null;
			TileIndex[,] result = new TileIndex[Width, Height];
			if (Mode == BGMode.Scale)
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = new TileIndex(file[Layout + y * Width + x], false, false, 0);
			}
			else
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = new TileIndex(file, Layout + (y * Width + x) * 2);
			}
			return result;
		}

		public byte[] GetLayoutRaw(byte[] file)
		{
			if (Layout == 0)
				return null;
			var size = Width * Height;
			if (Mode != BGMode.Scale)
				size *= 2;
			byte[] result = new byte[size];
			Array.Copy(file, Layout, result, 0, size);
			return result;
		}
	}

	public enum BGMode
	{
		Normal,
		Color256,
		Scale
	}

	public class LevelCollision
	{
		public int Address { get; }
		public int Heightmaps { get; set; }
		public int Angles { get; set; }
		public int Chunks { get; set; }
		public int ForegroundHigh { get; set; }
		public int ForegroundLow { get; set; }
		public int Flags { get; set; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public uint WidthPixels { get; set; }
		public uint HeightPixels { get; set; }

		public LevelCollision(byte[] file, int address, int game)
		{
			Address = address;
			Heightmaps = Utility.GetPointer(file, address);
			Angles = Utility.GetPointer(file, address + 4);
			Chunks = Utility.GetPointer(file, address + 8);
			ForegroundHigh = Utility.GetPointer(file, address + 0xC);
			ForegroundLow = Utility.GetPointer(file, address + 0x10);
			Flags = Utility.GetPointer(file, address + 0x14);
			Width = BitConverter.ToUInt16(file, address + 0x18);
			Height = BitConverter.ToUInt16(file, address + 0x1A);
			if (game == 1)
			{
				WidthPixels = BitConverter.ToUInt16(file, address + 0x1C);
				HeightPixels = BitConverter.ToUInt16(file, address + 0x1E);
			}
			else
			{
				WidthPixels = BitConverter.ToUInt32(file, address + 0x1C);
				HeightPixels = BitConverter.ToUInt32(file, address + 0x20);
			}
		}

		private ushort[,] ReadLayout(byte[] file, int game, int layout)
		{
			if (layout == 0)
				return null;
			ushort[,] result = new ushort[Width, Height];
			if (game == 1)
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = file[layout + y * Width + x];
			}
			else
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = BitConverter.ToUInt16(file, layout + (y * Width + x) * 2);
			}
			return result;
		}

		private byte[] ReadLayoutRaw(byte[] file, int game, int layout)
		{
			if (layout == 0)
				return null;
			int len;
			if (game == 1)
				len = Width * Height;
			else
				len = Width * Height * 2;
			byte[] result = new byte[len];
			Array.Copy(file, layout, result, 0, len);
			return result;
		}

		public ushort[,] ReadForegroundHigh(byte[] file, int game) => ReadLayout(file, game, ForegroundHigh);

		public byte[] ReadForegroundHighRaw(byte[] file, int game) => ReadLayoutRaw(file, game, ForegroundHigh);

		public ushort[,] ReadForegroundLow(byte[] file, int game) => ReadLayout(file, game, ForegroundLow);

		public byte[] ReadForegroundLowRaw(byte[] file, int game) => ReadLayoutRaw(file, game, ForegroundLow);
	}

	public struct Heightmap : IEquatable<Heightmap>
	{
		private byte vert;
		public byte Vertical
		{
			get => vert;
			set => vert = (byte)(value & 0xF);
		}

		private byte horz;
		public byte Horizontal
		{
			get => horz;
			set => horz = (byte)(value & 0xF);
		}

		public Heightmap(byte data)
		{
			vert = (byte)(data >> 4);
			horz = (byte)(data & 0xF);
		}

		public byte GetData() => (byte)((vert << 4) | horz);

		public bool Equals(Heightmap other) => Horizontal == other.Horizontal && Vertical == other.Vertical;
	}

	public abstract class Entry : IComparable<Entry>
	{
		public ushort X { get; set; }
		public ushort Y { get; set; }

		public virtual int ByteSize => 2;

		public virtual void Load(byte[] file, int address, int regionX, int regionY)
		{
			X = (ushort)(file[address] * 8 + regionX * 256);
			Y = (ushort)(file[address + 1] * 8 + regionY * 256);
		}

		public virtual byte[] GetBytes(int regionX, int regionY)
		{
			var result = new byte[ByteSize];
			result[0] = (byte)Math.Round((X - regionX * 256) / 8d);
			result[1] = (byte)Math.Round((Y - regionY * 256) / 8d);
			return result;
		}

		public int CompareTo(Entry other)
		{
			var res = Y - other.Y;
			if (res == 0)
				res = X - other.X;
			return res;
		}

		public static List<T> ReadLayoutCompressed<T>(string filename) where T : Entry, new() => ReadLayoutCompressed<T>(System.IO.File.ReadAllBytes(filename));

		public static List<T> ReadLayoutCompressed<T>(byte[] file, int address = 0) where T : Entry, new() => ReadLayout<T>(Utility.DecompressRLData(file, address));

		public static List<T> ReadLayout<T>(string filename) where T : Entry, new() => ReadLayout<T>(System.IO.File.ReadAllBytes(filename));

		public static List<T> ReadLayout<T>(byte[] file, int address = 0) where T : Entry, new()
		{
			List<T> result = new List<T>();
			var width = BitConverter.ToInt32(file, address + 4);
			var height = BitConverter.ToInt32(file, address + 8);
			for (var ry = 0; ry < height; ++ry)
				for (var rx = 0; rx < width; ++rx)
				{
					var off = BitConverter.ToInt32(file, address + 0xC + ((ry * width) + rx) * 4);
					if (off != 0)
					{
						off += 4 + address;
						while (file[off] != 0xFF)
						{
							var obj = new T();
							obj.Load(file, off, rx, ry);
							off += obj.ByteSize;
							result.Add(obj);
						}
					}
				}
			return result;
		}

		public static void WriteLayoutCompressed<T>(List<T> items, string filename) where T : Entry => System.IO.File.WriteAllBytes(filename, WriteLayoutCompressed(items));

		public static byte[] WriteLayoutCompressed<T>(List<T> items) where T : Entry => Utility.CompressRLData(WriteLayout(items));

		public static void WriteLayout<T>(List<T> items, string filename) where T : Entry => System.IO.File.WriteAllBytes(filename, WriteLayout(items));

		public static byte[] WriteLayout<T>(List<T> items) where T : Entry
		{
			using (var ms = new System.IO.MemoryStream())
			using (var bw = new System.IO.BinaryWriter(ms))
			{
				if (items.Count == 0)
				{
					bw.Write(0x10);
					bw.Write(1);
					bw.Write(1);
					bw.Write(0);
					return ms.ToArray();
				}
				var xrgns = (items.Max(a => a.X) + 255) / 256;
				var yrgns = (items.Max(a => a.Y) + 255) / 256;
				var regions = new List<T>[xrgns, yrgns];
				for (var y = 0; y < yrgns; y++)
					for (var x = 0; x < xrgns; x++)
						regions[x, y] = new List<T>();
				foreach (var obj in items)
					if (obj.X >= 0 && obj.Y >= 0)
					{
						int rx = obj.X / 256;
						int ry = obj.Y / 256;
						if (typeof(T) == typeof(RingEntry))
						{
							if (rx > 0 && Math.Round((obj.X & 0xFF) / 8d) == 0)
								--rx;
							if (ry > 0 && Math.Round((obj.Y & 0xFF) / 8d) <= 1)
								--ry;
						}
						regions[rx, ry].Add(obj);
					}
				for (var y = 0; y < yrgns; y++)
					for (var x = 0; x < xrgns; x++)
						regions[x, y].Sort();
				bw.Write(0);
				bw.Write(xrgns);
				bw.Write(yrgns);
				bw.Write(new byte[xrgns * yrgns * 4]);
				var ptroff = 0xC;
				long objoff = xrgns * yrgns * 4 + 0xC;
				for (var y = 0; y < yrgns; ++y)
					for (var x = 0; x < xrgns; ++x)
					{
						var list = regions[x, y];
						if (list.Count > 0)
						{
							ms.Seek(ptroff, System.IO.SeekOrigin.Begin);
							bw.Write(objoff - 4);
							ms.Seek(objoff, System.IO.SeekOrigin.Begin);
							foreach (var obj in list)
								bw.Write(obj.GetBytes(x, y));
							bw.Write((byte)0xFF);
							objoff = ms.Position;
						}
						ptroff += 4;
					}
				ms.Seek(0, System.IO.SeekOrigin.Begin);
				bw.Write((uint)(ms.Length << 8));
				return ms.ToArray();
			}
		}
	}

	public abstract class TypeEntry : Entry
	{
		public byte Type { get; set; }

		public override int ByteSize => 3;

		public override void Load(byte[] file, int address, int regionX, int regionY)
		{
			base.Load(file, address, regionX, regionY);
			Type = file[address + 2];
		}

		public override byte[] GetBytes(int regionX, int regionY)
		{
			var result = base.GetBytes(Type, Type);
			result[2] = Type;
			return result;
		}
	}

	public abstract class Data4Entry : TypeEntry
	{
		public byte Data1 { get; set; }
		public byte Data2 { get; set; }
		public byte Data3 { get; set; }
		public byte Data4 { get; set; }

		public override int ByteSize => 7;

		public override void Load(byte[] file, int address, int regionX, int regionY)
		{
			base.Load(file, address, regionX, regionY);
			Data1 = file[address + 3];
			Data2 = file[address + 4];
			Data3 = file[address + 5];
			Data4 = file[address + 6];
		}

		public override byte[] GetBytes(int regionX, int regionY)
		{
			var result = base.GetBytes(regionX, regionY);
			result[3] = Data1;
			result[4] = Data2;
			result[5] = Data3;
			result[6] = Data4;
			return result;
		}
	}

	public abstract class Data5Entry : Data4Entry
	{
		public byte Data5 { get; set; }

		public override int ByteSize => 8;

		public override void Load(byte[] file, int address, int regionX, int regionY)
		{
			base.Load(file, address, regionX, regionY);
			Data5 = file[address + 7];
		}

		public override byte[] GetBytes(int regionX, int regionY)
		{
			var result = base.GetBytes(regionX, regionY);
			result[7] = Data5;
			return result;
		}
	}

	public interface IInteractable { }

	public class InteractableEntry12 : Data4Entry, IInteractable { }

	public class InteractableEntry3 : Data5Entry, IInteractable { }

	public class ItemEntry : TypeEntry { }

	public interface IEnemy { }

	public class EnemyEntry12 : Data4Entry, IEnemy { }

	public class EnemyEntry3 : Data5Entry, IEnemy { }

	public class RingEntry : Entry { }

	public class PlayerEntry : Entry
	{
		public override int ByteSize => 4;

		public override void Load(byte[] file, int address, int regionX, int regionY)
		{
			X = BitConverter.ToUInt16(file, address);
			Y = BitConverter.ToUInt16(file, address + 2);
		}

		public override byte[] GetBytes(int regionX, int regionY)
		{
			var result = new byte[ByteSize];
			BitConverter.GetBytes(X).CopyTo(result, 0);
			BitConverter.GetBytes(Y).CopyTo(result, 2);
			return result;
		}

		public static PlayerEntry Read(string filename)
		{
			var result = new PlayerEntry();
			result.Load(System.IO.File.ReadAllBytes(filename), 0, 0, 0);
			return result;
		}

		public void Write(string filename)
		{
			System.IO.File.WriteAllBytes(filename, GetBytes(0, 0));
		}
	}
}
