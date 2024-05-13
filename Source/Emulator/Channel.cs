using System.Security.Cryptography.X509Certificates;

namespace GBSharp
{
	internal class Channel
	{
		public bool SoundOn = false;
	}

	// Sound channels 1 and 2 are a square wave.
	internal class SquareWave : Channel
	{
		// NR10 (0xFF10) register settings.
		public byte SweepTime = 0x00;
		public byte SweepIncDec = 0x00;
		public byte SweepShiftNumber = 0x00;

		// NR11 (0xFF11) and NR21 (0xFF21) register settings.
		public byte WaveformDuty = 0x00;
		public byte SoundLengthData = 0x00;

		// NR12 (0xFF12) and NR22 (0xFF22) register settings.
		public byte DefaultEnvelopeValue = 0x00;
		public byte EnvelopeUpDown = 0x00;
		public byte LengthOfEnvelopeSteps = 0x00;

		// NR13 (0xFF13) and NR23 (0xFF23) register settings.
		public byte LowOrderFrequencyData = 0x00;

		// NR14 (0xFF14) and NR24 (xFF24) register settings.
		public byte Initialize = 0x00;
		public byte CounterContinuousSelection = 0x00;
		public byte HighOrderFrequencyData = 0x00;
	}

	// Sound channel 3 is a user-defined wave channel.
	internal class WaveTable : Channel
	{
		// Is sound output enabled? (NR30, 0xFF1A)
		public bool SoundEnabled = false;

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
		// TODO: Other sound registers.
	}
}
