using System;
using System.Windows.Forms;

namespace AdvanceBG
{
	public partial class LoadErrorDialog : Form
	{
		public LoadErrorDialog(string error)
		{
			InitializeComponent();
			label1.Text += error;
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void reportButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
