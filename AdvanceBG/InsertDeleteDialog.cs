using System;
using System.Windows.Forms;

namespace AdvanceBG
{
	public partial class InsertDeleteDialog : Form
	{
		public InsertDeleteDialog()
		{
			InitializeComponent();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
