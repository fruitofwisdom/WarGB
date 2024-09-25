namespace GBSharp
{
	internal class APU
	{
		// Master volume and stereo output. (NR50, 0xFF24)
		// TODO: Support stereo sound.
		public bool LeftOutputOn = false;
		public uint LeftOutputVolume = 0;
		public bool RightOutputOn = false;
		public uint RightOutputVolume = 0;

		// Maps sound channels to terminals (speakers). (NR51, 0xFF25)
		// TODO: Support stereo sound.
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
			// TODO: Support stereo sound.
			LeftOutputOn = false;
			LeftOutputVolume = 0;
			RightOutputOn = false;
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
			Channel1LeftOn = Utilities.GetBitsFromByte(soundOutputTerminals, 4, 4) != 0x00;
			Channel1RightOn = Utilities.GetBitsFromByte(soundOutputTerminals, 0, 0) != 0x00;
			Channel2LeftOn = Utilities.GetBitsFromByte(soundOutputTerminals, 5, 5) != 0x00;
			Channel2RightOn = Utilities.GetBitsFromByte(soundOutputTerminals, 1, 1) != 0x00;
			Channel3LeftOn = Utilities.GetBitsFromByte(soundOutputTerminals, 6, 6) != 0x00;
			Channel3RightOn = Utilities.GetBitsFromByte(soundOutputTerminals, 2, 2) != 0x00;
			Channel4LeftOn = Utilities.GetBitsFromByte(soundOutputTerminals, 7, 7) != 0x00;
			Channel4RightOn = Utilities.GetBitsFromByte(soundOutputTerminals, 3, 3) != 0x00;
		}
	}
}
