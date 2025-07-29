namespace GBSharp
{
	internal class GameBoy
	{
		// Emulator state booleans.
		public bool Playing { get; private set; }
		private bool _needToStop;
		private bool _stepFrameRequested;
		private bool _stepOpcodeRequested;
		private bool _stepScanlineRequested;

		// Accurately time frames.
		public const float kFps = 59.7f;
		public bool DisplayFrameTime = false;
		private DateTime _lastFrameTime;
		private bool _frameDone;

		// Current count of clock cycles.
		private uint _clocks;
		private const uint kClocksPerFrame = PPU.kDotsPerLine * PPU.kLinesPerFrame;

		// Emulator options for the MainForm.
		private bool _mute = false;

		// Debug output for the MainForm.
		public static string DebugOutput = "";
		public static string DebugStatus = "";

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
			APU.Instance.Reset();

			Playing = false;
			_needToStop = false;
			_stepFrameRequested = false;
			_stepOpcodeRequested = false;
			_stepScanlineRequested = false;

			// Retain emulator options.
			//DisplayFrameTime = false;
			_lastFrameTime = DateTime.Now;
			_frameDone = false;

			_clocks = 0;

			// NOTE: Retain emulator options.
			//_mute = false;
		}

		// The Game Boy runs in its own thread.
		public void Run()
		{
			// NOTE: We skip any validation or BIOS handling.
			Thread.CurrentThread.Name = "GB# Game Boy";
			DebugOutput += "Ready to play " + ROM.Instance.Title + "!\n";

			uint clocksToNextCPUCycle = 0;

			// One loop is one cycle of the master clock.
			while (!_needToStop)
			{
				// Create a log file only if logging is requested.
				if (ShouldLogOpcodes && _logFile == null)
				{
					string logPath = Environment.CurrentDirectory + "\\" + ROM.Instance.Filename + ".log";
					_logFile = new StreamWriter(logPath);
				}

				// Do nothing if we're paused, unless a step was requested.
				if (!Playing && !(_stepFrameRequested || _stepOpcodeRequested || _stepScanlineRequested))
				{
					Thread.Sleep(0);
					continue;
				}

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
						// Update the APU.
						APU.Instance.Update();

						_lastFrameTime = DateTime.Now;
						_frameDone = false;

						// Potentially trigger a frame step.
						if (_stepFrameRequested)
						{
							Playing = false;
							_stepFrameRequested = false;
							APU.Instance.Stop();
						}
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
					uint cpuFrequency = (uint)(CPU.Instance.DoubleSpeed ? 2 : 4);
					clocksToNextCPUCycle *= cpuFrequency;

					// Potentially trigger an opcode step.
					if (_stepOpcodeRequested)
					{
						Playing = false;
						_stepOpcodeRequested = false;
						APU.Instance.Stop();
					}
				}

				// Update the PPU.
				bool didRender = PPU.Instance.Update();

				// Potentially trigger a scanline step.
				if (didRender && _stepScanlineRequested)
				{
					Playing = false;
					_stepScanlineRequested = false;
					APU.Instance.Stop();
				}

				_clocks++;

				// Update the divider and timer every CPU cycle (M-cycle).
				uint dividerAndTimerFrequency = (uint)(CPU.Instance.DoubleSpeed ? 2 : 4);
				if (_clocks % dividerAndTimerFrequency == 0)
				{
					CPU.Instance.UpdateDividerAndTimer();
				}

				// Write log output and close, if requested.
				if (_logFile != null)
				{
					_logFile.Write(LogOutput);
					LogOutput = "";

					if (!ShouldLogOpcodes)
					{
						_logFile.Close();
						_logFile = null;
					}
				}

				// Trigger end-of-frame activities, like saving.
				if (_clocks == kClocksPerFrame)
				{
					if (Memory.Instance.SaveNeeded)
					{
						Memory.Instance.Save();
					}

					double elapsedMs = (DateTime.Now - _lastFrameTime).TotalMilliseconds;
					if (DisplayFrameTime)
					{
						double fps = 1000.0d / elapsedMs;
						DebugStatus = $"{elapsedMs:F3}ms, {fps:F1}fps";
					}

					_frameDone = true;
					_clocks = 0;
				}
			}

			if (_logFile != null)
			{
				_logFile.Close();
				_logFile = null;
			}
		}

		// Stop the emulator and the thread.
		public void Stop()
		{
			_needToStop = true;
			APU.Instance.Stop();
		}

		// Start the emulator.
		public void Play()
		{
			Playing = true;
			APU.Instance.Play();
		}

		// Pause the emulator.
		public void Pause()
		{
			Playing = false;
			APU.Instance.Stop();
		}

		// Step the emulator through one frame.
		public void NextFrame()
		{
			_stepFrameRequested = true;
		}

		// Step the emulator through one opcode.
		public void NextOpcode()
		{
			_stepOpcodeRequested = true;
		}

		// Step the emulator through one scanline.
		public void NextScanline()
		{
			_stepScanlineRequested = true;
		}

		// Mute all sound.
		public void Mute(bool mute)
		{
			_mute = mute;
			APU.Instance.Mute = mute;
		}

		// Mute individual channels.
		public void MuteChannel(int channel, bool mute)
		{
			APU.Instance.MuteChannels[channel] = mute;
		}

		// Should the background be rendered?
		public void ShouldRenderBackground(bool shouldRender)
		{
			PPU.Instance.ShouldRenderBackground = shouldRender;
		}

		// Should the window be rendered?
		public void ShouldRenderWindow(bool shouldRender)
		{
			PPU.Instance.ShouldRenderWindow = shouldRender;
		}

		// Should the objects be rendered?
		public void ShouldRenderObjects(bool shouldRender)
		{
			PPU.Instance.ShouldRenderObjects = shouldRender;
		}
	}
}
