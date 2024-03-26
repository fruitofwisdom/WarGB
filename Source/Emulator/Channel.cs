using System.Security.Cryptography.X509Certificates;

namespace GBSharp
{
	internal class Channel
	{
		public bool SoundOn = false;
	}

	internal class SquareWave : Channel
	{
		// NR10 register settings.
		public byte SweepTime = 0;
		public byte SweepIncDec = 0;
		public byte SweepShiftNumber = 0;

		// NR12 and NR22 register settings.
		public byte DefaultEnvelopeValue = 0;
		public byte EnvelopeUpDown = 0;
		public byte LengthOfEnvelopeSteps = 0;

		// NR13 and NR23 register settings.
		public byte LowOrderFrequencyData = 0;

		// NR14 and NR24 register settings.
		public byte Initialize = 0;
		public byte CounterContinuousSelection = 0;
		public byte HighOrderFrequencyData = 0;
	}

	internal class WaveTable : Channel
	{
		
	}

	internal class NoiseGenerator : Channel
	{

	}
}
