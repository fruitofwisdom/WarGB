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
			optionsToolStripMenuItem = new ToolStripMenuItem();
			accurateRenderingToolStripMenuItem = new ToolStripMenuItem();
			controlsToolStripMenuItem = new ToolStripMenuItem();
			lcdColorToolStripMenuItem = new ToolStripMenuItem();
			originalGreenToolStripMenuItem = new ToolStripMenuItem();
			blackAndWhiteToolStripMenuItem = new ToolStripMenuItem();
			muteSoundToolStripMenuItem = new ToolStripMenuItem();
			debugToolStripMenuItem = new ToolStripMenuItem();
			displayFrameTimeToolStripMenuItem = new ToolStripMenuItem();
			logOpcodesToolStripMenuItem = new ToolStripMenuItem();
			showDebugOutputToolStripMenuItem = new ToolStripMenuItem();
			helpToolStripMenuItem = new ToolStripMenuItem();
			aboutGBSharpToolStripMenuItem = new ToolStripMenuItem();
			toolStrip = new ToolStrip();
			playButton = new ToolStripButton();
			pauseButton = new ToolStripButton();
			resetButton = new ToolStripButton();
			stepButton = new ToolStripButton();
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
			menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, optionsToolStripMenuItem, debugToolStripMenuItem, helpToolStripMenuItem });
			menuStrip.Location = new Point(0, 0);
			menuStrip.Name = "menuStrip";
			menuStrip.Size = new Size(504, 24);
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
			loadROMToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
			loadROMToolStripMenuItem.Size = new Size(182, 22);
			loadROMToolStripMenuItem.Text = "Load ROM...";
			loadROMToolStripMenuItem.Click += LoadROMToolStripMenuItemClick;
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
			exitToolStripMenuItem.Size = new Size(182, 22);
			exitToolStripMenuItem.Text = "Exit";
			exitToolStripMenuItem.Click += ExitToolStripMenuItemClick;
			// 
			// optionsToolStripMenuItem
			// 
			optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { accurateRenderingToolStripMenuItem, controlsToolStripMenuItem, lcdColorToolStripMenuItem, muteSoundToolStripMenuItem });
			optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			optionsToolStripMenuItem.Size = new Size(61, 20);
			optionsToolStripMenuItem.Text = "Options";
			// 
			// accurateRenderingToolStripMenuItem
			// 
			accurateRenderingToolStripMenuItem.Checked = true;
			accurateRenderingToolStripMenuItem.CheckState = CheckState.Checked;
			accurateRenderingToolStripMenuItem.Name = "accurateRenderingToolStripMenuItem";
			accurateRenderingToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.A;
			accurateRenderingToolStripMenuItem.Size = new Size(220, 22);
			accurateRenderingToolStripMenuItem.Text = "Accurate Rendering";
			accurateRenderingToolStripMenuItem.Click += AccurateRenderingToolStripMenuClick;
			// 
			// controlsToolStripMenuItem
			// 
			controlsToolStripMenuItem.Name = "controlsToolStripMenuItem";
			controlsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
			controlsToolStripMenuItem.Size = new Size(220, 22);
			controlsToolStripMenuItem.Text = "Controls...";
			controlsToolStripMenuItem.Click += ControlsToolStripMenuItemClick;
			// 
			// lcdColorToolStripMenuItem
			// 
			lcdColorToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { originalGreenToolStripMenuItem, blackAndWhiteToolStripMenuItem });
			lcdColorToolStripMenuItem.Name = "lcdColorToolStripMenuItem";
			lcdColorToolStripMenuItem.Size = new Size(220, 22);
			lcdColorToolStripMenuItem.Text = "LCD Color";
			// 
			// originalGreenToolStripMenuItem
			// 
			originalGreenToolStripMenuItem.Checked = true;
			originalGreenToolStripMenuItem.CheckState = CheckState.Checked;
			originalGreenToolStripMenuItem.Name = "originalGreenToolStripMenuItem";
			originalGreenToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.D1;
			originalGreenToolStripMenuItem.Size = new Size(199, 22);
			originalGreenToolStripMenuItem.Text = "Original Green";
			originalGreenToolStripMenuItem.Click += OriginalGreenToolStripMenuClick;
			// 
			// blackAndWhiteToolStripMenuItem
			// 
			blackAndWhiteToolStripMenuItem.Name = "blackAndWhiteToolStripMenuItem";
			blackAndWhiteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.D2;
			blackAndWhiteToolStripMenuItem.Size = new Size(199, 22);
			blackAndWhiteToolStripMenuItem.Text = "Black and White";
			blackAndWhiteToolStripMenuItem.Click += BlackAndWhiteToolStripMenuClick;
			// 
			// muteSoundToolStripMenuItem
			// 
			muteSoundToolStripMenuItem.Name = "muteSoundToolStripMenuItem";
			muteSoundToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.M;
			muteSoundToolStripMenuItem.Size = new Size(220, 22);
			muteSoundToolStripMenuItem.Text = "Mute Sound";
			muteSoundToolStripMenuItem.Click += MuteSoundToolStripMenuClick;
			// 
			// debugToolStripMenuItem
			// 
			debugToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { displayFrameTimeToolStripMenuItem, logOpcodesToolStripMenuItem, showDebugOutputToolStripMenuItem });
			debugToolStripMenuItem.Name = "debugToolStripMenuItem";
			debugToolStripMenuItem.Size = new Size(54, 20);
			debugToolStripMenuItem.Text = "Debug";
			// 
			// displayFrameTimeToolStripMenuItem
			// 
			displayFrameTimeToolStripMenuItem.Name = "displayFrameTimeToolStripMenuItem";
			displayFrameTimeToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F;
			displayFrameTimeToolStripMenuItem.Size = new Size(224, 22);
			displayFrameTimeToolStripMenuItem.Text = "Display Frame Time";
			displayFrameTimeToolStripMenuItem.Click += DisplayFrameTimeToolStripMenuClick;
			// 
			// logOpcodesToolStripMenuItem
			// 
			logOpcodesToolStripMenuItem.Name = "logOpcodesToolStripMenuItem";
			logOpcodesToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.L;
			logOpcodesToolStripMenuItem.Size = new Size(224, 22);
			logOpcodesToolStripMenuItem.Text = "Log Opcodes";
			logOpcodesToolStripMenuItem.ToolTipText = "Write opcodes, CPU state, etc to a log file. (This file will get very large.)";
			logOpcodesToolStripMenuItem.Click += LogOpcodesToolStripMenuClick;
			// 
			// showDebugOutputToolStripMenuItem
			// 
			showDebugOutputToolStripMenuItem.Name = "showDebugOutputToolStripMenuItem";
			showDebugOutputToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.D;
			showDebugOutputToolStripMenuItem.Size = new Size(224, 22);
			showDebugOutputToolStripMenuItem.Text = "Show Debug Output";
			showDebugOutputToolStripMenuItem.Click += ShowDebugOutputToolStripMenuClick;
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
			aboutGBSharpToolStripMenuItem.ShortcutKeys = Keys.F1;
			aboutGBSharpToolStripMenuItem.Size = new Size(174, 22);
			aboutGBSharpToolStripMenuItem.Text = "About GBSharp";
			aboutGBSharpToolStripMenuItem.Click += AboutGBSharpToolStripMenuItemClick;
			// 
			// toolStrip
			// 
			toolStrip.Items.AddRange(new ToolStripItem[] { playButton, pauseButton, resetButton, stepButton });
			toolStrip.Location = new Point(0, 24);
			toolStrip.Name = "toolStrip";
			toolStrip.Size = new Size(504, 25);
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
			// stepButton
			// 
			stepButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			stepButton.Enabled = false;
			stepButton.Font = new Font("Segoe MDL2 Assets", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			stepButton.Name = "stepButton";
			stepButton.Size = new Size(23, 22);
			stepButton.Text = "";
			stepButton.ToolTipText = "Step";
			stepButton.Visible = false;
			stepButton.Click += StepButtonClick;
			// 
			// lcdControl
			// 
			lcdControl.Location = new Point(12, 52);
			lcdControl.Name = "lcdControl";
			lcdControl.Size = new Size(480, 432);
			lcdControl.TabIndex = 2;
			lcdControl.KeyDown += lcdControl_KeyDown;
			lcdControl.KeyUp += lcdControl_KeyUp;
			lcdControl.PreviewKeyDown += lcdControl_PreviewKeyDown;
			// 
			// debugRichTextBox
			// 
			debugRichTextBox.Font = new Font("Cascadia Code", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			debugRichTextBox.Location = new Point(498, 52);
			debugRichTextBox.Name = "debugRichTextBox";
			debugRichTextBox.ReadOnly = true;
			debugRichTextBox.Size = new Size(250, 432);
			debugRichTextBox.TabIndex = 3;
			debugRichTextBox.Text = "";
			debugRichTextBox.Visible = false;
			// 
			// statusStrip
			// 
			statusStrip.Items.AddRange(new ToolStripItem[] { debugToolStripStatusLabel });
			statusStrip.Location = new Point(0, 496);
			statusStrip.Name = "statusStrip";
			statusStrip.Size = new Size(504, 22);
			statusStrip.TabIndex = 4;
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
			ClientSize = new Size(504, 518);
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
		private ToolStripMenuItem optionsToolStripMenuItem;
		private ToolStripMenuItem accurateRenderingToolStripMenuItem;
		private ToolStripMenuItem controlsToolStripMenuItem;
		private ToolStripMenuItem lcdColorToolStripMenuItem;
		private ToolStripMenuItem originalGreenToolStripMenuItem;
		private ToolStripMenuItem blackAndWhiteToolStripMenuItem;
		private ToolStripMenuItem muteSoundToolStripMenuItem;
		private ToolStripMenuItem debugToolStripMenuItem;
		private ToolStripMenuItem displayFrameTimeToolStripMenuItem;
		private ToolStripMenuItem logOpcodesToolStripMenuItem;
		private ToolStripMenuItem showDebugOutputToolStripMenuItem;
		private ToolStripMenuItem helpToolStripMenuItem;
		private ToolStripMenuItem aboutGBSharpToolStripMenuItem;
		private ToolStrip toolStrip;
		private ToolStripButton playButton;
		private ToolStripButton pauseButton;
		private ToolStripButton resetButton;
		private ToolStripButton stepButton;
		private LCDControl lcdControl;
		private RichTextBox debugRichTextBox;
		private StatusStrip statusStrip;
		private ToolStripStatusLabel debugToolStripStatusLabel;
	}
}
