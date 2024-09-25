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

	// Sound channel 3 is a user-defined wave channel.
	internal class WaveTableChannel : Channel
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

		public WaveTableChannel()
		{
			WaveformRAM = new byte[16];

			// TODO: Use the correct wave generator.
			_waveOut.Init(new SampleToWaveProvider(_pulseWaveProvider));
			_pulseWaveProvider._volume = 0.0f;
		}

		public override void Update()
		{
			// Are we muted?
			if (!APU.Instance.IsOn() ||
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
	internal class NoiseGeneratorChannel : Channel
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

		public NoiseGeneratorChannel()
		{
			// TODO: Use the correct wave generator.
			_waveOut.Init(new SampleToWaveProvider(_pulseWaveProvider));
			_pulseWaveProvider._volume = 0.0f;
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
