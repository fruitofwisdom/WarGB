namespace GBSharp
{
	internal class ROM
	{
		private byte[]? Data = null;
		public string Title { get; private set; }
		public bool CGBCompatible { get; private set; }
		public bool CGBOnly { get; private set; }

		private static ROM? _instance;
		public static ROM Instance
		{
			get
			{
				_instance ??= new ROM();
				return _instance;
			}
		}

		public ROM()
		{
			Title = "unknown";
			CGBCompatible = false;
			CGBOnly = false;
		}

		public bool Load(string romFilename)
		{
			bool loaded = false;

			try
			{
				Data = File.ReadAllBytes(romFilename);

				// Read in the title from the ROM (and CGB compatibility).
				Title = "";
				for (int i = 0; i < 15; i++)
				{
					if (Data[0x0134 + i] != 0x00)
					{
						Title += (char)Data[0x0134 + i];
					}
				}
				// NOTE: The last byte of the title may be a CGB compatibility flag.
				if (Data[0x0143] == 0x80)
				{
					CGBCompatible = true;
				}
				else if (Data[0x0143] == 0xc0)
				{
					CGBOnly = true;
				}
				else if (Data[0x0143] != 0x00)
				{
					Title += (char)Data[0x0143];
				}
				// TODO: Read the rest.

				loaded = true;
			}
			catch (System.Exception)
			{
				// TODO: Error handling.
			}

			return loaded;
		}
	}
}
