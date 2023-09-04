using AdvanceTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceExtract
{
	static class Program
	{
		static Dictionary<int, string> fileList = new Dictionary<int, string>();
		static SortedDictionary<int, string> chunkFiles = new SortedDictionary<int, string>();

		static void Main(string[] args)
		{
			string filename;
			if (args.Length > 0)
				filename = args[0];
			else
			{
				Console.Write("File: ");
				filename = Console.ReadLine().Trim('"');
			}
			byte[] file = File.ReadAllBytes(filename);
			var id = Encoding.ASCII.GetString(file, 0xAC, 3);
			var game = GameInfo.Read("GameInfo.json").SingleOrDefault(a => a.Code.Equals(id, StringComparison.Ordinal));
			if (game == null)
			{
				Console.WriteLine("File does not match any known Sonic Advance game.");
				return;
			}
			var version = game.Versions.SingleOrDefault(a => a.Region == (char)file[0xAF] && a.Version == file[0xBC]);
			if (version == null)
			{
				Console.WriteLine("File does not match any known version of the game.");
				return;
			}
			string extract = Path.ChangeExtension(filename, null) + "_extract";
			Directory.CreateDirectory(extract);
			Directory.CreateDirectory(Path.Combine(extract, "Tiles"));
			Directory.CreateDirectory(Path.Combine(extract, "AniTiles"));
			Directory.CreateDirectory(Path.Combine(extract, "Chunks"));
			Directory.CreateDirectory(Path.Combine(extract, "Palettes"));
			Directory.CreateDirectory(Path.Combine(extract, "Layout"));
			Directory.CreateDirectory(Path.Combine(extract, "Layout", "Foreground"));
			Directory.CreateDirectory(Path.Combine(extract, "Layout", "Background"));
			Directory.CreateDirectory(Path.Combine(extract, "Layout", "Objects"));
			Directory.CreateDirectory(Path.Combine(extract, "Layout", "Objects", "Interactables"));
			Directory.CreateDirectory(Path.Combine(extract, "Layout", "Objects", "Items"));
			Directory.CreateDirectory(Path.Combine(extract, "Layout", "Objects", "Enemies"));
			Directory.CreateDirectory(Path.Combine(extract, "Layout", "Objects", "Rings"));
			Directory.CreateDirectory(Path.Combine(extract, "Layout", "Objects", "Player"));
			Directory.CreateDirectory(Path.Combine(extract, "Levels"));
			Directory.CreateDirectory(Path.Combine(extract, "Foregrounds"));
			Directory.CreateDirectory(Path.Combine(extract, "Backgrounds"));
			Directory.CreateDirectory(Path.Combine(extract, "Collision"));
			Directory.CreateDirectory(Path.Combine(extract, "Collision", "Info"));
			Directory.CreateDirectory(Path.Combine(extract, "Collision", "Heightmaps"));
			Directory.CreateDirectory(Path.Combine(extract, "Collision", "Angles"));
			Directory.CreateDirectory(Path.Combine(extract, "Collision", "Flags"));
			Directory.CreateDirectory(Path.Combine(extract, "Sprites"));
			Directory.CreateDirectory(Path.Combine(extract, "Sprites", "Animations"));
			Directory.CreateDirectory(Path.Combine(extract, "Sprites", "Mappings"));
			Directory.CreateDirectory(Path.Combine(extract, "Sprites", "Attributes"));
			JsonConvert.DefaultSettings = SetDefaults;
			var proj = new ProjectFile
			{
				Game = game.Game,
				Version = version,
				Levels = new string[game.LevelNames.Length],
				Backgrounds = new string[game.Backgrounds.Length]
			};
			for (int lv = 0; lv < game.LevelNames.Length; lv++)
			{
				string lvlname = game.LevelNames[lv];
				if (lvlname == null)
					continue;
				var json = new LevelJson { ID = lv };
				if (version.MapTable != 0)
				{
					var artptrs = new LevelArtPointers(file, version.MapTable + lv * LevelArtPointers.GetSize(game.Game), game.Game);
					if (!fileList.ContainsKey(artptrs.ForegroundHigh))
					{
						var layer = artptrs.ReadForegroundHigh(file);
						if (layer != null)
							json.ForegroundHigh = ProcessForegroundLayer(file, extract, $"{lvlname} FG High", layer, game.Game);
					}
					else
						json.ForegroundHigh = fileList[artptrs.ForegroundHigh];
					if (!fileList.ContainsKey(artptrs.ForegroundLow))
					{
						var layer = artptrs.ReadForegroundLow(file);
						if (layer != null)
							json.ForegroundLow = ProcessForegroundLayer(file, extract, $"{lvlname} FG Low", layer, game.Game);
					}
					else
						json.ForegroundLow = fileList[artptrs.ForegroundLow];
					if (!fileList.ContainsKey(artptrs.Background1))
					{
						var layer = artptrs.ReadBackground1(file);
						if (layer != null)
							json.Background1 = ProcessBackgroundLayer(file, extract, $"{lvlname} {(game.Game == 3 ? "BG 1" : "BG")}", layer);
					}
					else
						json.Background1 = fileList[artptrs.Background1];
					if (game.Game == 3)
					{
						if (!fileList.ContainsKey(artptrs.Background2))
						{
							var layer = artptrs.ReadBackground2(file);
							if (layer != null)
								json.Background2 = ProcessBackgroundLayer(file, extract, $"{lvlname} BG 2", layer);
						}
						else
							json.Background2 = fileList[artptrs.Background2];
					}
				}
				if (version.CollisionTable != 0)
				{
					var ptr = Utility.GetPointer(file, version.CollisionTable + lv * 4);
					if (!fileList.ContainsKey(ptr))
					{
						if (ptr != 0)
							json.Collision = ProcessCollision(file, extract, lvlname, new LevelCollision(file, ptr, game.Game), game.Game);
					}
					else
						json.Collision = fileList[ptr];
				}
				if (version.InteractableTable != 0)
				{
					var ptr = Utility.GetPointer(file, version.InteractableTable + lv * 4);
					if (!fileList.ContainsKey(ptr))
					{
						if (ptr != 0)
						{
							string path = Path.Combine("Layout", "Objects", "Interactables", lvlname + ".bin");
							fileList.Add(ptr, path);
							json.Interactables = path;
							File.WriteAllBytes(Path.Combine(extract, path), Utility.DecompressRLData(file, ptr));
						}
					}
					else
						json.Interactables = fileList[ptr];
				}
				if (version.ItemTable != 0)
				{
					var ptr = Utility.GetPointer(file, version.ItemTable + lv * 4);
					if (!fileList.ContainsKey(ptr))
					{
						if (ptr != 0)
						{
							string path = Path.Combine("Layout", "Objects", "Items", lvlname + ".bin");
							fileList.Add(ptr, path);
							json.Items = path;
							File.WriteAllBytes(Path.Combine(extract, path), Utility.DecompressRLData(file, ptr));
						}
					}
					else
						json.Items = fileList[ptr];
				}
				if (version.EnemyTable != 0)
				{
					var ptr = Utility.GetPointer(file, version.EnemyTable + lv * 4);
					if (!fileList.ContainsKey(ptr))
					{
						if (ptr != 0)
						{
							string path = Path.Combine("Layout", "Objects", "Enemies", lvlname + ".bin");
							fileList.Add(ptr, path);
							json.Enemies = path;
							File.WriteAllBytes(Path.Combine(extract, path), Utility.DecompressRLData(file, ptr));
						}
					}
					else
						json.Enemies = fileList[ptr];
				}
				if (version.RingTable != 0)
				{
					var ptr = Utility.GetPointer(file, version.RingTable + lv * 4);
					if (!fileList.ContainsKey(ptr))
					{
						if (ptr != 0)
						{
							string path = Path.Combine("Layout", "Objects", "Rings", lvlname + ".bin");
							fileList.Add(ptr, path);
							json.Rings = path;
							File.WriteAllBytes(Path.Combine(extract, path), Utility.DecompressRLData(file, ptr));
						}
					}
					else
						json.Rings = fileList[ptr];
				}
				if (version.StartTable != 0)
				{
					var ptr = version.StartTable + lv * 4;
					if (game.Game == 3)
						ptr = Utility.GetPointer(file, ptr);
					if (!fileList.ContainsKey(ptr))
					{
						if (ptr != 0)
						{
							string path = Path.Combine("Layout", "Objects", "Player", lvlname + ".bin");
							fileList.Add(ptr, path);
							json.PlayerStart = path;
							byte[] data = new byte[4];
							Array.Copy(file, ptr, data, 0, 4);
							File.WriteAllBytes(Path.Combine(extract, path), data);
						}
					}
					else
						json.PlayerStart = fileList[ptr];
				}
				proj.Levels[lv] = Path.Combine("Levels", lvlname + ".salv");
				json.Save(Path.Combine(extract, proj.Levels[lv]));
			}
			if (version.MapTable != 0)
			{
				int bgstart = version.MapTable + (game.LevelNames.Length * LevelArtPointers.GetSize(game.Game));
				for (int bg = 0; bg < game.Backgrounds.Length; bg++)
				{
					BackgroundInfo bginf = game.Backgrounds[bg];
					if (bginf == null)
						continue;
					var ptr = Utility.GetPointer(file, bgstart + (bg * 4));
					if (ptr != 0)
					{
						string fn;
						if (!fileList.ContainsKey(ptr))
							fn = ProcessBackgroundLayer(file, extract, bginf.Name, new BackgroundLayer(file, ptr, bginf.Mode));
						else
							fn = fileList[ptr];
						proj.Backgrounds[bg] = fn;
					}
				}
			}
			if (version.SpriteTable != 0)
			{
				var spriteTable = new SpriteTable(file, version.SpriteTable, game.AnimationCount);
				List<string> animfiles = new List<string>();
				List<string> mapfiles = new List<string>();
				List<string> attrfiles = new List<string>();
				int palcnt = 0;
				uint tile16cnt = 0;
				uint tile256cnt = 0;
				for (int i = 0; i < game.AnimationCount; i++)
				{
					var anims = spriteTable.GetAnimation(file, i);
					if (anims != null)
					{
						List<string> anmvars = new List<string>();
						string path;
						int mapcnt = 0;
						for (int j = 0; j < anims.Count; j++)
						{
							if (!fileList.ContainsKey(anims[j].Address))
							{
								path = Path.Combine("Sprites", "Animations", $"{i}-{j}.bin");
								anmvars.Add(path);
								File.WriteAllBytes(Path.Combine(extract, path), AnimationCommand.GetBytes(anims[j]));
								fileList.Add(anims[j].Address, path);
							}
							else
								anmvars.Add(fileList[anims[j].Address]);
							for (int k = 0; k < anims[j].Count; k++)
							{
								switch (anims[j][k])
								{
									case AnimationCommandDrawFrame acdf:
										mapcnt = Math.Max(mapcnt, acdf.MappingIndex + 1);
										break;
									case AnimationCommandGetTiles acgt:
										if (acgt.Color256)
											tile256cnt = Math.Max(tile256cnt, acgt.TileIndex + acgt.TileCount);
										else
											tile16cnt = Math.Max(tile16cnt, acgt.TileIndex + acgt.TileCount);
										break;
									case AnimationCommandGetPalette acgp:
										palcnt = Math.Max(palcnt, acgp.PaletteIndex * 16 + acgp.PaletteSize);
										break;
								}
							}
						}
						if (!fileList.ContainsKey(anims.Address))
						{
							path = Path.Combine("Sprites", "Animations", $"{i}.sanm");
							animfiles.Add(path);
							AnimationJson.Save(Path.Combine(extract, path), anmvars.ToArray());
							fileList.Add(anims.Address, path);
						}
						else
							animfiles.Add(fileList[anims.Address]);
						var maps = spriteTable.GetMappings(file, i, mapcnt, game.Game);
						if (!fileList.ContainsKey(maps.Address))
						{
							path = Path.Combine("Sprites", "Mappings", $"{i}.bin");
							mapfiles.Add(path);
							File.WriteAllBytes(Path.Combine(extract, path), MappingFrame.GetBytes(maps, game.Game));
							fileList.Add(maps.Address, path);
						}
						else
							mapfiles.Add(fileList[maps.Address]);
						var attrs = spriteTable.GetAttributes(file, i, MappingFrame.GetAttributesCount(maps));
						if (!fileList.ContainsKey(attrs.Address))
						{
							path = Path.Combine("Sprites", "Attributes", $"{i}.bin");
							attrfiles.Add(path);
							File.WriteAllBytes(Path.Combine(extract, path), SpriteAttributes.GetBytes(attrs));
							fileList.Add(attrs.Address, path);
						}
						else
							attrfiles.Add(fileList[attrs.Address]);
					}
					else
					{
						animfiles.Add(null);
						mapfiles.Add(null);
						attrfiles.Add(null);
					}
				}
				proj.SpriteAnimations = animfiles.ToArray();
				proj.SpriteMappings = mapfiles.ToArray();
				proj.SpriteAttributes = attrfiles.ToArray();
				proj.SpritePalettes = Path.Combine("Palettes", "Sprites.bin");
				byte[] tmp = new byte[palcnt * 2];
				Array.Copy(file, spriteTable.Palette, tmp, 0, tmp.Length);
				File.WriteAllBytes(Path.Combine(extract, proj.SpritePalettes), tmp);
				fileList.Add(spriteTable.Palette, proj.SpritePalettes);
				proj.SpriteTiles16 = Path.Combine("Tiles", "Sprites16.bin");
				tmp = new byte[tile16cnt * 0x20];
				Array.Copy(file, spriteTable.Tiles16, tmp, 0, tmp.Length);
				File.WriteAllBytes(Path.Combine(extract, proj.SpriteTiles16), tmp);
				fileList.Add(spriteTable.Tiles16, proj.SpriteTiles16);
				if (tile256cnt > 0)
				{
					proj.SpriteTiles256 = Path.Combine("Tiles", "Sprites256.bin");
					tmp = new byte[tile256cnt * 0x40];
					Array.Copy(file, spriteTable.Tiles256, tmp, 0, tmp.Length);
					File.WriteAllBytes(Path.Combine(extract, proj.SpriteTiles256), tmp);
					fileList.Add(spriteTable.Tiles256, proj.SpriteTiles256);
				}
			}
			var addrs = fileList.Keys.ToArray();
			Array.Sort(addrs);
			foreach (var chunk in chunkFiles)
			{
				var i = Array.BinarySearch(addrs, chunk.Key);
				if (i < 0)
					i = ~i;
				else
					++i;
				int size;
				if (i < addrs.Length)
					size = addrs[i] - chunk.Key;
				else
					size = (game.Game == 1 ? 256 : 768) * 12 * 12 * 2;
				byte[] data = new byte[size];
				Array.Copy(file, chunk.Key, data, 0, size);
				File.WriteAllBytes(Path.Combine(extract, chunk.Value), data);
			}
			proj.Hashes = new Dictionary<string, AdvanceTools.FileInfo>(fileList.Count);
			foreach (var fn in fileList)
				proj.Hashes.Add(fn.Value, new AdvanceTools.FileInfo(fn.Key, Utility.HashFile(Path.Combine(extract, fn.Value)), (uint)new System.IO.FileInfo(Path.Combine(extract, fn.Value)).Length));
			proj.Save(Path.Combine(extract, Path.GetFileNameWithoutExtension(filename) + ".saproj"));
			if (Directory.Exists($@"Tiled\sa{game.Game}-tiled"))
				CopyDirectory($@"Tiled\sa{game.Game}-tiled", Path.Combine(extract, $"sa{game.Game}-tiled"));
		}

		private static void CopyDirectory(string source, string destination) => new DirectoryInfo(source).CopyTo(destination);

		private static void CopyTo(this DirectoryInfo source, string destination)
		{
			Directory.CreateDirectory(destination);
			foreach (var dir in source.EnumerateDirectories())
				dir.CopyTo(Path.Combine(destination, dir.Name));
			foreach (var file in source.EnumerateFiles())
				file.CopyTo(Path.Combine(destination, file.Name), true);
		}

		private static JsonSerializerSettings SetDefaults() => new JsonSerializerSettings() { Formatting = Formatting.Indented };

		private static void ProcessLayerBase(byte[] file, string extract, string name, LayerBase layer, LayerJsonBase json)
		{
			if (!fileList.ContainsKey(layer.Tiles))
			{
				var tiles = layer.GetTiles(file);
				if (tiles != null)
				{
					string path = Path.Combine("Tiles", name + ".bin");
					fileList.Add(layer.Tiles, path);
					json.Tiles = path;
					File.WriteAllBytes(Path.Combine(extract, path), tiles);
					if (layer.AniTilesSize > 0 && layer.AnimFrameCount > 0)
					{
						tiles = layer.GetAniTiles(file);
						path = Path.Combine("AniTiles", name + ".bin");
						fileList.Add((int)(layer.Tiles + layer.TilesSize), path);
						json.AniTiles = path;
						File.WriteAllBytes(Path.Combine(extract, path), tiles);
					}
				}
			}
			else
			{
				json.Tiles = fileList[layer.Tiles];
				if (layer.AniTilesSize > 0)
					json.AniTiles = fileList[(int)(layer.Tiles + layer.TilesSize)];
			}
			if (!fileList.ContainsKey(layer.Palette))
			{
				var palette = layer.GetPaletteRaw(file);
				if (palette != null)
				{
					string path = Path.Combine("Palettes", name + ".bin");
					fileList.Add(layer.Palette, path);
					json.Palette = path;
					File.WriteAllBytes(Path.Combine(extract, path), palette);
				}
			}
			else
				json.Palette = fileList[layer.Palette];
		}

		private static string ProcessForegroundLayer(byte[] file, string extract, string name, ForegroundLayer layer, int game)
		{
			var json = new ForegroundLayerJson(layer);
			ProcessLayerBase(file, extract, name, layer, json);
			if (!fileList.ContainsKey(layer.Chunks))
			{
				if (layer.Chunks != 0)
				{
					string path = Path.Combine("Chunks", name + ".bin");
					chunkFiles.Add(layer.Chunks, path);
					fileList.Add(layer.Chunks, path);
					json.Chunks = path;
				}
			}
			else
				json.Chunks = fileList[layer.Chunks];
			if (!fileList.ContainsKey(layer.Layout))
			{
				var layout = layer.GetLayoutRaw(file, game);
				if (layout != null)
				{
					string path = Path.Combine("Layout", "Foreground", name + ".bin");
					fileList.Add(layer.Layout, path);
					json.Layout = path;
					File.WriteAllBytes(Path.Combine(extract, path), layout);
				}
			}
			else
				json.Layout = fileList[layer.Layout];
			string result = Path.Combine("Foregrounds", name + ".safg");
			json.Save(Path.Combine(extract, result));
			fileList.Add(layer.Address, result);
			return result;
		}

		private static string ProcessBackgroundLayer(byte[] file, string extract, string name, BackgroundLayer layer)
		{
			var json = new BackgroundLayerJson(layer);
			ProcessLayerBase(file, extract, name, layer, json);
			if (!fileList.ContainsKey(layer.Layout))
			{
				var layout = layer.GetLayoutRaw(file);
				if (layout != null)
				{
					string path = Path.Combine("Layout", "Background", name + ".bin");
					fileList.Add(layer.Layout, path);
					json.Layout = path;
					File.WriteAllBytes(Path.Combine(extract, path), layout);
				}
			}
			else
				json.Layout = fileList[layer.Layout];
			string result = Path.Combine("Backgrounds", name + ".sabg");
			json.Save(Path.Combine(extract, result));
			fileList.Add(layer.Address, result);
			return result;
		}

		private static string ProcessCollision(byte[] file, string extract, string name, LevelCollision collision, int game)
		{
			var json = new CollisionJson(collision);
			if (!fileList.ContainsKey(collision.Heightmaps))
			{
				if (collision.Heightmaps != 0)
				{
					string path = Path.Combine("Collision", "Heightmaps", name + ".bin");
					chunkFiles.Add(collision.Heightmaps, path);
					fileList.Add(collision.Heightmaps, path);
					json.Heightmaps = path;
				}
			}
			else
				json.Heightmaps = fileList[collision.Heightmaps];
			if (!fileList.ContainsKey(collision.Angles))
			{
				if (collision.Angles != 0)
				{
					string path = Path.Combine("Collision", "Angles", name + ".bin");
					chunkFiles.Add(collision.Angles, path);
					fileList.Add(collision.Angles, path);
					json.Angles = path;
				}
			}
			else
				json.Angles = fileList[collision.Angles];
			if (!fileList.ContainsKey(collision.Chunks))
			{
				if (collision.Chunks != 0)
				{
					string path = Path.Combine("Chunks", name + ".bin");
					chunkFiles.Add(collision.Chunks, path);
					fileList.Add(collision.Chunks, path);
					json.Chunks = path;
				}
			}
			else
				json.Chunks = fileList[collision.Chunks];
			if (!fileList.ContainsKey(collision.ForegroundHigh))
			{
				var layout = collision.ReadForegroundHighRaw(file, game);
				if (layout != null)
				{
					string path = Path.Combine("Layout", "Foreground", name + ".bin");
					fileList.Add(collision.ForegroundHigh, path);
					json.ForegroundHigh = path;
					File.WriteAllBytes(Path.Combine(extract, path), layout);
				}
			}
			else
				json.ForegroundHigh = fileList[collision.ForegroundHigh];
			if (!fileList.ContainsKey(collision.ForegroundLow))
			{
				var layout = collision.ReadForegroundLowRaw(file, game);
				if (layout != null)
				{
					string path = Path.Combine("Layout", "Foreground", name + ".bin");
					fileList.Add(collision.ForegroundLow, path);
					json.ForegroundLow = path;
					File.WriteAllBytes(Path.Combine(extract, path), layout);
				}
			}
			else
				json.ForegroundLow = fileList[collision.ForegroundLow];
			if (!fileList.ContainsKey(collision.Flags))
			{
				if (collision.Flags != 0)
				{
					string path = Path.Combine("Collision", "Flags", name + ".bin");
					chunkFiles.Add(collision.Flags, path);
					fileList.Add(collision.Flags, path);
					json.Flags = path;
				}
			}
			else
				json.Flags = fileList[collision.Flags];
			string result = Path.Combine("Collision", "Info", name + ".sacl");
			json.Save(Path.Combine(extract, result));
			fileList.Add(collision.Address, result);
			return result;
		}
	}
}
