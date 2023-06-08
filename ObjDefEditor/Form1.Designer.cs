
namespace ObjDefEditor
{
	partial class Form1
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
			this.objectListSelector = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.objectTypeSelector = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.objectVariantSelector = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.levelsSelector = new System.Windows.Forms.CheckedListBox();
			this.spriteListBox = new System.Windows.Forms.ListBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.spriteDownButton = new System.Windows.Forms.Button();
			this.spriteUpButton = new System.Windows.Forms.Button();
			this.deleteSpriteButton = new System.Windows.Forms.Button();
			this.addSpriteButton = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.animationSelector = new ObjDefEditor.TileList();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.animVariantSelector = new ObjDefEditor.TileList();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.animFrameSelector = new ObjDefEditor.TileList();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.yFlipCheckbox = new System.Windows.Forms.CheckBox();
			this.xFlipCheckbox = new System.Windows.Forms.CheckBox();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.saveButton = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.objectNameBox = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.data5Enabled = new System.Windows.Forms.CheckBox();
			this.data4Enabled = new System.Windows.Forms.CheckBox();
			this.data3Enabled = new System.Windows.Forms.CheckBox();
			this.data2Value = new System.Windows.Forms.NumericUpDown();
			this.data2Enabled = new System.Windows.Forms.CheckBox();
			this.data1Enabled = new System.Windows.Forms.CheckBox();
			this.data1Value = new System.Windows.Forms.NumericUpDown();
			this.data3Value = new System.Windows.Forms.NumericUpDown();
			this.data4Value = new System.Windows.Forms.NumericUpDown();
			this.data5Value = new System.Windows.Forms.NumericUpDown();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.groupBox7.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.data2Value)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.data1Value)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.data3Value)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.data4Value)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.data5Value)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(26, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "List:";
			// 
			// objectListSelector
			// 
			this.objectListSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.objectListSelector.FormattingEnabled = true;
			this.objectListSelector.Items.AddRange(new object[] {
            "Interactables",
            "Items",
            "Enemies",
            "Rings",
            "Player"});
			this.objectListSelector.Location = new System.Drawing.Point(44, 14);
			this.objectListSelector.Name = "objectListSelector";
			this.objectListSelector.Size = new System.Drawing.Size(138, 21);
			this.objectListSelector.TabIndex = 1;
			this.objectListSelector.SelectedIndexChanged += new System.EventHandler(this.objectListSelector_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(188, 17);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Type:";
			// 
			// objectTypeSelector
			// 
			this.objectTypeSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.objectTypeSelector.FormattingEnabled = true;
			this.objectTypeSelector.Location = new System.Drawing.Point(228, 14);
			this.objectTypeSelector.Name = "objectTypeSelector";
			this.objectTypeSelector.Size = new System.Drawing.Size(247, 21);
			this.objectTypeSelector.TabIndex = 3;
			this.objectTypeSelector.SelectedIndexChanged += new System.EventHandler(this.objectTypeSelector_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 70);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(43, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Variant:";
			// 
			// objectVariantSelector
			// 
			this.objectVariantSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.objectVariantSelector.FormattingEnabled = true;
			this.objectVariantSelector.Location = new System.Drawing.Point(61, 67);
			this.objectVariantSelector.Name = "objectVariantSelector";
			this.objectVariantSelector.Size = new System.Drawing.Size(386, 21);
			this.objectVariantSelector.TabIndex = 5;
			this.objectVariantSelector.SelectedIndexChanged += new System.EventHandler(this.objectVariantSelector_SelectedIndexChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.levelsSelector);
			this.groupBox1.Location = new System.Drawing.Point(12, 94);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(299, 153);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Levels";
			// 
			// levelsSelector
			// 
			this.levelsSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.levelsSelector.Enabled = false;
			this.levelsSelector.FormattingEnabled = true;
			this.levelsSelector.Location = new System.Drawing.Point(3, 16);
			this.levelsSelector.Name = "levelsSelector";
			this.levelsSelector.Size = new System.Drawing.Size(293, 134);
			this.levelsSelector.TabIndex = 0;
			this.levelsSelector.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.levelsSelector_ItemCheck);
			// 
			// spriteListBox
			// 
			this.spriteListBox.FormattingEnabled = true;
			this.spriteListBox.Location = new System.Drawing.Point(6, 19);
			this.spriteListBox.Name = "spriteListBox";
			this.spriteListBox.Size = new System.Drawing.Size(169, 264);
			this.spriteListBox.TabIndex = 7;
			this.spriteListBox.SelectedIndexChanged += new System.EventHandler(this.spriteListBox_SelectedIndexChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.AutoSize = true;
			this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox2.Controls.Add(this.spriteDownButton);
			this.groupBox2.Controls.Add(this.spriteUpButton);
			this.groupBox2.Controls.Add(this.deleteSpriteButton);
			this.groupBox2.Controls.Add(this.addSpriteButton);
			this.groupBox2.Controls.Add(this.spriteListBox);
			this.groupBox2.Location = new System.Drawing.Point(12, 253);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox2.Size = new System.Drawing.Size(210, 328);
			this.groupBox2.TabIndex = 8;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Sprite List";
			// 
			// spriteDownButton
			// 
			this.spriteDownButton.AutoSize = true;
			this.spriteDownButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.spriteDownButton.Enabled = false;
			this.spriteDownButton.Location = new System.Drawing.Point(181, 48);
			this.spriteDownButton.Name = "spriteDownButton";
			this.spriteDownButton.Size = new System.Drawing.Size(23, 23);
			this.spriteDownButton.TabIndex = 11;
			this.spriteDownButton.Text = "˅";
			this.spriteDownButton.UseVisualStyleBackColor = true;
			this.spriteDownButton.Click += new System.EventHandler(this.spriteDownButton_Click);
			// 
			// spriteUpButton
			// 
			this.spriteUpButton.AutoSize = true;
			this.spriteUpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.spriteUpButton.Enabled = false;
			this.spriteUpButton.Location = new System.Drawing.Point(181, 19);
			this.spriteUpButton.Name = "spriteUpButton";
			this.spriteUpButton.Size = new System.Drawing.Size(23, 23);
			this.spriteUpButton.TabIndex = 10;
			this.spriteUpButton.Text = "˄";
			this.spriteUpButton.UseVisualStyleBackColor = true;
			this.spriteUpButton.Click += new System.EventHandler(this.spriteUpButton_Click);
			// 
			// deleteSpriteButton
			// 
			this.deleteSpriteButton.AutoSize = true;
			this.deleteSpriteButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.deleteSpriteButton.Enabled = false;
			this.deleteSpriteButton.Location = new System.Drawing.Point(48, 289);
			this.deleteSpriteButton.Name = "deleteSpriteButton";
			this.deleteSpriteButton.Size = new System.Drawing.Size(48, 23);
			this.deleteSpriteButton.TabIndex = 9;
			this.deleteSpriteButton.Text = "Delete";
			this.deleteSpriteButton.UseVisualStyleBackColor = true;
			this.deleteSpriteButton.Click += new System.EventHandler(this.deleteSpriteButton_Click);
			// 
			// addSpriteButton
			// 
			this.addSpriteButton.AutoSize = true;
			this.addSpriteButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.addSpriteButton.Location = new System.Drawing.Point(6, 289);
			this.addSpriteButton.Name = "addSpriteButton";
			this.addSpriteButton.Size = new System.Drawing.Size(36, 23);
			this.addSpriteButton.TabIndex = 8;
			this.addSpriteButton.Text = "Add";
			this.addSpriteButton.UseVisualStyleBackColor = true;
			this.addSpriteButton.Click += new System.EventHandler(this.addSpriteButton_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.animationSelector);
			this.groupBox3.Location = new System.Drawing.Point(228, 253);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(472, 122);
			this.groupBox3.TabIndex = 9;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Animation";
			// 
			// animationSelector
			// 
			this.animationSelector.BackColor = System.Drawing.SystemColors.Window;
			this.animationSelector.Direction = ObjDefEditor.Direction.Horizontal;
			this.animationSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.animationSelector.ImageHeight = 80;
			this.animationSelector.ImageSize = 80;
			this.animationSelector.ImageWidth = 80;
			this.animationSelector.Location = new System.Drawing.Point(3, 16);
			this.animationSelector.Name = "animationSelector";
			this.animationSelector.ScrollValue = 0;
			this.animationSelector.SelectedIndex = -1;
			this.animationSelector.Size = new System.Drawing.Size(466, 103);
			this.animationSelector.TabIndex = 0;
			this.animationSelector.SelectedIndexChanged += new System.EventHandler(this.animationSelector_SelectedIndexChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.animVariantSelector);
			this.groupBox4.Location = new System.Drawing.Point(228, 381);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(472, 122);
			this.groupBox4.TabIndex = 10;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Variant";
			// 
			// animVariantSelector
			// 
			this.animVariantSelector.BackColor = System.Drawing.SystemColors.Window;
			this.animVariantSelector.Direction = ObjDefEditor.Direction.Horizontal;
			this.animVariantSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.animVariantSelector.ImageHeight = 80;
			this.animVariantSelector.ImageSize = 80;
			this.animVariantSelector.ImageWidth = 80;
			this.animVariantSelector.Location = new System.Drawing.Point(3, 16);
			this.animVariantSelector.Name = "animVariantSelector";
			this.animVariantSelector.ScrollValue = 0;
			this.animVariantSelector.SelectedIndex = -1;
			this.animVariantSelector.Size = new System.Drawing.Size(466, 103);
			this.animVariantSelector.TabIndex = 0;
			this.animVariantSelector.SelectedIndexChanged += new System.EventHandler(this.animVariantSelector_SelectedIndexChanged);
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.animFrameSelector);
			this.groupBox5.Location = new System.Drawing.Point(228, 509);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(472, 122);
			this.groupBox5.TabIndex = 10;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Frame";
			// 
			// animFrameSelector
			// 
			this.animFrameSelector.BackColor = System.Drawing.SystemColors.Window;
			this.animFrameSelector.Direction = ObjDefEditor.Direction.Horizontal;
			this.animFrameSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.animFrameSelector.ImageHeight = 80;
			this.animFrameSelector.ImageSize = 80;
			this.animFrameSelector.ImageWidth = 80;
			this.animFrameSelector.Location = new System.Drawing.Point(3, 16);
			this.animFrameSelector.Name = "animFrameSelector";
			this.animFrameSelector.ScrollValue = 0;
			this.animFrameSelector.SelectedIndex = -1;
			this.animFrameSelector.Size = new System.Drawing.Size(466, 103);
			this.animFrameSelector.TabIndex = 0;
			this.animFrameSelector.SelectedIndexChanged += new System.EventHandler(this.animFrameSelector_SelectedIndexChanged);
			// 
			// groupBox6
			// 
			this.groupBox6.AutoSize = true;
			this.groupBox6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox6.Controls.Add(this.yFlipCheckbox);
			this.groupBox6.Controls.Add(this.xFlipCheckbox);
			this.groupBox6.Location = new System.Drawing.Point(228, 637);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox6.Size = new System.Drawing.Size(123, 53);
			this.groupBox6.TabIndex = 11;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Settings";
			// 
			// yFlipCheckbox
			// 
			this.yFlipCheckbox.AutoSize = true;
			this.yFlipCheckbox.Location = new System.Drawing.Point(65, 20);
			this.yFlipCheckbox.Name = "yFlipCheckbox";
			this.yFlipCheckbox.Size = new System.Drawing.Size(52, 17);
			this.yFlipCheckbox.TabIndex = 1;
			this.yFlipCheckbox.Text = "Y Flip";
			this.yFlipCheckbox.UseVisualStyleBackColor = true;
			this.yFlipCheckbox.CheckedChanged += new System.EventHandler(this.yFlipCheckbox_CheckedChanged);
			// 
			// xFlipCheckbox
			// 
			this.xFlipCheckbox.AutoSize = true;
			this.xFlipCheckbox.Location = new System.Drawing.Point(7, 20);
			this.xFlipCheckbox.Name = "xFlipCheckbox";
			this.xFlipCheckbox.Size = new System.Drawing.Size(52, 17);
			this.xFlipCheckbox.TabIndex = 0;
			this.xFlipCheckbox.Text = "X Flip";
			this.xFlipCheckbox.UseVisualStyleBackColor = true;
			this.xFlipCheckbox.CheckedChanged += new System.EventHandler(this.xFlipCheckbox_CheckedChanged);
			// 
			// groupBox7
			// 
			this.groupBox7.Controls.Add(this.pictureBox1);
			this.groupBox7.Location = new System.Drawing.Point(453, 41);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(244, 206);
			this.groupBox7.TabIndex = 12;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "Preview";
			// 
			// pictureBox1
			// 
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox1.Location = new System.Drawing.Point(3, 16);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(238, 187);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(622, 12);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(75, 23);
			this.saveButton.TabIndex = 13;
			this.saveButton.Text = "Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 44);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(38, 13);
			this.label4.TabIndex = 14;
			this.label4.Text = "Name:";
			// 
			// objectNameBox
			// 
			this.objectNameBox.Location = new System.Drawing.Point(56, 41);
			this.objectNameBox.Name = "objectNameBox";
			this.objectNameBox.Size = new System.Drawing.Size(391, 20);
			this.objectNameBox.TabIndex = 15;
			this.objectNameBox.TextChanged += new System.EventHandler(this.objectNameBox_TextChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.data5Enabled, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.data4Enabled, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.data3Enabled, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.data2Value, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.data2Enabled, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.data1Enabled, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.data1Value, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.data3Value, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.data4Value, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.data5Value, 1, 4);
			this.tableLayoutPanel1.Enabled = false;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(317, 94);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(133, 136);
			this.tableLayoutPanel1.TabIndex = 16;
			// 
			// data5Enabled
			// 
			this.data5Enabled.AutoSize = true;
			this.data5Enabled.Location = new System.Drawing.Point(4, 112);
			this.data5Enabled.Name = "data5Enabled";
			this.data5Enabled.Size = new System.Drawing.Size(55, 17);
			this.data5Enabled.TabIndex = 8;
			this.data5Enabled.Text = "Data5";
			this.data5Enabled.UseVisualStyleBackColor = true;
			this.data5Enabled.CheckedChanged += new System.EventHandler(this.data5Enabled_CheckedChanged);
			// 
			// data4Enabled
			// 
			this.data4Enabled.AutoSize = true;
			this.data4Enabled.Location = new System.Drawing.Point(4, 85);
			this.data4Enabled.Name = "data4Enabled";
			this.data4Enabled.Size = new System.Drawing.Size(55, 17);
			this.data4Enabled.TabIndex = 6;
			this.data4Enabled.Text = "Data4";
			this.data4Enabled.UseVisualStyleBackColor = true;
			this.data4Enabled.CheckedChanged += new System.EventHandler(this.data4Enabled_CheckedChanged);
			// 
			// data3Enabled
			// 
			this.data3Enabled.AutoSize = true;
			this.data3Enabled.Location = new System.Drawing.Point(4, 58);
			this.data3Enabled.Name = "data3Enabled";
			this.data3Enabled.Size = new System.Drawing.Size(55, 17);
			this.data3Enabled.TabIndex = 4;
			this.data3Enabled.Text = "Data3";
			this.data3Enabled.UseVisualStyleBackColor = true;
			this.data3Enabled.CheckedChanged += new System.EventHandler(this.data3Enabled_CheckedChanged);
			// 
			// data2Value
			// 
			this.data2Value.Location = new System.Drawing.Point(66, 31);
			this.data2Value.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.data2Value.Name = "data2Value";
			this.data2Value.Size = new System.Drawing.Size(63, 20);
			this.data2Value.TabIndex = 3;
			this.data2Value.ValueChanged += new System.EventHandler(this.data2Value_ValueChanged);
			// 
			// data2Enabled
			// 
			this.data2Enabled.AutoSize = true;
			this.data2Enabled.Location = new System.Drawing.Point(4, 31);
			this.data2Enabled.Name = "data2Enabled";
			this.data2Enabled.Size = new System.Drawing.Size(55, 17);
			this.data2Enabled.TabIndex = 2;
			this.data2Enabled.Text = "Data2";
			this.data2Enabled.UseVisualStyleBackColor = true;
			this.data2Enabled.CheckedChanged += new System.EventHandler(this.data2Enabled_CheckedChanged);
			// 
			// data1Enabled
			// 
			this.data1Enabled.AutoSize = true;
			this.data1Enabled.Location = new System.Drawing.Point(4, 4);
			this.data1Enabled.Name = "data1Enabled";
			this.data1Enabled.Size = new System.Drawing.Size(55, 17);
			this.data1Enabled.TabIndex = 0;
			this.data1Enabled.Text = "Data1";
			this.data1Enabled.UseVisualStyleBackColor = true;
			this.data1Enabled.CheckedChanged += new System.EventHandler(this.data1Enabled_CheckedChanged);
			// 
			// data1Value
			// 
			this.data1Value.Location = new System.Drawing.Point(66, 4);
			this.data1Value.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.data1Value.Name = "data1Value";
			this.data1Value.Size = new System.Drawing.Size(63, 20);
			this.data1Value.TabIndex = 1;
			this.data1Value.ValueChanged += new System.EventHandler(this.data1Value_ValueChanged);
			// 
			// data3Value
			// 
			this.data3Value.Location = new System.Drawing.Point(66, 58);
			this.data3Value.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.data3Value.Name = "data3Value";
			this.data3Value.Size = new System.Drawing.Size(63, 20);
			this.data3Value.TabIndex = 5;
			this.data3Value.ValueChanged += new System.EventHandler(this.data3Value_ValueChanged);
			// 
			// data4Value
			// 
			this.data4Value.Location = new System.Drawing.Point(66, 85);
			this.data4Value.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.data4Value.Name = "data4Value";
			this.data4Value.Size = new System.Drawing.Size(63, 20);
			this.data4Value.TabIndex = 7;
			this.data4Value.ValueChanged += new System.EventHandler(this.data4Value_ValueChanged);
			// 
			// data5Value
			// 
			this.data5Value.Location = new System.Drawing.Point(66, 112);
			this.data5Value.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.data5Value.Name = "data5Value";
			this.data5Value.Size = new System.Drawing.Size(63, 20);
			this.data5Value.TabIndex = 9;
			this.data5Value.ValueChanged += new System.EventHandler(this.data5Value_ValueChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(717, 700);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.objectNameBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.groupBox7);
			this.Controls.Add(this.groupBox6);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.objectVariantSelector);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.objectTypeSelector);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.objectListSelector);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.Text = "Object Definition Editor";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.groupBox7.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.data2Value)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.data1Value)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.data3Value)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.data4Value)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.data5Value)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox objectListSelector;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox objectTypeSelector;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox objectVariantSelector;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckedListBox levelsSelector;
		private System.Windows.Forms.ListBox spriteListBox;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button addSpriteButton;
		private System.Windows.Forms.Button spriteDownButton;
		private System.Windows.Forms.Button spriteUpButton;
		private System.Windows.Forms.Button deleteSpriteButton;
		private System.Windows.Forms.GroupBox groupBox3;
		private TileList animationSelector;
		private System.Windows.Forms.GroupBox groupBox4;
		private TileList animVariantSelector;
		private System.Windows.Forms.GroupBox groupBox5;
		private TileList animFrameSelector;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.CheckBox xFlipCheckbox;
		private System.Windows.Forms.CheckBox yFlipCheckbox;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox objectNameBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.CheckBox data1Enabled;
		private System.Windows.Forms.NumericUpDown data1Value;
		private System.Windows.Forms.NumericUpDown data2Value;
		private System.Windows.Forms.CheckBox data2Enabled;
		private System.Windows.Forms.CheckBox data3Enabled;
		private System.Windows.Forms.NumericUpDown data3Value;
		private System.Windows.Forms.CheckBox data4Enabled;
		private System.Windows.Forms.NumericUpDown data4Value;
		private System.Windows.Forms.CheckBox data5Enabled;
		private System.Windows.Forms.NumericUpDown data5Value;
	}
}

