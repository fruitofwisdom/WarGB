using NAudio.Wave;

namespace GBSharp
{
	internal class APU
	{
		// An emulator option.
		public bool Mute = false;
		public bool[] MuteChannels = { false, false, false, false };

		// Master volume and vin output. (NR50, 0xFF24)
		// NOTE: Vin output is external hardware.
		public bool VinLeftOn = false;
		public uint LeftOutputVolume = 0;
		public bool VinRightOn = false;
		public uint RightOutputVolume = 0;

		// Maps sound channels to terminals (speakers). (NR51, 0xFF25)
		public bool Channel1LeftOn = false;
		public bool Channel2LeftOn = false;
		public bool Channel3LeftOn = false;
		public bool Channel4LeftOn = false;
		public bool Channel1RightOn = false;
		public bool Channel2RightOn = false;
		public bool Channel3RightOn = false;
		public bool Channel4RightOn = false;

		// Master audio enabled, but each channel has their own register. (NR52, 0xFF26)
		private bool _allSoundOn = false;

		public Channel[] Channels = new Channel[4];

		private static APU? _instance;
		public static APU Instance
		{
			get
			{
				_instance ??= new APU();
				return _instance;
			}
		}

		public APU()
		{
			Reset();
		}

		public void Reset()
		{
			// Retain emulator options.
			//Mute = false;
			//MuteChannels = { false, false, false, false };

			VinLeftOn = false;
			LeftOutputVolume = 0;
			VinRightOn = false;
			RightOutputVolume = 0;

			Channel1LeftOn = false;
			Channel2LeftOn = false;
			Channel3LeftOn = false;
			Channel4LeftOn = false;
			Channel1RightOn = false;
			Channel2RightOn = false;
			Channel3RightOn = false;
			Channel4RightOn = false;

			_allSoundOn = false;

			Channels[0] = new PulseWaveChannel(true);
			Channels[1] = new PulseWaveChannel();
			Channels[2] = new WaveTableChannel();
			Channels[3] = new NoiseGeneratorChannel();

			Stop();
		}

		public void Play()
		{
			foreach (Channel channel in Channels)
			{
				channel.Play();
			}
		}

		public void Stop()
		{
			foreach (Channel channel in Channels)
			{
				channel.Stop();
			}
		}

		public void UpdateDiv(ushort divApu)
		{
			foreach (Channel channel in Channels)
			{
				channel.UpdateDiv(divApu);
			}
		}

		public void Update()
		{
			foreach (Channel channel in Channels)
			{
				channel.Update();
			}
		}

		public void On()
		{
			_allSoundOn = true;
		}

		public void Off()
		{
			Reset();
		}

		public bool IsOn()
		{
			return _allSoundOn;
		}

		public byte GetSoundOutputTerminals()
		{
			byte soundOutputTerminals = 0x00;

			soundOutputTerminals |= (byte)(Channel1LeftOn ? 0x10 : 0x00);
			soundOutputTerminals |= (byte)(Channel1RightOn ? 0x01 : 0x00);
			soundOutputTerminals |= (byte)(Channel2LeftOn ? 0x20 : 0x00);
			soundOutputTerminals |= (byte)(Channel2RightOn ? 0x02 : 0x00);
			soundOutputTerminals |= (byte)(Channel3LeftOn ? 0x40 : 0x00);
			soundOutputTerminals |= (byte)(Channel3RightOn ? 0x04 : 0x00);
			soundOutputTerminals |= (byte)(Channel4LeftOn ? 0x80 : 0x00);
			soundOutputTerminals |= (byte)(Channel4RightOn ? 0x08 : 0x00);

			return soundOutputTerminals;
		}

		public void SetSoundOutputTerminals(byte soundOutputTerminals)
		{
			Channel1LeftOn = Utilities.GetBoolFromByte(soundOutputTerminals, 4);
			Channel1RightOn = Utilities.GetBoolFromByte(soundOutputTerminals, 0);
			Channel2LeftOn = Utilities.GetBoolFromByte(soundOutputTerminals, 5);
			Channel2RightOn = Utilities.GetBoolFromByte(soundOutputTerminals, 1);
			Channel3LeftOn = Utilities.GetBoolFromByte(soundOutputTerminals, 6);
			Channel3RightOn = Utilities.GetBoolFromByte(soundOutputTerminals, 2);
			Channel4LeftOn = Utilities.GetBoolFromByte(soundOutputTerminals, 7);
			Channel4RightOn = Utilities.GetBoolFromByte(soundOutputTerminals, 3);
		}
	}

	// The base class for our sound channels.
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

		public abstract void Update();
	}

	// The base class for our sample providers.
	internal abstract class SampleProvider : ISampleProvider
	{
		public WaveFormat WaveFormat { get; private set; }

		// HACK: This sample rate avoids some timing issues?
		protected const int kSampleRate = 48000;
		protected readonly float[] _waveTable;
		private float _phase = 0.0f;

		// The circular audio buffer holds five seconds of two-channel audio.
		private const int kAudioBufferSize = kSampleRate * 2 * 5;
		private readonly float[] _audioBuffer;
		private int _bytesPending = 2;		// HACK: Can't start reading 0 bytes?
		private int _readPosition = 0;
		private int _writePosition = 0;

		// Anything louder than this is too loud.
		protected const float kMaxVolume = 0.2f;
		public float _frequency = 1000.0f;
		public float _leftVolume = 0.0f;
		public float _rightVolume = 0.0f;

		public SampleProvider()
		{
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(kSampleRate, 2);
			_waveTable = new float[kSampleRate];
			_audioBuffer = new float[kAudioBufferSize];
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int read = 0;

			// Fill the NAudio output buffer.
			for (int i = 0; i < count && _bytesPending > 0; i += 2)
			{
				buffer[i + offset] = _audioBuffer[_readPosition];
				buffer[i + offset + 1] = _audioBuffer[_readPosition + 1];
				read += 2;

				_bytesPending -= 2;
				_readPosition += 2;
				if (_readPosition >= _audioBuffer.Length)
				{
					_readPosition -= _audioBuffer.Length;
				}
			}

			// If we didn't read enough bytes, the audio is stalled.
			if (read < count)
			{
				// TODO: Fix this somehow?
				//GameBoy.DebugOutput += "Audio buffer starved after " + read + " bytes. Wanted " + count + ".\n";
			}

			return read;
		}

		public void FillAudioBuffer()
		{
			// Update the phase step based on frequency.
			float phaseStep = _waveTable.Length * (_frequency / WaveFormat.SampleRate);

			// Fill the circular audio buffer.
			for (int i = 0; i < (int)(kSampleRate / GameBoy.kFps); ++i)
			{
				int waveTableIndex = (int)_phase % _waveTable.Length;
				float leftVolume = _leftVolume * (APU.Instance.LeftOutputVolume / 7.0f) * kMaxVolume;
				_audioBuffer[_writePosition] = _waveTable[waveTableIndex] * leftVolume;
				float rightVolume = _rightVolume * (APU.Instance.RightOutputVolume / 7.0f) * kMaxVolume;
				_audioBuffer[_writePosition + 1] = _waveTable[waveTableIndex] * rightVolume;
				float leftValue = _audioBuffer[_writePosition];
				float rightValue = _audioBuffer[_writePosition + 1];

				_phase += phaseStep;
				while (_phase >= _waveTable.Length)
				{
					_phase -= _waveTable.Length;
				}

				_bytesPending += 2;
				_writePosition += 2;
				if (_writePosition >= _audioBuffer.Length)
				{
					_writePosition -= _writePosition;
				}
			}
		}
	}
}
