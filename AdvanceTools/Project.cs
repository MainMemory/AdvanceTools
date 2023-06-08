using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AdvanceTools
{
	public abstract class JsonBase
	{
		public void Save(string filename) => File.WriteAllText(filename, JsonConvert.SerializeObject(this));
	}

	public class ProjectFile : JsonBase
	{
		public int Game { get; set; }
		public VersionInfo Version { get; set; }
		public string[] Levels { get; set; }
		public string[] Backgrounds { get; set; }
		public string[] SpriteAnimations { get; set; }
		public string[] SpriteMappings { get; set; }
		public string[] SpriteAttributes { get; set; }
		public string SpritePalettes { get; set; }
		public string SpriteTiles16 { get; set; }
		public string SpriteTiles256 { get; set; }
		public Dictionary<string, FileInfo> Hashes { get; set; }

		public static ProjectFile Load(string filename) => JsonConvert.DeserializeObject<ProjectFile>(File.ReadAllText(filename));

		public LevelJson GetLevel(int id) => Levels[id] != null ? LevelJson.Load(Levels[id]) : null;

		public BackgroundLayerJson GetBackground(int id) => Backgrounds[id] != null ? BackgroundLayerJson.Load(Backgrounds[id]) : null;

		public AnimationCommand[][] GetSpriteAnimation(int id)
		{
			if (SpriteAnimations[id] == null)
				return null;
			var anims = AnimationJson.Load(SpriteAnimations[id]);
			AnimationCommand[][] result = new AnimationCommand[anims.Length][];
			for (int i = 0; i < SpriteAnimations[id].Length; i++)
				result[i] = AnimationCommand.LoadAnimation(File.ReadAllBytes(anims[i]), 0).ToArray();
			return result;
		}

		public MappingFrame[] GetSpriteMappings(int id) => SpriteMappings[id] != null ? MappingFrame.LoadFrames(SpriteMappings[id], Game).ToArray() : null;

		public SpriteAttributes[] GetSpriteAttributes(int id) => SpriteAttributes[id] != null ? AdvanceTools.SpriteAttributes.LoadAttributes(SpriteAttributes[id]).ToArray() : null;

		public GBAColor[] GetSpritePalettes() => GBAColor.Load(SpritePalettes);

		public byte[] GetSpriteTiles16() => File.ReadAllBytes(SpriteTiles16);
		public byte[] GetSpriteTiles256() => File.ReadAllBytes(SpriteTiles256);
	}

	public class FileInfo
	{
		[JsonConverter(typeof(IntHexConverter))]
		public int Address { get; set; }
		public string Hash { get; set; }

		public FileInfo() { }

		public FileInfo(int address, string hash)
		{
			Address = address;
			Hash = hash;
		}
	}

	public class LevelJson : JsonBase
	{
		public int ID { get; set; }
		public string ForegroundHigh { get; set; }
		public string ForegroundLow { get; set; }
		public string Background1 { get; set; }
		public string Background2 { get; set; }
		public string Collision { get; set; }
		public string Interactables { get; set; }
		public string Items { get; set; }
		public string Enemies { get; set; }
		public string Rings { get; set; }
		public string PlayerStart { get; set; }

		public static LevelJson Load(string filename) => JsonConvert.DeserializeObject<LevelJson>(File.ReadAllText(filename));

		public ForegroundLayerJson GetForegroundHigh() => ForegroundHigh != null ? ForegroundLayerJson.Load(ForegroundHigh) : null;

		public ForegroundLayerJson GetForegroundLow() => ForegroundLow != null ? ForegroundLayerJson.Load(ForegroundLow) : null;

		public BackgroundLayerJson GetBackground1() => Background1 != null ? BackgroundLayerJson.Load(Background1) : null;

		public BackgroundLayerJson GetBackground2() => Background2 != null ? BackgroundLayerJson.Load(Background2) : null;

		public CollisionJson GetCollision() => Collision != null ? CollisionJson.Load(Collision) : null;
	}

	public class CollisionJson : JsonBase
	{
		public string Heightmaps { get; set; }
		public string Angles { get; set; }
		public string Chunks { get; set; }
		public string ForegroundHigh { get; set; }
		public string ForegroundLow { get; set; }
		public string Flags { get; set; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public ushort WidthPixels { get; set; }
		public ushort HeightPixels { get; set; }

		public CollisionJson() { }

		public CollisionJson(LevelCollision collision)
		{
			Width = collision.Width;
			Height = collision.Height;
			WidthPixels = collision.WidthPixels;
			HeightPixels = collision.HeightPixels;
		}

		public static CollisionJson Load(string filename) => JsonConvert.DeserializeObject<CollisionJson>(File.ReadAllText(filename));

		public byte[] GetHeightmapsRaw() => File.ReadAllBytes(Heightmaps);

		public Heightmap[][] GetHeightmaps()
		{
			byte[] fc = GetHeightmapsRaw();
			Heightmap[][] result = new Heightmap[fc.Length / 8][];
			for (int i = 0; i < fc.Length / 8; i++)
			{
				result[i] = new Heightmap[8];
				for (int j = 0; j < 8; j++)
					result[i][j] = new Heightmap(fc[i * 8 + j]);
			}
			return result;
		}

		public byte[] GetAngles() => File.ReadAllBytes(Angles);

		public byte[] GetChunksRaw() => File.ReadAllBytes(Chunks);

		public TileIndex[][,] GetChunks()
		{
			byte[] data = GetChunksRaw();
			int chunksz = 12 * 12 * TileIndex.Size;
			List<TileIndex[,]> result = new List<TileIndex[,]>(data.Length / chunksz);
			int off = 0;
			while (off < data.Length)
			{
				TileIndex[,] chunk = new TileIndex[12, 12];
				for (int y = 0; y < 12; y++)
					for (int x = 0; x < 12; x++)
					{
						chunk[x, y] = new TileIndex(data, off);
						off += 2;
					}
				result.Add(chunk);
			}
			return result.ToArray();
		}

		private byte[] GetLayoutRaw(string layout) => File.ReadAllBytes(layout);

		private ushort[,] GetLayout(string layout, int game)
		{
			byte[] data = GetLayoutRaw(layout);
			ushort[,] result = new ushort[Width, Height];
			if (game == 1)
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = data[y * Width + x];
			}
			else
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = BitConverter.ToUInt16(data, (y * Width + x) * 2);
			}
			return result;
		}

		public byte[] GetForegroundHighRaw() => GetLayoutRaw(ForegroundHigh);

		public ushort[,] GetForegroundHigh(int game) => GetLayout(ForegroundHigh, game);

		public byte[] GetForegroundLowRaw() => GetLayoutRaw(ForegroundLow);

		public ushort[,] GetForegroundLow(int game) => GetLayout(ForegroundLow, game);

		public byte[] GetFlagsRaw() => File.ReadAllBytes(Flags);

		public byte[] GetFlags()
		{
			byte[] data = GetFlagsRaw();
			byte[] result = new byte[data.Length * 4];
			for (int i = 0; i < data.Length; i++)
			{
				result[i * 4] = (byte)(data[i] & 3);
				result[i * 4 + 1] = (byte)((data[i] >> 2) & 3);
				result[i * 4 + 2] = (byte)((data[i] >> 4) & 3);
				result[i * 4 + 3] = (byte)((data[i] >> 6) & 3);
			}
			return result;
		}
	}

	public abstract class LayerJsonBase
	{
		public string AniTiles { get; set; }
		public ushort AniTilesSize { get; set; }
		public byte AnimFrameCount { get; set; }
		public byte AnimDelay { get; set; }
		public string Tiles { get; set; }
		public string Palette { get; set; }
		public ushort PalDest { get; set; }
		public string Layout { get; set; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }

		public LayerJsonBase() { }

		public LayerJsonBase(LayerBase layer)
		{
			AniTilesSize = layer.AniTilesSize;
			AnimFrameCount = layer.AnimFrameCount;
			AnimDelay = layer.AnimDelay;
			PalDest = layer.PalDest;
			Width = layer.Width;
			Height = layer.Height;
		}

		public byte[] GetAniTiles() => AniTiles != null ? File.ReadAllBytes(AniTiles) : null;

		public byte[] GetTiles() => File.ReadAllBytes(Tiles);

		public GBAColor[] GetPalette()
		{
			if (Palette == null)
				return null;
			return GBAColor.Load(Palette);
		}

		public byte[] GetPaletteRaw() => Palette != null ? File.ReadAllBytes(Palette) : null;

		public byte[] GetLayoutRaw() => File.ReadAllBytes(Layout);
	}

	public class ForegroundLayerJson : LayerJsonBase
	{
		public ushort ChunkWidth { get; set; }
		public ushort ChunkHeight { get; set; }
		public string Chunks { get; set; }

		public ForegroundLayerJson() { }

		public ForegroundLayerJson(ForegroundLayer layer) : base(layer)
		{
			ChunkWidth = layer.ChunkWidth;
			ChunkHeight = layer.ChunkHeight;
		}

		public static ForegroundLayerJson Load(string filename) => JsonConvert.DeserializeObject<ForegroundLayerJson>(File.ReadAllText(filename));

		public ushort[,] GetLayout(int game)
		{
			byte[] data = GetLayoutRaw();
			ushort[,] result = new ushort[Width, Height];
			if (game == 1)
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = data[y * Width + x];
			}
			else
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = BitConverter.ToUInt16(data, (y * Width + x) * 2);
			}
			return result;
		}

		public TileIndex[][,] GetChunks()
		{
			byte[] data = GetChunksRaw();
			int chunksz = ChunkWidth * ChunkHeight * TileIndex.Size;
			List<TileIndex[,]> result = new List<TileIndex[,]>(data.Length / chunksz);
			int off = 0;
			while (off < data.Length)
			{
				TileIndex[,] chunk = new TileIndex[ChunkWidth, ChunkHeight];
				for (int y = 0; y < ChunkHeight; y++)
					for (int x = 0; x < ChunkWidth; x++)
					{
						chunk[x, y] = new TileIndex(data, off);
						off += 2;
					}
				result.Add(chunk);
			}
			return result.ToArray();
		}

		public byte[] GetChunksRaw() => File.ReadAllBytes(Chunks);
	}

	public class BackgroundLayerJson : LayerJsonBase
	{
		public BGMode Mode { get; set; }

		public BackgroundLayerJson() { }

		public BackgroundLayerJson(BackgroundLayer layer) : base(layer)
		{
			Mode = layer.Mode;
		}

		public static BackgroundLayerJson Load(string filename) => JsonConvert.DeserializeObject<BackgroundLayerJson>(File.ReadAllText(filename));

		public TileIndex[,] GetLayout()
		{
			byte[] data = GetLayoutRaw();
			TileIndex[,] result = new TileIndex[Width, Height];
			if (Mode == BGMode.Scale)
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = new TileIndex(data[y * Width + x], false, false, 0);
			}
			else
			{
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						result[x, y] = new TileIndex(data, (y * Width + x) * 2);
			}
			return result;
		}
	}

	public static class AnimationJson
	{
		public static string[] Load(string filename) => JsonConvert.DeserializeObject<string[]>(File.ReadAllText(filename));

		public static void Save(string filename, string[] variants) => File.WriteAllText(filename, JsonConvert.SerializeObject(variants));
	}
}
