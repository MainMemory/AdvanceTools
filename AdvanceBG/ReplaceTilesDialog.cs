using System;
using System.Windows.Forms;

namespace AdvanceBG
{
	public partial class ReplaceTilesDialog : Form
	{
		bool color256;
		public ReplaceTilesDialog(bool color256)
		{
			InitializeComponent();
			this.color256 = color256;
		}

		private void ReplaceBlockTilesDialog_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible)
			{
				findTile.UpdateStuff(color256);
				replaceTile.UpdateStuff(color256);
			}
		}
	}
}
