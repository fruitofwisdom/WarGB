using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GBSharp
{
	// A wave table generator for sound channel 3.
	internal class WaveTableProvider : ISampleProvider
	{
		public WaveFormat WaveFormat { get; private set; }

		private const int kSampleRate = 44100;
		private readonly float[] _waveTable;

		public float _frequency = 1000.0f;
		private float _phase = 0.0f;
		private float _phaseStep = 0.0f;
		public float _volume = 0.0f;

		public WaveTableProvider()
		{
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(kSampleRate, 1);
			_waveTable = new float[kSampleRate];
		}

		public void BuildWaveform(byte[] waveformRAM)
		{
			// Convert the waveform RAM into 32 steps.
			float[] waveTableFrequencies = new float[32];
			for (int i = 0; i < waveformRAM.Length; i++)
			{
				waveTableFrequencies[i * 2] = ((waveformRAM[i] & 0xF0) >> 4) / 15.0f;
				waveTableFrequencies[i * 2 + 1] = (waveformRAM[i] & 0x0F) / 15.0f;
			}

			// Fill the wave table from these steps.
			for (int i = 0; i < kSampleRate; i++)
			{
				// NOTE: Divide the sample rate into 32 steps.
				int index = i / (kSampleRate / 32);
				index = Math.Min(index, 31);
				_waveTable[i] = waveTableFrequencies[index];
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

	// Sound channel 3 is a user-defined wave channel.
	internal class WaveTableChannel : Channel
	{
		// TODO: Use the correct wave generator.
		private readonly WaveTableProvider _waveTableProvider = new();

		// Is sound output enabled? (NR30, 0xFF1A)
		public bool SoundEnabled = false;

		// The output level. (NR32, 0xFF1C)
		private float _outputLevel = 0.0f;

		// The low-order frequency period. (NR33, 0xFF1D)
		public uint LowOrderFrequencyData = 0;

		// The high-order frequency period. (NR34, 0xFF1E)
		public uint HighOrderFrequencyData = 0;

		// The composition of the waveform. (0xFF30 through 0xFF3F)
		private byte[] _waveformRAM;

		public WaveTableChannel()
		{
			_waveformRAM = new byte[16];
			_waveOut.Init(new SampleToWaveProvider(_waveTableProvider));
		}

		public override void Update()
		{
			// Are we muted?
			if (!APU.Instance.IsOn() ||
				// TODO: Support stereo sound.
				//!Sound.Instance.Channel3LeftOn ||
				//!Sound.Instance.Channel3RightOn ||
				!SoundOn || !SoundEnabled)
			{
				_waveTableProvider._volume = 0.0f;
			}
			else
			{
				_waveTableProvider._volume =
					APU.Instance.LeftOutputVolume / 7.0f * _outputLevel *
					kMaxVolume;

				uint frequencyData = LowOrderFrequencyData + (HighOrderFrequencyData << 8);
				float periodValue = 2048 - frequencyData;
				float newFrequency = 65536 / periodValue;
				_waveTableProvider._frequency = newFrequency;
			}
		}

		public void SetOutputLevel(byte outputLevel)
		{
			switch (outputLevel)
			{
				case 0x00:
					_outputLevel = 0.0f;
					break;

				case 0x01:
					_outputLevel = 1.0f;
					break;

				case 0x02:
					_outputLevel = 0.5f;
					break;

				case 0x03:
					_outputLevel = 0.25f;
					break;

				default:
					// Do nothing.
					break;
			}
		}

		public void SetWaveformRAM(int address, byte waveformRAM)
		{
			_waveformRAM[address] = waveformRAM;
			_waveTableProvider.BuildWaveform(_waveformRAM);
		}
	}
}
