using AdvanceTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LevelConverter
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			JsonConvert.DefaultSettings = SetDefaults;
		}

		private static JsonSerializerSettings SetDefaults() => new JsonSerializerSettings() { Formatting = Formatting.Indented };

		private void sourceProj_FileNameChanged(object sender, EventArgs e)
		{
			convertButton.Enabled = false;
			if (File.Exists(sourceProj.FileName))
			{
				sourceLevel.Items.Clear();
				sourceLevel.Items.AddRange(ProjectFile.Load(sourceProj.FileName).Levels.Select(a => Path.GetFileNameWithoutExtension(a) ?? "Unused Level").ToArray());
				sourceLevel.Enabled = true;
			}
			else
				sourceLevel.Enabled = false;
		}

		private void destinationProj_FileNameChanged(object sender, EventArgs e)
		{
			convertButton.Enabled = false;
			if (File.Exists(destinationProj.FileName))
			{
				destinationLevel.Items.Clear();
				destinationLevel.Items.AddRange(ProjectFile.Load(destinationProj.FileName).Levels.Select(a => Path.GetFileNameWithoutExtension(a) ?? "Unused Level").ToArray());
				destinationLevel.Enabled = true;
			}
			else
				destinationLevel.Enabled = false;
		}

		private void level_SelectedIndexChanged(object sender, EventArgs e)
		{
			convertButton.Enabled = sourceLevel.SelectedIndex != -1 && destinationLevel.SelectedIndex != -1;
		}

		List<string> processedFiles = new List<string>();
		string srcpath;
		string dstpath;
		private void convertButton_Click(object sender, EventArgs e)
		{
			processedFiles.Clear();
			srcpath = Path.GetDirectoryName(Path.GetFullPath(sourceProj.FileName));
			dstpath = Path.GetDirectoryName(Path.GetFullPath(destinationProj.FileName));
			ProjectFile srcproj = ProjectFile.Load(sourceProj.FileName);
			ProjectFile dstproj = ProjectFile.Load(destinationProj.FileName);
			int srcgame = srcproj.Game;
			int dstgame = dstproj.Game;
			LevelJson levinf = LevelJson.Load(Path.Combine(srcpath, srcproj.Levels[sourceLevel.SelectedIndex]));
			levinf.ID = destinationLevel.SelectedIndex;
			levinf.Save(Path.Combine(dstpath, dstproj.Levels[destinationLevel.SelectedIndex]));
			ProcessFGLayer(levinf.ForegroundHigh, srcgame, dstgame);
			ProcessFGLayer(levinf.ForegroundLow, srcgame, dstgame);
			ProcessBGLayer(levinf.Background1);
			ProcessBGLayer(levinf.Background2);
			if (levinf.Collision != null)
			{
				CollisionJson colinf = CollisionJson.Load(Path.Combine(srcpath, levinf.Collision));
				colinf.Save(Path.Combine(dstpath, levinf.Collision));
				ProcessGenericFile(colinf.Heightmaps);
				ProcessGenericFile(colinf.Angles);
				ProcessGenericFile(colinf.Chunks);
				ProcessFGLayout(colinf.ForegroundHigh, srcgame, dstgame);
				ProcessFGLayout(colinf.ForegroundLow, srcgame, dstgame);
				ProcessGenericFile(colinf.Flags);
			}

			ProcessObjectLayout(levinf.Interactables, srcgame, dstgame);
			ProcessGenericFile(levinf.Items);
			ProcessObjectLayout(levinf.Enemies, srcgame, dstgame);
			ProcessGenericFile(levinf.Rings);
			ProcessGenericFile(levinf.PlayerStart);
		}

		private void ProcessFGLayer(string layerfn, int srcgame, int dstgame)
		{
			if (layerfn != null && !processedFiles.Contains(layerfn))
			{
				ForegroundLayerJson layer = ForegroundLayerJson.Load(Path.Combine(srcpath, layerfn));
				layer.Save(Path.Combine(dstpath, layerfn));
				processedFiles.Add(layerfn);
				ProcessLayerCommon(layer);
				ProcessGenericFile(layer.Chunks);
				ProcessFGLayout(layer.Layout, srcgame, dstgame);
			}
		}

		private void ProcessFGLayout(string layout, int srcgame, int dstgame)
		{
			if ((srcgame == 1) ^ (dstgame == 1))
				if (layout != null && !processedFiles.Contains(layout))
				{
					using (FileStream srcfs = File.OpenRead(Path.Combine(srcpath, layout)))
					using (FileStream dstfs = File.Create(Path.Combine(dstpath, layout)))
					{
						BinaryReader br = new BinaryReader(srcfs);
						BinaryWriter bw = new BinaryWriter(dstfs);
						if (srcgame == 1)
							while (srcfs.Position < srcfs.Length)
								bw.Write((ushort)br.ReadByte());
						else
							while (srcfs.Position < srcfs.Length)
								bw.Write((byte)br.ReadUInt16());
					}
					processedFiles.Add(layout);
				}
				else
					ProcessGenericFile(layout);
		}

		private void ProcessBGLayer(string layerfn)
		{
			if (layerfn != null && !processedFiles.Contains(layerfn))
			{
				BackgroundLayerJson layer = BackgroundLayerJson.Load(Path.Combine(srcpath, layerfn));
				layer.Save(Path.Combine(dstpath, layerfn));
				processedFiles.Add(layerfn);
				ProcessLayerCommon(layer);
				ProcessGenericFile(layer.Layout);
			}
		}

		private void ProcessLayerCommon(LayerJsonBase layer)
		{
			ProcessGenericFile(layer.AniTiles);
			ProcessGenericFile(layer.Tiles);
			ProcessGenericFile(layer.Palette);
		}

		private void ProcessObjectLayout(string filename, int srcgame, int dstgame)
		{
			if (filename != null)
			{
				if (emptyObjects.Checked)
				{
					if (dstgame == 3)
						Entry.WriteLayout(new List<Data5Entry>(), Path.Combine(dstpath, filename));
					else
						Entry.WriteLayout(new List<Data4Entry>(), Path.Combine(dstpath, filename));
				}
				else
				{
					if ((srcgame == 3) ^ (dstgame == 3))
					{
						if (srcgame == 3)
							Entry.WriteLayout(Entry.ReadLayout<InteractableEntry3>(Path.Combine(srcpath, filename)).Select(a => new InteractableEntry12()
							{
								X = a.X,
								Y = a.Y,
								Type = a.Type,
								Data1 = a.Data1,
								Data2 = a.Data2,
								Data3 = a.Data3,
								Data4 = a.Data4
							}).ToList(), Path.Combine(dstpath, filename));
						else
							Entry.WriteLayout(Entry.ReadLayout<InteractableEntry12>(Path.Combine(srcpath, filename)).Select(a => new InteractableEntry3()
							{
								X = a.X,
								Y = a.Y,
								Type = a.Type,
								Data1 = a.Data1,
								Data2 = a.Data2,
								Data3 = a.Data3,
								Data4 = a.Data4
							}).ToList(), Path.Combine(dstpath, filename));
					}
					else
						ProcessGenericFile(filename);
				}
			}
		}

		private void ProcessGenericFile(string filename)
		{
			if (filename != null && !processedFiles.Contains(filename))
			{
				File.Copy(Path.Combine(srcpath, filename), Path.Combine(dstpath, filename), true);
				processedFiles.Add(filename);
			}
		}
	}
}
