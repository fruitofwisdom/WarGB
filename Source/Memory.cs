namespace GBSharp
{
	internal class Memory
	{
		private byte[] VRAM;
		private byte[] ExternalRAM;
		private byte[] WRAMBank0;       // work RAM bank 0
		private byte[] WRAMBank1;
		private byte[] OAM;             // sprite attribute table
		private byte[] IOPorts;
		private byte[] HRAM;            // high RAM
		private bool InterruptEnable;   // register

		private static Memory? _instance;
		public static Memory Instance
		{
			get
			{
				_instance ??= new Memory();
				return _instance;
			}
		}

		public Memory()
		{
			VRAM = new byte[8 * 1024];
			ExternalRAM = new byte[8 * 1024];
			WRAMBank0 = new byte[4 * 1024];
			WRAMBank1 = new byte[4 * 1024];
			OAM = new byte[159];
			IOPorts = new byte[127];
			HRAM = new byte[126];
			InterruptEnable = false;
		}

		public byte Read(uint address)
		{
			byte data = 0x00;

			if (address >= 0x0000 && address <= 0x3FFF)
			{
				if (ROM.Instance.Data is not null)
				{
					data = ROM.Instance.Data[address];
				}
			}
			else if (address >= 0x4000 && address <= 0x7FFF)
			{
				// TODO: Handle switchable ROM banks.
			}
			else if (address >= 0x8000 && address <= 0x9FFF)
			{
				data = VRAM[address - 0x8000];
			}
			else if (address >= 0xA000 && address <= 0xBFFF)
			{
				data = ExternalRAM[address - 0xA000];
			}
			else if (address >= 0xC000 && address <= 0xCFFF)
			{
				data = WRAMBank0[address - 0xC000];
			}
			else if (address >= 0xD000 && address <= 0xDFFF)
			{
				data = WRAMBank1[address - 0xD000];
			}
			else if (address >= 0xE000 && address <= 0xEFFF)
			{
				// NOTE: ECHO memory that maps to WRAMBank0
				data = WRAMBank0[address - 0xE000];
			}
			else if (address >= 0xF000 && address <= 0xFDFF)
			{
				// NOTE: ECHO memory that maps to WRAMBank1
				data = WRAMBank1[address - 0xF000];
			}
			else if (address >= 0xFE00 && address <= 0xFE9F)
			{
				data = OAM[address - 0xFE00];
			}
			else if (address >= 0xFEA0 && address <= 0xFEFF)
			{
				// TODO: Not usable.
			}
			else if (address >= 0xFF00 && address <= 0xFF7F)
			{
				data = IOPorts[address - 0xFF00];
			}
			else if (address >= 0xFF80 && address <= 0xFFFE)
			{
				data = HRAM[address - 0xFF80];
			}
			else if (address == 0xFFFF)
			{
				data = InterruptEnable ? (byte)0x01 : (byte)0x00;
			}

			return data;
		}

		public bool Write(int address, byte data)
		{
			bool wrote = false;

			if (address >= 0x0000 && address <= 0x7FFF)
			{
				// TODO: Can't write to ROM.
			}
			else if (address >= 0x8000 && address <= 0x9FFF)
			{
				VRAM[address - 0x8000] = data;
			}
			else if (address >= 0xA000 && address <= 0xBFFF)
			{
				ExternalRAM[address - 0xA000] = data;
			}
			else if (address >= 0xC000 && address <= 0xCFFF)
			{
				WRAMBank0[address - 0xC000] = data;
			}
			else if (address >= 0xD000 && address <= 0xDFFF)
			{
				WRAMBank1[address - 0xD000] = data;
			}
			else if (address >= 0xE000 && address <= 0xEFFF)
			{
				// NOTE: ECHO memory that maps to WRAMBank0
				WRAMBank0[address - 0xE000] = data;
			}
			else if (address >= 0xF000 && address <= 0xFDFF)
			{
				// NOTE: ECHO memory that maps to WRAMBank1
				WRAMBank1[address - 0xF000] = data;
			}
			else if (address >= 0xFE00 && address <= 0xFE9F)
			{
				OAM[address - 0xFE00] = data;
			}
			else if (address >= 0xFEA0 && address <= 0xFEFF)
			{
				// TODO: Not usable.
			}
			else if (address >= 0xFF00 && address <= 0xFF7F)
			{
				// TODO: Can't write to I/O ports?
				IOPorts[address - 0xFF00] = data;
			}
			else if (address >= 0xFF80 && address <= 0xFFFE)
			{
				HRAM[address - 0xFF80] = data;
			}
			else if (address == 0xFFFF)
			{
				// TODO: Is this possible?
				InterruptEnable = data == 0x01;
			}

			return wrote;
		}
	}
}
