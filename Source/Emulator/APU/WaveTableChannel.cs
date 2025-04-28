using NAudio.Wave.SampleProviders;

namespace GBSharp
{
	// A wave table generator for sound channel 3.
	internal class WaveTableProvider : SampleProvider
	{
		public WaveTableProvider()
		{
			BuildWaveform(new byte[16]);
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
	}

	// Sound channel 3 is a user-defined wave channel.
	internal class WaveTableChannel : Channel
	{
		// TODO: Use the correct wave generator.
		private readonly WaveTableProvider _waveTableProvider = new();

		// Is sound output enabled? (NR30, 0xFF1A)
		public bool SoundEnabled = false;

		// The output level. (NR32, 0xFF1C)
		private byte _outputLevel = 0;
		private float _outputLevelAsFloat = 0.0f;

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
			if (APU.Instance.Mute || APU.Instance.MuteChannels[2] || !APU.Instance.IsOn() || !SoundOn || !SoundEnabled)
			{
				_waveTableProvider._leftVolume = 0.0f;
				_waveTableProvider._rightVolume = 0.0f;
			}
			else
			{
				// Set volume levels.
				_waveTableProvider._leftVolume = APU.Instance.Channel3LeftOn ? _outputLevelAsFloat : 0.0f;
				_waveTableProvider._rightVolume = APU.Instance.Channel3RightOn ? _outputLevelAsFloat : 0.0f;

				// Update the frequency.
				uint frequencyData = LowOrderFrequencyData + (HighOrderFrequencyData << 8);
				float periodValue = 2048 - frequencyData;
				float newFrequency = 65536 / periodValue;
				_waveTableProvider._frequency = newFrequency;
			}

			// Fill the audio buffer with the latest wave table data.
			_waveTableProvider.FillAudioBuffer();
		}

		public byte GetOutputLevel()
		{
			return _outputLevel;
		}

		public void SetOutputLevel(byte outputLevel)
		{
			_outputLevel = outputLevel;
			switch (outputLevel)
			{
				case 0x00:
					_outputLevelAsFloat = 0.0f;
					break;

				case 0x01:
					_outputLevelAsFloat = 1.0f;
					break;

				case 0x02:
					_outputLevelAsFloat = 0.5f;
					break;

				case 0x03:
					_outputLevelAsFloat = 0.25f;
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
