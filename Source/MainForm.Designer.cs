namespace GBSharp
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			menuStrip = new MenuStrip();
			fileToolStripMenuItem = new ToolStripMenuItem();
			loadROMToolStripMenuItem = new ToolStripMenuItem();
			exitToolStripMenuItem = new ToolStripMenuItem();
			debugToolStripMenuItem = new ToolStripMenuItem();
			printOpcodesToolStripMenuItem = new ToolStripMenuItem();
			helpToolStripMenuItem = new ToolStripMenuItem();
			aboutGBSharpToolStripMenuItem = new ToolStripMenuItem();
			toolStrip = new ToolStrip();
			playButton = new ToolStripButton();
			pauseButton = new ToolStripButton();
			stepButton = new ToolStripButton();
			resetButton = new ToolStripButton();
			lcdControl = new LCDControl();
			debugRichTextBox = new RichTextBox();
			statusStrip = new StatusStrip();
			debugToolStripStatusLabel = new ToolStripStatusLabel();
			menuStrip.SuspendLayout();
			toolStrip.SuspendLayout();
			statusStrip.SuspendLayout();
			SuspendLayout();
			// 
			// menuStrip
			// 
			menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, debugToolStripMenuItem, helpToolStripMenuItem });
			menuStrip.Location = new Point(0, 0);
			menuStrip.Name = "menuStrip";
			menuStrip.Size = new Size(759, 24);
			menuStrip.TabIndex = 0;
			menuStrip.Text = "menuStrip";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadROMToolStripMenuItem, exitToolStripMenuItem });
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new Size(37, 20);
			fileToolStripMenuItem.Text = "File";
			// 
			// loadROMToolStripMenuItem
			// 
			loadROMToolStripMenuItem.Name = "loadROMToolStripMenuItem";
			loadROMToolStripMenuItem.Size = new Size(139, 22);
			loadROMToolStripMenuItem.Text = "Load ROM...";
			loadROMToolStripMenuItem.Click += LoadROMToolStripMenuItemClick;
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
			exitToolStripMenuItem.Size = new Size(139, 22);
			exitToolStripMenuItem.Text = "Exit";
			exitToolStripMenuItem.Click += ExitToolStripMenuItemClick;
			// 
			// debugToolStripMenuItem
			// 
			debugToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { printOpcodesToolStripMenuItem });
			debugToolStripMenuItem.Name = "debugToolStripMenuItem";
			debugToolStripMenuItem.Size = new Size(54, 20);
			debugToolStripMenuItem.Text = "Debug";
			// 
			// printOpcodesToolStripMenuItem
			// 
			printOpcodesToolStripMenuItem.Name = "printOpcodesToolStripMenuItem";
			printOpcodesToolStripMenuItem.Size = new Size(149, 22);
			printOpcodesToolStripMenuItem.Text = "Print Opcodes";
			printOpcodesToolStripMenuItem.ToolTipText = "Show running opcodes. Will be much slower.";
			printOpcodesToolStripMenuItem.Click += PrintOpcodesToolStripMenuClick;
			// 
			// helpToolStripMenuItem
			// 
			helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutGBSharpToolStripMenuItem });
			helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			helpToolStripMenuItem.Size = new Size(44, 20);
			helpToolStripMenuItem.Text = "Help";
			// 
			// aboutGBSharpToolStripMenuItem
			// 
			aboutGBSharpToolStripMenuItem.Name = "aboutGBSharpToolStripMenuItem";
			aboutGBSharpToolStripMenuItem.Size = new Size(155, 22);
			aboutGBSharpToolStripMenuItem.Text = "About GBSharp";
			aboutGBSharpToolStripMenuItem.Click += AboutGBSharpToolStripMenuItemClick;
			// 
			// toolStrip
			// 
			toolStrip.Items.AddRange(new ToolStripItem[] { playButton, pauseButton, stepButton, resetButton });
			toolStrip.Location = new Point(0, 24);
			toolStrip.Name = "toolStrip";
			toolStrip.Size = new Size(759, 25);
			toolStrip.TabIndex = 1;
			toolStrip.Text = "toolStrip";
			// 
			// playButton
			// 
			playButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			playButton.Enabled = false;
			playButton.Font = new Font("Segoe MDL2 Assets", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			playButton.Name = "playButton";
			playButton.Size = new Size(23, 22);
			playButton.Text = "";
			playButton.ToolTipText = "Play";
			playButton.Click += PlayButtonClick;
			// 
			// pauseButton
			// 
			pauseButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			pauseButton.Enabled = false;
			pauseButton.Font = new Font("Segoe MDL2 Assets", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			pauseButton.Name = "pauseButton";
			pauseButton.Size = new Size(23, 22);
			pauseButton.Text = "";
			pauseButton.ToolTipText = "Pause";
			pauseButton.Click += PauseButtonClick;
			// 
			// stepButton
			// 
			stepButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			stepButton.Enabled = false;
			stepButton.Font = new Font("Segoe MDL2 Assets", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			stepButton.Name = "stepButton";
			stepButton.Size = new Size(23, 22);
			stepButton.Text = "";
			stepButton.ToolTipText = "Step";
			stepButton.Click += StepButtonClick;
			// 
			// resetButton
			// 
			resetButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			resetButton.Enabled = false;
			resetButton.Font = new Font("Segoe MDL2 Assets", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			resetButton.Name = "resetButton";
			resetButton.Size = new Size(23, 22);
			resetButton.Text = "";
			resetButton.ToolTipText = "Reset";
			resetButton.Click += ResetButtonClick;
			// 
			// lcdControl
			// 
			lcdControl.Location = new Point(12, 52);
			lcdControl.Name = "lcdControl";
			lcdControl.Size = new Size(480, 432);
			lcdControl.TabIndex = 4;
			// 
			// debugRichTextBox
			// 
			debugRichTextBox.Font = new Font("Cascadia Code", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			debugRichTextBox.Location = new Point(498, 52);
			debugRichTextBox.Name = "debugRichTextBox";
			debugRichTextBox.ReadOnly = true;
			debugRichTextBox.Size = new Size(250, 432);
			debugRichTextBox.TabIndex = 2;
			debugRichTextBox.Text = "";
			// 
			// statusStrip
			// 
			statusStrip.Items.AddRange(new ToolStripItem[] { debugToolStripStatusLabel });
			statusStrip.Location = new Point(0, 496);
			statusStrip.Name = "statusStrip";
			statusStrip.Size = new Size(759, 22);
			statusStrip.TabIndex = 3;
			statusStrip.Text = "statusStrip";
			// 
			// debugToolStripStatusLabel
			// 
			debugToolStripStatusLabel.Name = "debugToolStripStatusLabel";
			debugToolStripStatusLabel.Size = new Size(0, 17);
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(759, 518);
			Controls.Add(statusStrip);
			Controls.Add(debugRichTextBox);
			Controls.Add(lcdControl);
			Controls.Add(toolStrip);
			Controls.Add(menuStrip);
			Icon = (Icon)resources.GetObject("$this.Icon");
			KeyPreview = true;
			MainMenuStrip = menuStrip;
			Name = "MainForm";
			Text = "GB#";
			KeyDown += MainForm_KeyDown;
			KeyUp += MainForm_KeyUp;
			menuStrip.ResumeLayout(false);
			menuStrip.PerformLayout();
			toolStrip.ResumeLayout(false);
			toolStrip.PerformLayout();
			statusStrip.ResumeLayout(false);
			statusStrip.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private MenuStrip menuStrip;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem loadROMToolStripMenuItem;
		private ToolStripMenuItem exitToolStripMenuItem;
		private ToolStripMenuItem debugToolStripMenuItem;
		private ToolStripMenuItem printOpcodesToolStripMenuItem;
		private ToolStripMenuItem helpToolStripMenuItem;
		private ToolStripMenuItem aboutGBSharpToolStripMenuItem;
		private ToolStrip toolStrip;
		private ToolStripButton playButton;
		private ToolStripButton pauseButton;
		private ToolStripButton stepButton;
		private LCDControl lcdControl;
		private RichTextBox debugRichTextBox;
		private StatusStrip statusStrip;
		private ToolStripStatusLabel debugToolStripStatusLabel;
		private ToolStripButton resetButton;
	}
}
