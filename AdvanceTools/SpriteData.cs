using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdvanceTools
{
	public class SpriteTable
	{
		public int[] Animations { get; }
		public int[] Mappings { get; }
		public int[] Attributes { get; }
		public int Palette { get; }
		public int Tiles16 { get; }
		public int Tiles256 { get; }

		private readonly int aniptr;

		public SpriteTable(byte[] file, int address, int length)
		{
			Animations = new int[length];
			Mappings = new int[length];
			Attributes = new int[length];
			aniptr = Utility.GetPointer(file, address);
			int mapptr = Utility.GetPointer(file, address + 4);
			int atrptr = Utility.GetPointer(file, address + 8);
			for (int i = 0; i < length; i++)
			{
				Animations[i] = Utility.GetPointer(file, aniptr + (i * 4));
				Mappings[i] = Utility.GetPointer(file, mapptr + (i * 4));
				Attributes[i] = Utility.GetPointer(file, atrptr + (i * 4));
			}
			Palette = Utility.GetPointer(file, address + 0xC);
			Tiles16 = Utility.GetPointer(file, address + 0x10);
			Tiles256 = Utility.GetPointer(file, address + 0x14);
		}

		public AddressList<AddressList<AnimationCommand>> GetAnimation(byte[] file, int id)
		{
			int addr = Animations[id];
			if (addr == 0) return null;
			int addr2 = addr;
			AddressList<AddressList<AnimationCommand>> result = new AddressList<AddressList<AnimationCommand>>(addr);
			int ptr = Utility.GetPointer(file, addr2);
			while (ptr > 0 && ptr < file.Length && ptr != addr)
			{
				result.Add(AnimationCommand.LoadAnimation(file, ptr));
				addr2 += 4;
				if (addr2 == aniptr) break;
				ptr = Utility.GetPointer(file, addr2);
			}
			return result;
		}

		public AddressList<MappingFrame> GetMappings(byte[] file, int id, int count, int game)
		{
			int addr = Mappings[id];
			if (addr == 0) return null;
			return MappingFrame.LoadFrames(file, addr, count, game);
		}

		public AddressList<SpriteAttributes> GetAttributes(byte[] file, int id, int count)
		{
			int addr = Attributes[id];
			if (addr == 0) return null;
			return SpriteAttributes.LoadAttributes(file, addr, count);
		}
	}

	public abstract class AnimationCommand
	{
		public abstract int Size { get; }
		public virtual bool IsEnd => false;

		public abstract byte[] GetBytes();

		public static byte[] GetBytes(IEnumerable<AnimationCommand> anim)
		{
			List<byte> result = new List<byte>(anim.Sum(a => a.Size));
			foreach (var item in anim)
				result.AddRange(item.GetBytes());
			return result.ToArray();
		}

		public static byte[] GetAnimationRaw(byte[] file, int address)
		{
			int newaddr = address;
			int cmd = BitConverter.ToInt32(file, address);
			while (true)
			{
				if (cmd >= 0)
				{
					newaddr += 8;
				}
				else
				{
					bool isend = false;
					switch (cmd)
					{
						case -10:
							newaddr += 16;
							break;
						case -1:
						case -2:
						case -6:
						case -8:
							newaddr += 12;
							break;
						case -5:
						case -7:
						case -11:
						case -12:
							newaddr += 8;
							break;
						case -3:
						case -9:
							newaddr += 8;
							isend = true;
							break;
						case -4:
							newaddr += 4;
							isend = true;
							break;
						default:
							throw new FormatException($"Unknown animation command {cmd}!");
					}
					if (isend)
					{
						var result = new byte[newaddr - address];
						Array.Copy(file, address, result, 0, result.Length);
						return result;
					}
				}
			}
		}

		public static AddressList<AnimationCommand> LoadAnimation(byte[] file, int address)
		{
			AddressList<AnimationCommand> result = new AddressList<AnimationCommand>(address);
			AnimationCommand cmd = Load(file, address);
			result.Add(cmd);
			while (!cmd.IsEnd)
			{
				address += cmd.Size;
				cmd = Load(file, address);
				result.Add(cmd);
			}
			return result;
		}

		public static AddressList<AnimationCommand> LoadAnimation(string filename) => LoadAnimation(System.IO.File.ReadAllBytes(filename), 0);

		public static AnimationCommand Load(byte[] file, int address)
		{
			int cmd = BitConverter.ToInt32(file, address);
			if (cmd >= 0)
				return new AnimationCommandDrawFrame(file, address);
			switch (cmd)
			{
				case -1:
					return new AnimationCommandGetTiles(file, address);
				case -2:
					return new AnimationCommandGetPalette(file, address);
				case -3:
					return new AnimationCommandJumpBack(file, address);
				case -4:
					return new AnimationCommandEnd(file, address);
				case -5:
					return new AnimationCommandPlaySFX(file, address);
				case -6:
					return new AnimationCommand_6(file, address);
				case -7:
					return new AnimationCommandTranslateSprite(file, address);
				case -8:
					return new AnimationCommand_8(file, address);
				case -9:
					return new AnimationCommandChangeAnim(file, address);
				case -10:
					return new AnimationCommand_10(file, address);
				case -11:
					return new AnimationCommand_11(file, address);
				case -12:
					return new AnimationCommand_12(file, address);
				default:
					throw new FormatException($"Unknown animation command {cmd}!");
			}
		}

		public static int GetMappingsCount(IEnumerable<AnimationCommand> anim) => Math.Max(anim.OfType<AnimationCommandDrawFrame>().Max(a => a.MappingIndex) + 1, 1);
	}

	public class AnimationCommandDrawFrame : AnimationCommand
	{
		public int Delay { get; set; }
		public int MappingIndex { get; set; }

		public override int Size => 8;

		public AnimationCommandDrawFrame(byte[] file, int address)
		{
			Delay = BitConverter.ToInt32(file, address);
			MappingIndex = BitConverter.ToInt32(file, address + 4);
		}

		public override byte[] GetBytes()
		{
			byte[] result = new byte[Size];
			BitConverter.GetBytes(Delay).CopyTo(result, 0);
			BitConverter.GetBytes(MappingIndex).CopyTo(result, 4);
			return result;
		}
	}

	public abstract class AnimationCommandID : AnimationCommand
	{
		public int CommandID { get; }

		public AnimationCommandID(byte[] file, int address)
		{
			CommandID = BitConverter.ToInt32(file, address);
		}

		public override byte[] GetBytes()
		{
			byte[] result = new byte[Size];
			BitConverter.GetBytes(CommandID).CopyTo(result, 0);
			return result;
		}
	}

	public class AnimationCommandGetTiles : AnimationCommandID, IEquatable<AnimationCommandGetTiles>
	{
		public bool Color256 { get; set; }
		private uint _ind;
		public uint TileIndex
		{
			get => _ind;
			set => _ind = value & 0x7FFFFFFF;
		}
		public uint TileCount { get; set; }

		public override int Size => 12;

		public AnimationCommandGetTiles(byte[] file, int address) : base(file, address)
		{
			var val = BitConverter.ToUInt32(file, address + 4);
			TileIndex = val;
			Color256 = (val & 0x80000000) == 0x80000000;
			TileCount = BitConverter.ToUInt32(file, address + 8);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			uint val = TileIndex;
			if (Color256)
				val |= 0x80000000;
			BitConverter.GetBytes(val).CopyTo(result, 4);
			BitConverter.GetBytes(TileCount).CopyTo(result, 8);
			return result;
		}

		public bool Equals(AnimationCommandGetTiles other)
		{
			if (other == null) return false;
			return CommandID == other.CommandID && Color256 == other.Color256 && TileIndex == other.TileIndex && TileCount == other.TileCount;
		}
	}

	public class AnimationCommandGetPalette : AnimationCommandID, IEquatable<AnimationCommandGetPalette>
	{
		public int PaletteIndex { get; set; }
		public short PaletteSize { get; set; }
		public short PaletteDest { get; set; }

		public override int Size => 12;

		public AnimationCommandGetPalette(byte[] file, int address) : base(file, address)
		{
			PaletteIndex = BitConverter.ToInt32(file, address + 4);
			PaletteSize = BitConverter.ToInt16(file, address + 8);
			PaletteDest = BitConverter.ToInt16(file, address + 10);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(PaletteIndex).CopyTo(result, 4);
			BitConverter.GetBytes(PaletteSize).CopyTo(result, 8);
			BitConverter.GetBytes(PaletteDest).CopyTo(result, 10);
			return result;
		}

		public bool Equals(AnimationCommandGetPalette other)
		{
			if (other == null) return false;
			return CommandID == other.CommandID && PaletteIndex == other.PaletteIndex && PaletteSize == other.PaletteSize && PaletteDest == other.PaletteDest;
		}
	}

	public class AnimationCommandJumpBack : AnimationCommandID
	{
		public int Offset { get; set; }

		public override int Size => 8;
		public override bool IsEnd => true;

		public AnimationCommandJumpBack(byte[] file, int address) : base(file, address)
		{
			Offset = BitConverter.ToInt32(file, address + 4);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(Offset).CopyTo(result, 4);
			return result;
		}
	}

	public class AnimationCommandEnd : AnimationCommandID
	{
		public override int Size => 4;
		public override bool IsEnd => true;

		public AnimationCommandEnd(byte[] file, int address) : base(file, address) { }
	}

	public class AnimationCommandPlaySFX : AnimationCommandID
	{
		public ushort SoundID { get; set; }

		public override int Size => 8;

		public AnimationCommandPlaySFX(byte[] file, int address) : base(file, address)
		{
			SoundID = BitConverter.ToUInt16(file, address + 4);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(SoundID).CopyTo(result, 4);
			return result;
		}
	}

	public class AnimationCommand_6 : AnimationCommandID
	{
		public int Unknown1 { get; set; }
		public int Unknown2 { get; set; }

		public override int Size => 12;

		public AnimationCommand_6(byte[] file, int address) : base(file, address)
		{
			Unknown1 = BitConverter.ToInt32(file, address + 4);
			Unknown2 = BitConverter.ToInt32(file, address + 8);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(Unknown1).CopyTo(result, 4);
			BitConverter.GetBytes(Unknown2).CopyTo(result, 8);
			return result;
		}
	}

	public class AnimationCommandTranslateSprite : AnimationCommandID
	{
		public short X { get; set; }
		public short Y { get; set; }

		public override int Size => 8;

		public AnimationCommandTranslateSprite(byte[] file, int address) : base(file, address)
		{
			X = BitConverter.ToInt16(file, address + 4);
			Y = BitConverter.ToInt16(file, address + 6);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(X).CopyTo(result, 4);
			BitConverter.GetBytes(Y).CopyTo(result, 6);
			return result;
		}
	}

	public class AnimationCommand_8 : AnimationCommandID
	{
		public int Unknown1 { get; set; }
		public int Unknown2 { get; set; }

		public override int Size => 12;

		public AnimationCommand_8(byte[] file, int address) : base(file, address)
		{
			Unknown1 = BitConverter.ToInt32(file, address + 4);
			Unknown2 = BitConverter.ToInt32(file, address + 8);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(Unknown1).CopyTo(result, 4);
			BitConverter.GetBytes(Unknown2).CopyTo(result, 8);
			return result;
		}
	}

	public class AnimationCommandChangeAnim : AnimationCommandID
	{
		public ushort AnimId { get; set; }
		public ushort Variant { get; set; }

		public override int Size => 8;
		public override bool IsEnd => true;

		public AnimationCommandChangeAnim(byte[] file, int address) : base(file, address)
		{
			AnimId = BitConverter.ToUInt16(file, address + 4);
			Variant = BitConverter.ToUInt16(file, address + 6);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(AnimId).CopyTo(result, 4);
			BitConverter.GetBytes(Variant).CopyTo(result, 6);
			return result;
		}
	}

	public class AnimationCommand_10 : AnimationCommandID
	{
		public int Unknown1 { get; set; }
		public int Unknown2 { get; set; }
		public int Unknown3 { get; set; }

		public override int Size => 16;

		public AnimationCommand_10(byte[] file, int address) : base(file, address)
		{
			Unknown1 = BitConverter.ToInt32(file, address + 4);
			Unknown2 = BitConverter.ToInt32(file, address + 8);
			Unknown3 = BitConverter.ToInt32(file, address + 12);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(Unknown1).CopyTo(result, 4);
			BitConverter.GetBytes(Unknown2).CopyTo(result, 8);
			BitConverter.GetBytes(Unknown3).CopyTo(result, 12);
			return result;
		}
	}

	public class AnimationCommand_11 : AnimationCommandID
	{
		public int Unknown { get; set; }

		public override int Size => 8;

		public AnimationCommand_11(byte[] file, int address) : base(file, address)
		{
			Unknown = BitConverter.ToInt32(file, address + 4);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(Unknown).CopyTo(result, 4);
			return result;
		}
	}

	public class AnimationCommand_12 : AnimationCommandID
	{
		public int Unknown { get; set; }

		public override int Size => 8;

		public AnimationCommand_12(byte[] file, int address) : base(file, address)
		{
			Unknown = BitConverter.ToInt32(file, address + 4);
		}

		public override byte[] GetBytes()
		{
			byte[] result = base.GetBytes();
			BitConverter.GetBytes(Unknown).CopyTo(result, 4);
			return result;
		}
	}

	public class MappingFrame
	{
		public bool XFlip { get; set; }
		public bool YFlip { get; set; }
		public ushort AttributeIndex { get; set; }
		public ushort SpriteCount { get; set; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public short X { get; set; }
		public short Y { get; set; }

		public static int Size => 12;

		public MappingFrame(byte[] file, int address, int game)
		{
			if (game < 3)
			{
				XFlip = (file[address] & 1) == 1;
				YFlip = (file[address] & 2) == 2;
				if ((file[address] & ~3) != 0)
					Console.WriteLine("Found flags {0:X2} at address {1:X}!", file[address], address);
				AttributeIndex = file[address + 1];
			}
			else
			{
				AttributeIndex = BitConverter.ToUInt16(file, address);
				XFlip = (AttributeIndex & 0x4000) == 0x4000;
				YFlip = (AttributeIndex & 0x8000) == 0x8000;
				AttributeIndex &= 0x3FFF;
			}
			SpriteCount = BitConverter.ToUInt16(file, address + 2);
			Width = BitConverter.ToUInt16(file, address + 4);
			Height = BitConverter.ToUInt16(file, address + 6);
			X = BitConverter.ToInt16(file, address + 8);
			Y = BitConverter.ToInt16(file, address + 10);
		}

		public static AddressList<MappingFrame> LoadFrames(byte[] file, int address, int count, int game)
		{
			AddressList<MappingFrame> result = new AddressList<MappingFrame>(address, count);
			for (int i = 0; i < count; i++)
			{
				result.Add(new MappingFrame(file, address, game));
				address += Size;
			}
			return result;
		}

		public static AddressList<MappingFrame> LoadFrames(string filename, int game)
		{
			var data = System.IO.File.ReadAllBytes(filename);
			return LoadFrames(data, 0, data.Length / Size, game);
		}

		public byte[] GetBytes(int game)
		{
			byte[] result = new byte[Size];
			if (game < 3)
			{
				if (XFlip)
					result[0] |= 1;
				if (YFlip)
					result[0] |= 2;
				result[1] = (byte)AttributeIndex;
			}
			else
			{
				ushort val = AttributeIndex;
				if (XFlip)
					val |= 0x4000;
				if (YFlip)
					val |= 0x8000;
				BitConverter.GetBytes(val).CopyTo(result, 0);
			}
			BitConverter.GetBytes(SpriteCount).CopyTo(result, 2);
			BitConverter.GetBytes(Width).CopyTo(result, 4);
			BitConverter.GetBytes(Height).CopyTo(result, 6);
			BitConverter.GetBytes(X).CopyTo(result, 8);
			BitConverter.GetBytes(Y).CopyTo(result, 10);
			return result;
		}

		public static byte[] GetBytes(IEnumerable<MappingFrame> frames, int game)
		{
			List<byte> result = new List<byte>(frames.Count() * Size);
			foreach (var item in frames)
				result.AddRange(item.GetBytes(game));
			return result.ToArray();
		}

		public static int GetAttributesCount(IEnumerable<MappingFrame> frames)
		{
			if (!frames.Any()) return 1;
			return frames.Max(a => a.AttributeIndex + a.SpriteCount) + 1;
		}
	}

	public class SpriteAttributes
	{
		public sbyte Y { get; set; }
		public bool RotScl { get; set; }
		public bool DoubleSize { get; set; }
		public bool Disable { get; set; }
		public ObjModes Mode { get; set; }
		public bool Mosaic { get; set; }
		public bool Color256 { get; set; }
		public ObjShapes Shape { get; set; }
		private short _x;
		public short X
		{
			get => _x;
			set
			{
				_x = (short)(value & 0xFF);
				if (value < 0)
					_x |= ~0xFF;
			}
		}
		private byte _rotsclparam;
		public byte RotSclParam
		{
			get => _rotsclparam;
			set => _rotsclparam = (byte)(value & 0x1F);
		}
		public bool XFlip { get; set; }
		public bool YFlip { get; set; }
		private byte _size;
		public byte Size
		{
			get => _size;
			set => _size = (byte)(value & 3);
		}
		private ushort _ind;
		public ushort Tile
		{
			get => _ind;
			set => _ind = (ushort)(value & 0x3FF);
		}
		private byte _pri;
		public byte Priority
		{
			get => _pri;
			set => _pri = (byte)(value & 3);
		}
		private byte _pal;
		public byte Palette
		{
			get => _pal;
			set => _pal = (byte)(value & 0xF);
		}

		private static readonly Size[,] spriteSizes = new Size[,]
		{
			{
				new Size(1, 1),
				new Size(2, 2),
				new Size(4, 4),
				new Size(8, 8)
			},
			{
				new Size(2, 1),
				new Size(4, 1),
				new Size(4, 2),
				new Size(8, 4)
			},
			{
				new Size(1, 2),
				new Size(1, 4),
				new Size(2, 4),
				new Size(4, 8)
			}
		};
		public Size GetTileSize() => spriteSizes[(int)Shape, Size];

		public static int ByteSize => 6;

		public SpriteAttributes(byte[] file, int address)
		{
			var val = BitConverter.ToUInt16(file, address);
			Y = unchecked((sbyte)(val & 0xFF));
			RotScl = (val & 0x100) == 0x100;
			if (RotScl)
				DoubleSize = (val & 0x200) == 0x200;
			else
				Disable = (val & 0x200) == 0x200;
			Mode = (ObjModes)((val & 0xC00) >> 10);
			Mosaic = (val & 0x1000) == 0x1000;
			Color256 = (val & 0x2000) == 0x2000;
			Shape = (ObjShapes)((val & 0xC000) >> 14);
			val = BitConverter.ToUInt16(file, address + 2);
			X = (short)(val & 0xFF);
			if ((val & 0x100) == 0x100)
				X |= ~0xFF;
			if (RotScl)
				RotSclParam = (byte)(val >> 9);
			else
			{
				XFlip = (val & 0x1000) == 0x1000;
				YFlip = (val & 0x2000) == 0x2000;
			}
			Size = (byte)(val >> 14);
			val = BitConverter.ToUInt16(file, address + 4);
			Tile = val;
			Priority = (byte)(val >> 10);
			Palette = (byte)(val >> 12);
		}

		public static AddressList<SpriteAttributes> LoadAttributes(byte[] file, int address, int count)
		{
			AddressList<SpriteAttributes> result = new AddressList<SpriteAttributes>(address, count);
			for (int i = 0; i < count; i++)
			{
				result.Add(new SpriteAttributes(file, address));
				address += ByteSize;
			}
			return result;
		}

		public static AddressList<SpriteAttributes> LoadAttributes(string filename)
		{
			var data = System.IO.File.ReadAllBytes(filename);
			return LoadAttributes(data, 0, data.Length / ByteSize);
		}

		public byte[] GetBytes()
		{
			byte[] result = new byte[ByteSize];
			ushort val = unchecked((byte)Y);
			if (RotScl)
			{
				val |= 0x100;
				if (DoubleSize)
					val |= 0x200;
			}
			else if (Disable)
				val |= 0x200;
			val |= (ushort)((int)Mode << 10);
			if (Mosaic)
				val |= 0x1000;
			if (Color256)
				val |= 0x2000;
			val |= (ushort)((int)Shape << 14);
			BitConverter.GetBytes(val).CopyTo(result, 0);
			val = (ushort)(X & 0x1FF);
			if (RotScl)
				val |= (ushort)(RotSclParam << 9);
			else
			{
				if (XFlip)
					val |= 0x1000;
				if (YFlip)
					val |= 0x2000;
			}
			val |= (ushort)(Size << 14);
			BitConverter.GetBytes(val).CopyTo(result, 2);
			val = Tile;
			val |= (ushort)(Priority << 10);
			val |= (ushort)(Palette << 12);
			BitConverter.GetBytes(val).CopyTo(result, 4);
			return result;
		}

		public static byte[] GetBytes(IEnumerable<SpriteAttributes> attributes)
		{
			List<byte> result = new List<byte>(attributes.Count() * ByteSize);
			foreach (var item in attributes)
				result.AddRange(item.GetBytes());
			return result.ToArray();
		}
	}

	public enum ObjModes
	{
		Normal,
		SemiTransparent,
		Window,
		Prohibited
	}

	public enum ObjShapes
	{
		Square,
		Horizontal,
		Vertical,
		Prohibited
	}
}
