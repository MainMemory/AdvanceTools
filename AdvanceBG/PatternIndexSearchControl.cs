using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace AdvanceBG
{
	public partial class PatternIndexSearchControl : UserControl
	{
		public PatternIndexSearchControl()
		{
			InitializeComponent();
		}

		bool initializing;
		bool color256;

		public void UpdateStuff(bool color256)
		{
			initializing = true;
			this.color256 = color256;
			tileList.Images.Clear();
			tile.Increment = 1;
			tileList.ImageHeight = 64;
			for (int i = 0; i < LevelData.Tiles.Count; i++)
				tileList.Images.Add(color256 ? LevelData.Tile8bppToBmp(LevelData.Tiles[i]) : LevelData.Tile4bppToBmp(LevelData.Tiles[i], (int)palette.Value));
			tileList.ChangeSize();
			if (tile.Value >= LevelData.Tiles.Count)
				tileList.SelectedIndex = -1;
			else
				tileList.SelectedIndex = (int)tile.Value;
			if (color256)
				searchPalette.Checked = false;
			searchPalette.Enabled = !color256;
			initializing = false;
		}

		private void tile_ValueChanged(object sender, EventArgs e)
		{
			if (!initializing)
			{
				initializing = true;
				if (tile.Value >= LevelData.Tiles.Count)
					tileList.SelectedIndex = -1;
				else
					tileList.SelectedIndex = (int)tile.Value;
				initializing = false;
			}
		}

		private void tileList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!initializing && tileList.SelectedIndex > -1)
				tile.Value = tileList.SelectedIndex;
		}

		private void searchTile_CheckedChanged(object sender, EventArgs e)
		{
			tileList.Enabled = tile.Enabled = searchTile.Checked;
		}

		private void searchPalette_CheckedChanged(object sender, EventArgs e)
		{
			palette.Enabled = searchPalette.Checked;
		}

		private void palette_ValueChanged(object sender, EventArgs e)
		{
			tileList.Images.Clear();
			tileList.ImageHeight = 64;
			for (int i = 0; i < LevelData.Tiles.Count; i++)
				tileList.Images.Add(color256 ? LevelData.Tile8bppToBmp(LevelData.Tiles[i]) : LevelData.Tile4bppToBmp(LevelData.Tiles[i], (int)palette.Value));
			tileList.ChangeSize();
		}

		[Browsable(false)]
		public bool? XFlip
		{
			get
			{
				if (xFlip.CheckState == CheckState.Indeterminate)
					return null;
				else
					return xFlip.Checked;
			}
		}

		[Browsable(false)]
		public bool? YFlip
		{
			get
			{
				if (yFlip.CheckState == CheckState.Indeterminate)
					return null;
				else
					return yFlip.Checked;
			}
		}

		[Browsable(false)]
		public byte? Palette
		{
			get
			{
				if (searchPalette.Checked)
					return (byte)palette.Value;
				else
					return null;
			}
		}

		[Browsable(false)]
		public ushort? Tile
		{
			get
			{
				if (searchTile.Checked)
					return (ushort)tile.Value;
				else
					return null;
			}
		}
	}
}
