
namespace MDGBALevelConverter
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mdProject = new SonicRetro.SonLVL.API.FileSelector();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.mdLevel = new System.Windows.Forms.ComboBox();
			this.gbaLevel = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.gbaProject = new SonicRetro.SonLVL.API.FileSelector();
			this.convertButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.mdProject)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gbaProject)).BeginInit();
			this.SuspendLayout();
			// 
			// mdProject
			// 
			this.mdProject.DefaultExt = "";
			this.mdProject.FileName = "";
			this.mdProject.Filter = "INI Files|*.ini";
			this.mdProject.Location = new System.Drawing.Point(86, 12);
			this.mdProject.Name = "mdProject";
			this.mdProject.Size = new System.Drawing.Size(263, 23);
			this.mdProject.TabIndex = 0;
			this.mdProject.FileNameChanged += new System.EventHandler(this.mdProject_FileNameChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(17, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "MD Project:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(24, 44);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "MD Level:";
			// 
			// mdLevel
			// 
			this.mdLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.mdLevel.Enabled = false;
			this.mdLevel.FormattingEnabled = true;
			this.mdLevel.Location = new System.Drawing.Point(86, 41);
			this.mdLevel.Name = "mdLevel";
			this.mdLevel.Size = new System.Drawing.Size(263, 21);
			this.mdLevel.TabIndex = 4;
			this.mdLevel.SelectedIndexChanged += new System.EventHandler(this.mdLevel_SelectedIndexChanged);
			// 
			// gbaLevel
			// 
			this.gbaLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.gbaLevel.Enabled = false;
			this.gbaLevel.FormattingEnabled = true;
			this.gbaLevel.Location = new System.Drawing.Point(86, 97);
			this.gbaLevel.Name = "gbaLevel";
			this.gbaLevel.Size = new System.Drawing.Size(263, 21);
			this.gbaLevel.TabIndex = 8;
			this.gbaLevel.SelectedIndexChanged += new System.EventHandler(this.gbaLevel_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(19, 100);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(61, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "GBA Level:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 73);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(68, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "GBA Project:";
			// 
			// gbaProject
			// 
			this.gbaProject.DefaultExt = "";
			this.gbaProject.FileName = "";
			this.gbaProject.Filter = "SAPROJ Files|*.saproj";
			this.gbaProject.Location = new System.Drawing.Point(86, 68);
			this.gbaProject.Name = "gbaProject";
			this.gbaProject.Size = new System.Drawing.Size(263, 23);
			this.gbaProject.TabIndex = 5;
			this.gbaProject.FileNameChanged += new System.EventHandler(this.gbaProject_FileNameChanged);
			// 
			// convertButton
			// 
			this.convertButton.Enabled = false;
			this.convertButton.Location = new System.Drawing.Point(274, 124);
			this.convertButton.Name = "convertButton";
			this.convertButton.Size = new System.Drawing.Size(75, 23);
			this.convertButton.TabIndex = 9;
			this.convertButton.Text = "Convert";
			this.convertButton.UseVisualStyleBackColor = true;
			this.convertButton.Click += new System.EventHandler(this.convertButton_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(366, 183);
			this.Controls.Add(this.convertButton);
			this.Controls.Add(this.gbaLevel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.gbaProject);
			this.Controls.Add(this.mdLevel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.mdProject);
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.ShowIcon = false;
			this.Text = "MD to GBA Level Converter";
			((System.ComponentModel.ISupportInitialize)(this.mdProject)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gbaProject)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SonicRetro.SonLVL.API.FileSelector mdProject;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox mdLevel;
		private System.Windows.Forms.ComboBox gbaLevel;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private SonicRetro.SonLVL.API.FileSelector gbaProject;
		private System.Windows.Forms.Button convertButton;
	}
}

