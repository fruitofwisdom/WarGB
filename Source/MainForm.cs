namespace GBSharp
{
	public partial class MainForm : Form
	{
		Thread? gameBoyThread;

		private delegate void PrintDebugMessageCallback(string debugMessage);
		private static PrintDebugMessageCallback? PrintDebugMessageCallbackInternal;

		public MainForm()
		{
			InitializeComponent();

			PrintDebugMessageCallbackInternal = PrintDebugMessageInternal;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (gameBoyThread != null)
			{
				CPU.Instance.Stop();
				gameBoyThread.Join();
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
					if (gameBoyThread != null)
					{
						CPU.Instance.Stop();
					}

					Text = "GB# - " + ROM.Instance.Title;
					gameBoyThread = new Thread(new ThreadStart(CPU.Instance.Run));
					gameBoyThread.Start();
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

		public static void PrintDebugMessage(string debugMessage)
		{
			PrintDebugMessageCallbackInternal?.Invoke(debugMessage);
		}

		private void PrintDebugMessageInternal(string debugMessage)
		{
			// We must invoke on the UI thread.
			debugRichTextBox.Invoke(new Action(() => debugRichTextBox.AppendText(debugMessage)));
		}
	}
}
