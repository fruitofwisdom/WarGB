namespace GBSharp
{
	internal class GameBoy
	{
		// Emulator state booleans.
		private bool _needToStop;
		private bool _playing;
		private bool _stepRequested;

		// Current count of clock cycles.
		private uint _clocks;

		// How often to sleep the thread.
		private const uint kClocksToSleep = PPU.kDotsPerLine * PPU.kLinesPerFrame;

		// Debug output for the MainForm.
		public static string DebugOutput = "";
		public static string DebugStatus = "Nothing Loaded";

		// Debug output for the log file.
		public static bool ShouldLogOpcodes = false;
		public static string LogOutput = "";
		private static StreamWriter? _logFile = null;

		public GameBoy()
		{
			Reset();
		}

		public void Reset()
		{
			CPU.Instance.Reset();
			Memory.Instance.Reset();
			PPU.Instance.Reset();
			Sound.Instance.Reset();

			_needToStop = false;
			_playing = false;
			_stepRequested = false;

			_clocks = 0;
		}

		// The Game Boy runs in its own thread.
		public void Run()
		{
			// NOTE: We skip any validation or BIOS handling.
			Thread.CurrentThread.Name = "GB# Game Boy";
			DebugOutput += "Ready to play " + ROM.Instance.Title + "!\n";

			// TODO: Don't make a log file when !ShouldLogOpcodes?
			string logPath = Environment.CurrentDirectory + "\\" + ROM.Instance.Filename + ".log";
			_logFile = new StreamWriter(logPath);

			uint clocksToNextCPUCycle = 0;

			// One loop is one cycle of the master clock.
			while (!_needToStop)
			{
				// Do nothing if we're paused, unless a step was requested.
				if (!_playing && !_stepRequested)
				{
					Thread.Sleep(1);
					continue;
				}

				// Run the next CPU instruction when enough clock cycles have elapsed.
				if (clocksToNextCPUCycle > 0)
				{
					clocksToNextCPUCycle--;
				}
				if (clocksToNextCPUCycle == 0)
				{
					// Read and execute the next CPU instruction.
					clocksToNextCPUCycle = CPU.Instance.Step();

					// NOTE: The CPU runs at one quarter of the master clock.
					clocksToNextCPUCycle *= 4;

					if (_stepRequested)
					{
						_playing = false;
						_stepRequested = false;
						Sound.Instance.Stop();
					}
				}

				// Update the PPU.
				PPU.Instance.Update();

				_clocks++;

				// Update the divider and timer every CPU cycle (M cycle).
				if (_clocks % 4 == 0)
				{
					CPU.Instance.UpdateDividerAndTimer();
				}

				// Write log output.
				_logFile.Write(LogOutput);
				LogOutput = "";

				// Do end-of-frame activities, like updating the sound chip, saving, and sleeping.
				if (_clocks == kClocksToSleep)
				{
					Sound.Instance.Update();

					if (Memory.Instance.SaveNeeded)
					{
						Memory.Instance.Save();
					}

					_clocks = 0;
					Thread.Sleep(1);
				}
			}

			_logFile.Close();
		}

		// Stop the emulator and the thread.
		public void Stop()
		{
			_needToStop = true;
		}

		// Start the emulator.
		public void Play()
		{
			_playing = true;
			Sound.Instance.Play();
		}

		// Pause the emulator.
		public void Pause()
		{
			_playing = false;
			Sound.Instance.Stop();
		}

		// Step the emulator through one opcode.
		public void Step()
		{
			_stepRequested = true;
		}
	}
}
