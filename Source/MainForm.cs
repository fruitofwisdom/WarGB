namespace GBSharp
{
	public partial class MainForm : Form
	{
		private readonly GameBoy _gameBoy = new();
		private Thread? _gameBoyThread;

		// A timer used to poll and render the state of the Game Boy.
		private readonly System.Windows.Forms.Timer _gameBoyTimer = new();

		// The two customizable sets of controller assignments.
		private readonly KeyMapping _keyMapping = new();

		// A callback to pause the emulator.
		private delegate void PauseCallback();
		private static PauseCallback? s_pauseCallbackInternal;

		public MainForm()
		{
			InitializeComponent();

			s_pauseCallbackInternal = PauseInternal;

			// Poll the Game Boy emulator at 60fps.
			_gameBoyTimer.Tick += new EventHandler(ProcessOutput);
			_gameBoyTimer.Interval = 1000 / 60;
			_gameBoyTimer.Start();
		}

		private void LoadROMToolStripMenuItemClick(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new()
			{
				Filter = "Game Boy ROMs (*.gb)|*.gb|All files (*.*)|*.*",
				RestoreDirectory = true
			};

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				if (ROM.Instance.Load(openFileDialog.FileName))
				{
					// Close any previous threads and reset the Game Boy.
					if (_gameBoyThread != null)
					{
						_gameBoy.Stop();
						_gameBoyThread.Join();
					}
					_gameBoy.Reset();

					// Put the game name and ROM type in our title.
					Text = "GB# - " + ROM.Instance.Title + " (" + ROM.Instance.CartridgeType.ToString().Replace("_", "+") + ")";

					// Start a new thread to run the Game Boy.
					_gameBoyThread = new Thread(new ThreadStart(_gameBoy.Run));
					_gameBoyThread.IsBackground = true;
					_gameBoyThread.Start();
					playButton.Enabled = true;
					pauseButton.Enabled = false;
					resetButton.Enabled = true;
					stepButton.Enabled = true;

					// Play automatically.
					PlayButtonClick(this, new EventArgs());
				}
			}
		}

		private void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}

		private void AccurateRenderingToolStripMenuClick(object sender, EventArgs e)
		{
			accurateRenderingToolStripMenuItem.Checked = !accurateRenderingToolStripMenuItem.Checked;
			_gameBoy.EnableAccurateRendering(accurateRenderingToolStripMenuItem.Checked);
		}

		private void ControlsToolStripMenuItemClick(object sender, EventArgs e)
		{
			ControlsForm controlsForm = new(_keyMapping);
			controlsForm.ShowDialog();
		}

		private void OriginalGreenToolStripMenuClick(object sender, EventArgs e)
		{
			if (!originalGreenToolStripMenuItem.Checked)
			{
				originalGreenToolStripMenuItem.Checked = true;
				blackAndWhiteToolStripMenuItem.Checked = false;
				lcdControl.UseOriginalGreen = true;
			}
		}

		private void BlackAndWhiteToolStripMenuClick(object sender, EventArgs e)
		{
			if (!blackAndWhiteToolStripMenuItem.Checked)
			{
				blackAndWhiteToolStripMenuItem.Checked = true;
				originalGreenToolStripMenuItem.Checked = false;
				lcdControl.UseOriginalGreen = false;
			}
		}

		private void OneXToolStripMenuItem_Click(object sender, EventArgs e)
		{
			oneXToolStripMenuItem.Checked = true;
			twoXToolStripMenuItem.Checked = false;
			threeXToolStripMenuItem.Checked = false;
			fourXToolStripMenuItem.Checked = false;
			fiveXToolStripMenuItem.Checked = false;
			lcdControl.Width = PPU.kWidth;
			lcdControl.Height = PPU.kHeight;
			Width = lcdControl.Width + (showDebugOutputToolStripMenuItem.Checked ? 295 : 40);
			Height = lcdControl.Height + 125;
			debugRichTextBox.Left = lcdControl.Right + 6;
		}

		private void TwoXToolStripMenuItem_Click(object sender, EventArgs e)
		{
			oneXToolStripMenuItem.Checked = false;
			twoXToolStripMenuItem.Checked = true;
			threeXToolStripMenuItem.Checked = false;
			fourXToolStripMenuItem.Checked = false;
			fiveXToolStripMenuItem.Checked = false;
			lcdControl.Width = PPU.kWidth * 2;
			lcdControl.Height = PPU.kHeight * 2;
			Width = lcdControl.Width + (showDebugOutputToolStripMenuItem.Checked ? 295 : 40);
			Height = lcdControl.Height + 125;
			debugRichTextBox.Left = lcdControl.Right + 6;
		}

		private void ThreeXToolStripMenuItem_Click(object sender, EventArgs e)
		{
			oneXToolStripMenuItem.Checked = false;
			twoXToolStripMenuItem.Checked = false;
			threeXToolStripMenuItem.Checked = true;
			fourXToolStripMenuItem.Checked = false;
			fiveXToolStripMenuItem.Checked = false;
			lcdControl.Width = PPU.kWidth * 3;
			lcdControl.Height = PPU.kHeight * 3;
			Width = lcdControl.Width + (showDebugOutputToolStripMenuItem.Checked ? 295 : 40);
			Height = lcdControl.Height + 125;
			debugRichTextBox.Left = lcdControl.Right + 6;
		}

		private void FourXToolStripMenuItem_Click(object sender, EventArgs e)
		{
			oneXToolStripMenuItem.Checked = false;
			twoXToolStripMenuItem.Checked = false;
			threeXToolStripMenuItem.Checked = false;
			fourXToolStripMenuItem.Checked = true;
			fiveXToolStripMenuItem.Checked = false;
			lcdControl.Width = PPU.kWidth * 4;
			lcdControl.Height = PPU.kHeight * 4;
			Width = lcdControl.Width + (showDebugOutputToolStripMenuItem.Checked ? 295 : 40);
			Height = lcdControl.Height + 125;
			debugRichTextBox.Left = lcdControl.Right + 6;
		}

		private void FiveXToolStripMenuItem_Click(object sender, EventArgs e)
		{
			oneXToolStripMenuItem.Checked = false;
			twoXToolStripMenuItem.Checked = false;
			threeXToolStripMenuItem.Checked = false;
			fourXToolStripMenuItem.Checked = false;
			fiveXToolStripMenuItem.Checked = true;
			lcdControl.Width = PPU.kWidth * 5;
			lcdControl.Height = PPU.kHeight * 5;
			Width = lcdControl.Width + (showDebugOutputToolStripMenuItem.Checked ? 295 : 40);
			Height = lcdControl.Height + 125;
			debugRichTextBox.Left = lcdControl.Right + 6;
		}

		private void MuteSoundToolStripMenuClick(object sender, EventArgs e)
		{
			muteSoundToolStripMenuItem.Checked = !muteSoundToolStripMenuItem.Checked;
			_gameBoy.Mute(muteSoundToolStripMenuItem.Checked);
		}

		private void pulseWaveChannel1ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			pulseWaveChannel1ToolStripMenuItem.Checked = !pulseWaveChannel1ToolStripMenuItem.Checked;
			_gameBoy.MuteChannel(0, !pulseWaveChannel1ToolStripMenuItem.Checked);
		}

		private void pulseWaveChannel2ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			pulseWaveChannel2ToolStripMenuItem.Checked = !pulseWaveChannel2ToolStripMenuItem.Checked;
			_gameBoy.MuteChannel(1, !pulseWaveChannel2ToolStripMenuItem.Checked);
		}

		private void waveTableChannel3ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			waveTableChannel3ToolStripMenuItem.Checked = !waveTableChannel3ToolStripMenuItem.Checked;
			_gameBoy.MuteChannel(2, !waveTableChannel3ToolStripMenuItem.Checked);
		}

		private void noiseGeneratorChannel4ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			noiseGeneratorChannel4ToolStripMenuItem.Checked = !noiseGeneratorChannel4ToolStripMenuItem.Checked;
			_gameBoy.MuteChannel(3, !noiseGeneratorChannel4ToolStripMenuItem.Checked);
		}

		private void DisplayFrameTimeToolStripMenuClick(object sender, EventArgs e)
		{
			displayFrameTimeToolStripMenuItem.Checked = !displayFrameTimeToolStripMenuItem.Checked;
			_gameBoy.DisplayFrameTime = displayFrameTimeToolStripMenuItem.Checked;
		}

		private void LogOpcodesToolStripMenuClick(object sender, EventArgs e)
		{
			logOpcodesToolStripMenuItem.Checked = !logOpcodesToolStripMenuItem.Checked;
			GameBoy.ShouldLogOpcodes = logOpcodesToolStripMenuItem.Checked;
		}

		private void ShowDebugOutputToolStripMenuClick(object sender, EventArgs e)
		{
			if (showDebugOutputToolStripMenuItem.Checked)
			{
				showDebugOutputToolStripMenuItem.Checked = false;
				debugRichTextBox.Hide();
				Size = new Size(Width - 255, Height);

				// Also hide the step button.
				stepButton.Visible = false;
			}
			else
			{
				showDebugOutputToolStripMenuItem.Checked = true;
				debugRichTextBox.Show();
				Size = new Size(Width + 255, Height);

				// Also show the step button.
				stepButton.Visible = true;
			}
		}

		private void AboutGBSharpToolStripMenuItemClick(object sender, EventArgs e)
		{
			AboutBox aboutBox = new();
			aboutBox.ShowDialog();
		}

		private void PlayButtonClick(object sender, EventArgs e)
		{
			_gameBoy.Play();

			playButton.Enabled = false;
			pauseButton.Enabled = true;
			stepButton.Enabled = false;
		}

		private void PauseButtonClick(object sender, EventArgs e)
		{
			_gameBoy.Pause();

			playButton.Enabled = true;
			pauseButton.Enabled = false;
			stepButton.Enabled = true;
		}

		private void ResetButtonClick(object sender, EventArgs e)
		{
			bool wasPlaying = _gameBoy.Playing;
			_gameBoy.Reset();

			if (wasPlaying)
			{
				_gameBoy.Play();

				playButton.Enabled = false;
				pauseButton.Enabled = true;
				stepButton.Enabled = false;
			}
			else
			{
				playButton.Enabled = true;
				pauseButton.Enabled = false;
				stepButton.Enabled = true;
			}
		}

		private void StepButtonClick(object sender, EventArgs e)
		{
			_gameBoy.Step();

			playButton.Enabled = true;
			pauseButton.Enabled = false;
			stepButton.Enabled = true;
		}

		public static void Pause()
		{
			s_pauseCallbackInternal?.Invoke();
		}

		private void PauseInternal()
		{
			Invoke(new Action(() => PauseButtonClick(this, EventArgs.Empty)));
		}

		// Poll and render the current state of the Game Boy.
		private void ProcessOutput(object? sender, EventArgs e)
		{
			if (GameBoy.DebugOutput != "")
			{
				debugRichTextBox.AppendText(GameBoy.DebugOutput);
				GameBoy.DebugOutput = "";
			}
			if (GameBoy.DebugStatus != "")
			{
				debugToolStripStatusLabel.Text = GameBoy.DebugStatus;
			}
			else
			{
				// If nothing is displayed, use the last real line of debug output.
				string[] textBoxText = debugRichTextBox.Text.Split('\n');
				int lastTextIndex = Math.Max(textBoxText.Length - 2, 0);
				debugToolStripStatusLabel.Text = textBoxText[lastTextIndex];
			}
			lcdControl.Refresh();
		}

		private void lcdControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == _keyMapping.Up1Key || e.KeyCode == _keyMapping.Up2Key)
			{
				Controller.Instance.Up = true;
			}
			if (e.KeyCode == _keyMapping.Left1Key || e.KeyCode == _keyMapping.Left2Key)
			{
				Controller.Instance.Left = true;
			}
			if (e.KeyCode == _keyMapping.Down1Key || e.KeyCode == _keyMapping.Down2Key)
			{
				Controller.Instance.Down = true;
			}
			if (e.KeyCode == _keyMapping.Right1Key || e.KeyCode == _keyMapping.Right2Key)
			{
				Controller.Instance.Right = true;
			}
			if (e.KeyCode == _keyMapping.A1Key || e.KeyCode == _keyMapping.A2Key)
			{
				Controller.Instance.A = true;
			}
			if (e.KeyCode == _keyMapping.B1Key || e.KeyCode == _keyMapping.B2Key)
			{
				Controller.Instance.B = true;
			}
			if (e.KeyCode == _keyMapping.Start1Key || e.KeyCode == _keyMapping.Start2Key)
			{
				Controller.Instance.Start = true;
			}
			if (e.KeyCode == _keyMapping.Select1Key || e.KeyCode == _keyMapping.Select2Key)
			{
				Controller.Instance.Select = true;
			}
		}

		private void lcdControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == _keyMapping.Up1Key || e.KeyCode == _keyMapping.Up2Key)
			{
				Controller.Instance.Up = false;
			}
			if (e.KeyCode == _keyMapping.Left1Key || e.KeyCode == _keyMapping.Left2Key)
			{
				Controller.Instance.Left = false;
			}
			if (e.KeyCode == _keyMapping.Down1Key || e.KeyCode == _keyMapping.Down2Key)
			{
				Controller.Instance.Down = false;
			}
			if (e.KeyCode == _keyMapping.Right1Key || e.KeyCode == _keyMapping.Right2Key)
			{
				Controller.Instance.Right = false;
			}
			if (e.KeyCode == _keyMapping.A1Key || e.KeyCode == _keyMapping.A2Key)
			{
				Controller.Instance.A = false;
			}
			if (e.KeyCode == _keyMapping.B1Key || e.KeyCode == _keyMapping.B2Key)
			{
				Controller.Instance.B = false;
			}
			if (e.KeyCode == _keyMapping.Start1Key || e.KeyCode == _keyMapping.Start2Key)
			{
				Controller.Instance.Start = false;
			}
			if (e.KeyCode == _keyMapping.Select1Key || e.KeyCode == _keyMapping.Select2Key)
			{
				Controller.Instance.Select = false;
			}
		}

		private void lcdControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			// NOTE: Arrow key (and some other) inputs aren't normally passed along. Fix that.
			switch (e.KeyCode)
			{
				case Keys.Down:
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Tab:
				case Keys.Escape:
					e.IsInputKey = true;
					break;
			}
		}
	}

	// A mapping of two sets of inputs (both are for player one).
	public class KeyMapping
	{
		public Keys Up1Key { get; set; } = Keys.W;
		public Keys Left1Key { get; set; } = Keys.A;
		public Keys Down1Key { get; set; } = Keys.S;
		public Keys Right1Key { get; set; } = Keys.D;
		public Keys A1Key { get; set; } = Keys.K;
		public Keys B1Key { get; set; } = Keys.J;
		public Keys Start1Key { get; set; } = Keys.Enter;
		public Keys Select1Key { get; set; } = Keys.ShiftKey;
		public Keys Up2Key { get; set; } = Keys.Up;
		public Keys Left2Key { get; set; } = Keys.Left;
		public Keys Down2Key { get; set; } = Keys.Down;
		public Keys Right2Key { get; set; } = Keys.Right;
		public Keys A2Key { get; set; } = Keys.NumPad5;
		public Keys B2Key { get; set; } = Keys.NumPad4;
		public Keys Start2Key { get; set; } = Keys.Enter;
		public Keys Select2Key { get; set; } = Keys.Add;
	}
}
