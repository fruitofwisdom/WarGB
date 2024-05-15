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

		// Debug output communication for the MainForm.
		public static string DebugOutput = "";
		public static string DebugStatus = "";
		public static bool ShouldPrintOpcodes = false;

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
					}
				}

				// Update the PPU.
				PPU.Instance.Update();

				_clocks++;

				// Prevent clocks from overflowing and remember to sleep each frame.
				if (_clocks == kClocksToSleep)
				{
					_clocks = 0;
					Thread.Sleep(1);
				}
			}
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
		}

		// Pause the emulator.
		public void Pause()
		{
			_playing = false;
		}

		// Step the emulator through one opcode.
		public void Step()
		{
			_stepRequested = true;
		}
	}
}
