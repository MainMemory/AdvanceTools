using AdvanceTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AdvanceBG
{
	public partial class MainForm : Form
	{
		public static MainForm Instance { get; private set; }

		private int TileMax => level.Mode == BGMode.Scale ? 0x100 : 0x400;
		Settings Settings;
		int pid;

		public MainForm()
		{
			Application.ThreadException += Application_ThreadException;
			Instance = this;
			pid = System.Diagnostics.Process.GetCurrentProcess().Id;
			if (Program.IsMonoRuntime)
				Log("Mono runtime detected.");
			Log("Operating system: " + Environment.OSVersion.ToString());
			Newtonsoft.Json.JsonConvert.DefaultSettings = () => new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented };
			InitializeComponent();
		}

		void PaletteChanged()
		{
			curpal = new Color[16];
			for (int i = 0; i < 16; i++)
				curpal[i] = LevelData.Palette[SelectedColor.Y * 16 + i].RGBColor;
			DrawPalette();
			DrawTilePicture();
			RefreshTileSelector();
			TileSelector.Invalidate();
			DrawLevel();
		}

		void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			Log(e.Exception.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
			File.WriteAllLines("AdvanceBG.log", LogFile.ToArray());
			using (ErrorDialog ed = new ErrorDialog("Unhandled Exception " + e.Exception.GetType().Name + "\nLog file has been saved.\n\nDo you want to try to continue running?", true))
			{
				if (ed.ShowDialog(this) == DialogResult.Cancel)
					Close();
			}
		}

		ImageAttributes imageTransparency = new ImageAttributes();
		Bitmap LevelBmp;
		Graphics LevelGfx, PalettePanelGfx;
		bool loaded;
		Rectangle FGSelection;
		ColorPalette LevelImgPalette;
		double ZoomLevel = 1;
		Point dragpoint;
		bool selecting = false;
		Point lastchunkpoint;
		Point lastmouse;
		internal List<string> LogFile = new List<string>();
		Dictionary<string, ToolStripMenuItem> levelMenuItems;
		ProjectFile game;
		string levelfilename;
		string levelname;
		BackgroundLayerJson level;
		TileIndex[,] planemap;
		ReplaceTilesDialog replaceTilesDialog;
		TextMapping textMapping;

		internal void Log(params string[] lines)
		{
			lock (LogFile)
			{
				LogFile.AddRange(lines);
			}
		}

		Tab CurrentTab
		{
			get { return (Tab)tabControl1.SelectedIndex; }
			set { tabControl1.SelectedIndex = (int)value; }
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			Settings = Settings.Load();
			imageTransparency.SetColorMatrix(new ColorMatrix() { Matrix33 = 0.75f }, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			enableGridToolStripMenuItem.Checked = Settings.ShowGrid;
			foreach (ToolStripMenuItem item in zoomToolStripMenuItem.DropDownItems)
				if (item.Text == Settings.ZoomLevel)
				{
					zoomToolStripMenuItem_DropDownItemClicked(this, new ToolStripItemClickedEventArgs(item));
					break;
				}
			transparentBackgroundToolStripMenuItem.Checked = Settings.TransparentBackgroundExport;
			CurrentTab = Settings.CurrentTab;
			switch (Settings.WindowMode)
			{
				case WindowMode.Maximized:
					WindowState = FormWindowState.Maximized;
					break;
				case WindowMode.Fullscreen:
					prevbnds = Bounds;
					prevstate = WindowState;
					TopMost = true;
					WindowState = FormWindowState.Normal;
					FormBorderStyle = FormBorderStyle.None;
					Bounds = Screen.FromControl(this).Bounds;
					break;
			}
			mainMenuStrip.Visible = Settings.ShowMenu;
			enableDraggingPaletteButton.Checked = Settings.EnableDraggingPalette;
			enableDraggingTilesButton.Checked = Settings.EnableDraggingTiles;
			if (Settings.MRUList == null)
				Settings.MRUList = new List<string>();
			List<string> mru = new List<string>();
			foreach (string item in Settings.MRUList)
			{
				if (File.Exists(item))
				{
					mru.Add(item);
					recentProjectsToolStripMenuItem.DropDownItems.Add(item.Replace("&", "&&"));
				}
			}
			Settings.MRUList = mru;
			if (mru.Count > 0) recentProjectsToolStripMenuItem.DropDownItems.Remove(noneToolStripMenuItem2);
			if (Program.Arguments.Length > 0)
				LoadINI(Path.GetFullPath(Program.Arguments[0]));
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (loaded)
			{
				switch (MessageBox.Show(this, "Do you want to save?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
				{
					case DialogResult.Yes:
						saveToolStripMenuItem_Click(this, EventArgs.Empty);
						break;
					case DialogResult.Cancel:
						e.Cancel = true;
						break;
				}
			}
			if (Settings != null)
			{
				Settings.ShowGrid = enableGridToolStripMenuItem.Checked;
				Settings.ZoomLevel = zoomToolStripMenuItem.DropDownItems.Cast<ToolStripMenuItem>().Single((a) => a.Checked).Text;
				Settings.CurrentTab = CurrentTab;
				if (TopMost)
					Settings.WindowMode = WindowMode.Fullscreen;
				else if (WindowState == FormWindowState.Maximized)
					Settings.WindowMode = WindowMode.Maximized;
				else
					Settings.WindowMode = WindowMode.Normal;
				Settings.ShowMenu = mainMenuStrip.Visible;
				Settings.EnableDraggingPalette = enableDraggingPaletteButton.Checked;
				Settings.EnableDraggingTiles = enableDraggingTilesButton.Checked;
				Settings.Save();
			}
		}

		private void LoadINI(string filename)
		{
			try
			{
				Log($"Opening project file \"{filename}\"...");
				game = ProjectFile.Load(filename);
				Environment.CurrentDirectory = Path.GetDirectoryName(filename);
				Log($"Game type is {game.Game}.");
			}
			catch (Exception ex)
			{
				using (LoadErrorDialog ed = new LoadErrorDialog(ex.GetType().Name + ": " + ex.Message))
					ed.ShowDialog(this);
				return;
			}
			changeLevelToolStripMenuItem.DropDownItems.Clear();
			levelMenuItems = new Dictionary<string, ToolStripMenuItem>();
			var parent = (ToolStripMenuItem)changeLevelToolStripMenuItem.DropDownItems.Add("Level Backgrounds");
			var parent2 = (ToolStripMenuItem)changeLevelToolStripMenuItem.DropDownItems.Add("Level Chunks");
			foreach (string item in game.Levels.Where(a => !string.IsNullOrEmpty(a)))
			{
				var level = LevelJson.Load(item);
				if (!string.IsNullOrEmpty(level.Background1))
				{
					string key = Path.GetFileNameWithoutExtension(level.Background1);
					if (!levelMenuItems.ContainsKey(key))
					{
						ToolStripMenuItem ts = new ToolStripMenuItem(key.Replace("&", "&&"), null, new EventHandler(LevelToolStripMenuItem_Clicked)) { Tag = level.Background1 };
						levelMenuItems.Add(key, ts);
						parent.DropDownItems.Add(ts);
					}
				}
				if (!string.IsNullOrEmpty(level.Background2))
				{
					string key = Path.GetFileNameWithoutExtension(level.Background2);
					if (!levelMenuItems.ContainsKey(key))
					{
						ToolStripMenuItem ts = new ToolStripMenuItem(key.Replace("&", "&&"), null, new EventHandler(LevelToolStripMenuItem_Clicked)) { Tag = level.Background2 };
						levelMenuItems.Add(key, ts);
						parent.DropDownItems.Add(ts);
					}
				}
				if (!string.IsNullOrEmpty(level.ForegroundHigh))
				{
					var path = level.GetForegroundHigh().Chunks;
					var key = Path.GetFileNameWithoutExtension(path);
					if (!levelMenuItems.ContainsKey(key))
					{
						ToolStripMenuItem ts = new ToolStripMenuItem(key.Replace("&", "&&"), null, new EventHandler(LevelToolStripMenuItem_Clicked)) { Tag = Path.ChangeExtension(path, ".sabg") };
						levelMenuItems.Add(key, ts);
						parent2.DropDownItems.Add(ts);
					}
				}
				if (!string.IsNullOrEmpty(level.ForegroundLow))
				{
					var path = level.GetForegroundLow().Chunks;
					var key = Path.GetFileNameWithoutExtension(path);
					if (!levelMenuItems.ContainsKey(key))
					{
						ToolStripMenuItem ts = new ToolStripMenuItem(key.Replace("&", "&&"), null, new EventHandler(LevelToolStripMenuItem_Clicked)) { Tag = Path.ChangeExtension(path, ".sabg") };
						levelMenuItems.Add(key, ts);
						parent2.DropDownItems.Add(ts);
					}
				}
			}
			var rootnode = new MenuNode();
			foreach (string item in game.Backgrounds.Where(a => !string.IsNullOrEmpty(a)).Distinct())
			{
				string key = Path.GetFileNameWithoutExtension(item);
				if (!levelMenuItems.ContainsKey(key))
				{
					var node = rootnode;
					string[] split = key.Split(' ');
					for (int i = 0; i < split.Length - 1; i++)
						node = node.AddSubItem(split[i]);
					node.AddSubItem(split[split.Length - 1], item);
				}
			}
			rootnode.CollapseSubitems();
			rootnode.CleanTree();
			AddMenuNodes(rootnode, changeLevelToolStripMenuItem);
			Text = "AdvanceBG - " + game.Game.ToString();
			if (Settings.MRUList.Count == 0)
				recentProjectsToolStripMenuItem.DropDownItems.Remove(noneToolStripMenuItem2);
			if (Settings.MRUList.Contains(filename))
			{
				recentProjectsToolStripMenuItem.DropDownItems.RemoveAt(Settings.MRUList.IndexOf(filename));
				Settings.MRUList.Remove(filename);
			}
			Settings.MRUList.Insert(0, filename);
			recentProjectsToolStripMenuItem.DropDownItems.Insert(0, new ToolStripMenuItem(filename));
		}

		private void AddMenuNodes(MenuNode node, ToolStripMenuItem parent)
		{
			if (node.SubItems != null)
				foreach (var item in node.SubItems)
				{
					ToolStripMenuItem ts;
					if (item.Filename != null)
						ts = new ToolStripMenuItem(item.Name.Replace("&", "&&"), null, new EventHandler(LevelToolStripMenuItem_Clicked)) { Tag = item.Filename };
					else
						ts = new ToolStripMenuItem(item.Name.Replace("&", "&&"));
					levelMenuItems.Add(item.GetFullName(), ts);
					parent.DropDownItems.Add(ts);
					AddMenuNodes(item, ts);
				}
		}

		#region Main Menu
		#region File Menu
		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (loaded)
			{
				switch (MessageBox.Show(this, "Do you want to save?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
				{
					case DialogResult.Yes:
						saveToolStripMenuItem_Click(this, EventArgs.Empty);
						break;
					case DialogResult.Cancel:
						return;
				}
			}
			using (OpenFileDialog a = new OpenFileDialog()
			{
				DefaultExt = "saproj",
				Filter = "SAPROJ Files|*.saproj|All Files|*.*"
			})
				if (a.ShowDialog(this) == DialogResult.OK)
				{
					loaded = false;
					LoadINI(a.FileName);
				}
		}

		private void LevelToolStripMenuItem_Clicked(object sender, EventArgs e)
		{
			if (loaded)
			{
				fileToolStripMenuItem.DropDown.Hide();
				switch (MessageBox.Show(this, "Do you want to save?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
				{
					case DialogResult.Yes:
						saveToolStripMenuItem_Click(this, EventArgs.Empty);
						break;
					case DialogResult.Cancel:
						return;
				}
			}
			loaded = false;
			foreach (KeyValuePair<string, ToolStripMenuItem> item in levelMenuItems)
				item.Value.Checked = false;
			((ToolStripMenuItem)sender).Checked = true;
			Enabled = false;
			UseWaitCursor = true;
			levelfilename = (string)((ToolStripItem)sender).Tag;
			levelname = Path.GetFileNameWithoutExtension(levelfilename);
			level = BackgroundLayerJson.Load(levelfilename);
			Text = $"AdvanceBG - {game.Game} - Loading {levelname}...";
#if !DEBUG
			initerror = null;
			backgroundLevelLoader.RunWorkerAsync();
#else
			backgroundLevelLoader_DoWork(null, null);
			backgroundLevelLoader_RunWorkerCompleted(null, null);
#endif
		}

		Exception initerror = null;
		private void backgroundLevelLoader_DoWork(object sender, DoWorkEventArgs e)
		{
#if !DEBUG
			try
			{
#endif
			Log("Loading " + levelname + "...");
#if !DEBUG
				System.Threading.Tasks.Parallel.Invoke(LoadLevelTiles, LoadLevelLayout, LoadLevelPalette, ProcessTextMapping);
#else
			LoadLevelTiles();
			LoadLevelLayout();
			LoadLevelPalette();
			ProcessTextMapping();
#endif
			using (Bitmap palbmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
				LevelData.BmpPal = palbmp.Palette;
			LevelData.BmpPal.Entries.Fill(Color.Black);
			LevelData.Palette.Select(a => a.RGBColor).ToArray().CopyTo(LevelData.BmpPal.Entries, level.PalDest);
			using (Bitmap palbmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
				LevelImgPalette = palbmp.Palette;
			LevelData.BmpPal.Entries.CopyTo(LevelImgPalette.Entries, 0);
			curpal = new Color[16];
#if !DEBUG
			}
			catch (Exception ex) { initerror = ex; }
#endif
		}

		private void LoadLevelTiles()
		{
			LevelData.Tiles = new List<byte[]>();
			if (level.Tiles == null)
				throw new FormatException("Can't load background with no tiles!");
			if (File.Exists(level.Tiles))
			{
				Log($"Loading {(level.Mode == BGMode.Normal ? 4 : 8)}bpp tiles from file \"{level.Tiles}\"...");
				using (var fs = File.OpenRead(level.Tiles))
				using (var br = new BinaryReader(fs))
					while (fs.Position < fs.Length)
						LevelData.Tiles.Add(br.ReadBytes(level.Mode == BGMode.Normal ? 32 : 64));
			}
			else
			{
				Log($"{(level.Mode == BGMode.Normal ? 4 : 8)}bpp tile file \"{level.Tiles}\" not found.");
				LevelData.Tiles.Add(new byte[level.Mode == BGMode.Normal ? 32 : 64]);
			}
		}

		private void LoadLevelLayout()
		{
			if (level.Layout == null)
				throw new FormatException("Can't load background with no layout!");
			if (File.Exists(level.Layout))
			{
				Log($"Loading plane mappings from file \"{level.Layout}\"...");
				planemap = level.GetLayout();
			}
			else
			{
				Log($"Plane mappings file \"{level.Layout}\" not found.");
				planemap = new TileIndex[level.Width, level.Height];
				for (int y = 0; y < level.Height; y++)
					for (int x = 0; x < level.Width; x++)
						planemap[x, y] = new TileIndex();
			}
		}

		private void LoadLevelPalette()
		{
			LevelData.Palette = level.GetPalette();
		}

		private void ProcessTextMapping()
		{
			textMapping = null;
			/*if (level.TextMapping != null)
				textMapping = new TextMapping(level.TextMapping);
			else if (level.TextMappingFile != null && File.Exists(level.TextMappingFile))
				textMapping = IniSerializer.Deserialize<TextMapping>(level.TextMappingFile);
			if (textMapping != null && level.TileOffset > 0)
				foreach (var cm in textMapping.Characters)
					for (int y = 0; y < textMapping.Height; y++)
						for (int x = 0; x < (cm.Value.Width ?? textMapping.DefaultWidth); x++)
							if (cm.Value.Map[x, y] > 0)
								cm.Value.Map[x, y] -= (ushort)(level.TileOffset - 1);*/
		}

		private void backgroundLevelLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (initerror != null)
			{
				Log(initerror.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
				File.WriteAllLines("AdvanceBG.log", LogFile.ToArray());
				string msg = initerror.GetType().Name + ": " + initerror.Message;
				if (initerror is AggregateException ae)
				{
					msg += " =>";
					foreach (Exception ex in ae.InnerExceptions)
						msg += Environment.NewLine + ex.GetType().Name + ": " + ex.Message;
				}
				using (LoadErrorDialog ed = new LoadErrorDialog(msg))
					ed.ShowDialog(this);
				Text = "AdvanceBG - " + game.Game;
				Enabled = true;
				return;
			}
			Log("Load completed.");
			importTilesToolStripButton.Enabled = LevelData.Tiles.Count < TileMax;
			drawTileToolStripButton.Enabled = importTilesToolStripButton.Enabled;
			RefreshTileSelector();
			TileSelector.SelectedIndex = 0;
			TileSelector.ChangeSize();
			replaceTilesDialog = new ReplaceTilesDialog(level.Mode != BGMode.Normal);
			Text = $"AdvanceBG - {game.Game} - {levelname}";
			UpdateScrollBars();
			foregroundPanel.HScrollValue = 0;
			foregroundPanel.HScrollSmallChange = 8;
			foregroundPanel.HScrollLargeChange = 128;
			foregroundPanel.VScrollValue = 0;
			foregroundPanel.VScrollSmallChange = 8;
			foregroundPanel.VScrollLargeChange = 128;
			foregroundPanel.HScrollEnabled = true;
			foregroundPanel.VScrollEnabled = true;
			PalettePanel.Height = (LevelData.Palette.Length + 15) / 16 * 20;
			PalettePanelGfx = PalettePanel.CreateGraphics();
			colorEditingPanel.Enabled = true;
			paletteToolStrip.Enabled = true;
			cyclePaletteToolStripMenuItem.Enabled = level.Mode == BGMode.Normal;
			flipToolStripMenuItem.Enabled = mirrorToolStripMenuItem.Enabled = flipPanel.Visible = level.Mode != BGMode.Scale;
			flipPanel.Enabled = true;
			copiedTile = new TileIndex();
			xFlip.Checked = false;
			yFlip.Checked = false;
			loaded = true;
			saveToolStripMenuItem.Enabled = true;
			editToolStripMenuItem.Enabled = true;
			exportToolStripMenuItem.Enabled = true;
			for (int i = 0; i < 16; i++)
				curpal[i] = LevelData.Palette[SelectedColor.Y * 16 + i].RGBColor;
			TileID.Maximum = LevelData.Tiles.Count - 1;
			TileCount.Text = $"{LevelData.Tiles.Count:X} / {TileMax:X}";
			deleteUnusedTilesToolStripButton.Enabled = removeDuplicateTilesToolStripButton.Enabled =
				replaceForegroundToolStripButton.Enabled = clearForegroundToolStripButton.Enabled =
				importToolStripButton.Enabled = usageCountsToolStripMenuItem.Enabled = true;
			Enabled = true;
			UseWaitCursor = false;
			DrawLevel();
		}

		private Bitmap TileToBmp(byte[] tile, int pal) => level.Mode == BGMode.Normal ? LevelData.Tile4bppToBmp(tile, pal) : LevelData.Tile8bppToBmp(tile);

		private BitmapBits TileToBmp(TileIndex tinf)
		{
			BitmapBits bmp;
			if (level.Mode == BGMode.Normal)
			{
				bmp = BitmapBits.FromTile4bpp(LevelData.Tiles[tinf.Tile], 0);
				byte pal = (byte)(tinf.Palette << 4);
				for (int i = 0; i < bmp.Bits.Length; i++)
					if (bmp.Bits[i] != 0)
						bmp.Bits[i] |= pal;
			}
			else
				bmp = BitmapBits.FromTile8bpp(LevelData.Tiles[tinf.Tile], 0);
			bmp.Flip(tinf.XFlip, tinf.YFlip);
			return bmp;
		}

		private void RefreshTileSelector()
		{
			TileSelector.Images.Clear();
			for (int i = 0; i < LevelData.Tiles.Count; i++)
				TileSelector.Images.Add(TileToBmp(LevelData.Tiles[i], SelectedColor.Y));
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Threading.Tasks.Parallel.Invoke(SaveLevelTiles, SaveLevelLayout, SaveLevelPalette);
			level.Width = (ushort)planemap.GetLength(0);
			level.Height = (ushort)planemap.GetLength(1);
			level.Save(levelfilename);
		}

		private void SaveLevelTiles()
		{
			using (var fs = File.Create(level.Tiles))
			using (var bw = new BinaryWriter(fs))
				foreach (var t in LevelData.Tiles)
					bw.Write(t);
		}

		private void SaveLevelLayout()
		{
			using (var fs = File.Create(level.Layout))
			using (var bw = new BinaryWriter(fs))
				for (int y = 0; y < planemap.GetLength(1); y++)
					for (int x = 0; x < planemap.GetLength(0); x++)
						if (level.Mode != BGMode.Scale)
							bw.Write(planemap[x, y].GetUShort());
						else
							bw.Write((byte)planemap[x, y].Tile);
		}

		private void SaveLevelPalette()
		{
			using (var fs = File.Create(level.Palette))
			using (var bw = new BinaryWriter(fs))
				foreach (var c in LevelData.Palette)
					bw.Write(c.Value);
		}

		private void recentProjectsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			loaded = false;
			LoadINI(Settings.MRUList[recentProjectsToolStripMenuItem.DropDownItems.IndexOf(e.ClickedItem)]);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}
		#endregion

		#region Edit Menu
		private void resizeLevelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (ResizeLevelDialog dg = new ResizeLevelDialog())
			{
				dg.levelHeight.Value = planemap.GetLength(1);
				dg.levelWidth.Value = planemap.GetLength(0);
				if (dg.ShowDialog(this) == DialogResult.OK)
				{
					ResizeMap((int)dg.levelWidth.Value, (int)dg.levelHeight.Value);
					loaded = false;
					UpdateScrollBars();
					loaded = true;
					DrawLevel();
				}
			}
		}

		private void ResizeMap(int width, int height)
		{
			int oldwidth = planemap.GetLength(0);
			int oldheight = planemap.GetLength(1);
			TileIndex[,] newFG = new TileIndex[width, height];
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
					if (x < oldwidth && y < oldheight)
						newFG[x, y] = planemap[x, y];
					else
						newFG[x, y] = new TileIndex();
			planemap = newFG;
		}
		#endregion

		#region View Menu
		private void gridColorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ColorDialog a = new ColorDialog
			{
				AllowFullOpen = true,
				AnyColor = true,
				FullOpen = true,
				SolidColorOnly = true,
				Color = Settings.GridColor
			};
			if (cols != null)
				a.CustomColors = cols;
			if (a.ShowDialog() == DialogResult.OK)
			{
				Settings.GridColor = a.Color;
				if (loaded)
					DrawLevel();
			}
			cols = a.CustomColors;
			a.Dispose();
		}

		private void zoomToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			foreach (ToolStripMenuItem item in zoomToolStripMenuItem.DropDownItems)
				item.Checked = false;
			((ToolStripMenuItem)e.ClickedItem).Checked = true;
			switch (zoomToolStripMenuItem.DropDownItems.IndexOf(e.ClickedItem))
			{
				case 0: // 1/8x
					ZoomLevel = 0.125;
					break;
				case 1: // 1/4x
					ZoomLevel = 0.25;
					break;
				case 2: // 1/2x
					ZoomLevel = 0.5;
					break;
				default:
					ZoomLevel = zoomToolStripMenuItem.DropDownItems.IndexOf(e.ClickedItem) - 2;
					break;
			}
			if (!loaded) return;
			loaded = false;
			UpdateScrollBars();
			loaded = true;
			DrawLevel();
		}
		#endregion

		#region Export Menu
		private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog a = new SaveFileDialog() { DefaultExt = "png", Filter = "PNG Files|*.png", RestoreDirectory = true })
				if (a.ShowDialog(this) == DialogResult.OK)
				{
					int numlines = (LevelData.Palette.Length + 15) / 16;
					BitmapBits bmp = new BitmapBits(16 * 8, numlines * 8);
					for (int y = 0; y < numlines; y++)
						for (int x = 0; x < 16; x++)
							bmp.FillRectangle((byte)((y * 16) + x), x * 8, y * 8, 8, 8);
					bmp.ToBitmap(LevelData.BmpPal).Save(a.FileName);
				}
		}

		private void yYCHRToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog a = new SaveFileDialog() { DefaultExt = "act", Filter = "Palette Files|*.act;*.pal", RestoreDirectory = true })
				if (a.ShowDialog(this) == DialogResult.OK)
					using (FileStream str = File.Create(a.FileName))
					using (BinaryWriter bw = new BinaryWriter(str))
					{
						foreach (var c in LevelData.Palette)
						{
							bw.Write(c.R);
							bw.Write(c.G);
							bw.Write(c.B);
						}
						if (LevelData.Palette.Length < 256)
							bw.Write(new byte[3 * (LevelData.Palette.Length - 256)]);
					}
		}

		private void jASCPALToolStripMenuItem_DropDownItemClicked(object sender, EventArgs e)
		{
			exportToolStripMenuItem.DropDown.Hide();
			using (SaveFileDialog a = new SaveFileDialog() { DefaultExt = "pal", Filter = "JASC-PAL Files|*.pal;*.PspPalette", RestoreDirectory = true })
				if (a.ShowDialog(this) == DialogResult.OK)
					using (StreamWriter writer = File.CreateText(a.FileName))
					{
						writer.WriteLine("JASC-PAL");
						writer.WriteLine("0100");
						writer.WriteLine("256");
						foreach (var c in LevelData.Palette)
							writer.WriteLine("{0} {1} {2}", c.R, c.G, c.B);
						for (int i = LevelData.Palette.Length; i < 256; i++)
							writer.WriteLine("0 0 0");
						writer.Close();
					}
		}

		private void tilesToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			exportToolStripMenuItem.DropDown.Hide();
			using (FolderBrowserDialog a = new FolderBrowserDialog() { SelectedPath = Environment.CurrentDirectory })
				if (a.ShowDialog() == DialogResult.OK)
					for (int i = 0; i < LevelData.Tiles.Count; i++)
						TileToBmp(LevelData.Tiles[i], SelectedColor.Y)
							.Save(Path.Combine(a.SelectedPath,
							(useHexadecimalIndexesToolStripMenuItem.Checked ? i.ToString("X2") : i.ToString()) + ".png"));
		}

		private void foregroundToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog a = new SaveFileDialog()
			{
				DefaultExt = "png",
				Filter = "PNG Files|*.png",
				RestoreDirectory = true
			})
				if (a.ShowDialog() == DialogResult.OK)
				{
					BitmapBits bmp = DrawPlane(null);
					using (Bitmap res = bmp.ToBitmap(LevelImgPalette))
						res.Save(a.FileName);
				}
		}

		private void transparentBackgroundToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Settings.TransparentBackgroundExport = transparentBackgroundToolStripMenuItem.Checked;
		}

		private void useHexadecimalIndexesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Settings.UseHexadecimalIndexesExport = useHexadecimalIndexesToolStripMenuItem.Checked;
		}
		#endregion

		#region Help Menu
		#endregion
		#endregion

		public BitmapBits DrawPlane(Rectangle? section)
		{
			Rectangle bounds;
			if (section.HasValue)
				bounds = section.Value;
			else
				bounds = new Rectangle(0, 0, planemap.GetLength(0) * 8, planemap.GetLength(1) * 8);
			BitmapBits levelImg8bpp = new BitmapBits(bounds.Size);
			for (int y = Math.Max(bounds.Y / 8, 0); y <= Math.Min((bounds.Bottom - 1) / 8, planemap.GetLength(1) - 1); y++)
				for (int x = Math.Max(bounds.X / 8, 0); x <= Math.Min((bounds.Right - 1) / 8, planemap.GetLength(0) - 1); x++)
					if (planemap[x, y].Tile < LevelData.Tiles.Count)
						levelImg8bpp.DrawBitmapBounded(TileToBmp(planemap[x, y]), x * 8 - bounds.X, y * 8 - bounds.Y);
			return levelImg8bpp;
		}

		BitmapBits LevelImg8bpp;
		static readonly Pen selectionPen = new Pen(Color.FromArgb(128, Color.Black)) { DashStyle = DashStyle.Dot };
		static readonly SolidBrush selectionBrush = new SolidBrush(Color.FromArgb(128, Color.White));
		internal void DrawLevel()
		{
			if (!loaded) return;
			ScrollingPanel panel;
			Rectangle selection;
			switch (CurrentTab)
			{
				case Tab.Foreground:
					panel = foregroundPanel;
					selection = FGSelection;
					break;
				default:
					return;
			}
			Point camera = new Point(panel.HScrollValue, panel.VScrollValue);
			Rectangle dispRect = new Rectangle(camera.X, camera.Y, (int)(panel.PanelWidth / ZoomLevel), (int)(panel.PanelHeight / ZoomLevel));
			LevelImg8bpp = DrawPlane(dispRect);
			LevelBmp = LevelImg8bpp.ToBitmap(LevelImgPalette).To32bpp();
			LevelGfx = Graphics.FromImage(LevelBmp);
			LevelGfx.SetOptions();
			BitmapBits gridbmp = new BitmapBits(LevelImg8bpp.Size);
			if (enableGridToolStripMenuItem.Checked)
			{
				for (int x = (8 - (camera.X % 8)) % 8; x < LevelImg8bpp.Width; x += 8)
					gridbmp.DrawLine(1, x, 0, x, LevelImg8bpp.Height - 1);
				for (int y = (8 - (camera.Y % 8)) % 8; y < LevelImg8bpp.Height; y += 8)
					gridbmp.DrawLine(1, 0, y, LevelImg8bpp.Width - 1, y);
			}
			using (var tmpbmp = gridbmp.ToBitmap(Color.Transparent, Settings.GridColor))
				LevelGfx.DrawImage(tmpbmp, 0, 0, LevelImg8bpp.Width, LevelImg8bpp.Height);
			Point pnlcur = panel.PanelPointToClient(Cursor.Position);
			if (!selecting && SelectedTile < LevelData.Tiles.Count)
				LevelGfx.DrawImage(tile.ToBitmap(curpal),
				new Rectangle(((((int)(pnlcur.X / ZoomLevel) + camera.X) / 8) * 8) - camera.X, ((((int)(pnlcur.Y / ZoomLevel) + camera.Y) / 8) * 8) - camera.Y, 8, 8),
				0, 0, 8, 8,
				GraphicsUnit.Pixel, imageTransparency);
			if (!selection.IsEmpty)
			{
				Rectangle selbnds = selection.Scale(8, 8);
				selbnds.Offset(-camera.X, -camera.Y);
				LevelGfx.FillRectangle(selectionBrush, selbnds);
				selbnds.Width--; selbnds.Height--;
				LevelGfx.DrawRectangle(selectionPen, selbnds);
			}
			panel.PanelGraphics.DrawImage(LevelBmp, 0, 0, panel.PanelWidth, panel.PanelHeight);
		}

		private void panel_Paint(object sender, PaintEventArgs e)
		{
			DrawLevel();
		}

		private void UpdateScrollBars()
		{
			foregroundPanel.HScrollMaximum = (int)Math.Max((planemap.GetLength(0) * 8) + foregroundPanel.HScrollLargeChange - (foregroundPanel.PanelWidth / ZoomLevel), 0);
			foregroundPanel.VScrollMaximum = (int)Math.Max((planemap.GetLength(1) * 8) + foregroundPanel.VScrollLargeChange - (foregroundPanel.PanelHeight / ZoomLevel), 0);
		}

		Rectangle prevbnds;
		FormWindowState prevstate;
		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Enter:
					if (e.Alt)
					{
						if (!TopMost)
						{
							prevbnds = Bounds;
							prevstate = WindowState;
							TopMost = true;
							WindowState = FormWindowState.Normal;
							FormBorderStyle = FormBorderStyle.None;
							Bounds = Screen.FromControl(this).Bounds;
						}
						else
						{
							TopMost = false;
							WindowState = prevstate;
							FormBorderStyle = FormBorderStyle.Sizable;
							Bounds = prevbnds;
						}
					}
					break;
				case Keys.F5:
					mainMenuStrip.ShowHide();
					break;
				case Keys.D1:
				case Keys.NumPad1:
					if (e.Control)
						CurrentTab = Tab.Foreground;
					break;
				case Keys.D2:
				case Keys.NumPad2:
					if (e.Control)
						CurrentTab = Tab.Art;
					break;
			}
		}

		private void foregroundPanel_KeyDown(object sender, KeyEventArgs e)
		{
			long step = e.Control ? int.MaxValue : e.Shift ? 8 : 16;
			switch (e.KeyCode)
			{
				case Keys.Up:
					if (!loaded) return;
					foregroundPanel.VScrollValue = (int)Math.Max(foregroundPanel.VScrollValue - step, foregroundPanel.VScrollMinimum);
					break;
				case Keys.Down:
					if (!loaded) return;
					foregroundPanel.VScrollValue = (int)Math.Min(foregroundPanel.VScrollValue + step, foregroundPanel.VScrollMaximum - 8 + 1);
					break;
				case Keys.Left:
					if (!loaded) return;
					foregroundPanel.HScrollValue = (int)Math.Max(foregroundPanel.HScrollValue - step, foregroundPanel.HScrollMinimum);
					break;
				case Keys.Right:
					if (!loaded) return;
					foregroundPanel.HScrollValue = (int)Math.Min(foregroundPanel.HScrollValue + step, foregroundPanel.HScrollMaximum - 8 + 1);
					break;
				case Keys.A:
					if (!loaded) return;
					SelectedTile = SelectedTile == 0 ? LevelData.Tiles.Count - 1 : SelectedTile - 1;
					DrawLevel();
					break;
				case Keys.Z:
					if (!loaded) return;
					SelectedTile = SelectedTile == LevelData.Tiles.Count - 1 ? 0 : SelectedTile + 1;
					DrawLevel();
					break;
				case Keys.I:
					enableGridToolStripMenuItem.Checked = !enableGridToolStripMenuItem.Checked;
					DrawLevel();
					break;
				case Keys.OemMinus:
				case Keys.Subtract:
					for (int i = 1; i < zoomToolStripMenuItem.DropDownItems.Count; i++)
						if (((ToolStripMenuItem)zoomToolStripMenuItem.DropDownItems[i]).Checked)
						{
							zoomToolStripMenuItem_DropDownItemClicked(sender, new ToolStripItemClickedEventArgs(zoomToolStripMenuItem.DropDownItems[i - 1]));
							break;
						}
					break;
				case Keys.Oemplus:
				case Keys.Add:
					for (int i = 0; i < zoomToolStripMenuItem.DropDownItems.Count - 1; i++)
						if (((ToolStripMenuItem)zoomToolStripMenuItem.DropDownItems[i]).Checked)
						{
							zoomToolStripMenuItem_DropDownItemClicked(sender, new ToolStripItemClickedEventArgs(zoomToolStripMenuItem.DropDownItems[i + 1]));
							break;
						}
					break;
			}
		}

		private void foregroundPanel_MouseDown(object sender, MouseEventArgs e)
		{
			if (!loaded) return;
			Point chunkpoint = new Point(((int)(e.X / ZoomLevel) + foregroundPanel.HScrollValue) / 8, ((int)(e.Y / ZoomLevel) + foregroundPanel.VScrollValue) / 8);
			if (chunkpoint.X >= planemap.GetLength(0) | chunkpoint.Y >= planemap.GetLength(1)) return;
			switch (e.Button)
			{
				case MouseButtons.Left:
					FGSelection = Rectangle.Empty;
					planemap[chunkpoint.X, chunkpoint.Y] = copiedTile.Clone();
					DrawLevel();
					break;
				case MouseButtons.Right:
					menuLoc = chunkpoint;
					if (!FGSelection.Contains(chunkpoint))
					{
						FGSelection = Rectangle.Empty;
						DrawLevel();
					}
					lastmouse = new Point((int)(e.X / ZoomLevel) + foregroundPanel.HScrollValue, (int)(e.Y / ZoomLevel) + foregroundPanel.VScrollValue);
					break;
			}
		}

		private void foregroundPanel_MouseMove(object sender, MouseEventArgs e)
		{
			if (!loaded) return;
			if (e.X < 0 || e.Y < 0 || e.X > foregroundPanel.PanelWidth || e.Y > foregroundPanel.PanelHeight) return;
			Point mouse = new Point((int)(e.X / ZoomLevel) + foregroundPanel.HScrollValue, (int)(e.Y / ZoomLevel) + foregroundPanel.VScrollValue);
			Point chunkpoint = new Point(mouse.X / 8, mouse.Y / 8);
			if (chunkpoint.X >= planemap.GetLength(0) | chunkpoint.Y >= planemap.GetLength(1)) return;
			switch (e.Button)
			{
				case MouseButtons.Left:
					planemap[chunkpoint.X, chunkpoint.Y] = copiedTile.Clone();
					DrawLevel();
					break;
				case MouseButtons.Right:
					if (!selecting)
						if (Math.Sqrt(Math.Pow(e.X - lastmouse.X, 2) + Math.Pow(e.Y - lastmouse.Y, 2)) > 5)
							selecting = true;
						else
							break;
					if (FGSelection.IsEmpty)
						FGSelection = new Rectangle(chunkpoint, new Size(1, 1));
					else
					{
						int l = Math.Min(FGSelection.Left, chunkpoint.X);
						int t = Math.Min(FGSelection.Top, chunkpoint.Y);
						int r = Math.Max(FGSelection.Right, chunkpoint.X + 1);
						int b = Math.Max(FGSelection.Bottom, chunkpoint.Y + 1);
						if (FGSelection.Width > 1 && lastchunkpoint.X == l && chunkpoint.X > lastchunkpoint.X)
							l = chunkpoint.X;
						if (FGSelection.Height > 1 && lastchunkpoint.Y == t && chunkpoint.Y > lastchunkpoint.Y)
							t = chunkpoint.Y;
						if (FGSelection.Width > 1 && lastchunkpoint.X == r - 1 && chunkpoint.X < lastchunkpoint.X)
							r = chunkpoint.X + 1;
						if (FGSelection.Height > 1 && lastchunkpoint.Y == b - 1 && chunkpoint.Y < lastchunkpoint.Y)
							b = chunkpoint.Y + 1;
						FGSelection = Rectangle.FromLTRB(l, t, r, b);
					}
					DrawLevel();
					break;
				default:
					if (chunkpoint != lastchunkpoint)
						DrawLevel();
					break;
			}
			lastchunkpoint = chunkpoint;
		}

		private void foregroundPanel_MouseUp(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Right:
					Point mouse = new Point((int)(e.X / ZoomLevel) + foregroundPanel.HScrollValue, (int)(e.Y / ZoomLevel) + foregroundPanel.VScrollValue);
					Point chunkpoint = new Point(mouse.X / 8, mouse.Y / 8);
					if (chunkpoint.X < 0 || chunkpoint.Y < 0 || chunkpoint.X >= planemap.GetLength(0) || chunkpoint.Y >= planemap.GetLength(1)) return;
					if (FGSelection.IsEmpty)
					{
						SelectedTile = planemap[chunkpoint.X, chunkpoint.Y].Tile;
						if (SelectedTile < LevelData.Tiles.Count)
							TileSelector.SelectedIndex = SelectedTile;
						copiedTile = planemap[chunkpoint.X, chunkpoint.Y].Clone();
						SetSelectedColor(new Point(SelectedColor.X, copiedTile.Palette));
						xFlip.Checked = copiedTile.XFlip;
						yFlip.Checked = copiedTile.YFlip;
						DrawLevel();
					}
					else if (!selecting)
					{
						pasteOnceToolStripMenuItem.Enabled = pasteRepeatingToolStripMenuItem.Enabled = Clipboard.ContainsData(typeof(TileIndex[,]).AssemblyQualifiedName);
						editTextToolStripMenuItem.Enabled = textMapping != null && FGSelection.Height % textMapping.Height == 0;
						layoutContextMenuStrip.Show(foregroundPanel, e.Location);
					}
					selecting = false;
					break;
			}
		}

		private void ScrollBar_ValueChanged(object sender, EventArgs e)
		{
			if (!loaded) return;
			DrawLevel();
		}

		private void panel_Resize(object sender, EventArgs e)
		{
			if (!loaded) return;
			loaded = false;
			UpdateScrollBars();
			loaded = true;
			DrawLevel();
		}

		Point menuLoc;
		private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
			selecting = false;
			switch (CurrentTab)
			{
				case Tab.Foreground:
					tableLayoutPanel1.Controls.Add(TileSelector, 0, 1);
					TileSelector.AllowDrop = false;
					foregroundPanel.Focus();
					break;
				case Tab.Art:
					panel1.Controls.Add(TileSelector);
					TileSelector.BringToFront();
					TileSelector.AllowDrop = enableDraggingTilesButton.Checked;
					break;
			}
			DrawLevel();
		}

		int SelectedTile;
		Point SelectedColor;

		Color[] disppal = null;
		private void DrawPalette()
		{
			if (!loaded) return;
			Color[] pal = disppal;
			if (pal == null)
				pal = LevelData.Palette.Select(a => a.RGBColor).ToArray();
			for (int x = 0; x < pal.Length; x++)
			{
				PalettePanelGfx.FillRectangle(new SolidBrush(pal[x]), x % 16 * 20, x / 16 * 20, 20, 20);
				PalettePanelGfx.DrawRectangle(Pens.White, x % 16 * 20, x / 16 * 20, 19, 19);
			}
			if (disppal == null)
				PalettePanelGfx.DrawRectangle(new Pen(Color.Yellow, 2), SelectedColor.X * 20, SelectedColor.Y * 20, 20, 20);
			else if (level.Mode != BGMode.Normal || lastmouse.Y == SelectedColor.Y)
				PalettePanelGfx.DrawRectangle(new Pen(Color.Yellow, 2), lastmouse.X * 20, lastmouse.Y * 20, 20, 20);
			else
				PalettePanelGfx.DrawRectangle(new Pen(Color.Yellow, 2), 0, lastmouse.Y * 20, 320, 20);
		}

		private void PalettePanel_Paint(object sender, PaintEventArgs e)
		{
			DrawPalette();
		}

		int[] cols;
		private void PalettePanel_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (!loaded || e.Button != MouseButtons.Left) return;
			int line = e.Y / 20;
			int index = e.X / 20;
			int mouseidx = line * 16 + index;
			if (mouseidx < 0 || mouseidx >= LevelData.Palette.Length) return;
			SelectedColor = new Point(index, line);
			using (ColorDialog a = new ColorDialog
			{
				AllowFullOpen = true,
				AnyColor = true,
				FullOpen = true,
				Color = LevelData.Palette[mouseidx].RGBColor
			})
			{
				if (cols != null)
					a.CustomColors = cols;
				if (a.ShowDialog() == DialogResult.OK)
				{
					LevelData.Palette[mouseidx].RGBColor = a.Color;
					PaletteChanged();
				}
				cols = a.CustomColors;
			}
			loaded = false;
			ushort md = LevelData.Palette[mouseidx].Value;
			colorRed.Value = md & 0x1F;
			colorGreen.Value = (md >> 5) & 0x1F;
			colorBlue.Value = (md >> 10) & 0x1F;
			loaded = true;
		}

		private void color_ValueChanged(object sender, EventArgs e)
		{
			if (!loaded) return;
			LevelData.Palette[SelectedColor.Y * 16 + SelectedColor.X] = new GBAColor((ushort)((int)colorRed.Value | (int)colorGreen.Value << 5 | (int)colorBlue.Value << 10));
			PaletteChanged();
		}

		private Color[] curpal;
		private void PalettePanel_MouseDown(object sender, MouseEventArgs e)
		{
			if (!loaded) return;
			Point mouseColor = new Point(e.X / 20, e.Y / 20);
			int mouseidx = mouseColor.Y * 16 + mouseColor.X;
			int selidx = SelectedColor.Y * 16 + SelectedColor.X;
			if (mouseidx < 0 || mouseidx >= LevelData.Palette.Length) return;
			if (mouseColor == SelectedColor) return;
			bool newpal = level.Mode == BGMode.Normal && mouseColor.Y != SelectedColor.Y;
			switch (e.Button)
			{
				case MouseButtons.Left:
					SetSelectedColor(mouseColor);
					if (newpal)
					{
						curpal = new Color[16];
						for (int i = 0; i < 16; i++)
							curpal[i] = LevelData.Palette[SelectedColor.Y * 16 + i].RGBColor;
						DrawTilePicture();
						RefreshTileSelector();
					}
					break;
				case MouseButtons.Right:
					if (!newpal)
					{
						int start = Math.Min(selidx, mouseidx);
						int end = Math.Max(selidx, mouseidx);
						if (end - start == 1) return;
						Color startcol = LevelData.Palette[start].RGBColor;
						Color endcol = LevelData.Palette[end].RGBColor;
						double r = startcol.R;
						double g = startcol.G;
						double b = startcol.B;
						double radd = (endcol.R - startcol.R) / (double)(end - start);
						double gadd = (endcol.G - startcol.G) / (double)(end - start);
						double badd = (endcol.B - startcol.B) / (double)(end - start);
						for (int x = start + 1; x < end; x++)
						{
							r += radd;
							g += gadd;
							b += badd;
							LevelData.Palette[x].RGBColor = Color.FromArgb((int)Math.Round(r, MidpointRounding.AwayFromZero), (int)Math.Round(g, MidpointRounding.AwayFromZero), (int)Math.Round(b, MidpointRounding.AwayFromZero));
						}
						PaletteChanged();
					}
					break;
			}
		}

		private void SetSelectedColor(Point color)
		{
			SelectedColor = color;
			lastmouse = color;
			DrawPalette();
			loaded = false;
			ushort md = LevelData.Palette[SelectedColor.Y * 16 + SelectedColor.X].Value;
			colorRed.Value = md & 0x1F;
			colorGreen.Value = (md >> 5) & 0x1F;
			colorBlue.Value = (md >> 10) & 0x1F;
			loaded = true;
			if (level.Mode == BGMode.Normal)
			{
				copiedTile.Palette = (byte)color.Y;
				RefreshTileSelector();
			}
		}

		private void PalettePanel_MouseMove(object sender, MouseEventArgs e)
		{
			if (!loaded || e.Button != MouseButtons.Left || !enableDraggingPaletteButton.Checked) return;
			Point mouseColor = new Point(e.X / 20, e.Y / 20);
			if (mouseColor == lastmouse) return;
			if (mouseColor == SelectedColor)
			{
				disppal = null;
				lastmouse = mouseColor;
				DrawPalette();
			}
			int mouseidx = mouseColor.Y * 16 + mouseColor.X;
			int selidx = SelectedColor.Y * 16 + SelectedColor.X;
			if (mouseidx < 0 || mouseidx >= LevelData.Palette.Length) return;
			if (level.Mode == BGMode.Normal)
			{
				List<List<int>> palidxs = new List<List<int>>();
				for (int y = 0; y < LevelData.Palette.Length / 16; y++)
				{
					List<int> l = new List<int>();
					for (int x = 0; x < 16; x++)
						l.Add(y * 16 + x);
					palidxs.Add(l);
				}
				if (mouseColor.Y != SelectedColor.Y)
				{
					if (mouseColor.Y == lastmouse.Y)
					{
						lastmouse = mouseColor;
						return;
					}
					if ((ModifierKeys & Keys.Control) == Keys.Control)
						palidxs.Swap(SelectedColor.Y, mouseColor.Y);
					else
						palidxs.Move(SelectedColor.Y, mouseColor.Y > SelectedColor.Y ? mouseColor.Y + 1 : mouseColor.Y);
				}
				else
				{
					if ((ModifierKeys & Keys.Control) == Keys.Control)
						palidxs[mouseColor.Y].Swap(SelectedColor.X, mouseColor.X);
					else
						palidxs[mouseColor.Y].Move(SelectedColor.X, mouseColor.X > SelectedColor.X ? mouseColor.X + 1 : mouseColor.X);
				}
				disppal = palidxs.SelectMany(a => a.Select(b => LevelData.Palette[b].RGBColor)).ToArray();
			}
			else
			{
				List<int> palidxs = Enumerable.Range(0, LevelData.Palette.Length).ToList();
				if ((ModifierKeys & Keys.Control) == Keys.Control)
					palidxs.Swap(selidx, mouseidx);
				else
					palidxs.Move(selidx, mouseidx > selidx ? mouseidx + 1 : mouseidx);
				disppal = palidxs.Select(a => LevelData.Palette[a].RGBColor).ToArray();
			}
			lastmouse = mouseColor;
			DrawPalette();
		}

		private void PalettePanel_MouseUp(object sender, MouseEventArgs e)
		{
			if (!loaded || e.Button != MouseButtons.Left || !enableDraggingPaletteButton.Checked) return;
			Point mouseColor = lastmouse;
			if (mouseColor == SelectedColor) return;
			int mouseidx = mouseColor.Y * 16 + mouseColor.X;
			int selidx = SelectedColor.Y * 16 + SelectedColor.X;
			if (mouseidx < 0 || mouseidx >= LevelData.Palette.Length) return;
			disppal = null;
			if (level.Mode == BGMode.Normal)
			{
				List<List<Point>> palidxs = new List<List<Point>>();
				for (int y = 0; y < LevelData.Palette.Length / 16; y++)
				{
					List<Point> l = new List<Point>();
					for (int x = 0; x < 16; x++)
						l.Add(new Point(x, y));
					palidxs.Add(l);
				}
				if (mouseColor.Y != SelectedColor.Y)
				{
					if (mouseColor.Y == lastmouse.Y)
					{
						lastmouse = mouseColor;
						return;
					}
					if ((ModifierKeys & Keys.Control) == Keys.Control)
						palidxs.Swap(SelectedColor.Y, mouseColor.Y);
					else
						palidxs.Move(SelectedColor.Y, mouseColor.Y > SelectedColor.Y ? mouseColor.Y + 1 : mouseColor.Y);
				}
				else
				{
					if ((ModifierKeys & Keys.Control) == Keys.Control)
						palidxs[mouseColor.Y].Swap(SelectedColor.X, mouseColor.X);
					else
						palidxs[mouseColor.Y].Move(SelectedColor.X, mouseColor.X > SelectedColor.X ? mouseColor.X + 1 : mouseColor.X);
				}
				List<int> tiles = new List<int>();
				for (int y = 0; y < planemap.GetLength(1); y++)
					for (int x = 0; x < planemap.GetLength(0); x++)
						if (planemap[x, y].Palette == mouseColor.Y && !tiles.Contains(planemap[x, y].Tile))
						{
							int t = planemap[x, y].Tile;
							byte[] til = LevelData.Tiles[t];
							if (til != null)
							{
								BitmapBits bmp = BitmapBits.FromTile4bpp(til, 0);
								for (int i = 0; i < bmp.Bits.Length; i++)
									bmp.Bits[i] = (byte)palidxs[mouseColor.Y].FindIndex((a) => a.X == bmp.Bits[i]);
								LevelData.Tiles[t] = bmp.ToTile4bpp();
							}
							tiles.Add(t);
						}
			}
			else
			{
				List<int> palidxs = Enumerable.Range(0, LevelData.Palette.Length).ToList();
				if ((ModifierKeys & Keys.Control) == Keys.Control)
					palidxs.Swap(selidx, mouseidx);
				else
					palidxs.Move(selidx, mouseidx > selidx ? mouseidx + 1 : mouseidx);
				foreach (var til in LevelData.Tiles)
					for (int i = 0; i < til.Length; i++)
						til[i] = (byte)palidxs[til[i]];
				LevelData.Palette = palidxs.Select(a => LevelData.Palette[a]).ToArray();
			}
			SelectedColor = mouseColor;
			PaletteChanged();
		}

		private void importPaletteToolStripButton_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog a = new OpenFileDialog())
			{
				a.DefaultExt = "bin";
				a.Filter = "MD Palettes|*.bin|Image Files|*.bmp;*.png;*.jpg;*.gif";
				a.RestoreDirectory = true;
				if (a.ShowDialog(this) == DialogResult.OK)
				{
					int selind = SelectedColor.Y * 16 + SelectedColor.X;
					switch (Path.GetExtension(a.FileName))
					{
						case ".bin":
							{
								GBAColor[] colors = GBAColor.Load(a.FileName);
								Array.Copy(colors, 0, LevelData.Palette, selind, Math.Min(colors.Length, LevelData.Palette.Length - selind));
							}
							break;
						case ".bmp":
						case ".png":
						case ".jpg":
						case ".gif":
							using (Bitmap bmp = new Bitmap(a.FileName))
							{
								if ((bmp.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
								{
									GBAColor[] colors = bmp.Palette.Entries.Select(b => new GBAColor(b)).ToArray();
									Array.Copy(colors, 0, LevelData.Palette, selind, Math.Min(colors.Length, LevelData.Palette.Length - selind));
								}
								else
									for (int y = 0; y < bmp.Height; y += 8)
										for (int ix = 0; ix < bmp.Width; ix += 8)
										{
											LevelData.Palette[selind++].RGBColor = bmp.GetPixel(ix, y);
											if (selind == LevelData.Palette.Length)
												break;
										}
							}
							break;
					}
				}
			}
			PaletteChanged();
		}

		private BitmapBits tile;
		private TileIndex copiedTile = new TileIndex();
		private void TileSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			importTilesToolStripButton.Enabled = LevelData.Tiles.Count < TileMax;
			drawTileToolStripButton.Enabled = importTilesToolStripButton.Enabled;
			if (TileSelector.SelectedIndex > -1)
			{
				rotateTileRightButton.Enabled = flipTileHButton.Enabled = flipTileVButton.Enabled = true;
				SelectedTile = TileSelector.SelectedIndex;
				if (level.Mode == BGMode.Normal)
					tile = BitmapBits.FromTile4bpp(LevelData.Tiles[SelectedTile], SelectedColor.Y);
				else
					tile = BitmapBits.FromTile8bpp(LevelData.Tiles[SelectedTile], 0);
				TileID.Value = SelectedTile;
				TileCount.Text = $"{LevelData.Tiles.Count:X} / {TileMax:X}";
				DrawTilePicture();
				copiedTile.Tile = (ushort)SelectedTile;
			}
			else
				rotateTileRightButton.Enabled = flipTileHButton.Enabled = flipTileVButton.Enabled = false;
		}

		private void TilePicture_Paint(object sender, PaintEventArgs e)
		{
			DrawTilePicture();
		}

		private void DrawTilePicture()
		{
			if (TileSelector.SelectedIndex == -1) return;
			using (Graphics gfx = TilePicture.CreateGraphics())
			{
				gfx.SetOptions();
				gfx.DrawImage(tile.Scale(16).ToBitmap(level.Mode == BGMode.Normal ? curpal : LevelImgPalette.Entries), 0, 0, TilePicture.Width, TilePicture.Height);
			}
		}

		private void TilePicture_MouseDown(object sender, MouseEventArgs e)
		{
			if (TileSelector.SelectedIndex == -1) return;
			if (e.Button == MouseButtons.Left)
			{
				tile[e.X / 16, e.Y / 16] = (byte)(level.Mode == BGMode.Normal ? SelectedColor.X : SelectedColor.Y * 16 + SelectedColor.X);
				DrawTilePicture();
			}
			else if (e.Button == MouseButtons.Right)
				SetSelectedColor(level.Mode == BGMode.Normal ? new Point(tile[e.X / 16, e.Y / 16], SelectedColor.Y) : new Point(e.X / 16, e.Y / 16));
		}

		private void TilePicture_MouseMove(object sender, MouseEventArgs e)
		{
			if (TileSelector.SelectedIndex == -1) return;
			if (e.Button == MouseButtons.Left && new Rectangle(Point.Empty, TilePicture.Size).Contains(e.Location))
			{
				tile[e.X / 16, e.Y / 16] = (byte)(level.Mode == BGMode.Normal ? SelectedColor.X : SelectedColor.Y * 16 + SelectedColor.X);
				DrawTilePicture();
			}
		}

		private void TilePicture_MouseUp(object sender, MouseEventArgs e)
		{
			if (TileSelector.SelectedIndex == -1 || e.Button != MouseButtons.Left) return;
			LevelData.Tiles[SelectedTile] = level.Mode == BGMode.Normal ? tile.ToTile4bpp() : tile.Bits;
			TileSelector.Images[SelectedTile] = TileToBmp(LevelData.Tiles[SelectedTile], SelectedColor.Y);
			TileSelector.Invalidate();
		}

		private void TileSelector_MouseDown(object sender, MouseEventArgs e)
		{
			if (!loaded) return;
			if (e.Button == MouseButtons.Right)
			{
				pasteOverToolStripMenuItem.Enabled = Clipboard.ContainsData(level.Mode == BGMode.Normal ? "GBATile16" : "GBATile256");
				pasteBeforeToolStripMenuItem.Enabled = pasteOverToolStripMenuItem.Enabled && LevelData.Tiles.Count < TileMax;
				pasteAfterToolStripMenuItem.Enabled = pasteBeforeToolStripMenuItem.Enabled;
				insertAfterToolStripMenuItem.Enabled = LevelData.Tiles.Count < TileMax;
				insertBeforeToolStripMenuItem.Enabled = insertAfterToolStripMenuItem.Enabled;
				duplicateTilesToolStripMenuItem.Enabled = insertAfterToolStripMenuItem.Enabled;
				deleteTilesToolStripMenuItem.Enabled = TileSelector.Images.Count > 1;
				cutTilesToolStripMenuItem.Enabled = deleteTilesToolStripMenuItem.Enabled;
				tileContextMenuStrip.Show(TileSelector, e.Location);
			}
		}

		private void cutTilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Clipboard.SetData(level.Mode == BGMode.Normal ? "GBATile16" : "GBATile256", LevelData.Tiles[SelectedTile]);
			DeleteTile();
		}

		private void DeleteTile()
		{
			LevelData.Tiles.RemoveAt(SelectedTile);
			for (int y = 0; y < planemap.GetLength(1); y++)
				for (int x = 0; x < planemap.GetLength(0); x++)
					if (planemap[x, y].Tile > SelectedTile && planemap[x, y].Tile < LevelData.Tiles.Count + 1)
						planemap[x, y].Tile--;
			TileSelector.Images.RemoveAt(SelectedTile);
			TileID.Maximum = LevelData.Tiles.Count - 1;
			TileSelector.SelectedIndex = Math.Min(TileSelector.SelectedIndex, TileSelector.Images.Count - 1);
			importTilesToolStripButton.Enabled = true;
			drawTileToolStripButton.Enabled = importTilesToolStripButton.Enabled;
			DrawLevel();
		}

		private void copyTilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Clipboard.SetData(level.Mode == BGMode.Normal ? "GBATile16" : "GBATile256", LevelData.Tiles[SelectedTile]);
		}

		private void InsertTile()
		{
			TileSelector.Images.Insert(SelectedTile, TileToBmp(LevelData.Tiles[SelectedTile], SelectedColor.Y));
			for (int y = 0; y < planemap.GetLength(1); y++)
				for (int x = 0; x < planemap.GetLength(0); x++)
					if (planemap[x, y].Tile >= SelectedTile && planemap[x, y].Tile < LevelData.Tiles.Count)
						planemap[x, y].Tile++;
			TileID.Maximum = LevelData.Tiles.Count - 1;
			TileSelector.SelectedIndex = SelectedTile;
			importTilesToolStripButton.Enabled = LevelData.Tiles.Count < TileMax;
			drawTileToolStripButton.Enabled = importTilesToolStripButton.Enabled;
		}

		private void pasteBeforeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LevelData.Tiles.Insert(SelectedTile, (byte[])Clipboard.GetData(level.Mode == BGMode.Normal ? "GBATile16" : "GBATile256"));
			InsertTile();
		}

		private void pasteAfterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LevelData.Tiles.Insert(++SelectedTile, (byte[])Clipboard.GetData(level.Mode == BGMode.Normal ? "GBATile16" : "GBATile256"));
			InsertTile();
		}

		private void duplicateTilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LevelData.Tiles.Insert(++SelectedTile, (byte[])LevelData.Tiles[SelectedTile].Clone());
			InsertTile();
		}

		private void insertBeforeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LevelData.Tiles.Insert(SelectedTile, new byte[32]);
			InsertTile();
		}

		private void insertAfterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LevelData.Tiles.Insert(++SelectedTile, new byte[32]);
			InsertTile();
		}

		private void deleteTilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DeleteTile();
		}

		private void importToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog opendlg = new OpenFileDialog())
			{
				opendlg.DefaultExt = "png";
				opendlg.Filter = "Image Files|*.bmp;*.png;*.jpg;*.gif";
				opendlg.RestoreDirectory = true;
				if (opendlg.ShowDialog(this) == DialogResult.OK)
				{
					Bitmap bmp = new Bitmap(opendlg.FileName);
					if (bmp.Width < 8 || bmp.Height < 8)
					{
						MessageBox.Show(this, $"The image you have selected is too small ({bmp.Width}x{bmp.Height}). It must be at least as large as one tile (8x8)", "AdvanceBG Tile Importer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						bmp.Dispose();
						return;
					}
					ImportImage(bmp, out _);
				}
			}
		}

		private bool ImportImage(Bitmap bmp, out TileIndex[,] layout)
		{
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			int w = bmp.Width;
			int h = bmp.Height;
			Enabled = false;
			UseWaitCursor = true;
			Application.DoEvents();
			BitmapInfo bmpi = new BitmapInfo(bmp);
			Application.DoEvents();
			bmp.Dispose();
			byte? forcepal = level.Mode == BGMode.Normal && (bmpi.PixelFormat == PixelFormat.Format1bppIndexed || bmpi.PixelFormat == PixelFormat.Format4bppIndexed) ? (byte)SelectedColor.Y : (byte?)null;
			Application.DoEvents();
			List<byte[]> tiles = new List<byte[]>(LevelData.Tiles.Count);
			for (int i = 0; i < LevelData.Tiles.Count; i++)
				tiles.Add(LevelData.Tiles[i]);
			Application.DoEvents();
			object proglock = new object();
			ImportResult ir = LevelData.BitmapToTiles(bmpi, level.Mode != BGMode.Normal, forcepal, tiles, false);
			List<byte[]> newTiles = ir.Art;
			layout = ir.Mappings;
			if (newTiles.Count > 0 && LevelData.Tiles.Count + newTiles.Count > TileMax)
			{
				Enabled = true;
				UseWaitCursor = false;
				MessageBox.Show(this, "There are " + (LevelData.Tiles.Count + newTiles.Count - TileMax) + " tiles over the limit.\nImport cannot proceed.", "AdvanceBG", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			if (newTiles.Count > 0)
			{
				foreach (byte[] t in newTiles)
					LevelData.Tiles.Add(t);
				RefreshTileSelector();
				TileID.Maximum = LevelData.Tiles.Count - 1;
				TileSelector.SelectedIndex = TileSelector.Images.Count - 1;
			}
			sw.Stop();
			StringBuilder msg = new StringBuilder();
			msg.AppendFormat("New tiles: {0:X}\n", newTiles.Count);
			msg.Append("\nCompleted in ");
			if (sw.Elapsed.Hours > 0)
			{
				msg.AppendFormat("{0}:{1:00}:{2:00}", sw.Elapsed.Hours, sw.Elapsed.Minutes, sw.Elapsed.Seconds);
				if (sw.Elapsed.Milliseconds > 0)
					msg.AppendFormat(".{000}", sw.Elapsed.Milliseconds);
			}
			else if (sw.Elapsed.Minutes > 0)
			{
				msg.AppendFormat("{0}:{1:00}", sw.Elapsed.Minutes, sw.Elapsed.Seconds);
				if (sw.Elapsed.Milliseconds > 0)
					msg.AppendFormat(".{000}", sw.Elapsed.Milliseconds);
			}
			else
			{
				msg.AppendFormat("{0}", sw.Elapsed.Seconds);
				if (sw.Elapsed.Milliseconds > 0)
					msg.AppendFormat(".{000}", sw.Elapsed.Milliseconds);
			}
			MessageBox.Show(this, msg.ToString(), "Import Results");
			Enabled = true;
			UseWaitCursor = false;
			return true;
		}

		private void rotateTileRightButton_Click(object sender, EventArgs e)
		{
			tile.Rotate(3);
			LevelData.Tiles[SelectedTile] = level.Mode == BGMode.Normal ? tile.ToTile4bpp() : tile.ToTile8bpp();
			TileSelector.Images[SelectedTile] = TileToBmp(LevelData.Tiles[SelectedTile], SelectedColor.Y);
			DrawTilePicture();
			DrawLevel();
		}

		private void drawToolStripButton_Click(object sender, EventArgs e)
		{
			using (DrawTileDialog dlg = new DrawTileDialog())
			{
				dlg.tile = new BitmapBits(8, 8);
				if (dlg.ShowDialog(this) == DialogResult.OK)
					ImportImage(dlg.tile.ToBitmap(LevelData.BmpPal), out _);
			}
		}

		private void TileList_KeyDown(object sender, KeyEventArgs e)
		{
			if (CurrentTab == Tab.Art)
			{
				switch (e.KeyCode)
				{
					case Keys.C:
						if (e.Control)
							copyTilesToolStripMenuItem_Click(sender, EventArgs.Empty);
						break;
					case Keys.D:
						if (e.Control && LevelData.Tiles.Count < TileMax)
							duplicateTilesToolStripMenuItem_Click(sender, EventArgs.Empty);
						break;
					case Keys.Delete:
						if (TileSelector.Images.Count > 1)
							deleteTilesToolStripMenuItem_Click(sender, EventArgs.Empty);
						break;
					case Keys.Insert:
						if (LevelData.Tiles.Count < TileMax)
							insertBeforeToolStripMenuItem_Click(sender, EventArgs.Empty);
						break;
					case Keys.V:
						if (e.Control && Clipboard.ContainsData("SonLVLTile") && LevelData.Tiles.Count < TileMax)
							pasteAfterToolStripMenuItem_Click(sender, EventArgs.Empty);
						break;
					case Keys.X:
						if (e.Control && TileSelector.Images.Count > 1)
							cutTilesToolStripMenuItem_Click(sender, EventArgs.Empty);
						break;
					case Keys.A:
						if (e.Control)
							LevelData.Tiles.Insert(++SelectedTile, new byte[32]);
						SelectedTile = LevelData.Tiles.Count - 1;
						InsertTile();
						break;
				}
			}
		}

		private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			TileIndex[,] layoutsection = new TileIndex[FGSelection.Width, FGSelection.Height];
			for (int y = 0; y < FGSelection.Height; y++)
				for (int x = 0; x < FGSelection.Width; x++)
				{
					layoutsection[x, y] = planemap[x + FGSelection.X, y + FGSelection.Y].Clone();
					planemap[x + FGSelection.X, y + FGSelection.Y] = new TileIndex();
				}
			Clipboard.SetData(typeof(TileIndex[,]).AssemblyQualifiedName, layoutsection);
			DrawLevel();
		}

		private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			TileIndex[,] layoutsection = new TileIndex[FGSelection.Width, FGSelection.Height];
			for (int y = 0; y < FGSelection.Height; y++)
				for (int x = 0; x < FGSelection.Width; x++)
					layoutsection[x, y] = planemap[x + FGSelection.X, y + FGSelection.Y].Clone();
			Clipboard.SetData(typeof(TileIndex[,]).AssemblyQualifiedName, layoutsection);
		}

		private void pasteOnceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TileIndex[,] section = (TileIndex[,])Clipboard.GetData(typeof(TileIndex[,]).AssemblyQualifiedName);
			int w = Math.Min(section.GetLength(0), planemap.GetLength(0) - menuLoc.X);
			int h = Math.Min(section.GetLength(1), planemap.GetLength(1) - menuLoc.Y);
			for (int y = 0; y < h; y++)
				for (int x = 0; x < w; x++)
					planemap[x + menuLoc.X, y + menuLoc.Y] = section[x, y].Clone();
			DrawLevel();
		}

		private void pasteRepeatingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TileIndex[,] section = (TileIndex[,])Clipboard.GetData(typeof(TileIndex[,]).AssemblyQualifiedName);
			int width = section.GetLength(0);
			int height = section.GetLength(1);
			for (int y = 0; y < FGSelection.Height; y++)
				for (int x = 0; x < FGSelection.Width; x++)
					planemap[x + FGSelection.X, y + FGSelection.Y] = section[x % width, y % height].Clone();
			DrawLevel();
		}

		private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			for (int y = FGSelection.Top; y < FGSelection.Bottom; y++)
				for (int x = FGSelection.Left; x < FGSelection.Right; x++)
					planemap[x, y] = new TileIndex();
			DrawLevel();
		}

		private void fillToolStripMenuItem_Click(object sender, EventArgs e)
		{
			for (int y = FGSelection.Top; y < FGSelection.Bottom; y++)
				for (int x = FGSelection.Left; x < FGSelection.Right; x++)
					planemap[x, y] = copiedTile.Clone();
			DrawLevel();
		}

		private void insertLayoutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (InsertDeleteDialog dlg = new InsertDeleteDialog())
			{
				dlg.Text = "Insert";
				if (dlg.ShowDialog(this) != DialogResult.OK) return;
				if (dlg.shiftH.Checked)
				{
					if (planemap.GetLength(0) < 65535)
						ResizeMap(Math.Min(65535, planemap.GetLength(0) + FGSelection.Width), planemap.GetLength(1));
					for (int y = FGSelection.Top; y < FGSelection.Bottom; y++)
						for (int x = planemap.GetLength(0) - FGSelection.Width - 1; x >= FGSelection.Left; x--)
							planemap[x + FGSelection.Width, y] = planemap[x, y];
					for (int y = FGSelection.Top; y < FGSelection.Bottom; y++)
						for (int x = FGSelection.Left; x < FGSelection.Right; x++)
							planemap[x, y] = new TileIndex();
				}
				else if (dlg.shiftV.Checked)
				{
					if (planemap.GetLength(1) < 65535)
						ResizeMap(planemap.GetLength(0), Math.Min(65535, planemap.GetLength(1) + FGSelection.Height));
					for (int x = FGSelection.Left; x < FGSelection.Right; x++)
						for (int y = planemap.GetLength(1) - FGSelection.Height - 1; y >= FGSelection.Top; y--)
							planemap[x, y + FGSelection.Height] = planemap[x, y];
					for (int x = FGSelection.Left; x < FGSelection.Right; x++)
						for (int y = FGSelection.Top; y < FGSelection.Bottom; y++)
							planemap[x, y] = new TileIndex();
				}
				else if (dlg.entireRow.Checked)
				{
					if (planemap.GetLength(1) < 65535)
						ResizeMap(planemap.GetLength(0), Math.Min(65535, planemap.GetLength(1) + FGSelection.Height));
					for (int x = 0; x < planemap.GetLength(0); x++)
						for (int y = planemap.GetLength(1) - FGSelection.Height - 1; y >= FGSelection.Top; y--)
							planemap[x, y + FGSelection.Height] = planemap[x, y];
					for (int x = 0; x < planemap.GetLength(0); x++)
						for (int y = FGSelection.Top; y < FGSelection.Bottom; y++)
							planemap[x, y] = new TileIndex();
				}
				else if (dlg.entireColumn.Checked)
				{
					if (planemap.GetLength(0) < 65535)
						ResizeMap(Math.Min(65535, planemap.GetLength(0) + FGSelection.Width), planemap.GetLength(1));
					for (int y = 0; y < planemap.GetLength(1); y++)
						for (int x = planemap.GetLength(0) - FGSelection.Width - 1; x >= FGSelection.Left; x--)
							planemap[x + FGSelection.Width, y] = planemap[x, y];
					for (int y = 0; y < planemap.GetLength(1); y++)
						for (int x = FGSelection.Left; x < FGSelection.Right; x++)
							planemap[x, y] = new TileIndex();
				}
			}
		}

		private void deleteLayoutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (InsertDeleteDialog dlg = new InsertDeleteDialog())
			{
				dlg.Text = "Delete";
				dlg.shiftH.Text = "Shift cells left";
				dlg.shiftV.Text = "Shift cells up";
				if (dlg.ShowDialog(this) != DialogResult.OK) return;
				if (dlg.shiftH.Checked)
				{
					for (int y = FGSelection.Top; y < FGSelection.Bottom; y++)
						for (int x = FGSelection.Left; x < planemap.GetLength(0) - FGSelection.Width; x++)
							planemap[x, y] = planemap[x + FGSelection.Width, y];
					for (int y = FGSelection.Top; y < FGSelection.Bottom; y++)
						for (int x = planemap.GetLength(0) - FGSelection.Width; x < planemap.GetLength(0); x++)
							planemap[x, y] = new TileIndex();
				}
				else if (dlg.shiftV.Checked)
				{
					for (int x = FGSelection.Left; x < FGSelection.Right; x++)
						for (int y = FGSelection.Top; y < planemap.GetLength(1) - FGSelection.Height; y++)
							planemap[x, y] = planemap[x, y + FGSelection.Height];
					for (int x = FGSelection.Left; x < FGSelection.Right; x++)
						for (int y = planemap.GetLength(1) - FGSelection.Height; y < planemap.GetLength(1); y++)
							planemap[x, y] = new TileIndex();
				}
				else if (dlg.entireRow.Checked)
				{
					for (int x = 0; x < planemap.GetLength(0); x++)
						for (int y = FGSelection.Top; y < planemap.GetLength(1) - FGSelection.Height; y++)
							planemap[x, y] = planemap[x, y + FGSelection.Height];
					for (int x = 0; x < planemap.GetLength(0); x++)
						for (int y = planemap.GetLength(1) - FGSelection.Height; y < planemap.GetLength(1); y++)
							planemap[x, y] = new TileIndex();
					if (planemap.GetLength(1) > FGSelection.Height)
						ResizeMap(planemap.GetLength(0), planemap.GetLength(1) - FGSelection.Height);
				}
				else if (dlg.entireColumn.Checked)
				{
					for (int y = 0; y < planemap.GetLength(1); y++)
						for (int x = FGSelection.Left; x < planemap.GetLength(0) - FGSelection.Width; x++)
							planemap[x, y] = planemap[x + FGSelection.Width, y];
					for (int y = 0; y < planemap.GetLength(1); y++)
						for (int x = planemap.GetLength(0) - FGSelection.Width; x < planemap.GetLength(0); x++)
							planemap[x, y] = new TileIndex();
					if (planemap.GetLength(0) > FGSelection.Width)
						ResizeMap(planemap.GetLength(0) - FGSelection.Width, planemap.GetLength(1));
				}
			}
		}

		private void TileSelector_ItemDrag(object sender, EventArgs e)
		{
			if (enableDraggingTilesButton.Checked)
				DoDragDrop(new DataObject("AdvanceBGTileIndex_" + pid, TileSelector.SelectedIndex), DragDropEffects.Move);
		}

		bool tile_dragdrop;
		int tile_dragobj;
		Point tile_dragpoint;
		private void TileSelector_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent("AdvanceBGTileIndex_" + pid))
			{
				e.Effect = DragDropEffects.Move;
				tile_dragdrop = true;
				tile_dragobj = (int)e.Data.GetData("AdvanceBGTileIndex_" + pid);
				tile_dragpoint = TileSelector.PointToClient(new Point(e.X, e.Y));
				TileSelector.Invalidate();
			}
			else
				tile_dragdrop = false;
		}

		private void TileSelector_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent("AdvanceBGTileIndex_" + pid))
			{
				e.Effect = DragDropEffects.Move;
				tile_dragdrop = true;
				tile_dragobj = (int)e.Data.GetData("AdvanceBGTileIndex_" + pid);
				tile_dragpoint = TileSelector.PointToClient(new Point(e.X, e.Y));
				if (tile_dragpoint.Y < 8)
					TileSelector.ScrollValue -= 8 - dragpoint.Y;
				else if (dragpoint.Y > TileSelector.Height - 8)
					TileSelector.ScrollValue += dragpoint.Y - (TileSelector.Height - 8);
				TileSelector.Invalidate();
			}
			else
				tile_dragdrop = false;
		}

		private void TileSelector_DragLeave(object sender, EventArgs e)
		{
			tile_dragdrop = false;
			TileSelector.Invalidate();
		}

		private void TileSelector_Paint(object sender, PaintEventArgs e)
		{
			if (tile_dragdrop)
			{
				e.Graphics.DrawImage(TileSelector.Images[tile_dragobj], tile_dragpoint.X - (TileSelector.ImageWidth / 2),
					tile_dragpoint.Y - (TileSelector.ImageHeight / 2), TileSelector.ImageWidth, TileSelector.ImageHeight);
				Rectangle r = TileSelector.GetItemBounds(TileSelector.GetItemAtPoint(tile_dragpoint));
				if ((ModifierKeys & Keys.Control) == Keys.Control)
					e.Graphics.DrawRectangle(new Pen(Color.Black, 2), r);
				else
					e.Graphics.DrawLine(new Pen(Color.Black, 2), r.Left + 1, r.Top, r.Left + 1, r.Bottom);
			}
		}

		private void TileSelector_DragDrop(object sender, DragEventArgs e)
		{
			tile_dragdrop = false;
			if (e.Data.GetDataPresent("AdvanceBGTileIndex_" + pid))
			{
				Point clientPoint = TileSelector.PointToClient(new Point(e.X, e.Y));
				ushort newindex = (ushort)TileSelector.GetItemAtPoint(clientPoint);
				ushort oldindex = (ushort)(int)e.Data.GetData("AdvanceBGTileIndex_" + pid);
				if (newindex == oldindex) return;
				if ((ModifierKeys & Keys.Control) == Keys.Control)
				{
					if (newindex == TileSelector.Images.Count) return;
						LevelData.Tiles.Swap(oldindex, newindex);
					TileSelector.Images.Swap(oldindex, newindex);
					for (int y = 0; y < planemap.GetLength(1); y++)
						for (int x = 0; x < planemap.GetLength(0); x++)
						{
							if (planemap[x, y].Tile == newindex)
								planemap[x, y].Tile = oldindex;
							else if (planemap[x, y].Tile == oldindex)
								planemap[x, y].Tile = newindex;
						}
					TileSelector.SelectedIndex = newindex;
				}
				else
				{
					if (newindex == oldindex + 1) return;
						LevelData.Tiles.Move(oldindex, newindex);
					TileSelector.Images.Move(oldindex, newindex);
					for (int y = 0; y < planemap.GetLength(1); y++)
						for (int x = 0; x < planemap.GetLength(0); x++)
						{
							ushort t = planemap[x, y].Tile;
							if (newindex > oldindex)
							{
								if (t == oldindex)
									planemap[x, y].Tile = (ushort)(newindex - 1);
								else if (t > oldindex && t < newindex)
									planemap[x, y].Tile = (ushort)(t - 1);
							}
							else
							{
								if (t == oldindex)
									planemap[x, y].Tile = newindex;
								else if (t >= newindex && t < oldindex)
									planemap[x, y].Tile = (ushort)(t + 1);
							}
						}
					if (newindex > oldindex)
						TileSelector.SelectedIndex = newindex - 1;
					else
						TileSelector.SelectedIndex = newindex;
				}
			}
		}

		private void remapTilesButton_Click(object sender, EventArgs e)
		{
			using (TileRemappingDialog dlg = new TileRemappingDialog(TileSelector.Images, TileSelector.ImageWidth, TileSelector.ImageHeight))
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					List<byte[]> oldtiles = LevelData.Tiles.ToList();
					List<Bitmap> oldimages = new List<Bitmap>(TileSelector.Images);
					Dictionary<ushort, ushort> ushortdict = new Dictionary<ushort, ushort>(dlg.TileMap.Count);
					foreach (KeyValuePair<int, int> item in dlg.TileMap)
					{
						LevelData.Tiles[item.Value] = oldtiles[item.Key];
						TileSelector.Images[item.Value] = oldimages[item.Key];
						ushortdict.Add((ushort)item.Key, (ushort)item.Value);
					}
					TileSelector.ChangeSize();
					TileSelector_SelectedIndexChanged(this, EventArgs.Empty);
				}
		}

		private void flipTileHButton_Click(object sender, EventArgs e)
		{
			tile.Flip(true, false);
			LevelData.Tiles[SelectedTile] = level.Mode == BGMode.Normal ? tile.ToTile4bpp() : tile.ToTile8bpp();
			TileSelector.Images[SelectedTile] = TileToBmp(LevelData.Tiles[SelectedTile], SelectedColor.Y);
			DrawTilePicture();
			TileSelector.Invalidate();
		}

		private void flipTileVButton_Click(object sender, EventArgs e)
		{
			tile.Flip(false, true);
			LevelData.Tiles[SelectedTile] = level.Mode == BGMode.Normal ? tile.ToTile4bpp() : tile.ToTile8bpp();
			TileSelector.Images[SelectedTile] = TileToBmp(LevelData.Tiles[SelectedTile], SelectedColor.Y);
			DrawTilePicture();
			TileSelector.Invalidate();
		}

		private void pasteOverToolStripMenuItem_Click(object sender, EventArgs e)
		{
			byte[] t = (byte[])Clipboard.GetData(level.Mode == BGMode.Normal ? "GBATile16" : "GBATile256");
			LevelData.Tiles[SelectedTile] = t;
			TileSelector.Images[SelectedTile] = TileToBmp(LevelData.Tiles[SelectedTile], SelectedColor.Y);
		}

		private void importToolStripMenuItem2_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog opendlg = new OpenFileDialog()
			{
				DefaultExt = "png",
				Filter = "Image Files|*.bmp;*.png;*.jpg;*.gif",
				RestoreDirectory = true
			})
				if (opendlg.ShowDialog(this) == DialogResult.OK)
					using (Bitmap bmp = new Bitmap(opendlg.FileName))
					{
						if (bmp.Width < 8 || bmp.Height < 8)
						{
							MessageBox.Show(this, $"The image you have selected is too small ({bmp.Width}x{bmp.Height}). It must be at least as large as one tile (8x8)", "AdvanceBG Mappings Importer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
							return;
						}
						if (!ImportImage(bmp, out TileIndex[,] section))
							return;
						int w, h;
						w = Math.Min(section.GetLength(0), planemap.GetLength(0) - menuLoc.X);
						h = Math.Min(section.GetLength(1), planemap.GetLength(1) - menuLoc.Y);
						for (int y = 0; y < h; y++)
							for (int x = 0; x < w; x++)
								planemap[x + menuLoc.X, y + menuLoc.Y] = section[x, y];
					}
		}

		private void deleteUnusedTilesToolStripButton_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(this, "Are you sure you want to delete all tiles not used in these mappings?", "Delete Unused Tiles", MessageBoxButtons.OKCancel) != DialogResult.OK)
				return;
			bool[] tilesused = new bool[LevelData.Tiles.Count];
			foreach (TileIndex pat in planemap)
				if (pat.Tile < tilesused.Length)
						tilesused[pat.Tile] = true;
			ushort c = 0;
			Dictionary<ushort, ushort> tilemap = new Dictionary<ushort, ushort>();
			for (ushort i = 0; i < tilesused.Length; i++)
				if (tilesused[i])
					tilemap[i] = c++;
			foreach (TileIndex pat in planemap)
				if (tilemap.ContainsKey(pat.Tile))
					pat.Tile = tilemap[pat.Tile];
			int numdel = 0;
			for (int i = tilesused.Length - 1; i >= 0; i--)
			{
				if (tilesused[i]) continue;
				LevelData.Tiles.RemoveAt(i);
				numdel++;
			}
			TileID.Maximum = LevelData.Tiles.Count - 1;
			RefreshTileSelector();
			TileSelector.SelectedIndex = Math.Min(TileSelector.SelectedIndex, TileSelector.Images.Count - 1);
			MessageBox.Show(this, "Deleted " + numdel + " unused tiles.", "AdvanceBG");
		}

		private void clearForegroundToolStripButton_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(this, "Are you sure you want to clear the plane?", "Clear Plane", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				for (int y = 0; y < planemap.GetLength(1); y++)
					for (int x = 0; x < planemap.GetLength(0); x++)
						planemap[x, y] = new TileIndex();
			}
		}

		private void usageCountsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (StatisticsDialog dlg = new StatisticsDialog(planemap))
				dlg.ShowDialog(this);
		}

		private void replaceForegroundToolStripButton_Click(object sender, EventArgs e)
		{
			if (replaceTilesDialog.ShowDialog(this) == DialogResult.OK)
			{
				var list = planemap.OfType<TileIndex>().ToList();
				ushort? tile = replaceTilesDialog.findTile.Tile;
				if (tile.HasValue)
					list = list.Where(a => a.Tile == tile.Value).ToList();
				bool? xflip = replaceTilesDialog.findTile.XFlip;
				if (xflip.HasValue)
					list = list.Where(a => a.XFlip == xflip.Value).ToList();
				bool? yflip = replaceTilesDialog.findTile.YFlip;
				if (yflip.HasValue)
					list = list.Where(a => a.YFlip = yflip.Value).ToList();
				byte? palette = replaceTilesDialog.findTile.Palette;
				if (palette.HasValue)
					list = list.Where(a => a.Palette == palette.Value).ToList();
				tile = replaceTilesDialog.replaceTile.Tile;
				xflip = replaceTilesDialog.replaceTile.XFlip;
				yflip = replaceTilesDialog.replaceTile.YFlip;
				palette = replaceTilesDialog.replaceTile.Palette;
				foreach (TileIndex blk in list)
				{
					if (tile.HasValue)
						blk.Tile = tile.Value;
					if (xflip.HasValue)
						blk.XFlip = xflip.Value;
					if (yflip.HasValue)
						blk.YFlip = yflip.Value;
					if (palette.HasValue)
						blk.Palette = palette.Value;
				}
				DrawLevel();
				MessageBox.Show(this, "Replaced " + list.Count + " tiles.", "AdvanceBG");
			}
		}

		private void importToolStripButton_Click(object sender, EventArgs e)
		{
			menuLoc = new Point();
			importToolStripMenuItem2_Click(sender, e);
		}

		private void TileID_ValueChanged(object sender, EventArgs e)
		{
			TileSelector.SelectedIndex = (int)TileID.Value;
		}

		private void ExportTileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog a = new SaveFileDialog() { FileName = (useHexadecimalIndexesToolStripMenuItem.Checked ? SelectedTile.ToString("X2") : SelectedTile.ToString()) + ".png", Filter = "PNG Images|*.png" })
				if (a.ShowDialog() == DialogResult.OK)
					TileToBmp(LevelData.Tiles[SelectedTile], SelectedColor.Y).Save(a.FileName);
		}

		private void removeDuplicateTilesToolStripButton_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(this, "Are you sure you want to remove all duplicate tiles?", "AdvanceBG", MessageBoxButtons.OKCancel) != DialogResult.OK)
				return;
			Dictionary<ushort, byte[]> tiles = new Dictionary<ushort, byte[]>(LevelData.Tiles.Count);
			Dictionary<ushort, TileIndex> tileMap = new Dictionary<ushort, TileIndex>(LevelData.Tiles.Count);
			Stack<int> deleted = new Stack<int>();
			for (int i = 0; i < LevelData.Tiles.Count; i++)
			{
				byte[] tile = LevelData.Tiles[i];
				byte[] tileh, tilev, tilehv;
				switch (level.Mode)
				{
					case BGMode.Normal:
						tileh = LevelData.FlipTile4bpp(tile, true, false);
						tilev = LevelData.FlipTile4bpp(tile, false, true);
						tilehv = LevelData.FlipTile4bpp(tileh, false, true);
						break;
					case BGMode.Color256:
						tileh = LevelData.FlipTile8bpp(tile, true, false);
						tilev = LevelData.FlipTile8bpp(tile, false, true);
						tilehv = LevelData.FlipTile8bpp(tileh, false, true);
						break;
					default:
						tileh = null;
						tilev = null;
						tilehv = null;
						break;
				}
				foreach (var item in tiles)
				{
					if (tile.FastArrayEqual(item.Value))
					{
						tileMap[(ushort)i] = new TileIndex() { Tile = item.Key };
						deleted.Push(i);
						break;
					}
					if (level.Mode != BGMode.Scale)
					{
						if (tileh.FastArrayEqual(item.Value))
						{
							tileMap[(ushort)i] = new TileIndex() { Tile = item.Key, XFlip = true };
							deleted.Push(i);
							break;
						}
						if (tilev.FastArrayEqual(item.Value))
						{
							tileMap[(ushort)i] = new TileIndex() { Tile = item.Key, YFlip = true };
							deleted.Push(i);
							break;
						}
						if (tilehv.FastArrayEqual(item.Value))
						{
							tileMap[(ushort)i] = new TileIndex() { Tile = item.Key, XFlip = true, YFlip = true };
							deleted.Push(i);
							break;
						}
					}
				}
				if (!tileMap.ContainsKey((ushort)i))
				{
					tileMap[(ushort)i] = new TileIndex() { Tile = (ushort)tiles.Count };
					tiles[(ushort)tiles.Count] = tile;
				}
			}
			if (deleted.Count > 0)
			{
				foreach (int i in deleted)
				{
					LevelData.Tiles.RemoveAt(i);
					TileSelector.Images.RemoveAt(i);
				}
				TileID.Maximum = LevelData.Tiles.Count - 1;
				TileSelector.SelectedIndex = Math.Min(TileSelector.SelectedIndex, LevelData.Tiles.Count - 1);
				foreach (TileIndex cb in planemap)
					if (tileMap.ContainsKey(cb.Tile))
					{
						TileIndex nb = tileMap[cb.Tile];
						cb.Tile = nb.Tile;
						cb.XFlip ^= nb.XFlip;
						cb.YFlip ^= nb.YFlip;
					}
				DrawLevel();
			}
			MessageBox.Show(this, "Removed " + deleted.Count + " duplicate tiles.", "AdvanceBG");
		}

		private void XFlip_CheckedChanged(object sender, EventArgs e)
		{
			if (loaded)
				copiedTile.XFlip = xFlip.Checked;
		}

		private void YFlip_CheckedChanged(object sender, EventArgs e)
		{
			if (loaded)
				copiedTile.YFlip = yFlip.Checked;
		}

		private void MirrorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TileIndex[,] res = (TileIndex[,])planemap.Clone();
			for (int y = 0; y < FGSelection.Height; y++)
				for (int x = 0; x < FGSelection.Width; x++)
				{
					TileIndex tmp = planemap[x + FGSelection.Left, y + FGSelection.Top];
					tmp.XFlip ^= tmp.XFlip;
					res[FGSelection.Width - 1 - x + FGSelection.Left, y + FGSelection.Top] = tmp;
				}
			planemap = res;
		}

		private void FlipToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TileIndex[,] res = (TileIndex[,])planemap.Clone();
			for (int y = 0; y < FGSelection.Height; y++)
				for (int x = 0; x < FGSelection.Width; x++)
				{
					TileIndex tmp = planemap[x + FGSelection.Left, y + FGSelection.Top];
					tmp.YFlip ^= tmp.YFlip;
					res[x + FGSelection.Left, FGSelection.Height - 1 - y + FGSelection.Top] = tmp;
				}
			planemap = res;
		}

		private void CyclePaletteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			for (int y = 0; y < FGSelection.Height; y++)
				for (int x = 0; x < FGSelection.Width; x++)
					planemap[x + FGSelection.Left, y + FGSelection.Top].Palette++;
		}

		private void EditTextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			bool unmapped = false;
			int[] pals = new int[4];
			string[] strs = new string[FGSelection.Height / textMapping.Height];
			for (int y = 0; y < FGSelection.Height; y += textMapping.Height)
			{
				StringBuilder sb = new StringBuilder(FGSelection.Width / textMapping.DefaultWidth);
				for (int x = 0; x < FGSelection.Width; )
				{
					KeyValuePair<char, CharMapInfo>? found = null;
					foreach (KeyValuePair<char, CharMapInfo> cm in textMapping.Characters.Where(a => (a.Value.Width ?? textMapping.DefaultWidth) <= FGSelection.Width - x))
					{
						for (int y2 = 0; y2 < textMapping.Height; y2++)
							for (int x2 = 0; x2 < (cm.Value.Width ?? textMapping.DefaultWidth); x2++)
								if (planemap[x + FGSelection.Left + x2, y + FGSelection.Top + y2].Tile != cm.Value.Map[x2, y2])
									goto next;
						found = cm;
						break;
						next:;
					}
					if (!found.HasValue)
					{
						unmapped = true;
						sb.Append(' ');
						x++;
					}
					else
					{
						sb.Append(found.Value.Key);
						if (found.Value.Key != ' ')
							for (int y2 = 0; y2 < textMapping.Height; y2++)
								for (int x2 = 0; x2 < (found.Value.Value.Width ?? textMapping.DefaultWidth); x2++)
									pals[planemap[x + FGSelection.Left + x2, y + FGSelection.Top + y2].Palette]++;
						x += found.Value.Value.Width ?? textMapping.DefaultWidth;
					}
				}
				strs[y / textMapping.Height] = sb.ToString();
			}
			if (unmapped && MessageBox.Show(this, "Selection contains tiles that aren't mapped to characters. These tiles will be converted to spaces.", "AdvanceBG", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				return;
			int pal = 0;
			for (int i = 1; i < 4; i++)
				if (pals[i] > pals[pal])
					pal = i;
			using (TextDialog dlg = new TextDialog(textMapping, FGSelection.Width, strs, pal))
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					byte pal2 = dlg.Palette;
					int y = FGSelection.Top;
					for (int l = 0; l < dlg.Lines.Length; l++)
					{
						int x = FGSelection.Left;
						for (int i = 0; i < dlg.Lines[l].Length; i++)
						{
							CharMapInfo cm = textMapping.Characters[dlg.Lines[l][i]];
							int w = cm.Width ?? textMapping.DefaultWidth;
							for (int y2 = 0; y2 < textMapping.Height; y2++)
								for (int x2 = 0; x2 < w; x2++)
									planemap[x + x2, y+y2] = new TileIndex(cm.Map[x2, y2], false, false, pal2);
							x += w;
						}
						y += textMapping.Height;
					}
					DrawLevel();
				}
		}

		private void importOverToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog opendlg = new OpenFileDialog())
			{
				opendlg.DefaultExt = "png";
				opendlg.Filter = "Image Files|*.bmp;*.png;*.jpg;*.gif";
				opendlg.RestoreDirectory = true;
				if (opendlg.ShowDialog(this) == DialogResult.OK)
				{
					BitmapInfo bmpi;
					using (Bitmap bmp = new Bitmap(opendlg.FileName))
						bmpi = new BitmapInfo(bmp);
					if (bmpi.Width < 8 || bmpi.Height < 8)
					{
						MessageBox.Show(this, "Image must be at least 8x8 to import tile.", "AdvanceBG");
						return;
					}
					ImportResult res = LevelData.BitmapToTiles(bmpi, level.Mode != BGMode.Normal, null, new List<byte[]>(), false, Application.DoEvents);
					List<int> editedTiles = new List<int>();
					LevelData.Tiles[SelectedTile] = res.Art[res.Mappings[0, 0].Tile];
					editedTiles.Add(SelectedTile);
					RefreshTileSelector();
					TileSelector.Invalidate();
					if (editedTiles.Contains(SelectedTile))
						TileSelector_SelectedIndexChanged(this, EventArgs.Empty);
					DrawLevel();
				}
			}
		}
	}

	class MenuNode
	{
		readonly MenuNode parent;
		public MenuNodeCollection SubItems { get; } = new MenuNodeCollection();
		public string Name { get; }
		public string Filename { get; }

		public MenuNode() { }

		private MenuNode(MenuNode parent, string name, string filename = null, IEnumerable<MenuNode> subItems = null)
		{
			this.parent = parent;
			Name = name;
			Filename = filename;
			if (subItems != null)
				foreach (var item in subItems)
					SubItems.Add(new MenuNode(this, item));
		}

		private MenuNode(MenuNode parent, MenuNode item) : this(parent, item.Name, item.Filename, item.SubItems) { }

		public MenuNode AddSubItem(string name, string filename = null, IEnumerable<MenuNode> subItems = null)
		{
			if (SubItems.Contains(name))
				return SubItems[name];
			var node = new MenuNode(this, name, filename, subItems);
			SubItems.Add(node);
			return node;
		}

		public MenuNode InsertSubItem(MenuNode after, string name, string filename = null, IEnumerable<MenuNode> subItems = null)
		{
			if (SubItems.Contains(name))
				return SubItems[name];
			var node = new MenuNode(this, name, filename, subItems);
			SubItems.Insert(SubItems.IndexOf(after) + 1, node);
			return node;
		}

		public void CollapseSubitems()
		{
			var newnodes = SubItems.SelectMany(a =>
			{
				var result = a.SubItems.Select(b => new MenuNode(this, a.Name + " " + b.Name, b.Filename, b.SubItems));
				if (a.Filename != null)
					result.Prepend(a);
				return result;
			}).ToArray();
			SubItems.Clear();
			foreach (var node in newnodes)
				SubItems.Add(node);
		}

		public void CleanTree()
		{
			if (SubItems.Count == 0)
				return;
			if (parent != null)
				foreach (var node in SubItems.Where(a => a.Filename == null && a.Name.Length == 1).ToArray())
				{
					parent.InsertSubItem(this, Name + " " + node.Name, null, node.SubItems).CleanTree();
					SubItems.Remove(node);
				}
			foreach (var node in SubItems.ToArray())
				node.CleanTree();
			if (parent != null && (Filename != null || SubItems.Count < 2))
			{
				foreach (var node in SubItems)
					parent.InsertSubItem(this, Name + " " + node.Name, node.Filename, node.SubItems);
				SubItems.Clear();
				if (Filename == null)
					parent.SubItems.Remove(this);
			}
		}

		public override string ToString() => Name;

		internal string GetFullName()
		{
			if (parent != null)
				return parent.GetFullName() + " " + Name;
			return Name;
		}
	}

	class MenuNodeCollection : System.Collections.ObjectModel.KeyedCollection<string, MenuNode>
	{
		protected override string GetKeyForItem(MenuNode item) => item.Name;
	}
}
