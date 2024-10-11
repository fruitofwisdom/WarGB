namespace GBSharp
{
	internal class Memory
	{
		// Character data, BG display data, etc. - 0x8000 to 0x9FFF
		private readonly byte[] VRAM;
		// External expansion working RAM - 0xA000 to 0xBFFF
		private readonly byte[] ExternalRAM;
		// Unit working RAM - 0xC000 to 0xDFFF
		private readonly byte[] WRAMBank0;
		// TODO: Support bank switching for CGB.
		private readonly byte[] WRAMBank1;
		private readonly byte[] OAM;             // sprite attribute table
		// Working and stack RAM - OxFF80 to 0xFFFE
		private readonly byte[] HRAM;            // high RAM

		private bool RAMEnabled;
		public uint ROMBank { get; private set; }
		private uint MBC1RAMBank = 0;
		private bool MBC1BankingMode = false;

		public bool SaveNeeded { get; private set; }

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
			Array.Clear(HRAM, 0, HRAM.Length);

			RAMEnabled = false;
			// NOTE: By default, 0x4000 to 0x7FFF is mapped to bank 1.
			ROMBank = 1;
			MBC1RAMBank = 0;
			MBC1BankingMode = false;

			SaveNeeded = false;
			if (ROM.Instance.HasBattery)
			{
				// Read the save file into external RAM.
				string savePath = Environment.CurrentDirectory + "\\" + ROM.Instance.Filename + ".sav";
				if (File.Exists(savePath))
				{
					using (Stream reader = File.OpenRead(savePath))
					{
						reader.Read(ExternalRAM, 0, ExternalRAM.Length);
					}
				}
			}
		}

		public void Save()
		{
			if (ROM.Instance.HasBattery)
			{
				// Write external RAM into the save file.
				string savePath = Environment.CurrentDirectory + "\\" + ROM.Instance.Filename + ".sav";
				using (Stream writer = File.OpenWrite(savePath))
				{
					writer.Write(ExternalRAM, 0, ExternalRAM.Length);
				}
			}
			SaveNeeded = false;
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
					// TODO: Support the MBC1's advanced banking mode.
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
				if (RAMEnabled)
				{
					// TODO: Support the MBC1's RAM bank switching.
					data = ExternalRAM[address - 0xA000];
				}
				else
				{
					data = 0xFF;
					// TODO: Is this a real problem?
					//GameBoy.DebugOutput += $"Reading from external RAM while RAM is disabled!\n";
					//MainForm.Pause();
				}
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
				GameBoy.DebugOutput += $"Reading from unusable memory: 0x{address:X4}!\n";
				MainForm.Pause();
			}
			else if (address >= 0xFF00 && address <= 0xFF7F)
			{
				// Actually read from the registers.
				data = ReadFromRegister(address);
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

		// Actually read from the specific registers or various other settings.
		byte ReadFromRegister(int address)
		{
			byte data = 0x00;

			if (address == 0xFF00)
			{
				data = Controller.Instance.ReadFromRegister();
			}
			else if (address == 0xFF04)
			{
				data = CPU.Instance.DIV;
			}
			// TODO: Implement the timer and interrupt.
			/*
			else if (address == 0xFF05)
			{
				data = CPU.Instance.TIMA;
			}
			else if (address == 0xFF06)
			{
				data = CPU.Instance.TMA;
			}
			else if (address == 0xFF07)
			{
				// TODO: Implement timer control.
			}
			*/
			else if (address == 0xFF0F)
			{
				data = CPU.Instance.IF;
			}
			else if (address == 0xFF25)
			{
				data = APU.Instance.GetSoundOutputTerminals();
			}
			else if (address == 0xFF40)
			{
				data = PPU.Instance.GetLCDC();
			}
			else if (address == 0xFF41)
			{
				data = PPU.Instance.GetSTAT();
			}
			else if (address == 0xFF42)
			{
				data = (byte)(PPU.Instance.SCY);
			}
			else if (address == 0xFF43)
			{
				data = (byte)(PPU.Instance.SCX);
			}
			else if (address == 0xFF44)
			{
				data = PPU.Instance.LY;
			}
			else if (address == 0xFF45)
			{
				data = PPU.Instance.LYC;
			}
			else if (address == 0xFF47)
			{
				data = PPU.Instance.BGPaletteData;
			}
			else if (address == 0xFF4A)
			{
				data = (byte)(PPU.Instance.WY);
			}
			else if (address == 0xFF4B)
			{
				data = (byte)(PPU.Instance.WX);
			}
			// TODO: The other registers.
			else
			{
				GameBoy.DebugOutput += $"Reading from unimplemented register: 0x{address:X4}!\n";
				MainForm.Pause();
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

			// Handle MBC-specific address ranges.
			if (ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC1 ||
				ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC1_RAM ||
				ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC1_RAM_BATTERY)
			{
				if (address >= 0x0000 && address <= 0x1FFF)
				{
					RAMEnabled = data == 0x0A;
				}
				else if (address >= 0x2000 && address <= 0x3FFF)
				{
					// Select the ROM bank (a 5-bit register).
					if (data == 0x00)
					{
						ROMBank = 1;
					}
					else
					{
						ROMBank = (uint)(data & 0x1F);
					}
				}
				else if (address >= 0x4000 && address <= 0x5FFF)
				{
					// TODO: Support the MBC1's RAM bank switching.
					MBC1RAMBank = (uint)(data & 0x03);
				}
				else if (address >= 0x6000 && address <= 0x7FFF)
				{
					// TODO: Support the MBC1's advanced banking mode.
					MBC1BankingMode = (data & 0x01) == 0x01;
				}
			}
			else if (ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC2 ||
				ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC2_BATTERY)
			{
				if (address >= 0x0000 && address <= 0x3FFF)
				{
					// This bit specifies selecting a ROM bank.
					if ((address & 0x0100) == 0x0100)
					{
						// NOTE: 0x00 is a special case.
						if (data == 0x00)
						{
							ROMBank = 1;
						}
						else
						{
							ROMBank = data;
						}
					}
					// Otherwise, enable or disable RAM.
					else
					{
						RAMEnabled = data == 0x0A;
					}
				}
				else if (address >= 0x4000 && address <= 0x7FFF)
				{
					// NOTE: Ignore?
					//GameBoy.DebugOutput += "Writing to ROM!\n";
					//MainForm.Pause();
				}
			}
			// TODO: Support other MBCs.
			else
			{
				if (address >= 0x0000 && address <= 0x7FFF)
				{
					// NOTE: Ignore?
					//GameBoy.DebugOutput += "Writing to ROM!\n";
					//MainForm.Pause();
				}
			}

			if (address >= 0x8000 && address <= 0x9FFF)
			{
				VRAM[address - 0x8000] = data;
			}
			else if (address >= 0xA000 && address <= 0xBFFF)
			{
				if (RAMEnabled)
				{
					ExternalRAM[address - 0xA000] = data;
					SaveNeeded = true;
				}
				else
				{
					// TODO: Is this a real problem?
					//GameBoy.DebugOutput += "Writing to external RAM while RAM is disabled!\n";
					//MainForm.Pause();
				}
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
				GameBoy.DebugOutput += $"Writing to unusable memory: 0x{address:X4}!\n";
				MainForm.Pause();
			}
			else if (address >= 0xFF00 && address <= 0xFF7F)
			{
				// Actually write to the registers.
				WriteToRegister(address, data);
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

		// Actually write to the specific registers or various other settings.
		private void WriteToRegister(int address, byte data)
		{
			if (address == 0xFF00)
			{
				// NOTE: Unexpectedly, inputs are considered 1 when not selected or pressed.
				Controller.Instance.SelectButtons = Utilities.GetBitsFromByte(data, 5, 5) != 1;
				Controller.Instance.SelectDpad = Utilities.GetBitsFromByte(data, 4, 4) != 1;
			}
			else if (address == 0xFF04)
			{
				// NOTE: Writing any value to DIV actually resets the internal divider.
				CPU.Instance.Divider = 0;
			}
			// TODO: Implement the timer and interrupt.
			/*
			else if (address == 0xFF05)
			{
				CPU.Instance.TIMA = data;
			}
			else if (address == 0xFF06)
			{
				CPU.Instance.TMA = data;
			}
			else if (address == 0xFF07)
			{
				// TODO: Implement timer control.
			}
			*/
			else if (address == 0xFF0F)
			{
				CPU.Instance.IF = data;
			}
			else if (address == 0xFF10)
			{
				uint sweepTime = Utilities.GetBitsFromByte(data, 4, 6);
				((PulseWaveChannel)APU.Instance.Channels[0]).SweepTime = sweepTime;
				bool sweepIncDec = Utilities.GetBitsFromByte(data, 3, 3) != 0x00;
				((PulseWaveChannel)APU.Instance.Channels[0]).SweepIncDec = sweepIncDec;
				int sweepShiftNumber = Utilities.GetBitsFromByte(data, 0, 2);
				((PulseWaveChannel)APU.Instance.Channels[0]).SweepShiftNumber = sweepShiftNumber;
			}
			else if (address == 0xFF11)
			{
				uint waveformDuty = Utilities.GetBitsFromByte(data, 6, 7);
				((PulseWaveChannel)APU.Instance.Channels[0]).WaveformDuty = waveformDuty;
				uint soundLength = Utilities.GetBitsFromByte(data, 0, 5);
				APU.Instance.Channels[0].SetSoundLength(soundLength);
			}
			else if (address == 0xFF12)
			{
				uint defaultEnvelopeValue = Utilities.GetBitsFromByte(data, 4, 7);
				((PulseWaveChannel)APU.Instance.Channels[0]).SetDefaultEnvelopeValue(defaultEnvelopeValue);
				bool envelopeUpDown = Utilities.GetBitsFromByte(data, 3, 3) != 0x00;
				((PulseWaveChannel)APU.Instance.Channels[0]).EnvelopeUpDown = envelopeUpDown;
				uint lengthOfEnvelopeSteps = Utilities.GetBitsFromByte(data, 0, 2);
				((PulseWaveChannel)APU.Instance.Channels[0]).SetLengthOfEnvelopeSteps(lengthOfEnvelopeSteps);
			}
			else if (address == 0xFF13)
			{
				uint lowOrderFrequencyData = data;
				((PulseWaveChannel)APU.Instance.Channels[0]).LowOrderFrequencyData = lowOrderFrequencyData;
			}
			else if (address == 0xFF14)
			{
				bool initialize = Utilities.GetBitsFromByte(data, 7, 7) != 0x00;
				if (initialize)
				{
					APU.Instance.Channels[0].Initialize();
				}
				bool counterContinuousSelection = Utilities.GetBitsFromByte(data, 6, 6) != 0x00;
				uint highOrderFrequencyData = Utilities.GetBitsFromByte(data, 0, 2);
				((PulseWaveChannel)APU.Instance.Channels[0]).CounterContinuousSelection = counterContinuousSelection;
				((PulseWaveChannel)APU.Instance.Channels[0]).HighOrderFrequencyData = highOrderFrequencyData;
			}
			else if (address == 0xFF16)
			{
				uint waveformDuty = Utilities.GetBitsFromByte(data, 6, 7);
				((PulseWaveChannel)APU.Instance.Channels[1]).WaveformDuty = waveformDuty;
				uint soundLength = Utilities.GetBitsFromByte(data, 0, 5);
				APU.Instance.Channels[1].SetSoundLength(soundLength);
			}
			else if (address == 0xFF17)
			{
				uint defaultEnvelopeValue = Utilities.GetBitsFromByte(data, 4, 7);
				((PulseWaveChannel)APU.Instance.Channels[1]).SetDefaultEnvelopeValue(defaultEnvelopeValue);
				bool envelopeUpDown = Utilities.GetBitsFromByte(data, 3, 3) != 0x00;
				((PulseWaveChannel)APU.Instance.Channels[1]).EnvelopeUpDown = envelopeUpDown;
				uint lengthOfEnvelopeSteps = Utilities.GetBitsFromByte(data, 0, 2);
				((PulseWaveChannel)APU.Instance.Channels[1]).SetLengthOfEnvelopeSteps(lengthOfEnvelopeSteps);
			}
			else if (address == 0xFF18)
			{
				uint lowOrderFrequencyData = data;
				((PulseWaveChannel)APU.Instance.Channels[1]).LowOrderFrequencyData = lowOrderFrequencyData;
			}
			else if (address == 0xFF19)
			{
				bool initialize = Utilities.GetBitsFromByte(data, 7, 7) != 0x00;
				if (initialize)
				{
					APU.Instance.Channels[1].Initialize();
				}
				bool counterContinuousSelection = Utilities.GetBitsFromByte(data, 6, 6) != 0x00;
				uint highOrderFrequencyData = Utilities.GetBitsFromByte(data, 0, 5);
				((PulseWaveChannel)APU.Instance.Channels[1]).CounterContinuousSelection = counterContinuousSelection;
				((PulseWaveChannel)APU.Instance.Channels[1]).HighOrderFrequencyData = highOrderFrequencyData;
			}
			else if (address == 0xFF1A)
			{
				((WaveTableChannel)APU.Instance.Channels[2]).SoundEnabled = data == 0x80;
			}
			else if (address == 0xFF1B)
			{
				APU.Instance.Channels[2].SetSoundLength(data);
			}
			else if (address == 0xFF1C)
			{
				byte outputLevel = Utilities.GetBitsFromByte(data, 5, 6);
				((WaveTableChannel)APU.Instance.Channels[2]).SetOutputLevel(outputLevel);
			}
			else if (address == 0xFF1D)
			{
				byte lowOrderFrequencyData = data;
				((WaveTableChannel)APU.Instance.Channels[2]).LowOrderFrequencyData = lowOrderFrequencyData;
			}
			else if (address == 0xFF1E)
			{
				bool initialize = Utilities.GetBitsFromByte(data, 7, 7) != 0x00;
				if (initialize)
				{
					APU.Instance.Channels[2].Initialize();
				}
				bool counterContinuousSelection = Utilities.GetBitsFromByte(data, 6, 6) != 0x00;
				byte highOrderFrequencyData = Utilities.GetBitsFromByte(data, 0, 2);
				((WaveTableChannel)APU.Instance.Channels[2]).CounterContinuousSelection = counterContinuousSelection;
				((WaveTableChannel)APU.Instance.Channels[2]).HighOrderFrequencyData = highOrderFrequencyData;
			}
			else if (address == 0xFF20)
			{
				byte soundLength = Utilities.GetBitsFromByte(data, 0, 5);
				APU.Instance.Channels[3].SetSoundLength(soundLength);
			}
			else if (address == 0xFF21)
			{
				uint defaultEnvelopeValue = Utilities.GetBitsFromByte(data, 4, 7);
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).SetDefaultEnvelopeValue(defaultEnvelopeValue);
				bool envelopeUpDown = Utilities.GetBitsFromByte(data, 3, 3) != 0x00;
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).EnvelopeUpDown = envelopeUpDown;
				uint lengthOfEnvelopeSteps = Utilities.GetBitsFromByte(data, 0, 2);
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).SetLengthOfEnvelopeSteps(lengthOfEnvelopeSteps);
			}
			else if (address == 0xFF22)
			{
				int shiftClockFrequency = Utilities.GetBitsFromByte(data, 4, 7);
				bool counterSteps = Utilities.GetBitsFromByte(data, 3, 3) != 0x00;
				uint divisionRatioFrequency = Utilities.GetBitsFromByte(data, 0, 2);
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).ShiftClockFrequency = shiftClockFrequency;
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).CounterSteps = counterSteps;
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).DivisionRatioFrequency = divisionRatioFrequency;
			}
			else if (address == 0xFF23)
			{
				bool initialize = Utilities.GetBitsFromByte(data, 7, 7) != 0x00;
				if (initialize)
				{
					APU.Instance.Channels[3].Initialize();
				}
				bool counterContinuousSelection = Utilities.GetBitsFromByte(data, 6, 6) != 0x00;
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).CounterContinuousSelection = counterContinuousSelection;
			}
			else if (address == 0xFF24)
			{
				bool vinLeftOn = Utilities.GetBitsFromByte(data, 7, 7) != 0x00;
				uint leftOutputVolume = Utilities.GetBitsFromByte(data, 4, 6);
				bool vinRightOn = Utilities.GetBitsFromByte(data, 3, 3) != 0x00;
				uint rightOutputVolume = Utilities.GetBitsFromByte(data, 0, 2);
				APU.Instance.VinLeftOn = vinLeftOn;
				APU.Instance.LeftOutputVolume = leftOutputVolume;
				APU.Instance.VinRightOn = vinRightOn;
				APU.Instance.RightOutputVolume = rightOutputVolume;
			}
			else if (address == 0xFF25)
			{
				APU.Instance.SetSoundOutputTerminals(data);
			}
			else if (address == 0xFF26)
			{
				bool allSoundOn = Utilities.GetBitsFromByte(data, 7, 7) != 0x00;
				if (allSoundOn)
				{
					APU.Instance.On();
				}
				else
				{
					APU.Instance.Off();
				}
				APU.Instance.Channels[0].SoundOn = Utilities.GetBitsFromByte(data, 0, 0) != 0x00;
				APU.Instance.Channels[1].SoundOn = Utilities.GetBitsFromByte(data, 1, 1) != 0x00;
				APU.Instance.Channels[2].SoundOn = Utilities.GetBitsFromByte(data, 2, 2) != 0x00;
				APU.Instance.Channels[3].SoundOn = Utilities.GetBitsFromByte(data, 3, 3) != 0x00;
			}
			else if (address >= 0xFF30 && address <= 0xFF3F)
			{
				((WaveTableChannel)APU.Instance.Channels[2]).SetWaveformRAM(address - 0xFF30, data);
			}
			else if (address == 0xFF40)
			{
				PPU.Instance.SetLCDC(data);
			}
			else if (address == 0xFF41)
			{
				PPU.Instance.SetSTAT(data);
			}
			else if (address == 0xFF42)
			{
				PPU.Instance.SCY = data;
			}
			else if (address == 0xFF43)
			{
				PPU.Instance.SCX = data;
			}
			else if (address == 0xFF44)
			{
				GameBoy.DebugOutput += "Register 0xFF44 is read-only!\n";
				MainForm.Pause();
			}
			else if (address == 0xFF45)
			{
				PPU.Instance.LYC = data;
			}
			else if (address == 0xFF46)
			{
				// Perform a DMA transfer from ROM or RAM to OAM.
				for (int i = 0; i < 0xA0; ++i)
				{
					int transferFrom = (int)(data << 8) + i;
					byte d8 = Read(transferFrom);
					int transferTo = (int)(0xFE << 8) + i;
					Write(transferTo, d8);
				}
			}
			else if (address == 0xFF47)
			{
				PPU.Instance.BGPaletteData = data;
			}
			else if (address == 0xFF48)
			{
				PPU.Instance.OBJPaletteData0 = data;
			}
			else if (address == 0xFF49)
			{
				PPU.Instance.OBJPaletteData1 = data;
			}
			else if (address == 0xFF4A)
			{
				PPU.Instance.WY = data;
			}
			else if (address == 0xFF4B)
			{
				PPU.Instance.WX = data;
			}
			// TODO: The other registers.
			else
			{
				GameBoy.DebugOutput += $"Writing to unimplemented register: 0x{address:X4}!\n";
				MainForm.Pause();
			}
		}
	}
}
