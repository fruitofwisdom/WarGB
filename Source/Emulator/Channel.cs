using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GBSharp
{
	internal abstract class Channel
	{
		public bool SoundOn = false;

		// Current sound length, initialized from the channels' sound length registers. (NR11, 0xFF11; NR21, 0xFF21; NR31, 0xFF1B; NR41, 0xFF20)
		protected uint _soundLengthTimer = 0;
		private const uint kSoundLengthTime = 0;

		// Other settings. (NR14, 0xFF14; NR24, 0xFF24; NR34, 0xFF1E; NR44, 0xFF23)
		public bool CounterContinuousSelection = false;

		// The actual sound output device.
		protected WaveOutEvent _waveOut = new();

		// Anything louder than this is too loud.
		protected const float kMaxVolume = 0.2f;

		~Channel()
		{
			_waveOut.Dispose();
		}

		public void Initialize()
		{
			SoundOn = true;
		}

		public void SetSoundLength(uint soundLength)
		{
			_soundLengthTimer = soundLength;
		}

		public void Play()
		{
			_waveOut.Play();
		}

		public void Stop()
		{
			_waveOut.Stop();
		}

		public virtual void UpdateDiv(ushort divApu)
		{
			// DIV-APU runs at 512Hz, sound length at 256Hz
			if (divApu % 2 == 0)
			{
				// Update length timer.
				if (CounterContinuousSelection)
				{
					_soundLengthTimer++;
					if (_soundLengthTimer == kSoundLengthTime)
					{
						SoundOn = false;
					}
				}
			}
		}

		public virtual void Update()
		{
			;
		}
	}

	// A pulse wave generator for sound channels 1 and 2.
	internal class PulseWaveProvider : ISampleProvider
	{
		public WaveFormat WaveFormat { get; private set; }

		private const int kSampleRate = 44100; 
		private readonly float[] _waveTable;

		public float _frequency = 1000.0f;
		private float _phase = 0.0f;
		private float _phaseStep = 0.0f;
		public float _volume = 0.0f;

		public PulseWaveProvider()
		{
			// Build the shape of the rectangular waveform.
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(kSampleRate, 1);
			_waveTable = new float[kSampleRate];
			BuildWaveform(0);
		}

		public void BuildWaveform(uint waveformDuty)
		{
			for (int i = 0; i < kSampleRate; i++)
			{
				if (waveformDuty == 0)
				{
					_waveTable[i] = i > kSampleRate / 8 ? 1.0f : 0.0f;
				}
				else if (waveformDuty == 1)
				{
					_waveTable[i] = i > kSampleRate / 4 ? 1.0f : 0.0f;
				}
				else if (waveformDuty == 2)
				{
					_waveTable[i] = i > kSampleRate / 2 ? 1.0f : 0.0f;
				}
				else if (waveformDuty == 3)
				{
					_waveTable[i] = i > kSampleRate * 0.75f ? 1.0f : 0.0f;
				}
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

	// TODO: Other generators for sound channels 3 and 4.

	// Sound channels 1 and 2 are a rectangular, "pulse" wave. Note that channel 2 doesn't support sweep.
	internal class PulseWave : Channel
	{
		private readonly PulseWaveProvider _pulseWaveProvider = new();

		// The sweep settings. (NR10, 0xFF10)
		private readonly bool _sweepEnabled = false;
		public byte SweepTime = 0x00;
		public byte SweepIncDec = 0x00;
		public byte SweepShiftNumber = 0x00;

		// The waveform duty. (NR11 and NR21, 0xFF11 and 0xFF21)
		private uint _lastWaveformDuty = 0;
		public uint WaveformDuty = 0;

		// The envelope settings. (NR12 and NR22, 0xFF12 and 0xFF22)
		private uint _currentEnvelopeValue = 0;
		public uint DefaultEnvelopeValue { get; private set; } = 0;
		public bool EnvelopeUpDown = false;
		private uint _currentEnvelopeStep = 0;
		public uint LengthOfEnvelopeSteps { get; private set; } = 0;

		// The low-order frequency period. (NR13 and NR23, 0xFF13 and 0xFF23)
		public uint LowOrderFrequencyData = 0;

		// The high-order frequency period. (NR14 and NR24, 0xFF14 and 0xFF24)
		public uint HighOrderFrequencyData = 0;

		public PulseWave(bool sweepEnabled = false)
		{
			_sweepEnabled = sweepEnabled;
			_waveOut.Init(new SampleToWaveProvider(_pulseWaveProvider));
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

			// DIV-APU runs at 512Hz, frequency sweep at 128Hz.
			if (divApu % 4 == 0)
			{
				// TODO: Implement sweep support.
			}
		}

		public override void Update()
		{
			// Are we muted?
			if (!Sound.Instance.IsOn() ||
				// TODO: Support stereo sound.
				/*
				(_sweepEnabled && !Sound.Instance.Channel1LeftOn) ||
				(_sweepEnabled && !Sound.Instance.Channel1RightOn) ||
				(!_sweepEnabled && !Sound.Instance.Channel2LeftOn) ||
				(!_sweepEnabled && !Sound.Instance.Channel2RightOn) ||
				*/
				!SoundOn)
			{
				_pulseWaveProvider._volume = 0.0f;
			}
			else
			{
                // TODO: Support stereo sound.
                _pulseWaveProvider._volume =
					Sound.Instance.LeftOutputVolume / 7.0f * _currentEnvelopeValue / 15.0f *
					kMaxVolume;

				// If the shape of the waveform has changed, rebuild it.
				if (WaveformDuty != _lastWaveformDuty)
				{
					_pulseWaveProvider.BuildWaveform(WaveformDuty);
					_lastWaveformDuty = WaveformDuty;
				}

				uint frequencyData = LowOrderFrequencyData + (HighOrderFrequencyData << 8);
				float periodValue = 2048 - frequencyData;
				float newFrequency = 131072 / periodValue;
				_pulseWaveProvider._frequency = newFrequency;
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

	// Sound channel 3 is a user-defined wave channel.
	internal class WaveTable : Channel
	{
		// TODO: Use the correct wave generator.
		private readonly PulseWaveProvider _pulseWaveProvider = new();

		// Is sound output enabled? (NR30, 0xFF1A)
		public bool SoundEnabled = false;

		// The output level. (NR32, 0xFF1C)
		public byte OutputLevel = 0x00;

		// The low-order frequency period. (NR33, 0xFF1D)
		public byte LowOrderFrequencyData = 0x00;

		// The high-order frequency period. (NR34, 0xFF1E)
		public byte HighOrderFrequencyData = 0x00;

		// The composition of the waveform. (0xFF30 through 0xFF3F)
		public byte[] WaveformRAM;

		public WaveTable()
		{
			WaveformRAM = new byte[16];

			// TODO: Use the correct wave generator.
			_waveOut.Init(new SampleToWaveProvider(_pulseWaveProvider));
			_pulseWaveProvider._volume = 0.0f;
		}

		public override void Update()
		{
			// Are we muted?
			if (!Sound.Instance.IsOn() ||
				// TODO: Support stereo sound.
				//!Sound.Instance.Channel3LeftOn ||
				//!Sound.Instance.Channel3RightOn ||
				!SoundOn)
			{
				_pulseWaveProvider._volume = 0.0f;
			}
			else
			{
				// TODO: Implementation.
				_pulseWaveProvider._volume = 0.0f;
			}
		}
	}

	// Sound channel 4 is a noise generator.
	internal class NoiseGenerator : Channel
	{
		// TODO: Use the correct wave generator.
		private readonly PulseWaveProvider _pulseWaveProvider = new();

		// The envelope settings. (NR42, 0xFF21)
		public byte DefaultEnvelopeValue = 0x00;
		public byte EnvelopeUpDown = 0x00;
		public byte LengthOfEnvelopeSteps = 0x00;

		// The frequency settings. (NR43, 0xFF22)
		public byte ShiftClockFrequency = 0x00;
		public bool CounterSteps = false;
		public byte DivisionRatioFrequency = 0x00;

		public NoiseGenerator()
		{
			// TODO: Use the correct wave generator.
			_waveOut.Init(new SampleToWaveProvider(_pulseWaveProvider));
			_pulseWaveProvider._volume = 0.0f;
		}

		public override void Update()
		{
			// Are we muted?
			if (!Sound.Instance.IsOn() ||
				// TODO: Support stereo sound.
				//!Sound.Instance.Channel4LeftOn ||
				//!Sound.Instance.Channel4RightOn ||
				!SoundOn)
			{
				_pulseWaveProvider._volume = 0.0f;
			}
			else
			{
				// TODO: Implementation.
				_pulseWaveProvider._volume = 0.0f;
			}
		}
	}
}
