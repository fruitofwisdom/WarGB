namespace GBSharp
{
	internal class ROM
	{
		public enum CartridgeTypes
		{
			ROM,
			MBC1,
			MBC1_RAM,
			MBC1_RAM_BATTERY,
			UNKNOWN_04,
			MBC2,
			MBC2_BATTERY,
			UNKNOWN_07,
			ROM_RAM,
			ROM_RAM_BATTERY,
			UNKNOWN_0A,
			MMM01,
			MMM01_RAM,
			MMM01_RAM_BATTERY,
			UNKNOWN_0E,
			MBC3_TIMER_BATTERY,
			MBC3_TIMER_RAM_BATTERY,
			MBC3,
			MBC3_RAM,
			MBC3_RAM_BATTERY,
			UNKNOWN_14,
			UNKNOWN_15,
			UNKNOWN_16,
			UNKNOWN_17,
			UNKNOWN_18,
			MBC5,
			MBC5_RAM,
			MBC5_RAM_BATTERY,
			MBC5_RUMBLE,
			MBC5_RUMBLE_RAM,
			MBC5_RUMBLE_RAM_BATTERY,
			UNKNOWN_1F,
			MBC6,
			UNKNOWN_21,
			MBC7_SENSOR_RUMBLE_RAM_BATTERY
			// TODO: Support POCKET_CAMERA, BANDAI_TAMA5, HuC3, HuC1_RAM_BATTERY?
		}

		public byte[]? Data {  get; private set; }
		public string Filename { get; private set; }

		// ROM header and other information.
		public string Title { get; private set; }
		public bool CGBCompatible { get; private set; }
		public bool CGBOnly { get; private set; }
		public bool SGBCompatible { get; private set; }
		public CartridgeTypes CartridgeType { get; private set; }
		public uint ROMSize { get; private set; }
		public uint RAMSize { get; private set; }

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
			Filename = "none";
			Title = "unknown";
			CGBCompatible = false;
			CGBOnly = false;
			SGBCompatible = false;
			CartridgeType = CartridgeTypes.ROM;
			ROMSize = 32768;
			RAMSize = 0;
		}

		public bool Load(string romFilename)
		{
			bool loaded = false;

			try
			{
				Data = File.ReadAllBytes(romFilename);
				Filename = Path.GetFileNameWithoutExtension(romFilename);

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
				else if (Data[0x0143] == 0xC0)
				{
					CGBOnly = true;
				}
				else if (Data[0x0143] != 0x00)
				{
					Title += (char)Data[0x0143];
				}
				if (Data[0x0146] == 0x03)
				{
					SGBCompatible = true;
				}
				CartridgeType = (CartridgeTypes)Data[0x0147];
				ROMSize = 32768 * (uint)(1 << Data[0x0148]);
				switch (Data[0x0149])
				{
					case 0x00:
						RAMSize = 0;
						break;

					case 0x01:
						// Unsupported RAM size?
						RAMSize = 2048;
						break;

					case 0x02:
						RAMSize = 8192;
						break;

					case 0x03:
						RAMSize = 32768;
						break;

					case 0x04:
						RAMSize = 131072;
						break;

					case 0x05:
						RAMSize = 65536;
						break;

					default:
						// Unknown RAM size?
						RAMSize = 0;
						break;
				}
				// TODO: Read any more?

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
