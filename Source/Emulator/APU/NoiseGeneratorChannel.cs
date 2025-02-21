using NAudio.Wave.SampleProviders;

namespace GBSharp
{
	// A noise generator for sound channel 4.
	internal class NoiseGeneratorProvider : SampleProvider
	{
		public NoiseGeneratorProvider()
		{
			BuildWaveform(false);
		}

		public void BuildWaveform(bool on)
		{
			// The "noise" waveform is actually an on or off, but toggled "randomly."
			for (int i = 0; i < kSampleRate; i++)
			{
				_waveTable[i] = on ? 1.0f : 0.0f;
			}
		}
	}

	// Sound channel 4 is a noise generator.
	internal class NoiseGeneratorChannel : Channel
	{
		// TODO: Use the correct wave generator.
		private readonly NoiseGeneratorProvider _noiseGeneratorProvider = new();

		// The envelope settings. (NR42, 0xFF21)
		private uint _currentEnvelopeValue = 0;
		public uint DefaultEnvelopeValue { get; private set; } = 0;
		public bool EnvelopeUpDown = false;
		private uint _currentEnvelopeStep = 0;
		public uint LengthOfEnvelopeSteps { get; private set; } = 0;

		// The frequency settings. (NR43, 0xFF22)
		public int ShiftClockFrequency = 0;
		public bool CounterSteps = false;
		public uint DivisionRatioFrequency = 0;

		// The LFSR for noise generation.
		private ushort _lfsr = 0xABCD;		// A random value.
		private uint _lfsrFrequency = 0;

		public NoiseGeneratorChannel()
		{
			_waveOut.Init(new SampleToWaveProvider(_noiseGeneratorProvider));
		}

		public override void UpdateDiv(ushort divApu)
		{
			// Update length timer.
			base.UpdateDiv(divApu);

			// DIV-APU runs at 512Hz, envelope sweep at 64Hz
			if (divApu % 8 == 0)
			{
				// Apply the envelope sweep, if it's enabled.
				if (LengthOfEnvelopeSteps != 0)
				{
					_currentEnvelopeStep++;
					if (_currentEnvelopeStep >= LengthOfEnvelopeSteps)
					{
						if (EnvelopeUpDown && _currentEnvelopeValue < 15)
						{
							_currentEnvelopeValue++;
						}
						else if (!EnvelopeUpDown && _currentEnvelopeValue > 0)
						{
							_currentEnvelopeValue--;
						}
						_currentEnvelopeStep = 0;
					}
				}
			}
		}

		public override void Update()
		{
			// Are we muted?
			if (APU.Instance.Mute || APU.Instance.MuteChannels[3] || !APU.Instance.IsOn() || !SoundOn)
			{
				_noiseGeneratorProvider._leftVolume = 0.0f;
				_noiseGeneratorProvider._rightVolume = 0.0f;
			}
			else
			{
				// Set volume levels.
				_noiseGeneratorProvider._leftVolume = APU.Instance.Channel4LeftOn ? _currentEnvelopeValue / 15.0f : 0.0f;
				_noiseGeneratorProvider._rightVolume = APU.Instance.Channel4RightOn ? _currentEnvelopeValue / 15.0f : 0.0f;

				// Update the frequency.
				float divider = DivisionRatioFrequency == 0 ? 0.5f : DivisionRatioFrequency;
				float periodValue = divider * (2 << ShiftClockFrequency);
				float newFrequency = 262144 / periodValue;
				_lfsrFrequency++;
				if (_lfsrFrequency >= newFrequency)
				{
					// Perform the random shift shenanigans.
					bool bit0 = ((_lfsr & 0x01) != 0x00);
					bool bit1 = ((_lfsr >> 1) & 0x01) != 0x00;
					bool bit15 = bit0 == bit1;
					_lfsr &= 0x7FFF;
					if (bit15)
					{
						_lfsr |= 0x8000;
					}
					if (CounterSteps)
					{
						_lfsr &= 0xFF7F;
						if (bit15)
						{
							_lfsr |= 0x0080;
						}
					}
					_lfsr >>= 1;
					_noiseGeneratorProvider.BuildWaveform(bit0);
					_lfsrFrequency = 0;
				}
			}

			// Fill the audio buffer with the latest wave table data.
			_noiseGeneratorProvider.FillAudioBuffer();
		}

		public void SetDefaultEnvelopeValue(uint defaultEnvelopeValue)
		{
			_currentEnvelopeValue = defaultEnvelopeValue;
			DefaultEnvelopeValue = defaultEnvelopeValue;
		}

		public void SetLengthOfEnvelopeSteps(uint lengthOfEnvelopeSteps)
		{
			_currentEnvelopeStep = 0;
			LengthOfEnvelopeSteps = lengthOfEnvelopeSteps;
		}
	}
}
