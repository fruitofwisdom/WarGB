namespace GBSharp
{
	internal class Sound
	{
		public bool AllSoundOn;
		public bool Sound1On;
		public bool Sound2On;
		public bool Sound3On;
		public bool Sound4On;

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
			AllSoundOn = false;
			Sound1On = false;
			Sound2On = false;
			Sound3On = false;
			Sound4On = false;
		}
	}
}
