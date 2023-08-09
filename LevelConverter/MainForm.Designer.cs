
namespace LevelConverter
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.sourceLevel = new System.Windows.Forms.ComboBox();
			this.destinationLevel = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.convertButton = new System.Windows.Forms.Button();
			this.destinationProj = new LevelConverter.FileSelector();
			this.sourceProj = new LevelConverter.FileSelector();
			this.emptyObjects = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.destinationProj)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sourceProj)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Source Project:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 45);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(73, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Source Level:";
			// 
			// sourceLevel
			// 
			this.sourceLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.sourceLevel.Enabled = false;
			this.sourceLevel.FormattingEnabled = true;
			this.sourceLevel.Location = new System.Drawing.Point(110, 42);
			this.sourceLevel.Name = "sourceLevel";
			this.sourceLevel.Size = new System.Drawing.Size(262, 21);
			this.sourceLevel.TabIndex = 3;
			this.sourceLevel.SelectedIndexChanged += new System.EventHandler(this.level_SelectedIndexChanged);
			// 
			// destinationLevel
			// 
			this.destinationLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.destinationLevel.Enabled = false;
			this.destinationLevel.FormattingEnabled = true;
			this.destinationLevel.Location = new System.Drawing.Point(110, 99);
			this.destinationLevel.Name = "destinationLevel";
			this.destinationLevel.Size = new System.Drawing.Size(262, 21);
			this.destinationLevel.TabIndex = 7;
			this.destinationLevel.SelectedIndexChanged += new System.EventHandler(this.level_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 102);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(92, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Destination Level:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 74);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(99, 13);
			this.label4.TabIndex = 4;
			this.label4.Text = "Destination Project:";
			// 
			// convertButton
			// 
			this.convertButton.Enabled = false;
			this.convertButton.Location = new System.Drawing.Point(297, 126);
			this.convertButton.Name = "convertButton";
			this.convertButton.Size = new System.Drawing.Size(75, 23);
			this.convertButton.TabIndex = 9;
			this.convertButton.Text = "Convert";
			this.convertButton.UseVisualStyleBackColor = true;
			this.convertButton.Click += new System.EventHandler(this.convertButton_Click);
			// 
			// destinationProj
			// 
			this.destinationProj.DefaultExt = "saproj";
			this.destinationProj.FileName = "";
			this.destinationProj.Filter = "SA Projects|*.saproj";
			this.destinationProj.Location = new System.Drawing.Point(110, 69);
			this.destinationProj.Name = "destinationProj";
			this.destinationProj.Size = new System.Drawing.Size(262, 24);
			this.destinationProj.TabIndex = 5;
			this.destinationProj.FileNameChanged += new System.EventHandler(this.destinationProj_FileNameChanged);
			// 
			// sourceProj
			// 
			this.sourceProj.DefaultExt = "saproj";
			this.sourceProj.FileName = "";
			this.sourceProj.Filter = "SA Projects|*.saproj";
			this.sourceProj.Location = new System.Drawing.Point(110, 12);
			this.sourceProj.Name = "sourceProj";
			this.sourceProj.Size = new System.Drawing.Size(262, 24);
			this.sourceProj.TabIndex = 1;
			this.sourceProj.FileNameChanged += new System.EventHandler(this.sourceProj_FileNameChanged);
			// 
			// emptyObjects
			// 
			this.emptyObjects.AutoSize = true;
			this.emptyObjects.Location = new System.Drawing.Point(12, 130);
			this.emptyObjects.Name = "emptyObjects";
			this.emptyObjects.Size = new System.Drawing.Size(127, 17);
			this.emptyObjects.TabIndex = 8;
			this.emptyObjects.Text = "Leave Objects Empty";
			this.emptyObjects.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(391, 172);
			this.Controls.Add(this.emptyObjects);
			this.Controls.Add(this.convertButton);
			this.Controls.Add(this.destinationLevel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.destinationProj);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.sourceLevel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.sourceProj);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "Advance Level Converter";
			((System.ComponentModel.ISupportInitialize)(this.destinationProj)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sourceProj)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private FileSelector sourceProj;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox sourceLevel;
		private System.Windows.Forms.ComboBox destinationLevel;
		private System.Windows.Forms.Label label3;
		private FileSelector destinationProj;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button convertButton;
		private System.Windows.Forms.CheckBox emptyObjects;
	}
}

