using AdvanceTools;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdvanceBG
{
	public partial class DrawTileDialog : Form
	{
		public DrawTileDialog()
		{
			InitializeComponent();
		}

		private void DrawTileDialog_Load(object sender, EventArgs e)
		{
			PalettePanel.Height = (LevelData.Palette.Length + 15) / 16 * 16;
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private Point selectedColor;
		private void PalettePanel_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(Color.Black);
			for (int i = 0; i < LevelData.Palette.Length; i++)
			{
				using (SolidBrush solidBrush = new SolidBrush(LevelData.Palette[i].RGBColor))
					e.Graphics.FillRectangle(solidBrush, i % 16 * 16, i / 16 * 16, 16, 16);
				e.Graphics.DrawRectangle(Pens.White, i % 16 * 16, i / 16 * 16, 15, 15);
			}
			e.Graphics.DrawRectangle(new Pen(Color.Yellow, 2), selectedColor.X * 16, selectedColor.Y * 16, 16, 16);
		}

		private void PalettePanel_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Y / 16 * 16 + e.X / 16 < LevelData.Palette.Length)
			{
				selectedColor = new Point(e.X / 16, e.Y / 16);
				PalettePanel.Invalidate();
			}
		}

		public BitmapBits tile;
		private void TilePicture_Paint(object sender, PaintEventArgs e)
		{
			DrawTile();
		}

		private Tool tool;
		private void TilePicture_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				switch (tool)
				{
					case Tool.Pencil:
						tile[e.X / (int)numericUpDown1.Value, e.Y / (int)numericUpDown1.Value] = (byte)((selectedColor.Y * 16) + selectedColor.X);
						lastpoint = new Point(e.X / (int)numericUpDown1.Value, e.Y / (int)numericUpDown1.Value);
						DrawTile();
						break;
					case Tool.Fill:
						tile.FloodFill((byte)((selectedColor.Y * 16) + selectedColor.X), e.X / (int)numericUpDown1.Value, e.Y / (int)numericUpDown1.Value);
						DrawTile();
						break;
				}
		}

		Point lastpoint;
		private void TilePicture_MouseMove(object sender, MouseEventArgs e)
		{
			if (tool == Tool.Pencil && e.Button == MouseButtons.Left)
			{
				if (new Rectangle(Point.Empty, TilePicture.Size).Contains(e.Location))
				{
					tile.DrawLine((byte)((selectedColor.Y * 16) + selectedColor.X), lastpoint, new Point(e.X / (int)numericUpDown1.Value, e.Y / (int)numericUpDown1.Value));
					tile[e.X / (int)numericUpDown1.Value, e.Y / (int)numericUpDown1.Value] = (byte)((selectedColor.Y * 16) + selectedColor.X);
				}
				lastpoint = new Point(e.X / (int)numericUpDown1.Value, e.Y / (int)numericUpDown1.Value);
				DrawTile();
			}
		}

		private Graphics tileGfx;
		private void DrawTile()
		{
			tileGfx.Clear(LevelData.Palette[0].RGBColor);
			tileGfx.DrawImage(tile.Scale((int)numericUpDown1.Value).ToBitmap(LevelData.BmpPal), 0, 0, tile.Width * (int)numericUpDown1.Value, tile.Height * (int)numericUpDown1.Value);
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			TilePicture.Size = new Size(tile.Width * (int)numericUpDown1.Value, tile.Height * (int)numericUpDown1.Value);
		}

		Cursor pencilcur, fillcur;
		private void DrawTileDialog_Shown(object sender, EventArgs e)
		{
			tileGfx = TilePicture.CreateGraphics();
			tileGfx.SetOptions();
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream(Properties.Resources.pencilcur))
				pencilcur = new Cursor(ms);
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream(Properties.Resources.fillcur))
				fillcur = new Cursor(ms);
			TilePicture.Cursor = pencilcur;
			TilePicture.Size = new Size(tile.Width * (int)numericUpDown1.Value, tile.Height * (int)numericUpDown1.Value);
		}

		private void TilePicture_Resize(object sender, EventArgs e)
		{
			tileGfx = TilePicture.CreateGraphics();
			tileGfx.SetOptions();
		}

		private void pencilToolStripButton_Click(object sender, EventArgs e)
		{
			pencilToolStripButton.Checked = true;
			fillToolStripButton.Checked = false;
			tool = Tool.Pencil;
			TilePicture.Cursor = pencilcur;
		}

		private void fillToolStripButton_Click(object sender, EventArgs e)
		{
			pencilToolStripButton.Checked = false;
			fillToolStripButton.Checked = true;
			tool = Tool.Fill;
			TilePicture.Cursor = fillcur;
		}

		enum Tool { Pencil, Fill }
	}
}
