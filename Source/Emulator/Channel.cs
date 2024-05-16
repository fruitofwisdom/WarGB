namespace GBSharp
{
	internal class Channel
	{
		public bool SoundOn = false;
	}

	// Sound channels 1 and 2 are a square wave.
	internal class SquareWave : Channel
	{
		// The sweep settings. (NR10, 0xFF10)
		public byte SweepTime = 0x00;
		public byte SweepIncDec = 0x00;
		public byte SweepShiftNumber = 0x00;

		// The waveform duty and sound length timer. (NR11 and NR21, 0xFF11 and 0xFF21)
		public byte WaveformDuty = 0x00;
		public byte SoundLength = 0x00;

		// The envelope settings. (NR12 and NR22, 0xFF12 and 0xFF22)
		public byte DefaultEnvelopeValue = 0x00;
		public byte EnvelopeUpDown = 0x00;
		public byte LengthOfEnvelopeSteps = 0x00;

		// The low-order frequency period. (NR13 and NR23, 0xFF13 and 0xFF23)
		public byte LowOrderFrequencyData = 0x00;

		// The high-order frequency period and other settings. (NR14 and NR24, 0xFF14 and 0xFF24)
		public bool Initialize = false;
		public bool CounterContinuousSelection = false;
		public byte HighOrderFrequencyData = 0x00;
	}

	// Sound channel 3 is a user-defined wave channel.
	internal class WaveTable : Channel
	{
		// Is sound output enabled? (NR30, 0xFF1A)
		public bool SoundEnabled = false;

		// The sound length timer. (NR31, 0xFF1B)
		public byte SoundLength = 0x00;

		// The output level. (NR32, 0xFF1C)
		public byte OutputLevel = 0x00;

		// The low-order frequency period. (NR33, 0xFF1D)
		public byte LowOrderFrequencyData = 0x00;

		// The high-order frequency period and other settings. (NR34, 0xFF1E)
		public bool Initialize = false;
		public bool CounterContinuousSelection = false;
		public byte HighOrderFrequencyData = 0x00;

		// The composition of the waveform. (0xFF30 through 0xFF3F)
		public byte[] WaveformRAM;

		public WaveTable()
		{
			WaveformRAM = new byte[16];
		}
	}

	// Sound channel 4 is a noise generator.
	internal class NoiseGenerator : Channel
	{
		// The sound length timer. (NR41, 0xFF20)
		public byte SoundLength = 0x00;

		// The envelope settings. (NR42, 0xFF21)
		public byte DefaultEnvelopeValue = 0x00;
		public byte EnvelopeUpDown = 0x00;
		public byte LengthOfEnvelopeSteps = 0x00;

		// The frequency settings. (NR43, 0xFF22)
		public byte ShiftClockFrequency = 0x00;
		public bool CounterSteps = false;
		public byte DivisionRatioFrequency = 0x00;

		// Other settings. (NR44, 0xFF23)
		public bool Initialize = false;
		public bool CounterContinuousSelection = false;
	}
}
