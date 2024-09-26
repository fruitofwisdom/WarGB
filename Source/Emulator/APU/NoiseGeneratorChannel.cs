using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GBSharp
{
	// A noise generator for sound channel 4.
	internal class NoiseProvider : ISampleProvider
	{
		public WaveFormat WaveFormat { get; private set; }

		private const int kSampleRate = 44100;
		private readonly float[] _waveTable;

		private float _frequency = 400.0f;
		private float _phase = 0.0f;
		private float _phaseStep = 0.0f;
		public float _volume = 0.0f;

		public NoiseProvider()
		{
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(kSampleRate, 1);
			_waveTable = new float[kSampleRate];
		}

		public void BuildWaveform(bool on)
		{
			// The "noise" waveform is actually an on or off, but toggled "randomly."
			for (int i = 0; i < kSampleRate; i++)
			{
				_waveTable[i] = on ? 1.0f : 0.0f;
			}
		}

		public int Read(float[] buffer, int offset, int count)
		{
			// Update the phase step based on frequency.
			_phaseStep = _waveTable.Length * (_frequency / WaveFormat.SampleRate);

			// Fill the buffer.
			for (int i = 0; i < count; i++)
			{
				int waveTableIndex = (int)_phase % _waveTable.Length;
				buffer[i + offset] = _waveTable[waveTableIndex] * _volume;
				_phase += _phaseStep;
				while (_phase > _waveTable.Length)
				{
					_phase -= _waveTable.Length;
				}
			}

			return count;
		}
	}

	// Sound channel 4 is a noise generator.
	internal class NoiseGeneratorChannel : Channel
	{
		// TODO: Use the correct wave generator.
		private readonly NoiseProvider _noiseProvider = new();

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
			_waveOut.Init(new SampleToWaveProvider(_noiseProvider));
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
			if (!APU.Instance.IsOn() ||
				// TODO: Support stereo sound.
				//!Sound.Instance.Channel4LeftOn ||
				//!Sound.Instance.Channel4RightOn ||
				!SoundOn)
			{
				_noiseProvider._volume = 0.0f;
			}
			else
			{
				// TODO: Support stereo sound.
				_noiseProvider._volume =
					APU.Instance.LeftOutputVolume / 7.0f * _currentEnvelopeValue / 15.0f *
					kMaxVolume;

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
					_noiseProvider.BuildWaveform(bit0);
					_lfsrFrequency = 0;
				}
			}
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
