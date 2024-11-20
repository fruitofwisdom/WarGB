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
			lcdSizeToolStripMenuItem = new ToolStripMenuItem();
			oneXToolStripMenuItem = new ToolStripMenuItem();
			twoXToolStripMenuItem = new ToolStripMenuItem();
			threeXToolStripMenuItem = new ToolStripMenuItem();
			fourXToolStripMenuItem = new ToolStripMenuItem();
			fiveXToolStripMenuItem = new ToolStripMenuItem();
			muteSoundToolStripMenuItem = new ToolStripMenuItem();
			soundChannelsToolStripMenuItem = new ToolStripMenuItem();
			pulseWaveChannel1ToolStripMenuItem = new ToolStripMenuItem();
			pulseWaveChannel2ToolStripMenuItem = new ToolStripMenuItem();
			waveTableChannel3ToolStripMenuItem = new ToolStripMenuItem();
			noiseGeneratorChannel4ToolStripMenuItem = new ToolStripMenuItem();
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
			menuStrip.Size = new Size(664, 24);
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
			optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { accurateRenderingToolStripMenuItem, controlsToolStripMenuItem, lcdColorToolStripMenuItem, lcdSizeToolStripMenuItem, muteSoundToolStripMenuItem, soundChannelsToolStripMenuItem });
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
			controlsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
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
			originalGreenToolStripMenuItem.Size = new Size(159, 22);
			originalGreenToolStripMenuItem.Text = "Original Green";
			originalGreenToolStripMenuItem.Click += OriginalGreenToolStripMenuClick;
			// 
			// blackAndWhiteToolStripMenuItem
			// 
			blackAndWhiteToolStripMenuItem.Name = "blackAndWhiteToolStripMenuItem";
			blackAndWhiteToolStripMenuItem.Size = new Size(159, 22);
			blackAndWhiteToolStripMenuItem.Text = "Black and White";
			blackAndWhiteToolStripMenuItem.Click += BlackAndWhiteToolStripMenuClick;
			// 
			// lcdSizeToolStripMenuItem
			// 
			lcdSizeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { oneXToolStripMenuItem, twoXToolStripMenuItem, threeXToolStripMenuItem, fourXToolStripMenuItem, fiveXToolStripMenuItem });
			lcdSizeToolStripMenuItem.Name = "lcdSizeToolStripMenuItem";
			lcdSizeToolStripMenuItem.Size = new Size(220, 22);
			lcdSizeToolStripMenuItem.Text = "LCD Size";
			// 
			// oneXToolStripMenuItem
			// 
			oneXToolStripMenuItem.Name = "oneXToolStripMenuItem";
			oneXToolStripMenuItem.Size = new Size(86, 22);
			oneXToolStripMenuItem.Text = "1x";
			oneXToolStripMenuItem.Click += OneXToolStripMenuItem_Click;
			// 
			// twoXToolStripMenuItem
			// 
			twoXToolStripMenuItem.Name = "twoXToolStripMenuItem";
			twoXToolStripMenuItem.Size = new Size(86, 22);
			twoXToolStripMenuItem.Text = "2x";
			twoXToolStripMenuItem.Click += TwoXToolStripMenuItem_Click;
			// 
			// threeXToolStripMenuItem
			// 
			threeXToolStripMenuItem.Name = "threeXToolStripMenuItem";
			threeXToolStripMenuItem.Size = new Size(86, 22);
			threeXToolStripMenuItem.Text = "3x";
			threeXToolStripMenuItem.Click += ThreeXToolStripMenuItem_Click;
			// 
			// fourXToolStripMenuItem
			// 
			fourXToolStripMenuItem.Checked = true;
			fourXToolStripMenuItem.CheckState = CheckState.Checked;
			fourXToolStripMenuItem.Name = "fourXToolStripMenuItem";
			fourXToolStripMenuItem.Size = new Size(86, 22);
			fourXToolStripMenuItem.Text = "4x";
			fourXToolStripMenuItem.Click += FourXToolStripMenuItem_Click;
			// 
			// fiveXToolStripMenuItem
			// 
			fiveXToolStripMenuItem.Name = "fiveXToolStripMenuItem";
			fiveXToolStripMenuItem.Size = new Size(86, 22);
			fiveXToolStripMenuItem.Text = "5x";
			fiveXToolStripMenuItem.Click += FiveXToolStripMenuItem_Click;
			// 
			// muteSoundToolStripMenuItem
			// 
			muteSoundToolStripMenuItem.Name = "muteSoundToolStripMenuItem";
			muteSoundToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.M;
			muteSoundToolStripMenuItem.Size = new Size(220, 22);
			muteSoundToolStripMenuItem.Text = "Mute Sound";
			muteSoundToolStripMenuItem.Click += MuteSoundToolStripMenuClick;
			// 
			// soundChannelsToolStripMenuItem
			// 
			soundChannelsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { pulseWaveChannel1ToolStripMenuItem, pulseWaveChannel2ToolStripMenuItem, waveTableChannel3ToolStripMenuItem, noiseGeneratorChannel4ToolStripMenuItem });
			soundChannelsToolStripMenuItem.Name = "soundChannelsToolStripMenuItem";
			soundChannelsToolStripMenuItem.Size = new Size(220, 22);
			soundChannelsToolStripMenuItem.Text = "Sound Channels";
			// 
			// pulseWaveChannel1ToolStripMenuItem
			// 
			pulseWaveChannel1ToolStripMenuItem.Checked = true;
			pulseWaveChannel1ToolStripMenuItem.CheckState = CheckState.Checked;
			pulseWaveChannel1ToolStripMenuItem.Name = "pulseWaveChannel1ToolStripMenuItem";
			pulseWaveChannel1ToolStripMenuItem.Size = new Size(223, 22);
			pulseWaveChannel1ToolStripMenuItem.Text = "Pulse Wave (Channel 1)";
			pulseWaveChannel1ToolStripMenuItem.Click += pulseWaveChannel1ToolStripMenuItem_Click;
			// 
			// pulseWaveChannel2ToolStripMenuItem
			// 
			pulseWaveChannel2ToolStripMenuItem.Checked = true;
			pulseWaveChannel2ToolStripMenuItem.CheckState = CheckState.Checked;
			pulseWaveChannel2ToolStripMenuItem.Name = "pulseWaveChannel2ToolStripMenuItem";
			pulseWaveChannel2ToolStripMenuItem.Size = new Size(223, 22);
			pulseWaveChannel2ToolStripMenuItem.Text = "Pulse Wave (Channel 2)";
			pulseWaveChannel2ToolStripMenuItem.Click += pulseWaveChannel2ToolStripMenuItem_Click;
			// 
			// waveTableChannel3ToolStripMenuItem
			// 
			waveTableChannel3ToolStripMenuItem.Checked = true;
			waveTableChannel3ToolStripMenuItem.CheckState = CheckState.Checked;
			waveTableChannel3ToolStripMenuItem.Name = "waveTableChannel3ToolStripMenuItem";
			waveTableChannel3ToolStripMenuItem.Size = new Size(223, 22);
			waveTableChannel3ToolStripMenuItem.Text = "Wave Table (Channel 3)";
			waveTableChannel3ToolStripMenuItem.Click += waveTableChannel3ToolStripMenuItem_Click;
			// 
			// noiseGeneratorChannel4ToolStripMenuItem
			// 
			noiseGeneratorChannel4ToolStripMenuItem.Checked = true;
			noiseGeneratorChannel4ToolStripMenuItem.CheckState = CheckState.Checked;
			noiseGeneratorChannel4ToolStripMenuItem.Name = "noiseGeneratorChannel4ToolStripMenuItem";
			noiseGeneratorChannel4ToolStripMenuItem.Size = new Size(223, 22);
			noiseGeneratorChannel4ToolStripMenuItem.Text = "Noise Generator (Channel 4)";
			noiseGeneratorChannel4ToolStripMenuItem.Click += noiseGeneratorChannel4ToolStripMenuItem_Click;
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
			toolStrip.Size = new Size(664, 25);
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
			lcdControl.Size = new Size(640, 576);
			lcdControl.TabIndex = 2;
			lcdControl.KeyDown += lcdControl_KeyDown;
			lcdControl.KeyUp += lcdControl_KeyUp;
			lcdControl.PreviewKeyDown += lcdControl_PreviewKeyDown;
			// 
			// debugRichTextBox
			// 
			debugRichTextBox.Font = new Font("Cascadia Code", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			debugRichTextBox.Location = new Point(658, 52);
			debugRichTextBox.Name = "debugRichTextBox";
			debugRichTextBox.ReadOnly = true;
			debugRichTextBox.Size = new Size(250, 576);
			debugRichTextBox.TabIndex = 3;
			debugRichTextBox.Text = "";
			debugRichTextBox.Visible = false;
			// 
			// statusStrip
			// 
			statusStrip.Items.AddRange(new ToolStripItem[] { debugToolStripStatusLabel });
			statusStrip.Location = new Point(0, 640);
			statusStrip.Name = "statusStrip";
			statusStrip.Size = new Size(664, 22);
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
			ClientSize = new Size(664, 662);
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
		private ToolStripMenuItem lcdSizeToolStripMenuItem;
		private ToolStripMenuItem oneXToolStripMenuItem;
		private ToolStripMenuItem twoXToolStripMenuItem;
		private ToolStripMenuItem threeXToolStripMenuItem;
		private ToolStripMenuItem fourXToolStripMenuItem;
		private ToolStripMenuItem fiveXToolStripMenuItem;
		private ToolStripMenuItem muteSoundToolStripMenuItem;
		private ToolStripMenuItem soundChannelsToolStripMenuItem;
		private ToolStripMenuItem pulseWaveChannel1ToolStripMenuItem;
		private ToolStripMenuItem pulseWaveChannel2ToolStripMenuItem;
		private ToolStripMenuItem waveTableChannel3ToolStripMenuItem;
		private ToolStripMenuItem noiseGeneratorChannel4ToolStripMenuItem;
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
