namespace GBSharp
{
	internal class Memory
	{
		// Character data, BG display data, etc. - 0x8000 to 0x9FFF
		private byte[] VRAM;
		// External expansion working RAM - 0xA000 to 0xBFFF
		private byte[] ExternalRAM;
		// Unit working RAM - 0xC000 to 0xDFFF
		private byte[] WRAMBank0;
		// TODO: Support bank switching for CGB.
		private byte[] WRAMBank1;
		private byte[] OAM;             // sprite attribute table
		// Port/mode registers, control register, and sound register - 0xFF00 to 0xFF7F
		private byte[] IOPorts;
		// Working and stack RAM - OxFF80 to 0xFFFE
		private byte[] HRAM;            // high RAM

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
			// TODO: Support VRAM of 16 KB for CGB via bank switching.
			VRAM = new byte[8 * 1024];
			ExternalRAM = new byte[8 * 1024];
			WRAMBank0 = new byte[4 * 1024];
			// TODO: Support bank switching for CGB.
			WRAMBank1 = new byte[4 * 1024];
			OAM = new byte[159];
			IOPorts = new byte[127];
			HRAM = new byte[127];
		}

		public byte Read(int address)
		{
			byte data = 0x00;

			// NOTE: address should be a ushort, but an int is cleaner in C#.
			if (address < 0 || address > 0xFFFF)
			{
				return data;
			}

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
				// TODO: Support VRAM of 16 KB for CGB via bank switching.
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
				// TODO: Support WRAMBank1 switching for CGB.
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
				MainForm.PrintDebugMessage($"Reading from unusable memory: 0x{address:X4}!\n");
			}
			else if (address >= 0xFF00 && address <= 0xFF7F)
			{
				data = IOPorts[address - 0xFF00];

				if (address == 0xFF0F)
				{
					data = CPU.Instance.IF;
				}
				if (address == 0xFF44)
				{
					data = CPU.Instance.LY;
				}
				// TODO: The other registers.
				else
				{
					MainForm.PrintDebugMessage($"Unimplemented register: 0x{address:X4}!\n");
				}
			}
			else if (address >= 0xFF80 && address <= 0xFFFE)
			{
				data = HRAM[address - 0xFF80];
			}
			else if (address == 0xFFFF)
			{
				data = CPU.Instance.IE;
			}

			return data;
		}

		public bool Write(int address, byte data)
		{
			bool wrote = false;

			// NOTE: address should be a ushort, but an int is cleaner in C#.
			if (address < 0 || address > 0xFFFF)
			{
				return wrote;
			}

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
				MainForm.PrintDebugMessage($"Writing to unusable memory: 0x{address:X4}!\n");
			}
			else if (address >= 0xFF00 && address <= 0xFF7F)
			{
				IOPorts[address - 0xFF00] = data;

				if (address == 0xFF0F)
				{
					CPU.Instance.IF = data;
				}
				else if (address == 0xFF44)
				{
					MainForm.PrintDebugMessage($"Register 0xFF44 is read-only!\n");
				}
				// TODO: The other registers.
				else
				{
					MainForm.PrintDebugMessage($"Unimplemented register: 0x{address:X4}!\n");
				}
			}
			else if (address >= 0xFF80 && address <= 0xFFFE)
			{
				HRAM[address - 0xFF80] = data;
			}
			else if (address == 0xFFFF)
			{
				CPU.Instance.IE = data;
			}

			return wrote;
		}
	}
}
