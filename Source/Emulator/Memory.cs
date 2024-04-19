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
		private byte[] Registers;
		// Working and stack RAM - OxFF80 to 0xFFFE
		private byte[] HRAM;            // high RAM

		public uint ROMBank { get; private set; }

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
			OAM = new byte[160];
			Registers = new byte[128];
			HRAM = new byte[127];

			Reset();
		}

		// Reset the memory and state.
		public void Reset()
		{
			Array.Clear(VRAM, 0, VRAM.Length);
			Array.Clear(ExternalRAM, 0, ExternalRAM.Length);
			Array.Clear(WRAMBank0, 0, WRAMBank0.Length);
			Array.Clear(WRAMBank1, 0, WRAMBank1.Length);
			Array.Clear(OAM, 0, OAM.Length);
			Array.Clear(Registers, 0, Registers.Length);
			Array.Clear(HRAM, 0, HRAM.Length);

			// NOTE: By default, 0x4000 to 0x7FFF is mapped to bank 1.
			ROMBank = 1;
		}

		public byte Read(int address)
		{
			byte data = 0x00;

			// Ensure ROM data was loaded.
			if (ROM.Instance.Data is null)
			{
				return data;
			}

			// NOTE: address should be a ushort, but an int is cleaner in C#.
			if (address < 0x0000 || address > 0xFFFF)
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
				// NOTE: Bank 1 maps to 0x4000 to 0x7FFF, bank 2 to 0x8000 to 0xBFFF, etc.
				uint bankOffset = (ROMBank - 1) * 0x4000;
				data = ROM.Instance.Data[bankOffset + address];
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
				MainForm.Pause();
			}
			else if (address >= 0xFF00 && address <= 0xFF7F)
			{
				data = Registers[address - 0xFF00];

				if (address == 0xFF00)
				{
					data = Controller.Instance.ReadFromRegister();
				}
				else if (address == 0xFF0F)
				{
					data = CPU.Instance.IF;
				}
				else if (address == 0xFF40)
				{
					data = Graphics.Instance.GetLCDC();
				}
				else if (address == 0xFF41)
				{
					data = Graphics.Instance.GetSTAT();
				}
				else if (address == 0xFF44)
				{
					data = Graphics.Instance.LY;
				}
				else if (address == 0xFF45)
				{
					data = Graphics.Instance.LYC;
				}
				// TODO: The other registers.
				else
				{
					MainForm.PrintDebugMessage($"Reading from unimplemented register: 0x{address:X4}!\n");
					MainForm.Pause();
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

		public void Write(int address, byte data)
		{
			// NOTE: address should be a ushort, but an int is cleaner in C#.
			if (address < 0x0000 || address > 0xFFFF)
			{
				return;
			}

			if (address >= 0x0000 && address <= 0x1FFF)
			{
				// TODO: Enable or disable RAM.
				MainForm.PrintDebugMessage($"Writing to ROM: 0x{address:X4}!\n");
				MainForm.Pause();
			}
			else if (address >= 0x2000 && address <= 0x3FFF)
			{
				// Writing to this address range is a ROM bank select.
				// NOTE: 0x00 is a special case.
				if (data == 0x00)
				{
					ROMBank = 1;
				}
				else
				{
					ROMBank = (uint)data;
				}
			}
			else if (address >= 0x4000 && address <= 0x5FFF)
			{
				// TODO: ROM/RAM bank number.
				MainForm.PrintDebugMessage($"Writing to ROM: 0x{address:X4}!\n");
				MainForm.Pause();
			}
			else if (address >= 0x6000 && address <= 0x7FFF)
			{
				// TODO: ROM/RAM mode select.
				MainForm.PrintDebugMessage($"Writing to ROM: 0x{address:X4}!\n");
				MainForm.Pause();
			}
			else if (address >= 0x8000 && address <= 0x9FFF)
			{
				VRAM[address - 0x8000] = data;
			}
			else if (address >= 0xA000 && address <= 0xBFFF)
			{
				// TODO: Save data to backup RAM (a file).
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
				MainForm.Pause();
			}
			else if (address >= 0xFF00 && address <= 0xFF7F)
			{
				Registers[address - 0xFF00] = data;

				// Actually handle the register changes.
				HandleWriteToRegister(address, data);
			}
			else if (address >= 0xFF80 && address <= 0xFFFE)
			{
				HRAM[address - 0xFF80] = data;
			}
			else if (address == 0xFFFF)
			{
				CPU.Instance.IE = data;
			}
		}

		// Handle the changes from writing to registers.
		private void HandleWriteToRegister(int address, byte data)
		{
			if (address == 0xFF00)
			{
				// NOTE: Unexpectedly, inputs are considered 1 when not selected or pressed.
				Controller.Instance.SelectButtons = Utilities.GetBitsFromByte(data, 5, 5) != 1;
				Controller.Instance.SelectDpad = Utilities.GetBitsFromByte(data, 4, 4) != 1;
			}
			else if (address == 0xFF0F)
			{
				CPU.Instance.IF = data;
			}
			else if (address == 0xFF10)
			{
				byte sweepTime = Utilities.GetBitsFromByte(data, 4, 6);
				byte sweepIncDec = Utilities.GetBitsFromByte(data, 3, 3);
				byte sweepShiftNumber = Utilities.GetBitsFromByte(data, 0, 2);
				((SquareWave)Sound.Instance.Channels[0]).SweepTime = sweepTime;
				((SquareWave)Sound.Instance.Channels[0]).SweepIncDec = sweepIncDec;
				((SquareWave)Sound.Instance.Channels[0]).SweepShiftNumber = sweepShiftNumber;
			}
			else if (address == 0xFF12)
			{
				byte defaultEnvelopeValue = Utilities.GetBitsFromByte(data, 4, 7);
				byte envelopeUpDown = Utilities.GetBitsFromByte(data, 3, 3);
				byte lengthOfEnvelopeSteps = Utilities.GetBitsFromByte(data, 0, 2);
				((SquareWave)Sound.Instance.Channels[0]).DefaultEnvelopeValue = defaultEnvelopeValue;
				((SquareWave)Sound.Instance.Channels[0]).EnvelopeUpDown = envelopeUpDown;
				((SquareWave)Sound.Instance.Channels[0]).LengthOfEnvelopeSteps = lengthOfEnvelopeSteps;
			}
			else if (address == 0xFF13)
			{
				byte lowOrderFrequencyData = data;
				((SquareWave)Sound.Instance.Channels[0]).LowOrderFrequencyData = lowOrderFrequencyData;
			}
			else if (address == 0xFF14)
			{
				byte initialize = Utilities.GetBitsFromByte(data, 7, 7);
				byte counterContinuousSelection = Utilities.GetBitsFromByte(data, 6, 6);
				byte highOrderFrequencyData = Utilities.GetBitsFromByte(data, 0, 2);
				((SquareWave)Sound.Instance.Channels[0]).Initialize = initialize;
				((SquareWave)Sound.Instance.Channels[0]).CounterContinuousSelection = counterContinuousSelection;
				((SquareWave)Sound.Instance.Channels[0]).HighOrderFrequencyData = highOrderFrequencyData;
			}
			else if (address == 0xFF17)
			{
				byte defaultEnvelopeValue = Utilities.GetBitsFromByte(data, 4, 7);
				byte envelopeUpDown = Utilities.GetBitsFromByte(data, 3, 3);
				byte lengthOfEnvelopeSteps = Utilities.GetBitsFromByte(data, 0, 2);
				((SquareWave)Sound.Instance.Channels[1]).DefaultEnvelopeValue = defaultEnvelopeValue;
				((SquareWave)Sound.Instance.Channels[1]).EnvelopeUpDown = envelopeUpDown;
				((SquareWave)Sound.Instance.Channels[1]).LengthOfEnvelopeSteps = lengthOfEnvelopeSteps;
			}
			else if (address == 0xFF18)
			{
				byte lowOrderFrequencyData = data;
				((SquareWave)Sound.Instance.Channels[1]).LowOrderFrequencyData = lowOrderFrequencyData;
			}
			else if (address == 0xFF19)
			{
				byte initialize = Utilities.GetBitsFromByte(data, 7, 7);
				byte counterContinuousSelection = Utilities.GetBitsFromByte(data, 6, 6);
				byte highOrderFrequencyData = Utilities.GetBitsFromByte(data, 0, 5);
				((SquareWave)Sound.Instance.Channels[1]).Initialize = initialize;
				((SquareWave)Sound.Instance.Channels[1]).CounterContinuousSelection = counterContinuousSelection;
				((SquareWave)Sound.Instance.Channels[1]).HighOrderFrequencyData = highOrderFrequencyData;
			}
			else if (address == 0xFF1B)
			{
				// TODO: Implement sound 3 sound length.
			}
			else if (address == 0xFF1C)
			{
				// TODO: Implement sound 3 output levels.
			}
			else if (address == 0xFF1D)
			{
				// TODO: Implement sound 3 frequency data.
			}
			else if (address == 0xFF1E)
			{
				// TODO: Implement other sound 3 settings.
			}
			else if (address == 0xFF21)
			{
				// TODO: Implement sound 4 envelope settings.
			}
			else if (address == 0xFF23)
			{
				// TODO: Implement other sound 4 settings.
			}
			else if (address == 0xFF24)
			{
				// TODO: Implement all sound output levels.
			}
			else if (address == 0xFF25)
			{
				// TODO: Implement sound inputs and outputs.
			}
			else if (address == 0xFF26)
			{
				Sound.Instance.AllSoundOn = Utilities.GetBitsFromByte(data, 7, 7) != 0x00;
				Sound.Instance.Channels[0].SoundOn = Utilities.GetBitsFromByte(data, 0, 0) != 0x00;
				Sound.Instance.Channels[1].SoundOn = Utilities.GetBitsFromByte(data, 1, 1) != 0x00;
				Sound.Instance.Channels[2].SoundOn = Utilities.GetBitsFromByte(data, 2, 2) != 0x00;
				Sound.Instance.Channels[3].SoundOn = Utilities.GetBitsFromByte(data, 3, 3) != 0x00;
			}
			else if (address == 0xFF40)
			{
				Graphics.Instance.SetLCDC(data);
			}
			else if (address == 0xFF41)
			{
				Graphics.Instance.SetSTAT(data);
			}
			else if (address == 0xFF44)
			{
				MainForm.PrintDebugMessage("Register 0xFF44 is read-only!\n");
				MainForm.Pause();
			}
			else if (address == 0xFF45)
			{
				Graphics.Instance.LYC = data;
			}
			else if (address == 0xFF47)
			{
				Graphics.Instance.BGPaletteData = data;
			}
			else if (address == 0xFF48)
			{
				Graphics.Instance.OBJPaletteData0 = data;
			}
			else if (address == 0xFF49)
			{
				Graphics.Instance.OBJPaletteData1 = data;
			}
			// TODO: The other registers.
			else
			{
				MainForm.PrintDebugMessage($"Writing to unimplemented register: 0x{address:X4}!\n");
				MainForm.Pause();
			}
		}
	}
}
