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
			_gameBoyTimer.Interval = 10;
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
					stepButton.Enabled = true;
				}
			}
		}

		private void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}

		private void PrintOpcodesToolStripMenuClick(object sender, EventArgs e)
		{
			printOpcodesToolStripMenuItem.Checked = !printOpcodesToolStripMenuItem.Checked;
			GameBoy.ShouldPrintOpcodes = printOpcodesToolStripMenuItem.Checked;
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
			resetButton.Enabled = false;
		}

		private void PauseButtonClick(object sender, EventArgs e)
		{
			_gameBoy.Pause();

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
	}
}
