using AdvanceTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchGen
{
	class Program
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
			string patchpath;
			if (args.Length > 1)
				patchpath = Path.GetFullPath(args[1]);
			else
				patchpath = Path.ChangeExtension(projpath, ".sapatch");
			Console.WriteLine("Patch: {0}", patchpath);
			using (FileStream patchstream = File.Create(patchpath))
			using (ZipArchive zipfile = new ZipArchive(patchstream, ZipArchiveMode.Create))
			{
				var project = ProjectFile.Load(projpath);
				Directory.SetCurrentDirectory(Path.GetDirectoryName(projpath));
				List<string> modifiedFiles = new List<string>();
				for (int lv = 0; lv < project.Levels.Length; lv++)
				{
					LevelJson lvjs = project.GetLevel(lv);
					if (lvjs != null)
					{
						ProcessFGLayer(lvjs.ForegroundHigh, zipfile, project, modifiedFiles);
						ProcessFGLayer(lvjs.ForegroundLow, zipfile, project, modifiedFiles);
						ProcessBGLayer(lvjs.Background1, zipfile, project, modifiedFiles);
						if (project.Game == 3)
							ProcessBGLayer(lvjs.Background2, zipfile, project, modifiedFiles);
						ProcessCollision(lvjs.Collision, zipfile, project, modifiedFiles);
						CheckFileData(lvjs.Interactables, zipfile, project, modifiedFiles);
						CheckFileData(lvjs.Items, zipfile, project, modifiedFiles);
						CheckFileData(lvjs.Enemies, zipfile, project, modifiedFiles);
						CheckFileData(lvjs.Rings, zipfile, project, modifiedFiles);
						CheckFileData(lvjs.PlayerStart, zipfile, project, modifiedFiles);
						zipfile.CreateEntryFromFile(project.Levels[lv], project.Levels[lv]);
					}
				}
				for (int bg = 0; bg < project.Backgrounds.Length; bg++)
					ProcessBGLayer(project.Backgrounds[bg], zipfile, project, modifiedFiles);
				for (int an = 0; an < project.SpriteAnimations.Length; an++)
				{
					string filename = project.SpriteAnimations[an];
					if (filename != null && !modifiedFiles.Contains(filename))
					{
						var animvars = AnimationJson.Load(filename);
						var modified = false;
						foreach (var a in animvars)
							modified |= CheckFileData(a, zipfile, project, modifiedFiles);
						if (modified)
						{
							Console.WriteLine(filename);
							modifiedFiles.Add(filename);
							zipfile.CreateEntryFromFile(filename, filename);
						}
					}
				}
				for (int mn = 0; mn < project.SpriteMappings.Length; mn++)
					CheckFileData(project.SpriteMappings[mn], zipfile, project, modifiedFiles);
				for (int an = 0; an < project.SpriteAttributes.Length; an++)
					CheckFileData(project.SpriteAttributes[an], zipfile, project, modifiedFiles);
				CheckFileData(project.SpritePalettes, zipfile, project, modifiedFiles);
				CheckFileData(project.SpriteTiles16, zipfile, project, modifiedFiles);
				CheckFileData(project.SpriteTiles256, zipfile, project, modifiedFiles);
				zipfile.CreateEntryFromFile(projpath, "Info.saproj");
			}
		}

		private static bool CheckFileData(string filename, ZipArchive zipfile, ProjectFile project, List<string> modifiedFiles)
		{
			if (filename != null)
				if (!modifiedFiles.Contains(filename))
				{
					var data = File.ReadAllBytes(filename);
					if (!project.Hashes.TryGetValue(filename, out var hash) || hash.Hash != Utility.HashBytes(data))
					{
						Console.WriteLine(filename);
						modifiedFiles.Add(filename);
						zipfile.CreateEntryFromFile(filename, filename);
						return true;
					}
				}
				else
					return true;
			return false;
		}

		private static bool ProcessLayerCommon(LayerJsonBase layerjs, ZipArchive zipfile, ProjectFile project, List<string> modifiedFiles)
		{
			bool result = false;
			if (layerjs.Tiles != null)
				if (!modifiedFiles.Contains(layerjs.Tiles))
				{
					var tiles = layerjs.GetTiles();
					var anitiles = layerjs.GetAniTiles();
					bool modified = !project.Hashes.TryGetValue(layerjs.Tiles, out var hash) || hash.Hash != Utility.HashBytes(tiles);
					if (!modified && anitiles != null)
						modified = !project.Hashes.TryGetValue(layerjs.AniTiles, out hash) || hash.Hash != Utility.HashBytes(anitiles);
					if (modified)
					{
						Console.WriteLine(layerjs.Tiles);
						modifiedFiles.Add(layerjs.Tiles);
						zipfile.CreateEntryFromFile(layerjs.Tiles, layerjs.Tiles);
						if (anitiles != null)
							zipfile.CreateEntryFromFile(layerjs.AniTiles, layerjs.AniTiles);
						result = true;
					}
				}
				else
					result = true;
			result |= CheckFileData(layerjs.Palette, zipfile, project, modifiedFiles);
			result |= CheckFileData(layerjs.Layout, zipfile, project, modifiedFiles);
			return result;
		}

		private static void ProcessFGLayer(string filename, ZipArchive zipfile, ProjectFile project, List<string> modifiedFiles)
		{
			if (filename != null && !modifiedFiles.Contains(filename))
			{
				var fgjs = ForegroundLayerJson.Load(filename);
				if ((ProcessLayerCommon(fgjs, zipfile, project, modifiedFiles) | CheckFileData(fgjs.Chunks, zipfile, project, modifiedFiles))
					|| !project.Hashes.TryGetValue(filename, out var hash)
					|| hash.Hash != Utility.HashFile(filename))
				{
					Console.WriteLine(filename);
					modifiedFiles.Add(filename);
					zipfile.CreateEntryFromFile(filename, filename);
				}
			}
		}

		private static void ProcessBGLayer(string filename, ZipArchive zipfile, ProjectFile project, List<string> modifiedFiles)
		{
			if (filename != null && !modifiedFiles.Contains(filename))
			{
				var bgjs = BackgroundLayerJson.Load(filename);
				if (ProcessLayerCommon(bgjs, zipfile, project, modifiedFiles)
					|| !project.Hashes.TryGetValue(filename, out var hash)
					|| hash.Hash != Utility.HashFile(filename))
				{
					Console.WriteLine(filename);
					modifiedFiles.Add(filename);
					zipfile.CreateEntryFromFile(filename, filename);
				}
			}
		}

		private static void ProcessCollision(string filename, ZipArchive zipfile, ProjectFile project, List<string> modifiedFiles)
		{
			if (filename != null && !modifiedFiles.Contains(filename))
			{
				var cljs = CollisionJson.Load(filename);
				bool modified = CheckFileData(cljs.Heightmaps, zipfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.Angles, zipfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.Chunks, zipfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.ForegroundHigh, zipfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.ForegroundLow, zipfile, project, modifiedFiles);
				modified |= CheckFileData(cljs.Flags, zipfile, project, modifiedFiles);
				if (modified || !project.Hashes.TryGetValue(filename, out var hash) || hash.Hash != Utility.HashFile(filename))
				{
					Console.WriteLine(filename);
					modifiedFiles.Add(filename);
					zipfile.CreateEntryFromFile(filename, filename);
				}
			}
		}
	}
}
