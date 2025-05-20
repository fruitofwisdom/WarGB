using GBSharp.Properties;
using SharpDX.XInput;
//using Windows.Gaming.Input;

namespace GBSharp
{
	public partial class MainForm : Form
	{
		private readonly GameBoy _gameBoy = new();
		private Thread? _gameBoyThread;

		// A timer used to poll and render the state of the Game Boy.
		private readonly System.Windows.Forms.Timer _gameBoyTimer = new();

		// A timer used to poll any inputs.
		private readonly System.Windows.Forms.Timer _gamepadTimer = new();

		// For XInput controllers.
		private const int kThumbThreshold = 15000;
		private int _lastPacketNumber = 0;
		private readonly SharpDX.XInput.Controller _xInputController;

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

			// Poll any gamepads as well.
			_gamepadTimer.Tick += new EventHandler(ProcessInput);
			_gamepadTimer.Interval = 1000 / 60;
			_gamepadTimer.Start();

			_xInputController = new SharpDX.XInput.Controller(UserIndex.One);
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
				LoadAndPlayROM(openFileDialog.FileName);
			}
		}

		private void RecentROM1ToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (Settings.Default.RecentROM1 != null)
			{
				LoadAndPlayROM(Settings.Default.RecentROM1);
			}
		}

		private void RecentROM2ToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (Settings.Default.RecentROM2 != null)
			{
				LoadAndPlayROM(Settings.Default.RecentROM2);
			}
		}

		private void RecentROM3ToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (Settings.Default.RecentROM3 != null)
			{
				LoadAndPlayROM(Settings.Default.RecentROM3);
			}
		}

		private void RecentROM4ToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (Settings.Default.RecentROM4 != null)
			{
				LoadAndPlayROM(Settings.Default.RecentROM4);
			}
		}

		private void RecentROM5ToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (Settings.Default.RecentROM5 != null)
			{
				LoadAndPlayROM(Settings.Default.RecentROM5);
			}
		}

		private void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
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
				Settings.Default.LCDColorOriginalGreen = true;
			}
		}

		private void BlackAndWhiteToolStripMenuClick(object sender, EventArgs e)
		{
			if (!blackAndWhiteToolStripMenuItem.Checked)
			{
				blackAndWhiteToolStripMenuItem.Checked = true;
				originalGreenToolStripMenuItem.Checked = false;
				lcdControl.UseOriginalGreen = false;
				Settings.Default.LCDColorOriginalGreen = false;
			}
		}

		private void OneXToolStripMenuItemClick(object sender, EventArgs e)
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
			Settings.Default.LCDSize = 1;
		}

		private void TwoXToolStripMenuItemClick(object sender, EventArgs e)
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
			Settings.Default.LCDSize = 2;
		}

		private void ThreeXToolStripMenuItemClick(object sender, EventArgs e)
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
			Settings.Default.LCDSize = 3;
		}

		private void FourXToolStripMenuItemClick(object sender, EventArgs e)
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
			Settings.Default.LCDSize = 4;
		}

		private void FiveXToolStripMenuItemClick(object sender, EventArgs e)
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
			Settings.Default.LCDSize = 5;
		}

		private void MuteSoundToolStripMenuClick(object sender, EventArgs e)
		{
			muteSoundToolStripMenuItem.Checked = !muteSoundToolStripMenuItem.Checked;
			_gameBoy.Mute(muteSoundToolStripMenuItem.Checked);
			Settings.Default.MuteSound = muteSoundToolStripMenuItem.Checked;
		}

		private void pulseWaveChannel1ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			pulseWaveChannel1ToolStripMenuItem.Checked = !pulseWaveChannel1ToolStripMenuItem.Checked;
			_gameBoy.MuteChannel(0, !pulseWaveChannel1ToolStripMenuItem.Checked);
			Settings.Default.MuteChannel0 = !pulseWaveChannel1ToolStripMenuItem.Checked;
		}

		private void pulseWaveChannel2ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			pulseWaveChannel2ToolStripMenuItem.Checked = !pulseWaveChannel2ToolStripMenuItem.Checked;
			_gameBoy.MuteChannel(1, !pulseWaveChannel2ToolStripMenuItem.Checked);
			Settings.Default.MuteChannel1 = !pulseWaveChannel2ToolStripMenuItem.Checked;
		}

		private void waveTableChannel3ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			waveTableChannel3ToolStripMenuItem.Checked = !waveTableChannel3ToolStripMenuItem.Checked;
			_gameBoy.MuteChannel(2, !waveTableChannel3ToolStripMenuItem.Checked);
			Settings.Default.MuteChannel2 = !waveTableChannel3ToolStripMenuItem.Checked;
		}

		private void noiseGeneratorChannel4ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			noiseGeneratorChannel4ToolStripMenuItem.Checked = !noiseGeneratorChannel4ToolStripMenuItem.Checked;
			_gameBoy.MuteChannel(3, !noiseGeneratorChannel4ToolStripMenuItem.Checked);
			Settings.Default.MuteChannel3 = !noiseGeneratorChannel4ToolStripMenuItem.Checked;
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

		private void NextFrameToolStripMenuItemClick(object sender, EventArgs e)
		{
			_gameBoy.NextFrame();
			UpdatePlayState();
		}

		private void NextOpcodeToolStripMenuItemClick(object sender, EventArgs e)
		{
			_gameBoy.NextOpcode();
			UpdatePlayState();
		}

		private void NextScanlineToolStripMenuItemClick(object sender, EventArgs e)
		{
			_gameBoy.NextScanline();
			UpdatePlayState();
		}

		private void ShowDebugOutputToolStripMenuClick(object sender, EventArgs e)
		{
			if (showDebugOutputToolStripMenuItem.Checked)
			{
				showDebugOutputToolStripMenuItem.Checked = false;
				debugRichTextBox.Hide();
				Size = new Size(Width - 255, Height);
			}
			else
			{
				showDebugOutputToolStripMenuItem.Checked = true;
				debugRichTextBox.Show();
				Size = new Size(Width + 255, Height);
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
			UpdatePlayState();
		}

		private void PauseButtonClick(object sender, EventArgs e)
		{
			_gameBoy.Pause();
			UpdatePlayState();
		}

		private void ResetButtonClick(object sender, EventArgs e)
		{
			bool wasPlaying = _gameBoy.Playing;
			_gameBoy.Reset();
			if (wasPlaying)
			{
				_gameBoy.Play();
			}
			UpdatePlayState();
		}

		private void NextFrameButtonClick(object sender, EventArgs e)
		{
			_gameBoy.NextFrame();
			UpdatePlayState();
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

		// Poll and process gamepad input.
		private void ProcessInput(object? sender, EventArgs e)
		{
			bool needJoypadInterrupt = false;

			// Check for XInput (Xbox) controllers.
			if (_xInputController.IsConnected)
			{
				int packetNumber = _xInputController.GetState().PacketNumber;
				if (packetNumber != _lastPacketNumber)
				{
					needJoypadInterrupt = HandleXInputController();
					_lastPacketNumber = packetNumber;
				}
			}
			// TODO: Check for other (non-Xbox) controllers.
			/*
			else if (RawGameController.RawGameControllers.Count > 0)
			{
				needJoypadInterrupt = HandleDirectInputController();
			}
			*/

			// A button was pressed, so trigger a joypad interrupt.
			if (needJoypadInterrupt)
			{
				Controller.Instance.TriggerJoypadInterrupt();
			}
		}

		private void lcdControl_KeyDown(object sender, KeyEventArgs e)
		{
			bool needJoypadInterrupt = false;

			if (e.KeyCode == _keyMapping.Up1Key)
			{
				Controller.Instance.Up = true;
				needJoypadInterrupt = true;
			}
			if (e.KeyCode == _keyMapping.Left1Key)
			{
				Controller.Instance.Left = true;
				needJoypadInterrupt = true;
			}
			if (e.KeyCode == _keyMapping.Down1Key)
			{
				Controller.Instance.Down = true;
				needJoypadInterrupt = true;
			}
			if (e.KeyCode == _keyMapping.Right1Key)
			{
				Controller.Instance.Right = true;
				needJoypadInterrupt = true;
			}
			if (e.KeyCode == _keyMapping.A1Key)
			{
				Controller.Instance.A = true;
				needJoypadInterrupt = true;
			}
			if (e.KeyCode == _keyMapping.B1Key)
			{
				Controller.Instance.B = true;
				needJoypadInterrupt = true;
			}
			if (e.KeyCode == _keyMapping.Start1Key)
			{
				Controller.Instance.Start = true;
				needJoypadInterrupt = true;
			}
			if (e.KeyCode == _keyMapping.Select1Key)
			{
				Controller.Instance.Select = true;
				needJoypadInterrupt = true;
			}

			// A button was pressed, so trigger a joypad interrupt.
			if (needJoypadInterrupt)
			{
				Controller.Instance.TriggerJoypadInterrupt();
			}
		}

		private void lcdControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == _keyMapping.Up1Key)
			{
				Controller.Instance.Up = false;
			}
			if (e.KeyCode == _keyMapping.Left1Key)
			{
				Controller.Instance.Left = false;
			}
			if (e.KeyCode == _keyMapping.Down1Key)
			{
				Controller.Instance.Down = false;
			}
			if (e.KeyCode == _keyMapping.Right1Key)
			{
				Controller.Instance.Right = false;
			}
			if (e.KeyCode == _keyMapping.A1Key)
			{
				Controller.Instance.A = false;
			}
			if (e.KeyCode == _keyMapping.B1Key)
			{
				Controller.Instance.B = false;
			}
			if (e.KeyCode == _keyMapping.Start1Key)
			{
				Controller.Instance.Start = false;
			}
			if (e.KeyCode == _keyMapping.Select1Key)
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

		// Poll and process XInput controllers.
		private bool HandleXInputController()
		{
			bool needJoypadInterrupt = false;

			State state = _xInputController.GetState();
			if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
			{
				// NOTE: Game Boy and other controllers have A and B swapped.
				Controller.Instance.A = true;
				needJoypadInterrupt = true;
			}
			else
			{
				Controller.Instance.A = false;
			}
			if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
			{
				// NOTE: Game Boy and other controllers have A and B swapped.
				Controller.Instance.B = true;
				needJoypadInterrupt = true;
			}
			else
			{
				Controller.Instance.B = false;
			}
			if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back))
			{
				Controller.Instance.Select = true;
				needJoypadInterrupt = true;
			}
			else
			{
				Controller.Instance.Select = false;
			}
			if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start))
			{
				Controller.Instance.Start = true;
				needJoypadInterrupt = true;
			}
			else
			{
				Controller.Instance.Start = false;
			}
			if (state.Gamepad.LeftThumbX > kThumbThreshold || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
			{
				Controller.Instance.Right = true;
				needJoypadInterrupt = true;
			}
			else
			{
				Controller.Instance.Right = false;
			}
			if (state.Gamepad.LeftThumbX < -kThumbThreshold || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft))
			{
				Controller.Instance.Left = true;
				needJoypadInterrupt = true;
			}
			else
			{
				Controller.Instance.Left = false;
			}
			if (state.Gamepad.LeftThumbY > kThumbThreshold || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
			{
				Controller.Instance.Up = true;
				needJoypadInterrupt = true;
			}
			else
			{
				Controller.Instance.Up = false;
			}
			if (state.Gamepad.LeftThumbY < -kThumbThreshold || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
			{
				Controller.Instance.Down = true;
				needJoypadInterrupt = true;
			}
			else
			{
				Controller.Instance.Down = false;
			}

			return needJoypadInterrupt;
		}

		// TODO: Poll and process DirectInput controllers.
		/*
		bool HandleDirectInputController()
		{
			bool needJoypadInterrupt = false;

			// Get the first gamepad and its current reading.
			RawGameController controller = RawGameController.RawGameControllers[0];
			bool[] controllerButtonReading = new bool[controller.ButtonCount];
			GameControllerSwitchPosition[] controllerSwitchReading = new GameControllerSwitchPosition[controller.SwitchCount];
			double[] controllerAxisReading = new double[controller.AxisCount];
			controller.GetCurrentReading(controllerButtonReading, controllerSwitchReading, controllerAxisReading);

			for (int i = 0; i < controllerButtonReading.Length; i++)
			{
				GameControllerButtonLabel buttonLabel = controller.GetButtonLabel(i);
				if (buttonLabel == GameControllerButtonLabel.Right)
				{
					if (controllerButtonReading[i])
					{
						Controller.Instance.Right = true;
						needJoypadInterrupt = true;
					}
					else
					{
						Controller.Instance.Right = false;
					}
				}
				else if (buttonLabel == GameControllerButtonLabel.Left)
				{
					if (controllerButtonReading[i])
					{
						Controller.Instance.Left = true;
						needJoypadInterrupt = true;
					}
					else
					{
						Controller.Instance.Left = false;
					}
				}
				else if (buttonLabel == GameControllerButtonLabel.Up)
				{
					if (controllerButtonReading[i])
					{
						Controller.Instance.Up = true;
						needJoypadInterrupt = true;
					}
					else
					{
						Controller.Instance.Up = false;
					}
				}
				else if (buttonLabel == GameControllerButtonLabel.Down)
				{
					if (controllerButtonReading[i])
					{
						Controller.Instance.Down = true;
						needJoypadInterrupt = true;
					}
					else
					{
						Controller.Instance.Down = false;
					}
				}
			}

			return needJoypadInterrupt;
		}
		*/

		// Load and play a ROM.
		private void LoadAndPlayROM(string fileName)
		{
			if (ROM.Instance.Load(fileName))
			{
				// Add this ROM to the recent ROMs list.
				AddROMToRecentROMs(fileName);

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
				resetButton.Enabled = true;
				UpdatePlayState();

				// Play automatically.
				PlayButtonClick(this, new EventArgs());
			}
		}

		// Update buttons and menu items based on whether the emulator is running.
		private void UpdatePlayState()
		{
			bool playing = _gameBoy.Playing;

			playButton.Enabled = !playing;
			pauseButton.Enabled = playing;
			nextFrameButton.Enabled = !playing;

			nextFrameToolStripMenuItem.Enabled = !playing;
			nextOpcodeToolStripMenuItem.Enabled = !playing;
			nextScanlineToolStripMenuItem.Enabled = !playing;
		}

		// Add a ROM to the list of recent ROMs.
		private void AddROMToRecentROMs(string fileName)
		{
			// List all the recent ROMs.
			List<string> recentROMs = new(
			[
				Settings.Default.RecentROM1,
				Settings.Default.RecentROM2,
				Settings.Default.RecentROM3,
				Settings.Default.RecentROM4,
				Settings.Default.RecentROM5
			]);

			// If this ROM is already in the list, remove it.
			if (recentROMs.Contains(fileName))
			{
				recentROMs.RemoveAll(rom => rom == fileName);
			}
			// And add this ROM to the front of the list.
			recentROMs.Insert(0, fileName);
			// Lastly, add any empty strings, if necessary.
			while (recentROMs.Count < 5)
			{
				recentROMs.Add("");
			}

			// Write the list back out.
			Settings.Default.RecentROM1 = recentROMs[0];
			Settings.Default.RecentROM2 = recentROMs[1];
			Settings.Default.RecentROM3 = recentROMs[2];
			Settings.Default.RecentROM4 = recentROMs[3];
			Settings.Default.RecentROM5 = recentROMs[4];

			UpdateRecentROMs();
		}

		// Update the UI elements for recent ROMs.
		private void UpdateRecentROMs()
		{
			// Either hide the recent ROM menu item or display its filename.
			if (Settings.Default.RecentROM1 == "")
			{
				recentROM1ToolStripMenuItem.Visible = false;
			}
			else
			{
				recentROM1ToolStripMenuItem.Visible = true;
				recentROM1ToolStripMenuItem.Text = "1 " + Settings.Default.RecentROM1;
			}
			if (Settings.Default.RecentROM2 == "")
			{
				recentROM2ToolStripMenuItem.Visible = false;
			}
			else
			{
				recentROM2ToolStripMenuItem.Visible = true;
				recentROM2ToolStripMenuItem.Text = "2 " + Settings.Default.RecentROM2;
			}
			if (Settings.Default.RecentROM3 == "")
			{
				recentROM3ToolStripMenuItem.Visible = false;
			}
			else
			{
				recentROM3ToolStripMenuItem.Visible = true;
				recentROM3ToolStripMenuItem.Text = "3 " + Settings.Default.RecentROM3;
			}
			if (Settings.Default.RecentROM4 == "")
			{
				recentROM4ToolStripMenuItem.Visible = false;
			}
			else
			{
				recentROM4ToolStripMenuItem.Visible = true;
				recentROM4ToolStripMenuItem.Text = "4 " + Settings.Default.RecentROM4;
			}
			if (Settings.Default.RecentROM5 == "")
			{
				recentROM5ToolStripMenuItem.Visible = false;
			}
			else
			{
				recentROM5ToolStripMenuItem.Visible = true;
				recentROM5ToolStripMenuItem.Text = "5 " + Settings.Default.RecentROM5;
			}

			// If there were no recent ROMs, also hide the separator.
			recentROMToolStripSeparator.Visible =
				Settings.Default.RecentROM1 != "" ||
				Settings.Default.RecentROM2 != "" ||
				Settings.Default.RecentROM3 != "" ||
				Settings.Default.RecentROM4 != "" ||
				Settings.Default.RecentROM5 != "";
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Close and stop any current Game Boy thread.
			if (_gameBoyThread != null)
			{
				_gameBoy.Stop();
				_gameBoyThread.Join();
			}

			// Save the user's application settings.
			Settings.Default.Save();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			// Apply user settings.
			UpdateRecentROMs();
			if (Settings.Default.LCDColorOriginalGreen)
			{
				OriginalGreenToolStripMenuClick(sender, e);
			}
			else
			{
				BlackAndWhiteToolStripMenuClick(sender, e);
			}
			switch (Settings.Default.LCDSize)
			{
				case 1:
					OneXToolStripMenuItemClick(sender, e);
					break;
				case 2:
					TwoXToolStripMenuItemClick(sender, e);
					break;
				case 3:
					ThreeXToolStripMenuItemClick(sender, e);
					break;
				case 4:
					FourXToolStripMenuItemClick(sender, e);
					break;
				case 5:
					FiveXToolStripMenuItemClick(sender, e);
					break;
				default:
					OneXToolStripMenuItemClick(sender, e);
					break;
			}
			if (Settings.Default.MuteSound)
			{
				MuteSoundToolStripMenuClick(sender, e);
			}
			if (Settings.Default.MuteChannel0)
			{
				pulseWaveChannel1ToolStripMenuItem_Click(sender, e);
			}
			if (Settings.Default.MuteChannel1)
			{
				pulseWaveChannel2ToolStripMenuItem_Click(sender, e);
			}
			if (Settings.Default.MuteChannel2)
			{
				waveTableChannel3ToolStripMenuItem_Click(sender, e);
			}
			if (Settings.Default.MuteChannel3)
			{
				noiseGeneratorChannel4ToolStripMenuItem_Click(sender, e);
			}
		}
	}

	// A mapping for keyboard input.
	public class KeyMapping
	{
		public Keys Up1Key { get; set; } = Enum.Parse<Keys>(Settings.Default.KeyboardUp);
		public Keys Left1Key { get; set; } = Enum.Parse<Keys>(Settings.Default.KeyboardLeft);
		public Keys Down1Key { get; set; } = Enum.Parse<Keys>(Settings.Default.KeyboardDown);
		public Keys Right1Key { get; set; } = Enum.Parse<Keys>(Settings.Default.KeyboardRight);
		public Keys A1Key { get; set; } = Enum.Parse<Keys>(Settings.Default.KeyboardA);
		public Keys B1Key { get; set; } = Enum.Parse<Keys>(Settings.Default.KeyboardB);
		public Keys Start1Key { get; set; } = Enum.Parse<Keys>(Settings.Default.KeyboardStart);
		public Keys Select1Key { get; set; } = Enum.Parse<Keys>(Settings.Default.KeyboardSelect);
		// TODO: Xbox controller mapping.
		/*
		public Keys Up2Key { get; set; } = Keys.Up;
		public Keys Left2Key { get; set; } = Keys.Left;
		public Keys Down2Key { get; set; } = Keys.Down;
		public Keys Right2Key { get; set; } = Keys.Right;
		public Keys A2Key { get; set; } = Keys.NumPad5;
		public Keys B2Key { get; set; } = Keys.NumPad4;
		public Keys Start2Key { get; set; } = Keys.Enter;
		public Keys Select2Key { get; set; } = Keys.Add;
		*/
	}
}
