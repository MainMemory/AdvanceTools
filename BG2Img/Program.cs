using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AdvanceTools;
using System.Drawing;
using Newtonsoft.Json;

namespace BG2Img
{
	static class Program
	{
		static ProjectFile proj;
		static List<BitmapBits> sprtiles16 = new List<BitmapBits>();
		static List<BitmapBits> sprtiles256 = new List<BitmapBits>();
		static Color[] sprpal;

		static void Main(string[] args)
		{
			string filename;
			if (args.Length > 0)
				filename = args[0];
			else
			{
				Console.Write("Project: ");
				filename = Console.ReadLine().Trim('"');
			}
			proj = ProjectFile.Load(filename);
			Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(filename)));
			var data = proj.GetSpriteTiles16();
			for (var i = 0; i < data.Length; i += 32)
				sprtiles16.Add(BitmapBits.FromTile4bpp(data, i));
			data = proj.GetSpriteTiles256();
			for (var i = 0; i < data.Length; i += 64)
				sprtiles256.Add(BitmapBits.FromTile8bpp(data, i));
			sprpal = proj.GetSpritePalettes().Select(a => a.RGBColor).ToArray();
			var interactableSprites = ReadObjDefs("Interactables");
			var itemSprites = ReadObjDefs("Items");
			var enemySprites = ReadObjDefs("Enemies");
			var ringSprites = ReadObjDefs("Rings");
			var playerSprites = ReadObjDefs("Player");
			for (int lv = 0; lv < proj.Levels.Length; lv++)
			{
				var lvinf = proj.GetLevel(lv);
				if (lvinf == null)
					continue;
				var palette = new Color[256];
				palette.Fill(Color.Black);
				var fghigh = lvinf.GetForegroundHigh();
				if (fghigh != null)
					CopyPalette(fghigh, palette);
				var fglow = lvinf.GetForegroundLow();
				if (fglow != null)
					CopyPalette(fglow, palette);
				var bg1 = lvinf.GetBackground1();
				var bg2 = lvinf.GetBackground2();
				var col = lvinf.GetCollision();
				if (proj.Game == 3 && proj.Levels[lv].EndsWith("Map.salv"))
				{
					if (bg1 != null)
						CopyPalette(bg1, palette);
					if (bg2 != null)
						CopyPalette(bg2, palette);
				}
				if (bg1 != null)
					BackgroundToImage(bg1, palette, lvinf.Background1);
				if (bg2 != null)
					BackgroundToImage(bg1, palette, lvinf.Background1);
				palette[0] = Color.Transparent;
				if (fghigh != null)
					ForegroundToImage(fghigh, palette, lvinf.ForegroundHigh, proj.Game);
				if (fglow != null)
					ForegroundToImage(fglow, palette, lvinf.ForegroundLow, proj.Game);
				if (col != null)
					CollisionToImage(col, lvinf.Collision, proj.Game);
			}
			for (int bg = 0; bg < proj.Backgrounds.Length; bg++)
			{
				var bginf = proj.GetBackground(bg);
				if (bginf == null)
					continue;
				var palette = new Color[256];
				palette.Fill(Color.Black);
				CopyPalette(bginf, palette);
				BackgroundToImage(bginf, palette, proj.Backgrounds[bg]);
			}
			for (int an = 0; an < proj.SpriteAnimations.Length; an++)
			{
				var anims = proj.GetSpriteAnimation(an);
				if (anims == null) continue;
				var maps = proj.GetSpriteMappings(an);
				var attrs = proj.GetSpriteAttributes(an);
				var vars = AnimationJson.Load(proj.SpriteAnimations[an]);
				for (int sub = 0; sub < anims.Length; sub++)
				{
					var path = Path.ChangeExtension(vars[sub], ".gif");
					Console.WriteLine(path);
					var anitiles = new BitmapBits[0];
					var anipal = new Color[256];
					anipal.Fill(Color.Black);
					anipal[0] = Color.Transparent;
					int xmin = int.MaxValue;
					int xmax = int.MinValue;
					int ymin = int.MaxValue;
					int ymax = int.MinValue;
					foreach (var cmd in anims[sub])
						switch (cmd)
						{
							case AnimationCommandDrawFrame acdf:
								if (acdf.MappingIndex != -1)
								{
									var mf = maps[acdf.MappingIndex];
									for (int sp = 0; sp < mf.SpriteCount; sp++)
									{
										var sa = attrs[mf.AttributeIndex + sp];
										var size = sa.GetTileSize();
										xmin = Math.Min(xmin, -mf.X + sa.X);
										xmax = Math.Max(xmax, -mf.X + sa.X + size.Width * 8);
										ymin = Math.Min(ymin, -mf.Y + sa.Y);
										ymax = Math.Max(ymax, -mf.Y + sa.Y + size.Height * 8);
									}
								}
								break;
						}
					var rect = Rectangle.FromLTRB(xmin, ymin, xmax, ymax);
					using (var gif = AnimatedGif.AnimatedGif.Create(path, 16))
						foreach (var cmd in anims[sub])
							switch (cmd)
							{
								case AnimationCommandGetTiles acgt:
									if (acgt.Color256)
										anitiles = sprtiles256.Skip((int)acgt.TileIndex).Take((int)acgt.TileCount).ToArray();
									else
										anitiles = sprtiles16.Skip((int)acgt.TileIndex).Take((int)acgt.TileCount).ToArray();
									break;
								case AnimationCommandGetPalette acgp:
									Array.Copy(sprpal, acgp.PaletteIndex * 16, anipal, acgp.PaletteDest, acgp.PaletteSize);
									anipal[0] = Color.Transparent;
									break;
								case AnimationCommandDrawFrame acdf:
									if (acdf.MappingIndex != -1)
									{
										var mf = maps[acdf.MappingIndex];
										var bmp = new BitmapBits(rect.Width, rect.Height);
										for (int sp = 0; sp < mf.SpriteCount; sp++)
										{
											var sa = attrs[mf.AttributeIndex + sp];
											var size = sa.GetTileSize();
											var tmp = new BitmapBits(size.Width * 8, size.Height * 8);
											int tind = sa.Tile;
											for (var y = 0; y < size.Height; y++)
												for (var x = 0; x < size.Width; x++)
												{
													var tile = new BitmapBits(anitiles[tind++]);
													if (!sa.Color256 && sa.Palette > 0)
													{
														byte pal = (byte)(sa.Palette << 4);
														for (var i = 0; i < tile.Bits.Length; i++)
															if (tile.Bits[i] != 0)
																tile.Bits[i] |= pal;
													}
													tmp.DrawBitmap(tile, x * 8, y * 8);
												}
											tmp.Flip(sa.XFlip, sa.YFlip);
											bmp.DrawBitmap(tmp, -(rect.X + mf.X) + sa.X, -(rect.Y + mf.Y) + sa.Y);
										}
										bmp.Flip(mf.XFlip, mf.YFlip);
										using (var res = bmp.ToBitmap(anipal))
											gif.AddFrame(res, acdf.Delay * 1000 / 60, AnimatedGif.GifQuality.Bit8);
									}
									break;
							}
				}
			}
		}

		private static List<List<(ObjectVariant variant, BmpOff sprite)>> ReadObjDefs(string listname)
		{
			var objSprites = new List<List<(ObjectVariant variant, BmpOff sprite)>>();
			string path = Path.Combine("objdefs", listname + ".json");
			Console.WriteLine(path);
			if (File.Exists(path))
			{
				var objdefs = JsonConvert.DeserializeObject<List<ObjectDefinition>>(File.ReadAllText(path));
				foreach (var od in objdefs)
				{
					var varSprites = new List<(ObjectVariant variant, BmpOff sprite)>();
					foreach (var ov in od.Variants)
					{
						var sprs = new List<Sprite>();
						var anipal = new Color[256];
						anipal.Fill(Color.Black);
						anipal[0] = Color.Transparent;
						foreach (var os in ov.Sprites)
						{
							var anims = proj.GetSpriteAnimation(os.Animation);
							if (anims == null) continue;
							var maps = proj.GetSpriteMappings(os.Animation);
							var attrs = proj.GetSpriteAttributes(os.Animation);
							int sub = os.Variant;
							var anitiles = new BitmapBits[0];
							int frm = os.Frame;
							foreach (var cmd in anims[sub])
								switch (cmd)
								{
									case AnimationCommandGetTiles acgt:
										if (acgt.Color256)
											anitiles = sprtiles256.Skip((int)acgt.TileIndex).Take((int)acgt.TileCount).ToArray();
										else
											anitiles = sprtiles16.Skip((int)acgt.TileIndex).Take((int)acgt.TileCount).ToArray();
										break;
									case AnimationCommandGetPalette acgp:
										Array.Copy(sprpal, acgp.PaletteIndex * 16, anipal, acgp.PaletteDest, acgp.PaletteSize);
										anipal[0] = Color.Transparent;
										break;
									case AnimationCommandDrawFrame acdf:
										if (frm-- == 0)
										{
											if (acdf.MappingIndex != -1)
											{
												var mf = maps[acdf.MappingIndex];
												List<Sprite> framesprs = new List<Sprite>(mf.SpriteCount);
												for (int sp = 0; sp < mf.SpriteCount; sp++)
												{
													var sa = attrs[mf.AttributeIndex + sp];
													var size = sa.GetTileSize();
													var tmp = new BitmapBits(size.Width * 8, size.Height * 8);
													int tind = sa.Tile;
													for (var y = 0; y < size.Height; y++)
														for (var x = 0; x < size.Width; x++)
														{
															var tile = new BitmapBits(anitiles[tind++]);
															if (!sa.Color256 && sa.Palette > 0)
															{
																byte pal = (byte)(sa.Palette << 4);
																for (var i = 0; i < tile.Bits.Length; i++)
																	if (tile.Bits[i] != 0)
																		tile.Bits[i] |= pal;
															}
															tmp.DrawBitmap(tile, x * 8, y * 8);
														}
													tmp.Flip(sa.XFlip, sa.YFlip);
													framesprs.Add(new Sprite(tmp, sa.X, sa.Y));
												}
												Sprite spr = new Sprite(framesprs);
												spr.Flip(mf.XFlip, mf.YFlip);
												spr.Offset(-mf.X, -mf.Y);
												spr.Flip(os.XFlip, os.YFlip);
												sprs.Add(spr);
											}
										}
										break;
								}
						}
						Sprite vsp = new Sprite(sprs);
						varSprites.Add((ov, new BmpOff(vsp.GetBitmap().ToBitmap(anipal), vsp.Location)));
					}
					objSprites.Add(varSprites);
				}
			}
			return objSprites;
		}

		private static void ForegroundToImage(ForegroundLayerJson fginf, Color[] palette, string path, int game)
		{
			Console.WriteLine(path);
			List<BitmapBits> tiles = new List<BitmapBits>();
			var data = fginf.GetTiles();
			for (var i = 0; i < data.Length; i += 32)
				tiles.Add(BitmapBits.FromTile4bpp(data, i));
			byte[] tilepals = new byte[tiles.Count];
			var chunks = fginf.GetChunks();
			foreach (var c in chunks)
				for (int y = 0; y < c.GetLength(1); y++)
					for (int x = 0; x < c.GetLength(0); x++)
						tilepals[c[x, y].Tile] = c[x, y].Palette;
			var tlayout = new TileIndex[16, (tiles.Count + 15) / 16];
			for (ushort i = 0; i < tiles.Count; i++)
				tlayout[i % 16, i / 16] = new TileIndex(i, false, false, tilepals[i]);
			BitmapBits bmp = TilemapToImage(tlayout, tiles);
			using (var res = bmp.ToBitmap(palette))
				res.Save(Path.ChangeExtension(fginf.Tiles, ".png"));
			int chunkWidth = fginf.ChunkWidth * 8;
			int chunkHeight = fginf.ChunkHeight * 8;
			var chunksImgs = chunks.Select(a => TilemapToImage(a, tiles)).ToArray();
			bmp = new BitmapBits(chunkWidth * 8, (chunks.Length + 7) / 8 * chunkHeight);
			for (int i = 0; i < chunks.Length; i++)
				bmp.DrawBitmap(chunksImgs[i], i % 8 * chunkWidth, i / 8 * chunkHeight);
			using (var res = bmp.ToBitmap(palette))
				res.Save(Path.ChangeExtension(fginf.Chunks, ".png"));
			var layout = fginf.GetLayout(game);
			bmp = new BitmapBits(fginf.Width * chunkWidth, fginf.Height * chunkHeight);
			for (int y = 0; y < fginf.Height; y++)
				for (int x = 0; x < fginf.Width; x++)
					if (layout[x, y] < chunksImgs.Length)
						bmp.DrawBitmap(chunksImgs[layout[x, y]], x * chunkWidth, y * chunkHeight);
			using (var res = bmp.ToBitmap(palette))
				res.Save(Path.ChangeExtension(path, ".png"));
		}

		private static void BackgroundToImage(BackgroundLayerJson bginf, Color[] palette, string path)
		{
			Console.WriteLine(path);
			List<BitmapBits> tiles = new List<BitmapBits>();
			var data = bginf.GetTiles();
			switch (bginf.Mode)
			{
				case BGMode.Color256:
				case BGMode.Scale:
					for (var i = 0; i < data.Length; i += 64)
						tiles.Add(BitmapBits.FromTile8bpp(data, i));
					break;
				default:
					for (var i = 0; i < data.Length; i += 32)
						tiles.Add(BitmapBits.FromTile4bpp(data, i));
					break;
			}
			TileIndex[,] layout = bginf.GetLayout();
			byte[] tilepals = new byte[tiles.Count];
			for (int y = 0; y < layout.GetLength(1); y++)
				for (int x = 0; x < layout.GetLength(0); x++)
					tilepals[layout[x, y].Tile] = layout[x, y].Palette;
			var tlayout = new TileIndex[16, (tiles.Count + 15) / 16];
			for (ushort i = 0; i < tiles.Count; i++)
				tlayout[i % 16, i / 16] = new TileIndex(i, false, false, tilepals[i]);
			BitmapBits bmp = TilemapToImage(tlayout, tiles);
			using (var res = bmp.ToBitmap(palette))
				res.Save(Path.ChangeExtension(bginf.Tiles, ".png"));
			bmp = TilemapToImage(layout, tiles);
			using (var res = bmp.ToBitmap(palette))
				res.Save(Path.ChangeExtension(path, ".png"));
		}

		private static void CollisionToImage(CollisionJson colinf, string path, int game)
		{
			Console.WriteLine(path);
			var palette = new[] { Color.Black, Color.Red, Color.Green, Color.Blue, Color.White };
			var heights = colinf.GetHeightmaps();
			var flags = colinf.GetFlags();
			var heightsImgs = heights.Zip(flags, (a, b) =>
			{
				BitmapBits res = new BitmapBits(8, 8);
				for (int x = 0; x < 8; x++)
				{
					byte v = a[x].Vertical;
					if (v != 0)
					{
						if (v < 8)
							res.DrawLine((byte)(b + 1), x, v, x, 7);
						else
							res.DrawLine((byte)(b + 1), x, 0, x, 7 - (v - 8));
					}
				}

				return res;
			}).ToList();
			var tlayout = new TileIndex[16, (heights.Length + 15) / 16];
			for (ushort i = 0; i < heights.Length; i++)
				tlayout[i % 16, i / 16] = new TileIndex(i, false, false, 0);
			BitmapBits bmp = TilemapToImage(tlayout, heightsImgs);
			using (var res = bmp.ToBitmap4bpp(palette))
				res.Save(Path.ChangeExtension(colinf.Heightmaps, ".png"));
			var chunks = colinf.GetChunks();
			var chunksImgs = chunks.Select(a => TilemapToImage(a, heightsImgs)).ToArray();
			const int chunkWidth = 12 * 8;
			const int chunkHeight = 12 * 8;
			bmp = new BitmapBits(chunkWidth * 8, (chunks.Length + 7) / 8 * chunkHeight);
			for (int i = 0; i < chunks.Length; i++)
				bmp.DrawBitmap(chunksImgs[i], i % 8 * chunkWidth, i / 8 * chunkHeight);
			using (var res = bmp.ToBitmap4bpp(palette))
				res.Save(Path.ChangeExtension(Path.ChangeExtension(colinf.Chunks, null) + " Collision", ".png"));
			var layout = colinf.GetForegroundHigh(game);
			bmp = new BitmapBits(colinf.Width * chunkWidth, colinf.Height * chunkHeight);
			for (int y = 0; y < colinf.Height; y++)
				for (int x = 0; x < colinf.Width; x++)
					if (layout[x, y] < chunksImgs.Length)
						bmp.DrawBitmap(chunksImgs[layout[x, y]], x * chunkWidth, y * chunkHeight);
			using (var res = bmp.ToBitmap4bpp(palette))
				res.Save(Path.ChangeExtension(Path.ChangeExtension(colinf.ForegroundHigh, null) + " Collision", ".png"));
			layout = colinf.GetForegroundLow(game);
			bmp = new BitmapBits(colinf.Width * chunkWidth, colinf.Height * chunkHeight);
			for (int y = 0; y < colinf.Height; y++)
				for (int x = 0; x < colinf.Width; x++)
					if (layout[x, y] < chunksImgs.Length)
						bmp.DrawBitmap(chunksImgs[layout[x, y]], x * chunkWidth, y * chunkHeight);
			using (var res = bmp.ToBitmap4bpp(palette))
				res.Save(Path.ChangeExtension(Path.ChangeExtension(colinf.ForegroundLow, null) + " Collision", ".png"));
		}

		private static BitmapBits TilemapToImage(TileIndex[,] layout, List<BitmapBits> tiles)
		{
			int width = layout.GetLength(0);
			int height = layout.GetLength(1);
			BitmapBits bmp = new BitmapBits(width * 8, height * 8);
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
				{
					var tinf = layout[x, y];
					if (tinf?.Tile < tiles.Count)
					{
						var tile = new BitmapBits(tiles[tinf.Tile]);
						tile.Flip(tinf.XFlip, tinf.YFlip);
						if (tinf.Palette > 0)
						{
							byte pal = (byte)(tinf.Palette << 4);
							for (var i = 0; i < tile.Bits.Length; i++)
								if (tile.Bits[i] != 0)
									tile.Bits[i] |= pal;
						}
						bmp.DrawBitmap(tile, x * 8, y * 8);
					}
				}

			return bmp;
		}

		private static void CopyPalette(LayerJsonBase bginf, Color[] palette)
		{
			if (bginf.Palette != null)
				bginf.GetPalette().Select(a => a.RGBColor).ToArray().CopyTo(palette, bginf.PalDest);
		}
	}

	class BmpOff
	{
		public Bitmap Bitmap { get; }
		public Point Offset { get; }

		public BmpOff()
		{
			Bitmap = new Bitmap(1, 1);
		}

		public BmpOff(Bitmap bmp, Point off)
		{
			Bitmap = bmp;
			Offset = off;
		}
	}
}
