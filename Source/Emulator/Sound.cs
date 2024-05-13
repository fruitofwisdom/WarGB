namespace GBSharp
{
	internal class Sound
	{
		public bool AllSoundOn;

		public Channel[] Channels = new Channel[4];

		// Maps sound channels to terminals (speakers). (NR51, 0xFF25)
		public byte SoundOutputTerminals;

		private static Sound? _instance;
		public static Sound Instance
		{
			get
			{
				_instance ??= new Sound();
				return _instance;
			}
		}

		public Sound()
		{
			Reset();
		}

		public void Reset()
		{
			AllSoundOn = false;

			Channels[0] = new SquareWave();
			Channels[1] = new SquareWave();
			Channels[2] = new WaveTable();
			Channels[3] = new NoiseGenerator();

			SoundOutputTerminals = 0x00;
		}
	}
}
