namespace GBSharp
{
	public partial class MainForm : Form
	{
		private readonly GameBoy _gameBoy = new();
		private Thread? _gameBoyThread;

		// A timer used to poll and render the state of the Game Boy.
		private readonly System.Windows.Forms.Timer _gameBoyTimer = new();

		// A callback to pause the emulator.
		private delegate void PauseCallback();
		private static PauseCallback? s_pauseCallbackInternal;

		public MainForm()
		{
			InitializeComponent();

			s_pauseCallbackInternal = PauseInternal;

			// Poll the Game Boy emulator every 10ms.
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
					GameBoy.DebugStatus = "Loaded";

					// Start a new thread to run the Game Boy.
					_gameBoyThread = new Thread(new ThreadStart(_gameBoy.Run));
					_gameBoyThread.IsBackground = true;
					_gameBoyThread.Start();
					playButton.Enabled = true;
					pauseButton.Enabled = false;
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

		private void LogOpcodesToolStripMenuClick(object sender, EventArgs e)
		{
			logOpcodesToolStripMenuItem.Checked = !logOpcodesToolStripMenuItem.Checked;
			GameBoy.ShouldLogOpcodes = logOpcodesToolStripMenuItem.Checked;
		}

		private void AboutGBSharpToolStripMenuItemClick(object sender, EventArgs e)
		{
			AboutBox aboutBox = new();
			aboutBox.ShowDialog();
		}

		private void PlayButtonClick(object sender, EventArgs e)
		{
			_gameBoy.Play();
			GameBoy.DebugStatus = "Playing";

			playButton.Enabled = false;
			pauseButton.Enabled = true;
			stepButton.Enabled = false;
			resetButton.Enabled = false;
		}

		private void PauseButtonClick(object sender, EventArgs e)
		{
			_gameBoy.Pause();
			GameBoy.DebugStatus = "Paused";

			playButton.Enabled = true;
			pauseButton.Enabled = false;
			stepButton.Enabled = true;
			resetButton.Enabled = true;
		}

		private void StepButtonClick(object sender, EventArgs e)
		{
			_gameBoy.Step();

			playButton.Enabled = true;
			pauseButton.Enabled = false;
			stepButton.Enabled = true;
			resetButton.Enabled = true;
		}

		private void ResetButtonClick(object sender, EventArgs e)
		{
			_gameBoy.Reset();
			GameBoy.DebugStatus = "Loaded";

			playButton.Enabled = true;
			pauseButton.Enabled = false;
			stepButton.Enabled = true;
			resetButton.Enabled = false;
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
			debugToolStripStatusLabel.Text = GameBoy.DebugStatus;
			lcdControl.Refresh();
		}

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.K || e.KeyCode == Keys.NumPad2)
			{
				Controller.Instance.A = true;
			}
			if (e.KeyCode == Keys.J || e.KeyCode == Keys.NumPad1)
			{
				Controller.Instance.B = true;
			}
			if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Oemplus)
			{
				Controller.Instance.Select = true;
			}
			if (e.KeyCode == Keys.Enter)
			{
				Controller.Instance.Start = true;
			}
			if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
			{
				Controller.Instance.Right = true;
			}
			if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
			{
				Controller.Instance.Left = true;
			}
			if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
			{
				Controller.Instance.Up = true;
			}
			if (e.KeyCode == Keys.S ||  e.KeyCode == Keys.Down)
			{
				Controller.Instance.Down = true;
			}
		}

		private void MainForm_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.K || e.KeyCode == Keys.NumPad2)
			{
				Controller.Instance.A = false;
			}
			if (e.KeyCode == Keys.J || e.KeyCode == Keys.NumPad1)
			{
				Controller.Instance.B = false;
			}
			if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Oemplus)
			{
				Controller.Instance.Select = false;
			}
			if (e.KeyCode == Keys.Enter)
			{
				Controller.Instance.Start = false;
			}
			if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
			{
				Controller.Instance.Right = false;
			}
			if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
			{
				Controller.Instance.Left = false;
			}
			if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
			{
				Controller.Instance.Up = false;
			}
			if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
			{
				Controller.Instance.Down = false;
			}
		}
	}
}
