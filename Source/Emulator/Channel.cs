using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GBSharp
{
	internal abstract class Channel
	{
		public bool SoundOn = false;

		// The actual sound output device.
		protected WaveOutEvent _waveOut = new();

		// Anything louder than this is too loud.
		protected float _maxVolume = 0.2f;

		~Channel()
		{
			_waveOut.Dispose();
		}

		public void Play()
		{
			_waveOut.Play();
		}

		public void Stop()
		{
			_waveOut.Stop();
		}

		public abstract void Update();
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

	// Sound channels 1 and 2 are a rectangular, "pulse" wave.
	internal class PulseWave : Channel
	{
		private readonly PulseWaveProvider _pulseWaveProvider = new();

		// The sweep settings. (NR10, 0xFF10)
		public byte SweepTime = 0x00;
		public byte SweepIncDec = 0x00;
		public byte SweepShiftNumber = 0x00;

		// The waveform duty and sound length timer. (NR11 and NR21, 0xFF11 and 0xFF21)
		private uint _lastWaveformDuty = 0;
		public uint WaveformDuty = 0;
		public byte SoundLength = 0x00;

		// The envelope settings. (NR12 and NR22, 0xFF12 and 0xFF22)
		public uint DefaultEnvelopeValue = 0;
		public byte EnvelopeUpDown = 0x00;
		public byte LengthOfEnvelopeSteps = 0x00;

		// The low-order frequency period. (NR13 and NR23, 0xFF13 and 0xFF23)
		public uint LowOrderFrequencyData = 0;

		// The high-order frequency period and other settings. (NR14 and NR24, 0xFF14 and 0xFF24)
		public bool Initialize = false;
		public bool CounterContinuousSelection = false;
		public uint HighOrderFrequencyData = 0;

		public PulseWave()
		{
			_waveOut.Init(new SampleToWaveProvider(_pulseWaveProvider));
		}

		public override void Update()
		{
			// Are we muted?
			if (!Sound.Instance.AllSoundOn || !SoundOn)
			{
				_pulseWaveProvider._volume = 0.0f;
			}
			else
			{
				_pulseWaveProvider._volume = DefaultEnvelopeValue / 15.0f * _maxVolume;

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
	}

	// Sound channel 3 is a user-defined wave channel.
	internal class WaveTable : Channel
	{
		// TODO: Use the correct wave generator.
		private readonly PulseWaveProvider _pulseWaveProvider = new();

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

			// TODO: Use the correct wave generator.
			_waveOut.Init(new SampleToWaveProvider(_pulseWaveProvider));
			_pulseWaveProvider._volume = 0.0f;
		}

		public override void Update()
		{
			// TODO: Implement.
		}
	}

	// Sound channel 4 is a noise generator.
	internal class NoiseGenerator : Channel
	{
		// TODO: Use the correct wave generator.
		private readonly PulseWaveProvider _pulseWaveProvider = new();

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

		public NoiseGenerator()
		{
			// TODO: Use the correct wave generator.
			_waveOut.Init(new SampleToWaveProvider(_pulseWaveProvider));
			_pulseWaveProvider._volume = 0.0f;
		}

		public override void Update()
		{
			// TODO: Implement.
		}
	}
}
