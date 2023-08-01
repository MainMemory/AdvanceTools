using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdvanceTools;
using Newtonsoft.Json;

namespace ObjDefEditor
{
	public partial class Form1 : Form
	{
		BmpOff[][][] sprites;
		List<ObjectDefinition> objdefs;
		string[] levelnames;
		int lastObj = -1;
		bool suppressEvents = false;

		public Form1()
		{
			InitializeComponent();
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Formatting = Formatting.Indented };
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			using (OpenFileDialog dlg = new OpenFileDialog() { DefaultExt = "saproj", Filter = "Project Files|*.saproj", RestoreDirectory = true, Title = "Select Project File" })
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					var proj = ProjectFile.Load(dlg.FileName);
					Directory.SetCurrentDirectory(Path.GetDirectoryName(dlg.FileName));
					levelnames = proj.Levels.Select(a => Path.GetFileNameWithoutExtension(a) ?? "Unused Level").ToArray();
					levelsSelector.Items.AddRange(levelnames);
					List<BitmapBits> sprtiles16 = new List<BitmapBits>();
					var data = proj.GetSpriteTiles16();
					if (data != null)
						for (var i = 0; i < data.Length; i += 32)
							sprtiles16.Add(BitmapBits.FromTile4bpp(data, i));
					List<BitmapBits> sprtiles256 = new List<BitmapBits>();
					data = proj.GetSpriteTiles256();
					if (data != null)
						for (var i = 0; i < data.Length; i += 64)
							sprtiles256.Add(BitmapBits.FromTile8bpp(data, i));
					var sprpal = proj.GetSpritePalettes().Select(a => a.RGBColor).ToArray();
					var sprlst = new List<BmpOff[][]>(proj.SpriteAnimations.Length);
					for (int an = 0; an < proj.SpriteAnimations.Length; an++)
					{
						var anims = proj.GetSpriteAnimation(an);
						if (anims == null)
						{
							BmpOff bmpOff = new BmpOff();
							sprlst.Add(new BmpOff[][] { new BmpOff[] { bmpOff } });
							animationSelector.Images.Add(bmpOff.Bitmap);
							continue;
						}
						var maps = proj.GetSpriteMappings(an);
						var attrs = proj.GetSpriteAttributes(an);
						var anmvars = new List<BmpOff[]>(anims.Length);
						for (int sub = 0; sub < anims.Length; sub++)
						{
							var anitiles = new BitmapBits[0];
							var anipal = new Color[256];
							anipal.Fill(Color.Black);
							anipal[0] = Color.Transparent;
							var varfrms = new List<BmpOff>();
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
											varfrms.Add(new BmpOff(spr.GetBitmap().ToBitmap(anipal), spr.Location));
										}
										else
											varfrms.Add(new BmpOff());
										break;
								}
							anmvars.Add(varfrms.ToArray());
						}
						sprlst.Add(anmvars.ToArray());
						animationSelector.Images.Add(anmvars[0][0].Bitmap);
					}
					sprites = sprlst.ToArray();
					objectListSelector.SelectedIndex = 0;
				}
				else
					Close();
		}

		private void objectListSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			string path = Path.Combine("objdefs", objectListSelector.SelectedItem.ToString() + ".json");
			if (File.Exists(path))
				objdefs = JsonConvert.DeserializeObject<List<ObjectDefinition>>(File.ReadAllText(path));
			else
				objdefs = new List<ObjectDefinition>();
			lastObj = -1;
			objectTypeSelector.BeginUpdate();
			objectTypeSelector.Items.Clear();
			objectTypeSelector.Items.AddRange(objdefs.Select(a => a.Name).ToArray());
			objectTypeSelector.Items.Add("Add Object");
			objectTypeSelector.EndUpdate();
			objectTypeSelector.SelectedIndex = 0;
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			File.WriteAllText(Path.Combine("objdefs", objectListSelector.SelectedItem.ToString() + ".json"), JsonConvert.SerializeObject(objdefs));
		}

		private void objectTypeSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (suppressEvents || objectTypeSelector.SelectedIndex == -1)
				return;
			if (objectTypeSelector.SelectedIndex == objdefs.Count)
			{
				ObjectDefinition item = new ObjectDefinition() { Name = "New Object", Variants = new List<ObjectVariant>() };
				AddVariant(item);
				objdefs.Add(item);
				int i = objectTypeSelector.SelectedIndex;
				suppressEvents = true;
				objectTypeSelector.Items.Insert(i, "New Object");
				objectTypeSelector.SelectedIndex = i;
				suppressEvents = false;
			}
			lastObj = objectTypeSelector.SelectedIndex;
			ObjectDefinition def = objdefs[objectTypeSelector.SelectedIndex];
			suppressEvents = true;
			objectNameBox.Text = def.Name;
			suppressEvents = false;
			objectVariantSelector.BeginUpdate();
			objectVariantSelector.Items.Clear();
			string[] names = def.Variants.Select(GetLevelNames).ToArray();
			names[0] = "Default";
			objectVariantSelector.Items.AddRange(names);
			objectVariantSelector.Items.Add("Add Variant");
			objectVariantSelector.EndUpdate();
			objectVariantSelector.SelectedIndex = 0;
		}

		private void AddVariant(ObjectDefinition def)
		{
			ObjectSprite spr = new ObjectSprite();
			if (lastObj != -1)
				spr = objdefs[lastObj].Variants[0].Sprites[0].Clone();
			def.Variants.Add(new ObjectVariant
			{
				Levels = new List<int>(),
				Sprites = new List<ObjectSprite>() { spr }
			});
		}

		private string GetLevelNames(ObjectVariant variant)
		{
			if (variant.Levels.Count == 0)
				return "None";
			List<string> names = new List<string>();
			foreach (var item in variant.Levels.Select(a => levelnames[a]))
			{
				bool found = false;
				for (int i = 0; i < names.Count; i++)
				{
					int match = 0;
					while (match < item.Length && match < names[i].Length && item[match] == names[i][match])
						++match;
					if (match > 5)
					{
						names[i] = names[i].Remove(match).TrimEnd();
						found = true;
						break;
					}
				}
				if (!found)
					names.Add(item);
			}
			return string.Join(", ", names);
		}

		private void objectNameBox_TextChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			objdefs[objectTypeSelector.SelectedIndex].Name = objectNameBox.Text;
			suppressEvents = true;
			objectTypeSelector.Items[objectTypeSelector.SelectedIndex] = objectNameBox.Text;
			suppressEvents = false;
		}

		private void objectVariantSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (suppressEvents || objectVariantSelector.SelectedIndex == -1)
				return;
			ObjectDefinition def = objdefs[objectTypeSelector.SelectedIndex];
			if (objectVariantSelector.SelectedIndex == def.Variants.Count)
			{
				AddVariant(def);
				int i = objectVariantSelector.SelectedIndex;
				suppressEvents = true;
				objectVariantSelector.Items.Insert(i, "None");
				objectVariantSelector.SelectedIndex = i;
				suppressEvents = false;
			}
			tableLayoutPanel1.Enabled = levelsSelector.Enabled = objectVariantSelector.SelectedIndex != 0;
			var v = def.Variants[objectVariantSelector.SelectedIndex];
			suppressEvents = true;
			for (int i = 0; i < levelsSelector.Items.Count; i++)
				levelsSelector.SetItemChecked(i, v.Levels.Contains(i));
			if (v.Data1.HasValue)
			{
				data1Value.Enabled = data1Enabled.Checked = true;
				data1Value.Value = v.Data1.Value;
			}
			else
				data1Value.Enabled = data1Enabled.Checked = false;
			if (v.Data2.HasValue)
			{
				data2Value.Enabled = data2Enabled.Checked = true;
				data2Value.Value = v.Data2.Value;
			}
			else
				data2Value.Enabled = data2Enabled.Checked = false;
			if (v.Data3.HasValue)
			{
				data3Value.Enabled = data3Enabled.Checked = true;
				data3Value.Value = v.Data3.Value;
			}
			else
				data3Value.Enabled = data3Enabled.Checked = false;
			if (v.Data4.HasValue)
			{
				data4Value.Enabled = data4Enabled.Checked = true;
				data4Value.Value = v.Data4.Value;
			}
			else
				data4Value.Enabled = data4Enabled.Checked = false;
			if (v.Data5.HasValue)
			{
				data5Value.Enabled = data5Enabled.Checked = true;
				data5Value.Value = v.Data5.Value;
			}
			else
				data5Value.Enabled = data5Enabled.Checked = false;
			suppressEvents = false;
			spriteListBox.BeginUpdate();
			spriteListBox.Items.Clear();
			spriteListBox.Items.AddRange(v.Sprites.Select(a => $"{a.Animation}-{a.Variant}-{a.Frame}").ToArray());
			spriteListBox.EndUpdate();
			spriteListBox.SelectedIndex = 0;
			deleteSpriteButton.Enabled = v.Sprites.Count > 1;
			DrawPreview();
		}

		private void DrawPreview()
		{
			var sprs = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites;
			int l = int.MaxValue;
			int t = int.MaxValue;
			int r = int.MinValue;
			int b = int.MinValue;
			foreach (var spr in sprs)
			{
				var img = sprites[spr.Animation][spr.Variant][spr.Frame];
				if (spr.XFlip)
				{
					l = Math.Min(l, img.Offset.X + img.Bitmap.Width);
					r = Math.Max(r, img.Bitmap.Width * 2 + img.Offset.X);
				}
				else
				{
					l = Math.Min(l, img.Offset.X);
					r = Math.Max(r, img.Bitmap.Width + img.Offset.X);
				}
				if (spr.YFlip)
				{
					t = Math.Min(t, img.Offset.Y + img.Bitmap.Height);
					b = Math.Max(b, img.Bitmap.Height * 2 + img.Offset.Y);
				}
				else
				{
					t = Math.Min(t, img.Offset.Y);
					b = Math.Max(b, img.Bitmap.Height + img.Offset.Y);
				}
			}
			var bmp = new Bitmap(r - l, b - t);
			using (var gfx = Graphics.FromImage(bmp))
			{
				gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				gfx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
				gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
				foreach (var spr in sprs)
				{
					var img = sprites[spr.Animation][spr.Variant][spr.Frame];
					var x = img.Offset.X - l;
					var y = img.Offset.Y - t;
					var w = img.Bitmap.Width;
					var h = img.Bitmap.Height;
					if (spr.XFlip)
					{
						x += w * 2 - 1;
						w = -w;
					}
					if (spr.YFlip)
					{
						y += h * 2 - 1;
						h = -h;
					}
					gfx.DrawImage(img.Bitmap, x, y, w, h);
				}
			}
			pictureBox1.Image = bmp;
		}

		private void levelsSelector_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (suppressEvents)
				return;
			ObjectVariant objectVariant = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex];
			if (e.NewValue == CheckState.Checked)
				objectVariant.Levels.Add(e.Index);
			else
				objectVariant.Levels.Remove(e.Index);
			objectVariant.Levels.Sort();
			objectVariantSelector.Items[objectVariantSelector.SelectedIndex] = GetLevelNames(objectVariant);
		}

		private void data1Enabled_CheckedChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			ObjectVariant objectVariant = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex];
			if (data1Enabled.Checked)
			{
				data1Value.Enabled = true;
				objectVariant.Data1 = (byte)data1Value.Value;
			}
			else
			{
				data1Value.Enabled = false;
				objectVariant.Data1 = null;
			}
		}

		private void data1Value_ValueChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Data1 = (byte)data1Value.Value;
		}

		private void data2Enabled_CheckedChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			ObjectVariant objectVariant = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex];
			if (data2Enabled.Checked)
			{
				data2Value.Enabled = true;
				objectVariant.Data2 = (byte)data2Value.Value;
			}
			else
			{
				data2Value.Enabled = false;
				objectVariant.Data2 = null;
			}
		}

		private void data2Value_ValueChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Data2 = (byte)data2Value.Value;
		}

		private void data3Enabled_CheckedChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			ObjectVariant objectVariant = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex];
			if (data3Enabled.Checked)
			{
				data3Value.Enabled = true;
				objectVariant.Data3 = (byte)data3Value.Value;
			}
			else
			{
				data3Value.Enabled = false;
				objectVariant.Data3 = null;
			}
		}

		private void data3Value_ValueChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Data3 = (byte)data3Value.Value;
		}

		private void data4Enabled_CheckedChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			ObjectVariant objectVariant = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex];
			if (data4Enabled.Checked)
			{
				data4Value.Enabled = true;
				objectVariant.Data4 = (byte)data4Value.Value;
			}
			else
			{
				data4Value.Enabled = false;
				objectVariant.Data4 = null;
			}
		}

		private void data4Value_ValueChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Data4 = (byte)data4Value.Value;
		}

		private void data5Enabled_CheckedChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			ObjectVariant objectVariant = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex];
			if (data5Enabled.Checked)
			{
				data5Value.Enabled = true;
				objectVariant.Data5 = (byte)data5Value.Value;
			}
			else
			{
				data5Value.Enabled = false;
				objectVariant.Data5 = null;
			}
		}

		private void data5Value_ValueChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Data5 = (byte)data5Value.Value;
		}

		private void spriteListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (suppressEvents || spriteListBox.SelectedIndex == -1)
				return;
			spriteUpButton.Enabled = spriteListBox.SelectedIndex > 0;
			spriteDownButton.Enabled = spriteListBox.SelectedIndex < spriteListBox.Items.Count - 1;
			var spr = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites[spriteListBox.SelectedIndex];
			suppressEvents = true;
			animationSelector.SelectedIndex = spr.Animation;
			RefreshAnimVariants();
			animVariantSelector.SelectedIndex = spr.Variant;
			RefreshAnimFrames();
			animFrameSelector.SelectedIndex = spr.Frame;
			xFlipCheckbox.Checked = spr.XFlip;
			yFlipCheckbox.Checked = spr.YFlip;
			suppressEvents = false;
		}

		private void RefreshAnimVariants()
		{
			animVariantSelector.Images.Clear();
			animVariantSelector.Images = sprites[animationSelector.SelectedIndex].Select(a => a[0].Bitmap).ToList();
		}

		private void RefreshAnimFrames()
		{
			animFrameSelector.Images.Clear();
			animFrameSelector.Images = sprites[animationSelector.SelectedIndex][animVariantSelector.SelectedIndex].Select(a => a.Bitmap).ToList();
		}

		private void addSpriteButton_Click(object sender, EventArgs e)
		{
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites.Add(new ObjectSprite());
			spriteListBox.Items.Add("0-0-0");
			spriteListBox.SelectedIndex = spriteListBox.Items.Count - 1;
			deleteSpriteButton.Enabled = true;
			DrawPreview();
		}

		private void deleteSpriteButton_Click(object sender, EventArgs e)
		{
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites.RemoveAt(spriteListBox.SelectedIndex);
			spriteListBox.Items.RemoveAt(spriteListBox.SelectedIndex);
			deleteSpriteButton.Enabled = spriteListBox.Items.Count > 1;
			DrawPreview();
		}

		private void spriteUpButton_Click(object sender, EventArgs e)
		{
			var sprs = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites;
			var spr = sprs[spriteListBox.SelectedIndex - 1];
			sprs.RemoveAt(spriteListBox.SelectedIndex - 1);
			sprs.Insert(spriteListBox.SelectedIndex, spr);
			var item = spriteListBox.Items[spriteListBox.SelectedIndex - 1];
			suppressEvents = true;
			spriteListBox.Items.RemoveAt(spriteListBox.SelectedIndex - 1);
			spriteListBox.Items.Insert(spriteListBox.SelectedIndex + 1, item);
			suppressEvents = false;
			DrawPreview();
		}

		private void spriteDownButton_Click(object sender, EventArgs e)
		{
			var sprs = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites;
			var spr = sprs[spriteListBox.SelectedIndex + 1];
			sprs.RemoveAt(spriteListBox.SelectedIndex + 1);
			sprs.Insert(spriteListBox.SelectedIndex, spr);
			var item = spriteListBox.Items[spriteListBox.SelectedIndex + 1];
			suppressEvents = true;
			spriteListBox.Items.RemoveAt(spriteListBox.SelectedIndex + 1);
			spriteListBox.Items.Insert(spriteListBox.SelectedIndex, item);
			suppressEvents = false;
			DrawPreview();
		}

		private void animationSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (suppressEvents || animationSelector.SelectedIndex == -1)
				return;
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites[spriteListBox.SelectedIndex].Animation = animationSelector.SelectedIndex;
			RefreshAnimVariants();
			animVariantSelector.ChangeSize();
			animVariantSelector.SelectedIndex = -1;
			animVariantSelector.SelectedIndex = 0;
		}

		private void animVariantSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (suppressEvents || animVariantSelector.SelectedIndex == -1)
				return;
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites[spriteListBox.SelectedIndex].Variant = animVariantSelector.SelectedIndex;
			RefreshAnimFrames();
			animFrameSelector.ChangeSize();
			animFrameSelector.SelectedIndex = -1;
			animFrameSelector.SelectedIndex = 0;
		}

		private void animFrameSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (suppressEvents || animFrameSelector.SelectedIndex == -1)
				return;
			ObjectSprite spr = objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites[spriteListBox.SelectedIndex];
			spr.Frame = animFrameSelector.SelectedIndex;
			spriteListBox.Items[spriteListBox.SelectedIndex] = $"{spr.Animation}-{spr.Variant}-{spr.Frame}";
			DrawPreview();
		}

		private void xFlipCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites[spriteListBox.SelectedIndex].XFlip = xFlipCheckbox.Checked;
			DrawPreview();
		}

		private void yFlipCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			objdefs[objectTypeSelector.SelectedIndex].Variants[objectVariantSelector.SelectedIndex].Sprites[spriteListBox.SelectedIndex].YFlip = yFlipCheckbox.Checked;
			DrawPreview();
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
