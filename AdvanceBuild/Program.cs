using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvanceTools;

namespace AdvanceBuild
{
	static class Program
	{
		static void Main(string[] args)
		{
			string projpath;
			if (args.Length > 0)
			{
				projpath = args[0];
				Console.WriteLine("Project: {0}", projpath);
			}
			else
			{
				Console.Write("Project: ");
				projpath = Console.ReadLine().Trim('"');
			}
			projpath = Path.GetFullPath(projpath);
			string rompath;
			if (args.Length > 1)
			{
				rompath = args[1];
				Console.WriteLine("ROM: {0}", rompath);
			}
			else
			{
				Console.Write("ROM: ");
				rompath = Console.ReadLine().Trim('"');
			}
			rompath = Path.GetFullPath(rompath);
			string outrom;
			if (args.Length > 2)
			{
				outrom = Path.GetFullPath(args[2]);
				Console.WriteLine("Output ROM: {0}", outrom);
			}
			else
				outrom = Path.ChangeExtension(Path.ChangeExtension(rompath, null) + "_mod", ".gba");
			FileStream romfile;
			if (!rompath.Equals(outrom, StringComparison.OrdinalIgnoreCase))
			{
				romfile = File.Create(outrom);
				using (var orig = File.OpenRead(rompath))
					orig.CopyTo(romfile);
			}
			else
				romfile = File.Open(rompath, FileMode.Open, FileAccess.ReadWrite);
			using (romfile)
			{
				var bw = new BinaryWriter(romfile);
				var project = ProjectFile.Load(projpath);
				Directory.SetCurrentDirectory(Path.GetDirectoryName(projpath));
				Dictionary<string, int> modifiedFiles = new Dictionary<string, int>();
				List<LevelJson> levels = new List<LevelJson>(project.Levels?.Length ?? 0);
				for (var lv = 0; lv < project.Levels?.Length; lv++)
					levels.Add(project.GetLevel(lv));
				foreach (var lvjs in levels)
					if (lvjs != null)
					{
						ProcessFGLayer(lvjs.ForegroundHigh, romfile, project, modifiedFiles);
						ProcessFGLayer(lvjs.ForegroundLow, romfile, project, modifiedFiles);
						ProcessBGLayer(lvjs.Background1, romfile, project, modifiedFiles);
						if (project.Game == 3)
							ProcessBGLayer(lvjs.Background2, romfile, project, modifiedFiles);
						ProcessCollision(lvjs.Collision, romfile, project, modifiedFiles);
						CheckCompressedFileData(lvjs.Interactables, romfile, project, modifiedFiles);
						CheckCompressedFileData(lvjs.Items, romfile, project, modifiedFiles);
						CheckCompressedFileData(lvjs.Enemies, romfile, project, modifiedFiles);
						CheckCompressedFileData(lvjs.Rings, romfile, project, modifiedFiles);
					}
				foreach (string bg in project.Backgrounds)
					ProcessBGLayer(bg, romfile, project, modifiedFiles);
				var animsmod = false;
				foreach (string filename in project.SpriteAnimations)
					if (filename != null && !modifiedFiles.ContainsKey(filename))
					{
						var animvars = AnimationJson.Load(filename);
						var modified = false;
						foreach (var a in animvars)
							modified |= CheckFileData(a, romfile, project, modifiedFiles);
						if (modified)
						{
							Console.WriteLine(filename);
							animsmod = true;
							modifiedFiles.Add(filename, (int)romfile.Length);
							romfile.Seek(0, SeekOrigin.End);
							foreach (var a in animvars)
								bw.Write(GetFilePointer(a, project, modifiedFiles));
						}
					}
				int animsaddr = 0;
				if (animsmod)
				{
					animsaddr = (int)romfile.Length + 0x8000000;
					romfile.Seek(0, SeekOrigin.End);
					foreach (var filename in project.SpriteAnimations)
						bw.Write(GetFilePointer(filename, project, modifiedFiles));
				}
				var mapsmod = false;
				foreach (string filename in project.SpriteMappings)
					mapsmod |= CheckFileData(filename, romfile, project, modifiedFiles);
				int mapsaddr = 0;
				if (mapsmod)
				{
					mapsaddr = (int)romfile.Length + 0x8000000;
					romfile.Seek(0, SeekOrigin.End);
					foreach (var filename in project.SpriteMappings)
						bw.Write(GetFilePointer(filename, project, modifiedFiles));
				}
				var attrsmod = false;
				foreach (string filename in project.SpriteAttributes)
					attrsmod |= CheckFileData(filename, romfile, project, modifiedFiles);
				int attrsaddr = 0;
				if (attrsmod)
				{
					attrsaddr = (int)romfile.Length + 0x8000000;
					romfile.Seek(0, SeekOrigin.End);
					foreach (var filename in project.SpriteAttributes)
						bw.Write(GetFilePointer(filename, project, modifiedFiles));
				}
				CheckFileData(project.SpritePalettes, romfile, project, modifiedFiles);
				CheckFileData(project.SpriteTiles16, romfile, project, modifiedFiles);
				CheckFileData(project.SpriteTiles256, romfile, project, modifiedFiles);
				romfile.Seek(project.Version.MapTable, SeekOrigin.Begin);
				foreach (var lvjs in levels)
					if (lvjs != null)
					{
						bw.Write(GetFilePointer(lvjs.ForegroundHigh, project, modifiedFiles));
						bw.Write(GetFilePointer(lvjs.ForegroundLow, project, modifiedFiles));
						bw.Write(GetFilePointer(lvjs.Background1, project, modifiedFiles));
						if (project.Game == 3)
							bw.Write(GetFilePointer(lvjs.Background2, project, modifiedFiles));
					}
					else
						bw.Write(new byte[project.Game == 3 ? 16 : 12]);
				foreach (string bg in project.Backgrounds)
					bw.Write(GetFilePointer(bg, project, modifiedFiles));
				romfile.Seek(project.Version.CollisionTable, SeekOrigin.Begin);
				foreach (var lvjs in levels)
					if (lvjs != null)
						bw.Write(GetFilePointer(lvjs.Collision, project, modifiedFiles));
					else
						bw.Write(0);
				romfile.Seek(project.Version.InteractableTable, SeekOrigin.Begin);
				foreach (var lvjs in levels)
					if (lvjs != null)
						bw.Write(GetFilePointer(lvjs.Interactables, project, modifiedFiles));
					else
						bw.Write(0);
				romfile.Seek(project.Version.ItemTable, SeekOrigin.Begin);
				foreach (var lvjs in levels)
					if (lvjs != null)
						bw.Write(GetFilePointer(lvjs.Items, project, modifiedFiles));
					else
						bw.Write(0);
				romfile.Seek(project.Version.EnemyTable, SeekOrigin.Begin);
				foreach (var lvjs in levels)
					if (lvjs != null)
						bw.Write(GetFilePointer(lvjs.Enemies, project, modifiedFiles));
					else
						bw.Write(0);
				romfile.Seek(project.Version.RingTable, SeekOrigin.Begin);
				foreach (var lvjs in levels)
					if (lvjs != null)
						bw.Write(GetFilePointer(lvjs.Rings, project, modifiedFiles));
					else
						bw.Write(0);
				romfile.Seek(project.Version.StartTable, SeekOrigin.Begin);
				if (project.Game == 3)
				{
					int[] startptrs = new int[levels.Count];
					var br = new BinaryReader(romfile);
					for (int i = 0; i < startptrs.Length; i++)
						startptrs[i] = br.ReadInt32();
					for (int i = 0; i < startptrs.Length; i++)
						if (startptrs[i] != 0 && levels[i] != null)
						{
							romfile.Seek(startptrs[i] - 0x8000000, SeekOrigin.Begin);
							bw.Write(File.ReadAllBytes(levels[i].PlayerStart));
						}
				}
				else
					foreach (var lvjs in levels)
						if (lvjs != null)
							bw.Write(File.ReadAllBytes(lvjs.PlayerStart));
						else
							bw.Write(0);
				romfile.Seek(project.Version.SpriteTable, SeekOrigin.Begin);
				if (animsmod)
					bw.Write(animsaddr);
				else
					romfile.Seek(4, SeekOrigin.Current);
				if (mapsmod)
					bw.Write(mapsaddr);
				else
					romfile.Seek(4, SeekOrigin.Current);
				if (attrsmod)
					bw.Write(attrsaddr);
				else
					romfile.Seek(4, SeekOrigin.Current);
				bw.Write(GetFilePointer(project.SpritePalettes, project, modifiedFiles));
				bw.Write(GetFilePointer(project.SpriteTiles16, project, modifiedFiles));
				bw.Write(GetFilePointer(project.SpriteTiles256, project, modifiedFiles));
			}
		}

		private static int GetFilePointer(string filename, ProjectFile project, Dictionary<string, int> modifiedFiles)
		{
			if (filename is null)
				return 0;
			if (!modifiedFiles.TryGetValue(filename, out int addr))
				addr = project.Hashes[filename].Address;
			return addr + 0x8000000;
		}

		private static bool CheckFileData(string filename, Stream romfile, ProjectFile project, Dictionary<string, int> modifiedFiles)
		{
			if (filename != null)
				if (!modifiedFiles.ContainsKey(filename))
				{
					var data = File.ReadAllBytes(filename);
					if (!project.Hashes.TryGetValue(filename, out var hash) || hash.Hash != Utility.HashBytes(data))
					{
						Console.WriteLine(filename);
						modifiedFiles.Add(filename, (int)romfile.Length);
						romfile.Seek(0, SeekOrigin.End);
						romfile.Write(data, 0, data.Length);
						return true;
					}
				}
				else
					return true;
			return false;
		}

		private static bool ProcessLayerCommon(LayerJsonBase layerjs, Stream romfile, ProjectFile project, Dictionary<string, int> modifiedFiles)
		{
			bool result = false;
			if (layerjs.Tiles != null)
				if (!modifiedFiles.ContainsKey(layerjs.Tiles))
				{
					var tiles = layerjs.GetTiles();
					var anitiles = layerjs.GetAniTiles();
					bool modified = !project.Hashes.TryGetValue(layerjs.Tiles, out var hash) || hash.Hash != Utility.HashBytes(tiles);
					if (!modified && anitiles != null)
						modified = !project.Hashes.TryGetValue(layerjs.AniTiles, out hash) || hash.Hash != Utility.HashBytes(anitiles);
					if (modified)
					{
						Console.WriteLine(layerjs.Tiles);
						modifiedFiles.Add(layerjs.Tiles, (int)romfile.Length);
						romfile.Seek(0, SeekOrigin.End);
						romfile.Write(tiles, 0, tiles.Length);
						if (anitiles != null)
							romfile.Write(anitiles, 0, anitiles.Length);
						result = true;
					}
				}
				else
					result = true;
			result |= CheckFileData(layerjs.Palette, romfile, project, modifiedFiles);
			result |= CheckFileData(layerjs.Layout, romfile, project, modifiedFiles);
			return result;
		}

		private static void WriteLayerDataCommon(LayerJsonBase layerjs, Stream romfile, ProjectFile project, Dictionary<string, int> modifiedFiles)
		{
			var bw = new BinaryWriter(romfile);
			bw.Write(layerjs.AniTilesSize);
			bw.Write(layerjs.AnimFrameCount);
			bw.Write(layerjs.AnimDelay);
			bw.Write(GetFilePointer(layerjs.Tiles, project, modifiedFiles));
			bw.Write((uint)new System.IO.FileInfo(layerjs.Tiles).Length);
			bw.Write(GetFilePointer(layerjs.Palette, project, modifiedFiles));
			bw.Write(layerjs.PalDest);
			bw.Write((ushort)(new System.IO.FileInfo(layerjs.Palette).Length / 2));
		}

		private static void ProcessFGLayer(string filename, Stream romfile, ProjectFile project, Dictionary<string, int> modifiedFiles)
		{
			if (filename != null && !modifiedFiles.ContainsKey(filename))
			{
				var fgjs = ForegroundLayerJson.Load(filename);
				if ((ProcessLayerCommon(fgjs, romfile, project, modifiedFiles) | CheckFileData(fgjs.Chunks, romfile, project, modifiedFiles))
					|| !project.Hashes.TryGetValue(filename, out var hash)
					|| hash.Hash != Utility.HashFile(filename))
				{
					Console.WriteLine(filename);
					modifiedFiles.Add(filename, (int)romfile.Length);
					romfile.Seek(0, SeekOrigin.End);
					var bw = new BinaryWriter(romfile);
					bw.Write(fgjs.ChunkWidth);
					bw.Write(fgjs.ChunkHeight);
					WriteLayerDataCommon(fgjs, romfile, project, modifiedFiles);
					bw.Write(GetFilePointer(fgjs.Chunks, project, modifiedFiles));
					bw.Write(GetFilePointer(fgjs.Layout, project, modifiedFiles));
					bw.Write(fgjs.Width);
					bw.Write(fgjs.Height);
				}
			}
		}

		private static void ProcessBGLayer(string filename, Stream romfile, ProjectFile project, Dictionary<string, int> modifiedFiles)
		{
			if (filename != null && !modifiedFiles.ContainsKey(filename))
			{
				var bgjs = BackgroundLayerJson.Load(filename);
				if (ProcessLayerCommon(bgjs, romfile, project, modifiedFiles)
					|| !project.Hashes.TryGetValue(filename, out var hash)
					|| hash.Hash != Utility.HashFile(filename))
				{
					Console.WriteLine(filename);
					modifiedFiles.Add(filename, (int)romfile.Length);
					romfile.Seek(0, SeekOrigin.End);
					var bw = new BinaryWriter(romfile);
					bw.Write(bgjs.Width);
					bw.Write(bgjs.Height);
					WriteLayerDataCommon(bgjs, romfile, project, modifiedFiles);
					bw.Write(GetFilePointer(bgjs.Layout, project, modifiedFiles));
				}
			}
		}

		private static void ProcessCollision(string filename, Stream romfile, ProjectFile project, Dictionary<string, int> modifiedFiles)
		{
			if (filename != null && !modifiedFiles.ContainsKey(filename))
			{
				var cljs = CollisionJson.Load(filename);
				bool modified = CheckFileData(cljs.Heightmaps, romfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.Angles, romfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.Chunks, romfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.ForegroundHigh, romfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.ForegroundLow, romfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.Flags, romfile, project, modifiedFiles);
				if (modified || !project.Hashes.TryGetValue(filename, out var hash) || hash.Hash != Utility.HashFile(filename))
				{
					Console.WriteLine(filename);
					modifiedFiles.Add(filename, (int)romfile.Length);
					romfile.Seek(0, SeekOrigin.End);
					var bw = new BinaryWriter(romfile);
					bw.Write(GetFilePointer(cljs.Chunks, project, modifiedFiles));
					bw.Write(cljs.Width);
					bw.Write(cljs.Height);
				}
			}
		}

		private static void CheckCompressedFileData(string filename, FileStream romfile, ProjectFile project, Dictionary<string, int> modifiedFiles)
		{
			if (filename != null && !modifiedFiles.ContainsKey(filename))
			{
				var data = File.ReadAllBytes(filename);
				if (!project.Hashes.TryGetValue(filename, out var hash) || hash.Hash != Utility.HashBytes(data))
				{
					Console.WriteLine(filename);
					data = Utility.CompressRLData(data);
					modifiedFiles.Add(filename, (int)romfile.Length);
					romfile.Seek(0, SeekOrigin.End);
					romfile.Write(data, 0, data.Length);
				}
			}
		}
	}
}
