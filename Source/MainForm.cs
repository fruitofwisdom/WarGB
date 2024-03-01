namespace GBSharp
{
	public partial class MainForm : Form
	{
		Thread? GameBoyThread;

		// Available debug callbacks.
		private delegate void PauseCallback();
		private static PauseCallback? PauseCallbackInternal;
		private delegate void PrintDebugMessageCallback(string debugMessage);
		private static PrintDebugMessageCallback? PrintDebugMessageCallbackInternal;
		private delegate void PrintDebugStatusCallback(string debugStatus);
		private static PrintDebugStatusCallback? PrintDebugStatisCallbackInternal;

		public MainForm()
		{
			InitializeComponent();

			PauseCallbackInternal = PauseInternal;
			PrintDebugMessageCallbackInternal = PrintDebugMessageInternal;
			PrintDebugStatisCallbackInternal = PrintDebugStatusInternal;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (GameBoyThread != null)
			{
				CPU.Instance.Stop();
				GameBoyThread.Join();
			}

			base.OnFormClosing(e);
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
					// Close any previously running threads.
					if (GameBoyThread != null)
					{
						CPU.Instance.Stop();
					}

					// Put the game name and ROM type in our title.
					Text = "GB# - " + ROM.Instance.Title + " (" + ROM.Instance.CartridgeType.ToString().Replace("_", "+") + ")";

					GameBoyThread = new Thread(new ThreadStart(CPU.Instance.Run));
					GameBoyThread.Start();
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

		private void AboutGBSharpToolStripMenuItemClick(object sender, EventArgs e)
		{
			AboutBox aboutBox = new();
			aboutBox.ShowDialog();
		}

		private void PlayButtonClick(object sender, EventArgs e)
		{
			CPU.Instance.Play();
			playButton.Enabled = false;
			pauseButton.Enabled = true;
			stepButton.Enabled = false;
		}

		private void PauseButtonClick(object sender, EventArgs e)
		{
			CPU.Instance.Pause();
			playButton.Enabled = true;
			pauseButton.Enabled = false;
			stepButton.Enabled = true;
		}

		private void StepButtonClick(object sender, EventArgs e)
		{
			CPU.Instance.Step();
			playButton.Enabled = true;
			pauseButton.Enabled = false;
			stepButton.Enabled = true;
		}

		public static void Pause()
		{
			PauseCallbackInternal?.Invoke();
		}

		private void PauseInternal()
		{
			// We must invoke on the UI thread.
			Invoke(new Action(() => PauseButtonClick(this, EventArgs.Empty)));
		}

		public static void PrintDebugMessage(string debugMessage)
		{
			PrintDebugMessageCallbackInternal?.Invoke(debugMessage);
		}

		private void PrintDebugMessageInternal(string debugMessage)
		{
			// We must invoke on the UI thread.
			Invoke(new Action(() => debugRichTextBox.AppendText(debugMessage)));
		}

		public static void PrintDebugStatus(string debugStatus)
		{
			PrintDebugStatisCallbackInternal?.Invoke(debugStatus);
		}

		private void PrintDebugStatusInternal(string debugStatus)
		{
			// We must invoke on the UI thread.
			Invoke(new Action(() => debugToolStripStatusLabel.Text = debugStatus));
		}
	}
}
