namespace GBSharp
{
	internal class GameBoy
	{
		// Emulator state booleans.
		private bool _needToStop;
		private bool _playing;
		private bool _stepRequested;

		// Accurately time frames.
		private DateTime _lastFrameTime;
		private bool _frameDone;
		private const float kFps = 59.7f;

		// Current count of clock cycles.
		private uint _clocks;
		private const uint kClocksPerFrame = PPU.kDotsPerLine * PPU.kLinesPerFrame;

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

			_lastFrameTime = DateTime.Now;
			_frameDone = false;

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
					Thread.Sleep(0);
					continue;
				}

				// When ready for a new frame, wait so our timing is accurate.
				// When a frame is done, wait until our timing is accurate.
				if (_frameDone)
				{
					double elapsedMs = (DateTime.Now - _lastFrameTime).TotalMilliseconds;
					double msToSleep = 1000 / kFps - elapsedMs;
					if (msToSleep > 0.0d)
					{
						Thread.Sleep(0);
						continue;
					}
					else
					{
						_lastFrameTime = DateTime.Now;
						_frameDone = false;
					}
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

				// Update the APU.
				Sound.Instance.Update();

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

				// Trigger end-of-frame activities, like saving.
				if (_clocks == kClocksPerFrame)
				{
					if (Memory.Instance.SaveNeeded)
					{
						Memory.Instance.Save();
					}

					_frameDone = true;
					_clocks = 0;
				}
			}

			_logFile.Close();
		}

		// Stop the emulator and the thread.
		public void Stop()
		{
			_needToStop = true;
			Sound.Instance.Stop();
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
