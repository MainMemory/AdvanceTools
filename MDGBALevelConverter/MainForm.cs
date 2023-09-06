using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SonicRetro.SonLVL.API;
using AdvanceTools;
using Newtonsoft.Json;

namespace MDGBALevelConverter
{
	public partial class MainForm : Form
	{
		string mdFolder;
		string projectFolder;
		ProjectFile project;
		List<string> gbalevels;

		public MainForm()
		{
			InitializeComponent();
			JsonConvert.DefaultSettings = SetDefaults;
		}

		private static JsonSerializerSettings SetDefaults() => new JsonSerializerSettings() { Formatting = Formatting.Indented };

		private void mdProject_FileNameChanged(object sender, EventArgs e)
		{
			mdLevel.Items.Clear();
			if (File.Exists(mdProject.FileName))
			{
				mdFolder = Path.GetDirectoryName(mdProject.FileName);
				LevelData.LoadProject(mdProject.FileName);
				mdLevel.Items.AddRange(LevelData.Game.Levels.Select(a => a.Value.DisplayName ?? a.Key.Split('\\').Last()).ToArray());
				mdLevel.Enabled = true;
				mdLevel.SelectedIndex = 0;
			}
			else
				mdLevel.Enabled = false;
		}

		private void mdLevel_SelectedIndexChanged(object sender, EventArgs e)
		{
			convertButton.Enabled = mdLevel.SelectedIndex != -1 && gbaLevel.SelectedIndex != -1;
		}

		private void gbaProject_FileNameChanged(object sender, EventArgs e)
		{
			gbaLevel.Items.Clear();
			if (File.Exists(gbaProject.FileName))
			{
				projectFolder = Path.GetDirectoryName(gbaProject.FileName);
				project = ProjectFile.Load(gbaProject.FileName);
				gbalevels = project.Levels.Where(a => a != null).ToList();
				gbaLevel.Items.AddRange(gbalevels.Select(a => Path.GetFileNameWithoutExtension(a)).ToArray());
				gbaLevel.Enabled = true;
				gbaLevel.SelectedIndex = 0;
			}
			else
				gbaLevel.Enabled = false;
		}

		private void gbaLevel_SelectedIndexChanged(object sender, EventArgs e)
		{
			convertButton.Enabled = mdLevel.SelectedIndex != -1 && gbaLevel.SelectedIndex != -1;
		}

		private void convertButton_Click(object sender, EventArgs e)
		{
			Directory.SetCurrentDirectory(mdFolder);
			LevelData.LoadLevel(LevelData.Game.Levels.Keys.ElementAt(mdLevel.SelectedIndex), false);
			byte[] tmp;
			sbyte[][] ColArr2 = new sbyte[256][];
			if (LevelData.Level.CollisionArray2 != null && File.Exists(LevelData.Level.CollisionArray2))
				tmp = Compression.Decompress(LevelData.Level.CollisionArray2, LevelData.Level.CollisionArrayCompression);
			else
				tmp = new byte[256 * 16];
			for (int i = 0; i < 256; i++)
			{
				ColArr2[i] = new sbyte[16];
				for (int j = 0; j < 16; j++)
					ColArr2[i][j] = unchecked((sbyte)tmp[(i * 16) + j]);
			}
			if (LevelData.Level.AnimatedTiles != null)
				foreach (AnimatedTileInfo info in LevelData.Level.AnimatedTiles)
				{
					tmp = File.ReadAllBytes(info.Filename);
					List<byte[]> tiles = new List<byte[]>();
					for (int i = 0; i < info.Length * 32; i += 32)
					{
						byte[] tile = new byte[32];
						Array.Copy(tmp, i + (info.Source * 32), tile, 0, 32);
						tiles.Add(tile);
					}
					LevelData.Tiles.AddFile(tiles, info.Destination);
				}
			if (LevelData.Level.AnimatedBlocks != null)
			{
				List<Block> blocks = new List<Block>();
				tmp = File.ReadAllBytes(LevelData.Level.AnimatedBlocks);
				int end = ((SonicRetro.SonLVL.API.ByteConverter.ToUInt16(tmp, 2) + 1) * 2) + 4;
				for (int i = 4; i < end; i += Block.Size)
					blocks.Add(new Block(tmp, i));
				LevelData.Blocks.AddFile(blocks, SonicRetro.SonLVL.API.ByteConverter.ToUInt16(tmp, 0));
			}

			Directory.SetCurrentDirectory(projectFolder);
			LevelJson gbalevel = LevelJson.Load(gbalevels[gbaLevel.SelectedIndex]);
			CollisionJson gbacolinf = gbalevel.GetCollision();
			ForegroundLayerJson gbafgh = gbalevel.GetForegroundHigh();
			ForegroundLayerJson gbafgl = gbalevel.GetForegroundLow();
			BackgroundLayerJson gbabg = gbalevel.GetBackground1();

			int[] blockflip = new int[LevelData.Blocks.Count];
			if (LevelData.Chunks[0].Blocks[0, 0] is S1ChunkBlock)
			{
				LevelData.ColInds2 = new List<byte>(LevelData.ColInds1);
				for (int item = 0; item < LevelData.Chunks.Count; item++)
				{
					Chunk cnk = LevelData.Chunks[item];
					Chunk cnk2 = LevelData.Chunks[item + 1];
					for (int y = 0; y < LevelData.Level.ChunkHeight / 16; y++)
						for (int x = 0; x < LevelData.Level.ChunkWidth / 16; x++)
						{
							ChunkBlock old = cnk.Blocks[x, y];
							Solidity solid2 = old.Solid1;
							if (LevelData.Level.LoopChunks.Contains((byte)item))
							{
								ChunkBlock old2 = cnk2.Blocks[x, y];
								solid2 = old2.Solid1;
								if (old.Block < LevelData.ColInds1.Count)
								{
									LevelData.ColInds2[old.Block] = LevelData.ColInds1[old2.Block];
									blockflip[old.Block] = (old.XFlip ^ old2.XFlip ? 1 : 0) | (old.YFlip ^ old2.YFlip ? 2 : 0);
								}
							}
							cnk.Blocks[x, y] = new S2ChunkBlock() { Block = old.Block, Solid1 = old.Solid1, Solid2 = solid2, XFlip = old.XFlip, YFlip = old.YFlip };
						}
					if (LevelData.Level.LoopChunks.Contains((byte)item))
						LevelData.Chunks[++item] = cnk;
				}
				LevelData.Level.ChunkFormat = EngineVersion.S2;
			}
			List<ColInfo> tilecol = Enumerable.Range(0, LevelData.Tiles.Count).Select(a => new ColInfo((ushort)a)).ToList();
			tilecol[0] = new ColInfo(0, false, new Heightmap[8], 0);
			List<TileIndex[][,]> blockconv = new List<TileIndex[][,]>(LevelData.Blocks.Count);
			for (int i = 0; i < LevelData.Blocks.Count; i++)
			{
				Block blk = LevelData.Blocks[i];
				TileIndex[][,] newblk = new TileIndex[2][,];
				newblk[0] = new TileIndex[2, 2];
				newblk[1] = new TileIndex[2, 2];
				if (blk != null)
				{
					byte cih = i < LevelData.ColInds1.Count ? LevelData.ColInds1[i] : (byte)0;
					byte cil = i < LevelData.ColInds2.Count ? LevelData.ColInds2[i] : (byte)0;
					sbyte[] col1h = LevelData.ColArr1[cih];
					sbyte[] col1l = LevelData.ColArr1[cil];
					sbyte[] col2h = ColArr2[cih];
					sbyte[] col2l = ColArr2[cil];
					byte angh = LevelData.Angles[cih];
					byte angl = LevelData.Angles[cil];
					FlipHeightmap(ref col1l, ref col2l, ref angl, (blockflip[i] & 1) == 1, (blockflip[i] & 2) == 2);
					for (int y = 0; y < 2; y++)
						for (int x = 0; x < 2; x++)
						{
							PatternIndex blktil = blk.Tiles[x, y];
							if (blktil.Tile >= LevelData.Tiles.Count)
								blktil.Tile = 0;
							sbyte[] tmpcol1l = CropHeightmap(col1l, x, y);
							sbyte[] tmpcol2l = CropHeightmap(col2l, y, x);
							byte tmpangl = angl;
							sbyte[] tmpcol1h = CropHeightmap(col1h, x, y);
							sbyte[] tmpcol2h = CropHeightmap(col2h, y, x);
							byte tmpangh = angh;
							FlipHeightmap(ref tmpcol1l, ref tmpcol2l, ref tmpangl, blktil.XFlip, blktil.YFlip);
							FlipHeightmap(ref tmpcol1h, ref tmpcol2h, ref tmpangh, blktil.XFlip, blktil.YFlip);
							if (AdvanceTools.Extensions.FastArrayEqual(tmpcol1h, tmpcol1l))
							{
								ColInfo colinf = new ColInfo(blktil.Tile, null, ConvertHeightmap(tmpcol1l, tmpcol2l), tmpangl);
								if (tilecol[blktil.Tile].CollisionSet)
								{
									int ind = tilecol.IndexOf(colinf);
									if (ind == -1)
									{
										ind = tilecol.Count;
										tilecol.Add(colinf);
									}
									newblk[1][x, y] = new TileIndex((ushort)ind, blktil.YFlip, blktil.XFlip, blktil.Palette);
								}
								else
								{
									tilecol[blktil.Tile] = colinf;
									newblk[1][x, y] = new TileIndex(blktil.Tile, blktil.YFlip, blktil.XFlip, blktil.Palette);
								}
								if (blktil.Priority)
									newblk[0][x, y] = newblk[1][x, y].Clone();
								else
									newblk[0][x, y] = new TileIndex();
							}
							else if (blktil.Priority)
							{
								ColInfo colinf = new ColInfo(0, null, ConvertHeightmap(tmpcol1l, tmpcol2l), tmpangl);
								int ind = tilecol.IndexOf(colinf);
								if (ind == -1)
								{
									ind = tilecol.Count;
									tilecol.Add(colinf);
								}
								newblk[1][x, y] = new TileIndex((ushort)ind, blktil.YFlip, blktil.XFlip, blktil.Palette);
								colinf = new ColInfo(blktil.Tile, null, ConvertHeightmap(tmpcol1h, tmpcol2h), tmpangh);
								if (tilecol[blktil.Tile].CollisionSet)
								{
									ind = tilecol.IndexOf(colinf);
									if (ind == -1)
									{
										ind = tilecol.Count;
										tilecol.Add(colinf);
									}
									newblk[0][x, y] = new TileIndex((ushort)ind, blktil.YFlip, blktil.XFlip, blktil.Palette);
								}
								else
								{
									tilecol[blktil.Tile] = colinf;
									newblk[0][x, y] = new TileIndex(blktil.Tile, blktil.YFlip, blktil.XFlip, blktil.Palette);
								}
							}
							else
							{
								int ind;
								ColInfo colinf = new ColInfo(blktil.Tile, null, ConvertHeightmap(tmpcol1l, tmpcol2l), tmpangl);
								if (tilecol[blktil.Tile].CollisionSet)
								{
									ind = tilecol.IndexOf(colinf);
									if (ind == -1)
									{
										ind = tilecol.Count;
										tilecol.Add(colinf);
									}
									newblk[1][x, y] = new TileIndex((ushort)ind, blktil.YFlip, blktil.XFlip, blktil.Palette);
								}
								else
								{
									tilecol[blktil.Tile] = colinf;
									newblk[1][x, y] = new TileIndex(blktil.Tile, blktil.YFlip, blktil.XFlip, blktil.Palette);
								}
								colinf = new ColInfo(0, null, ConvertHeightmap(tmpcol1h, tmpcol2h), tmpangh);
								ind = tilecol.IndexOf(colinf);
								if (ind == -1)
								{
									ind = tilecol.Count;
									tilecol.Add(colinf);
								}
								newblk[0][x, y] = new TileIndex((ushort)ind, blktil.YFlip, blktil.XFlip, blktil.Palette);
							}
						}
				}
				else
					for (int y = 0; y < 2; y++)
						for (int x = 0; x < 2; x++)
						{
							newblk[0][x, y] = new TileIndex();
							newblk[1][x, y] = new TileIndex();
						}
				blockconv.Add(newblk);
			}
			for (int i = 0; i < tilecol.Count; i++)
				if (!tilecol[i].CollisionSet)
					tilecol[i] = new ColInfo(tilecol[i].TileIndex, null, new Heightmap[8], 0);
			Dictionary<int, TileIndex[,]> blockcoldict = new Dictionary<int, TileIndex[,]>();
			List<TileIndex[][,]> chunktiles = new List<TileIndex[][,]>();
			int chunktilewidth = LevelData.Level.ChunkWidth / 8;
			int chunktileheight = LevelData.Level.ChunkHeight / 8;
			for (int item = 0; item < LevelData.Chunks.Count; item++)
			{
				Chunk cnk = LevelData.Chunks[item];
				TileIndex[][,] newcnk = new TileIndex[2][,];
				newcnk[0] = new TileIndex[chunktilewidth, chunktileheight];
				newcnk[1] = new TileIndex[chunktilewidth, chunktileheight];
				if (cnk != null)
					for (int y = 0; y < LevelData.Level.ChunkHeight / 16; y++)
						for (int x = 0; x < LevelData.Level.ChunkWidth / 16; x++)
						{
							S2ChunkBlock old = (S2ChunkBlock)cnk.Blocks[x, y];
							TileIndex[,] newblk = GetBlockCol(tilecol, blockconv, blockcoldict, old.Block, old.Solid1, 0);
							for (int ty = 0; ty < 2; ty++)
								for (int tx = 0; tx < 2; tx++)
								{
									TileIndex tileIndex = newblk[old.XFlip ? 1 - tx : tx, old.YFlip ? 1 - ty : ty].Clone();
									tileIndex.XFlip ^= old.XFlip;
									tileIndex.YFlip ^= old.YFlip;
									newcnk[0][x * 2 + tx, y * 2 + ty] = tileIndex;
								}
							newblk = GetBlockCol(tilecol, blockconv, blockcoldict, old.Block, old.Solid2, 1);
							for (int ty = 0; ty < 2; ty++)
								for (int tx = 0; tx < 2; tx++)
								{
									TileIndex tileIndex = newblk[old.XFlip ? 1 - tx : tx, old.YFlip ? 1 - ty : ty].Clone();
									tileIndex.XFlip ^= old.XFlip;
									tileIndex.YFlip ^= old.YFlip;
									newcnk[1][x * 2 + tx, y * 2 + ty] = tileIndex;
								}
						}
				chunktiles.Add(newcnk);
			}
			int newfgwidth = (LevelData.FGWidth * chunktilewidth+ 11) / 12 * 12 ;
			int newfgheight = (LevelData.FGHeight * chunktileheight + 11) / 12 * 12;
			TileIndex[][,] fgtilelayout = new TileIndex[2][,];
			fgtilelayout[0] = new TileIndex[newfgwidth, newfgheight];
			fgtilelayout[1] = new TileIndex[newfgwidth, newfgheight];
			for (int y = 0; y < newfgheight; y++)
				for (int x = 0; x < newfgwidth; x++)
				{
					fgtilelayout[0][x, y] = new TileIndex();
					fgtilelayout[1][x, y] = new TileIndex();
				}
			for (int y = 0; y < LevelData.FGHeight; y++)
				for (int x = 0; x < LevelData.FGWidth; x++)
				{
					ushort cid = LevelData.Layout.FGLayout[x, y];
					if (cid >= chunktiles.Count)
						cid = 0;
					TileIndex[][,] cnk = chunktiles[cid];
					for (int ty = 0; ty < chunktileheight; ty++)
						for (int tx = 0; tx < chunktilewidth; tx++)
						{
							fgtilelayout[0][x * chunktilewidth + tx, y * chunktileheight + ty] = cnk[0][tx, ty].Clone();
							fgtilelayout[1][x * chunktilewidth + tx, y * chunktileheight + ty] = cnk[1][tx, ty].Clone();
						}
				}
			int newbgwidth = LevelData.BGWidth * chunktilewidth;
			int newbgheight = LevelData.BGHeight * chunktileheight;
			TileIndex[,] bgtilelayout = new TileIndex[newbgwidth, newbgheight];
			for (int y = 0; y < LevelData.BGHeight; y++)
				for (int x = 0; x < LevelData.BGWidth; x++)
				{
					ushort cid = LevelData.Layout.BGLayout[x, y];
					if (cid >= chunktiles.Count)
						cid = 0;
					TileIndex[][,] cnk = chunktiles[cid];
					for (int ty = 0; ty < chunktileheight; ty++)
						for (int tx = 0; tx < chunktilewidth; tx++)
							bgtilelayout[x * chunktilewidth + tx, y * chunktileheight + ty] = cnk[1][tx, ty].Clone();
				}
			byte[][] gbatiles = LevelData.Tiles.Select(a => a is null ? new byte[32] : a.Select(b => (byte)(((b & 0xF) << 4) | (b >> 4))).ToArray()).ToArray();
			List<ushort> fgtiles = fgtilelayout.SelectMany(a => a.OfType<TileIndex>().Select(b => b.Tile)).Distinct().ToList();
			if (fgtiles.Count > 0x400 && MessageBox.Show(this, "Level uses too many tiles! Output will not display correctly.\n\nContinue anyway?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
				return;
			fgtiles.Sort();
			for (int y = 0; y < newfgheight; y++)
				for (int x = 0; x < newfgwidth; x++)
				{
					fgtilelayout[0][x, y].Tile = (ushort)fgtiles.IndexOf(fgtilelayout[0][x, y].Tile);
					fgtilelayout[1][x, y].Tile = (ushort)fgtiles.IndexOf(fgtilelayout[1][x, y].Tile);
				}
			List<byte>[] fglayout = new List<byte>[2];
			fglayout[0] = new List<byte>();
			fglayout[1] = new List<byte>();
			List<byte[]> gbachunks = new List<byte[]>() { new byte[12 * 12 * 2] };
			for (int y = 0; y < newfgheight; y += 12)
				for (int x = 0; x < newfgwidth; x += 12)
				{
					GetGBAChunk(fgtilelayout, fglayout, x, y, 0, gbachunks);
					GetGBAChunk(fgtilelayout, fglayout, x, y, 1, gbachunks);
				}
			if (project.Game == 1 && gbachunks.Count > 0x100 && MessageBox.Show(this, "Level uses too many chunks! Output will not display correctly.\n\nContinue anyway?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
				return;
			List<ColInfo> fgcol = fgtiles.Select(a => tilecol[a]).ToList();
			File.WriteAllBytes(gbafgh.Tiles, fgcol.SelectMany(a => gbatiles[a.TileIndex]).ToArray());
			gbafgh.AniTiles = null;
			gbafgh.AniTilesSize = 0;
			gbafgh.AnimDelay = 0;
			gbafgh.AnimFrameCount = 0;
			File.WriteAllBytes(gbacolinf.Heightmaps, fgcol.SelectMany(a => a.HeightMap.Select(b => b.GetData())).ToArray());
			File.WriteAllBytes(gbacolinf.Angles, fgcol.Select(a => a.Angle).ToArray());
			byte[] colflags = new byte[(fgcol.Count + 3) / 4];
			for (int i = 0; i < fgcol.Count; i++)
				if (fgcol[i].TopSolid ?? false)
					colflags[i / 4] |= (byte)(3 << (2 * (i % 4)));
			File.WriteAllBytes(gbacolinf.Flags, colflags);
			List<byte> gbapal = new List<byte>(512);
			for (int p = 0; p < Math.Min(LevelData.Palette.Count, 4); p++)
				for (int l = 0; l < 4; l++)
					for (int i = 0; i < 16; i++)
						gbapal.AddRange(BitConverter.GetBytes(new GBAColor(LevelData.Palette[p][l, i].RGBColor).Value));
			File.WriteAllBytes(gbafgh.Palette, gbapal.ToArray());
			gbafgh.PalDest = 0;
			gbafgl.PalDest = 0;
			gbabg.Palette = gbafgh.Palette;
			gbabg.PalDest = 0;
			File.WriteAllBytes(gbacolinf.Chunks, gbachunks.SelectMany(a => a).ToArray());
			File.WriteAllBytes(gbacolinf.ForegroundHigh, fglayout[0].ToArray());
			File.WriteAllBytes(gbacolinf.ForegroundLow, fglayout[1].ToArray());
			gbafgh.Width = (ushort)(newfgwidth / 12);
			gbafgh.Height = (ushort)(newfgheight / 12);
			gbafgh.Save(gbalevel.ForegroundHigh);
			gbafgl.Width = (ushort)(newfgwidth / 12);
			gbafgl.Height = (ushort)(newfgheight / 12);
			gbafgl.Save(gbalevel.ForegroundLow);
			gbacolinf.Width = (ushort)(newfgwidth / 12);
			gbacolinf.Height = (ushort)(newfgheight / 12);
			gbacolinf.WidthPixels = (uint)(newfgwidth * 8);
			gbacolinf.HeightPixels = (uint)(newfgheight * 8);
			gbacolinf.Save(gbalevel.Collision);
			List<ushort> bgtiles = bgtilelayout.OfType<TileIndex>().Select(b => b.Tile).Distinct().ToList();
			bgtiles.Sort();
			for (int y = 0; y < newbgheight; y++)
				for (int x = 0; x < newbgwidth; x++)
					bgtilelayout[x, y].Tile = (ushort)bgtiles.IndexOf(bgtilelayout[x, y].Tile);
			File.WriteAllBytes(gbabg.Tiles, bgtiles.SelectMany(a => gbatiles[tilecol[a].TileIndex]).ToArray());
			gbabg.AniTiles = null;
			gbabg.AniTilesSize = 0;
			gbabg.AnimDelay = 0;
			gbabg.AnimFrameCount = 0;
			List<byte> bglayout = new List<byte>(newbgwidth * newbgheight * 2);
			for (int y = 0; y < newbgheight; y++)
				for (int x = 0; x < newbgwidth; x++)
					bglayout.AddRange(bgtilelayout[x, y].GetBytes());
			File.WriteAllBytes(gbabg.Layout, bglayout.ToArray());
			gbabg.Width = (ushort)newbgwidth;
			gbabg.Height = (ushort)newbgheight;
			gbabg.Save(gbalevel.Background1);
			if (project.Game == 3)
			{
				AdvanceTools.Entry.WriteLayout(new List<InteractableEntry3>(), gbalevel.Interactables);
				AdvanceTools.Entry.WriteLayout(new List<EnemyEntry3>(), gbalevel.Enemies);
			}
			else
			{
				AdvanceTools.Entry.WriteLayout(new List<InteractableEntry12>(), gbalevel.Interactables);
				AdvanceTools.Entry.WriteLayout(new List<EnemyEntry12>(), gbalevel.Enemies);
			}
			AdvanceTools.Entry.WriteLayout(new List<ItemEntry>(), gbalevel.Items);
			List<AdvanceTools.RingEntry> rings = new List<AdvanceTools.RingEntry>();
			foreach (var rng in LevelData.Rings)
				if (rng is SonicRetro.SonLVL.API.S2.S2RingEntry s2rng)
				{
					ushort x = s2rng.X;
					ushort y = s2rng.Y;
					for (int i = 0; i < s2rng.Count; i++)
					{
						switch (s2rng.Direction)
						{
							case Direction.Horizontal:
								x += 24;
								break;
							case Direction.Vertical:
								y += 24;
								break;
						}
						rings.Add(new AdvanceTools.RingEntry() { X = x, Y = y });
					}
				}
				else
					rings.Add(new AdvanceTools.RingEntry() { X = rng.X, Y = rng.Y });
			AdvanceTools.Entry.WriteLayout(rings, gbalevel.Rings);
			if (LevelData.StartPositions.Count > 0)
				new PlayerEntry()
				{
					X = LevelData.StartPositions[0].X,
					Y = LevelData.StartPositions[0].Y
				}.Write(gbalevel.PlayerStart);
		}

		private void GetGBAChunk(TileIndex[][,] fgtilelayout, List<byte>[] fglayout, int x, int y, int layer, List<byte[]> gbachunks)
		{
			byte[] newcnk = new byte[12 * 12 * 2];
			for (int ty = 0; ty < 12; ty++)
				for (int tx = 0; tx < 12; tx++)
					fgtilelayout[layer][x + tx, y + ty].GetBytes().CopyTo(newcnk, (ty * 12 + tx) * 2);
			int ind = gbachunks.FindIndex(a => AdvanceTools.Extensions.FastArrayEqual(newcnk, a));
			if (ind == -1)
			{
				ind = gbachunks.Count;
				gbachunks.Add(newcnk);
			}
			if (project.Game == 1)
				fglayout[layer].Add((byte)ind);
			else
				fglayout[layer].AddRange(BitConverter.GetBytes((ushort)ind));
		}

		private static TileIndex[,] GetBlockCol(List<ColInfo> tilecol, List<TileIndex[][,]> blockconv, Dictionary<int, TileIndex[,]> blockcoldict, int blk, Solidity solid, int layer)
		{
			if (blk >= LevelData.Blocks.Count)
				blk = 0;
			if (solid == Solidity.LRBSolid)
				solid = Solidity.AllSolid;
			int ind = blk | ((int)solid << 16) | (layer << 18);
			if (!blockcoldict.TryGetValue(ind, out TileIndex[,] newblk))
			{
				newblk = new TileIndex[2, 2];
				for (int ty = 0; ty < 2; ty++)
					for (int tx = 0; tx < 2; tx++)
					{
						TileIndex tileIndex = blockconv[blk][layer][tx, ty].Clone();
						newblk[tx, ty] = tileIndex;
						ColInfo colInfo = tilecol[tileIndex.Tile];
						if (!colInfo.CollisionEmpty)
						{
							if (solid == Solidity.NotSolid)
							{
								tileIndex.Tile = (ushort)tilecol.Count;
								tilecol.Add(new ColInfo(colInfo.TileIndex, false, new Heightmap[8], 0));
							}
							else
							{
								if (colInfo.TopSolid != null)
								{
									if (colInfo.TopSolid.Value != (solid == Solidity.TopSolid))
									{
										tileIndex.Tile = (ushort)tilecol.Count;
										tilecol.Add(new ColInfo(colInfo.TileIndex, solid == Solidity.TopSolid, colInfo.HeightMap, colInfo.Angle));
									}
								}
								else
									tilecol[tileIndex.Tile] = new ColInfo(colInfo.TileIndex, solid == Solidity.TopSolid, colInfo.HeightMap, colInfo.Angle);
							}
						}
					}
				blockcoldict.Add(ind, newblk);
			}
			return newblk;
		}

		private void FlipHeightmap(ref sbyte[] col1, ref sbyte[] col2, ref byte angle, bool xflip, bool yflip)
		{
			if (!xflip && !yflip)
				return;
			col1 = (sbyte[])col1.Clone();
			col2 = (sbyte[])col2.Clone();
			if (xflip)
			{
				if (yflip)
				{
					Array.Reverse(col1);
					for (int i = 0; i < col1.Length; i++)
						col1[i] = (sbyte)-col1[i];
					Array.Reverse(col2);
					for (int i = 0; i < col2.Length; i++)
						col2[i] = (sbyte)-col2[i];
					if (angle != 0xFF)
						angle = (byte)((angle + 0x80) & 0xFF);
				}
				else
				{
					Array.Reverse(col1);
					for (int i = 0; i < col2.Length; i++)
						col2[i] = (sbyte)-col2[i];
					if (angle != 0xFF)
						angle = (byte)(-angle & 0xFF);
				}
			}
			else if (yflip)
			{
				for (int i = 0; i < col1.Length; i++)
					col1[i] = (sbyte)-col1[i];
				Array.Reverse(col2);
				if (angle != 0xFF)
					angle = (byte)((-(angle + 0x40) - 0x40) & 0xFF);
			}
		}

		private sbyte[] CropHeightmap(sbyte[] col, int x, int y)
		{
			x *= 8;
			sbyte[] result = new sbyte[8];
			for (int j = 0; j < 8; j++)
			{
				if (y == 0)
				{
					if (col[j + x] <= -8)
						result[j] = 8;
					else if (col[j + x] <= 0)
						result[j] = col[j + x];
					else if (col[j + x] >= 8)
						result[j] = (sbyte)(col[j + x] - 8);
				}
				else
				{
					if (col[j + x] >= 8)
						result[j] = 8;
					else if (col[j + x] >= 0)
						result[j] = col[j + x];
					else if (col[j + x] <= -8)
						result[j] = (sbyte)(col[j + x] + 8);
				}
			}
			return result;
		}

		private Heightmap[] ConvertHeightmap(sbyte[] col1, sbyte[] col2)
		{
			Heightmap[] result = new Heightmap[col1.Length];
			for (int i = 0; i < col1.Length; i++)
			{
				if (col1[i] == 8)
					result[i].Vertical = 8;
				else if (col1[i] > 0)
					result[i].Vertical = (byte)(8 - col1[i]);
				else if (col1[i] < 0)
					result[i].Vertical = (byte)(0x10 + col1[i]);
				if (col2[i] == 8)
					result[i].Horizontal = 8;
				else if (col2[i] > 0)
					result[i].Horizontal = (byte)(8 - col2[i]);
				else if (col2[i] < 0)
					result[i].Horizontal = (byte)(0x10 + col2[i]);
			}
			return result;
		}
	}

	class ColInfo : IEquatable<ColInfo>
	{
		public ushort TileIndex { get; }
		public bool? TopSolid { get; }
		public Heightmap[] HeightMap { get; }
		public byte Angle { get; }

		public bool CollisionSet { get; }
		public bool CollisionEmpty { get; }

		public ColInfo(ushort tileIndex)
		{
			TileIndex = tileIndex;
			CollisionSet = false;
		}

		public ColInfo(ushort tileIndex, sbyte[] col1, sbyte[] col2)
		{
			TileIndex = tileIndex;
			HeightMap = new Heightmap[col1.Length];
			Point? start = null;
			Point? end = null;
			bool inverted = false;
			for (int i = 0; i < 8; i++)
			{
				if (col1[i] == 8)
				{
					HeightMap[i].Vertical = 8;
					if (!start.HasValue || start.Value.Y == 0)
						start = new Point(i, 0);
					if (!end.HasValue || end.Value.Y != 0)
						end = new Point(i, 0);
				}
				else if (col1[i] > 0)
				{
					HeightMap[i].Vertical = (byte)(8 - col1[i]);
					if (!start.HasValue)
						start = new Point(i, 8 - col1[i]);
					end = new Point(i, 8 - col1[i]);
				}
				else if (col1[i] < 0)
				{
					HeightMap[i].Vertical = (byte)(0x10 + col1[i]);
					inverted = true;
					if (!start.HasValue)
						start = new Point(i, -col1[i] - 1);
					end = new Point(i, -col1[i] - 1);
				}
				if (col2[i] == 8)
					HeightMap[i].Horizontal = 8;
				else if (col2[i] > 0)
					HeightMap[i].Horizontal = (byte)(8 - col2[i]);
				else if (col2[i] < 0)
					HeightMap[i].Horizontal = (byte)(0x10 + col2[i]);
			}
			if (inverted)
			{
				if (start.HasValue && start.Value.Y == 0)
					start = new Point(start.Value.X, 7);
				if (end.HasValue && end.Value.Y == 0)
					end = new Point(end.Value.X, 7);
			}
			Angle = 0;
			if (start.HasValue && start.Value.Y != end.Value.Y)
				Angle = (byte)(Math.Atan2(end.Value.Y - start.Value.Y, (end.Value.X - start.Value.X) * (inverted ? -1 : 1)) * (256 / (2 * Math.PI)));
			CollisionSet = true;
			CollisionEmpty = HeightMap.All(a => a.Horizontal == 0 && a.Vertical == 0);
			if (CollisionEmpty)
			{
				TopSolid = false;
				Angle = 0;
			}
		}

		public ColInfo(ushort tileIndex, bool? topSolid, Heightmap[] heightMap, byte angle)
		{
			TileIndex = tileIndex;
			TopSolid = topSolid;
			HeightMap = heightMap;
			Angle = angle;
			CollisionSet = true;
			CollisionEmpty = heightMap.All(a => a.Horizontal == 0 && a.Vertical == 0);
			if (CollisionEmpty)
			{
				TopSolid = false;
				Angle = 0;
			}
		}

		public bool Equals(ColInfo other)
		{
			if (other is null) return false;
			if (!CollisionSet)
				return !other.CollisionSet && TileIndex == other.TileIndex;
			return TileIndex == other.TileIndex && TopSolid == other.TopSolid && Angle == other.Angle && AdvanceTools.Extensions.ArrayEqual(HeightMap, other.HeightMap);
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
				_ind = value;
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
			ushort val = (ushort)(_ind & 0x3FF);
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
}
