namespace GBSharp
{
	public partial class MainForm : Form
	{
		Thread? gameBoyThread;

		public delegate void PrintDebugMessageCallback(string debugMessage);

		public MainForm()
		{
			InitializeComponent();

			CPU.Instance.PrintDebugMessageCallback = PrintDebugMessage;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (gameBoyThread != null)
			{
				CPU.Instance.Stop();
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
		}

		private void PauseButtonClick(object sender, EventArgs e)
		{
			CPU.Instance.Pause();
			playButton.Enabled = true;
			pauseButton.Enabled = false;
		}

		public void PrintDebugMessage(string debugMessage)
		{
			// Since we're called from the Game Boy thread, invoke on this thread.
			debugRichTextBox.Invoke(new Action(() => debugRichTextBox.AppendText(debugMessage)));
		}
	}
}
