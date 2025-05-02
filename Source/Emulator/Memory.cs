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

		public byte SerialData;
		public bool SerialTransferEnabled;
		private byte _serialTransferControl;        // bits 2 through 6
		// TODO: Implement the link cable?
		private bool _serialClockSpeed;
		public bool SerialClockSelect;

		private bool RAMEnabled;
		public uint ROMBank { get; private set; }
		private uint MBC1RAMBank;
		private bool MBC1BankingMode;

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

			SerialData = 0x00;
			SerialTransferEnabled = false;
			_serialTransferControl = 0x7C;
			// TODO: CGB clock speed?
			_serialClockSpeed = true;
			SerialClockSelect = false;

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
					uint bankOffset = 0;
					if (MBC1BankingMode)
					{
						// NOTE: MBC1's ROM banks are in sizes of 16 KB?
						// TODO: Is this correct???
						bankOffset = MBC1RAMBank * 0x4000;
					}
					data = ROM.Instance.Data[address + bankOffset];
				}
			}
			else if (address >= 0x4000 && address <= 0x7FFF)
			{
				// NOTE: Bank 1 maps to 0x4000 to 0x7FFF, bank 2 to 0x8000 to 0xBFFF, etc.
				uint bankOffset = (ROMBank - 1) * 0x4000;
				data = ROM.Instance.Data[address + bankOffset];
			}
			else if (address >= 0x8000 && address <= 0x9FFF)
			{
				// TODO: Support VRAM of 16 KB for CGB via bank switching.
				data = VRAM[address - 0x8000];
			}
			else if (address >= 0xA000 && address <= 0xBFFF)
			{
				uint bankOffset = 0;
				if (MBC1BankingMode)
				{
					// NOTE: MBC1's external RAM banks are in sizes of 8KB.
					bankOffset = MBC1RAMBank * 0x2000;
				}
				data = ExternalRAM[(address + bankOffset) - 0xA000];

				if (ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC1_RAM ||
					ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC1_RAM_BATTERY ||
					ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC2 ||
					ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC2_BATTERY)
				{
					if (!RAMEnabled)
					{
						data = 0xFF;

						// TODO: Is this a real problem?
						GameBoy.DebugOutput += $"Reading from external RAM while RAM is disabled!\n";
						//MainForm.Pause();
					}
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
				// NOTE: Ignore?
				//GameBoy.DebugOutput += $"Reading from unusable memory: 0x{address:X4}!\n";
				//MainForm.Pause();
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
			else if (address == 0xFF01)
			{
				data = SerialData;
			}
			else if (address == 0xFF02)
			{
				byte serialTransferEnabled = SerialTransferEnabled ? (byte)0x80 : (byte)0x00;
				byte serialClockSpeed = _serialClockSpeed ? (byte)0x02 : (byte)0x00;
				byte serialClockSelect = SerialClockSelect ? (byte)0x01 : (byte)0x00;
				data = (byte)(serialTransferEnabled | _serialTransferControl | serialClockSpeed | serialClockSelect);
			}
			else if (address == 0xFF04)
			{
				data = CPU.Instance.DIV;
			}
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
				byte timerEnabled = (byte)(CPU.Instance.TimerEnabled ? 0x04 : 0x00);
				byte timerClockSelect = CPU.Instance.TimerClockSelect;
				data = (byte)(timerEnabled | timerClockSelect);
			}
			else if (address == 0xFF0F)
			{
				data = 0xE0;
				data = (byte)(data | CPU.Instance.IF);
			}
			else if (address == 0xFF12)
			{
				byte defaultEnvelopeValue = (byte)(((PulseWaveChannel)APU.Instance.Channels[0]).DefaultEnvelopeValue << 4);
				byte envelopeUpDownByte = (byte)(((PulseWaveChannel)APU.Instance.Channels[0]).EnvelopeUpDown ? 0x08 : 0x00);
				byte lengthOfEnvelopeSteps = (byte)((PulseWaveChannel)APU.Instance.Channels[0]).LengthOfEnvelopeSteps;
				data = (byte)(defaultEnvelopeValue | envelopeUpDownByte | lengthOfEnvelopeSteps);
			}
			else if (address == 0xFF14)
			{
				data = 0xBF;
				byte counterContinuousSelection = (byte)(((PulseWaveChannel)APU.Instance.Channels[0]).CounterContinuousSelection ? 0x40 : 0x00);
				data = (byte)(data | counterContinuousSelection);
			}
			else if (address == 0xFF17)
			{
				byte defaultEnvelopeValue = (byte)(((PulseWaveChannel)APU.Instance.Channels[1]).DefaultEnvelopeValue << 4);
				byte envelopeUpDownByte = (byte)(((PulseWaveChannel)APU.Instance.Channels[1]).EnvelopeUpDown ? 0x08 : 0x00);
				byte lengthOfEnvelopeSteps = (byte)((PulseWaveChannel)APU.Instance.Channels[1]).LengthOfEnvelopeSteps;
				data = (byte)(defaultEnvelopeValue | envelopeUpDownByte | lengthOfEnvelopeSteps);
			}
			else if (address == 0xFF19)
			{
				data = 0xBF;
				byte counterContinuousSelection = (byte)(((PulseWaveChannel)APU.Instance.Channels[1]).CounterContinuousSelection ? 0x40 : 0x00);
				data = (byte)(data | counterContinuousSelection);
			}
			else if (address == 0xFF1A)
			{
				data = 0x7F;
				byte soundEnabled = (byte)(((WaveTableChannel)APU.Instance.Channels[2]).SoundEnabled ? 0x80 : 0x00);
				data = (byte)(data | soundEnabled);
			}
			else if (address == 0xFF1C)
			{
				data = 0x9F;
				byte outputLevel = (byte)(((WaveTableChannel)APU.Instance.Channels[2]).GetOutputLevel() << 5);
				data = (byte)(data | outputLevel);
			}
			else if (address == 0xFF1E)
			{
				data = 0xBF;
				byte counterContinuousSelection = (byte)(((WaveTableChannel)APU.Instance.Channels[2]).CounterContinuousSelection ? 0x40 : 0x00);
				data = (byte)(data | counterContinuousSelection);
			}
			else if (address == 0xFF21)
			{
				byte defaultEnvelopeValue = (byte)(((NoiseGeneratorChannel)APU.Instance.Channels[3]).DefaultEnvelopeValue << 4);
				byte envelopeUpDownByte = (byte)(((NoiseGeneratorChannel)APU.Instance.Channels[3]).EnvelopeUpDown ? 0x08 : 0x00);
				byte lengthOfEnvelopeSteps = (byte)((NoiseGeneratorChannel)APU.Instance.Channels[3]).LengthOfEnvelopeSteps;
				data = (byte)(defaultEnvelopeValue | envelopeUpDownByte | lengthOfEnvelopeSteps);
			}
			else if (address == 0xFF23)
			{
				data = 0xBF;
				byte counterContinuousSelection = (byte)(((NoiseGeneratorChannel)APU.Instance.Channels[3]).CounterContinuousSelection ? 0x40 : 0x00);
				data = (byte)(data | counterContinuousSelection);
			}
			else if (address == 0xFF24)
			{
				byte vinLeftOn = APU.Instance.VinLeftOn ? (byte)0x80 : (byte)0x00;
				byte leftOutputVolume = (byte)APU.Instance.LeftOutputVolume;
				byte vinRightOn = APU.Instance.VinRightOn ? (byte)0x08 : (byte)0x00;
				byte rightOutputVolume = (byte)APU.Instance.RightOutputVolume;
				data = (byte)(vinLeftOn | leftOutputVolume | vinRightOn | rightOutputVolume);
			}
			else if (address == 0xFF25)
			{
				data = APU.Instance.GetSoundOutputTerminals();
			}
			else if (address == 0xFF26)
			{
				data = 0x70;
				byte allSoundOn = APU.Instance.IsOn() ? (byte)0x80 : (byte)0x00;
				byte channel1On = APU.Instance.Channels[0].SoundOn ? (byte)0x01 : (byte)0x00;
				byte channel2On = APU.Instance.Channels[1].SoundOn ? (byte)0x02 : (byte)0x00;
				byte channel3On = APU.Instance.Channels[2].SoundOn ? (byte)0x04 : (byte)0x00;
				byte channel4On = APU.Instance.Channels[3].SoundOn ? (byte)0x08 : (byte)0x00;
				data = (byte)(data | allSoundOn | channel1On | channel2On | channel3On | channel4On);
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
			else if (address == 0xFF48)
			{
				data = PPU.Instance.OBJPaletteData0;
			}
			else if (address == 0xFF49)
			{
				data = PPU.Instance.OBJPaletteData1;
			}
			else if (address == 0xFF4A)
			{
				data = (byte)(PPU.Instance.WY);
			}
			else if (address == 0xFF4B)
			{
				data = (byte)(PPU.Instance.WX);
			}
			else if (address == 0xFF4D)
			{
				if (ROM.Instance.CGBCompatible || ROM.Instance.CGBOnly)
				{
					data = 0x7E;
					byte doubleSpeed = (byte)(CPU.Instance.DoubleSpeed ? 0x80 : 0x00);
					byte doubleSpeedArmed = (byte)(CPU.Instance.DoubleSpeedArmed ? 0x01 : 0x00);
					data = (byte)(data | doubleSpeed | doubleSpeedArmed);
				}
				else
				{
					GameBoy.DebugOutput += $"Reading from CGB register in non-CGB game: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
			}
			else if (address == 0xFF4F)
			{
				if (ROM.Instance.CGBCompatible || ROM.Instance.CGBOnly)
				{
					// TODO: CGB support.
					GameBoy.DebugOutput += $"Reading from unimplemented CGB register: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
				else
				{
					GameBoy.DebugOutput += $"Reading from CGB register in non-CGB game: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
			}
			else if (address == 0xFF53)
			{
				if (ROM.Instance.CGBCompatible || ROM.Instance.CGBOnly)
				{
					// TODO: CGB support.
					GameBoy.DebugOutput += $"Reading from unimplemented CGB register: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
				else
				{
					GameBoy.DebugOutput += $"Reading from CGB register in non-CGB game: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
			}
			else if (address >= 0xFF71 && address <= 0xFF7F)
			{
				// NOTE: Ignore?
				//GameBoy.DebugOutput += $"Reading from undocumented register: 0x{address:X4}!\n";
				//MainForm.Pause();
			}
			// TODO: The other registers.
			else
			{
				if (!GameBoy.ShouldLogOpcodes)
				{
					GameBoy.DebugOutput += $"Reading from unimplemented register: 0x{address:X4}!\n";
					MainForm.Pause();
				}
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
					// TODO: Does this addressing apply to simple MBC1 carts?
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
						// Mask down to the correct number of ROM banks.
						uint numROMBanks = ROM.Instance.ROMSize / 1024 / 16;
						ROMBank &= (numROMBanks - 1);
					}
				}
				else if (address >= 0x4000 && address <= 0x5FFF)
				{
					MBC1RAMBank = (uint)(data & 0x03);
				}
				else if (address >= 0x6000 && address <= 0x7FFF)
				{
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
				ExternalRAM[address - 0xA000] = data;

				if (ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC1_RAM ||
					ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC1_RAM_BATTERY ||
					ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC2 ||
					ROM.Instance.CartridgeType == ROM.CartridgeTypes.MBC2_BATTERY)
				{
					if (RAMEnabled)
					{
						SaveNeeded = true;
					}
					else
					{
						// TODO: Is this a real problem?
						GameBoy.DebugOutput += "Writing to external RAM while RAM is disabled!\n";
						//MainForm.Pause();
					}
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
				// NOTE: Ignore?
				//GameBoy.DebugOutput += $"Writing to unusable memory: 0x{address:X4}!\n";
				//MainForm.Pause();
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
				Controller.Instance.SelectButtons = !Utilities.GetBoolFromByte(data, 5);
				Controller.Instance.SelectDpad = !Utilities.GetBoolFromByte(data, 4);
			}
			else if (address == 0xFF01)
			{
				SerialData = data;
			}
			else if (address == 0xFF02)
			{
				SerialTransferEnabled = Utilities.GetBoolFromByte(data, 7);
				// TODO: CGB clock speed?
				//_serialClockSpeed = Utilities.GetBoolFromByte(data, 1);
				SerialClockSelect = Utilities.GetBoolFromByte(data, 0);
			}
			else if (address == 0xFF04)
			{
				// NOTE: Writing any value to DIV actually resets the internal divider.
				CPU.Instance.Divider = 0;
			}
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
				CPU.Instance.TimerEnabled = Utilities.GetBoolFromByte(data, 2);
				CPU.Instance.TimerClockSelect = Utilities.GetBitsFromByte(data, 0, 1);
			}
			else if (address == 0xFF0F)
			{
				CPU.Instance.IF = data;
			}
			else if (address == 0xFF10)
			{
				uint sweepTime = Utilities.GetBitsFromByte(data, 4, 6);
				((PulseWaveChannel)APU.Instance.Channels[0]).SweepTime = sweepTime;
				bool sweepIncDec = Utilities.GetBoolFromByte(data, 3);
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
				bool envelopeUpDown = Utilities.GetBoolFromByte(data, 3);
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
				bool initialize = Utilities.GetBoolFromByte(data, 7);
				if (initialize)
				{
					APU.Instance.Channels[0].Initialize();
				}
				bool counterContinuousSelection = Utilities.GetBoolFromByte(data, 6);
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
				bool envelopeUpDown = Utilities.GetBoolFromByte(data, 3);
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
				bool initialize = Utilities.GetBoolFromByte(data, 7);
				if (initialize)
				{
					APU.Instance.Channels[1].Initialize();
				}
				bool counterContinuousSelection = Utilities.GetBoolFromByte(data, 6);
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
				bool initialize = Utilities.GetBoolFromByte(data, 7);
				if (initialize)
				{
					APU.Instance.Channels[2].Initialize();
				}
				bool counterContinuousSelection = Utilities.GetBoolFromByte(data, 6);
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
				bool envelopeUpDown = Utilities.GetBoolFromByte(data, 3);
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).EnvelopeUpDown = envelopeUpDown;
				uint lengthOfEnvelopeSteps = Utilities.GetBitsFromByte(data, 0, 2);
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).SetLengthOfEnvelopeSteps(lengthOfEnvelopeSteps);
			}
			else if (address == 0xFF22)
			{
				int shiftClockFrequency = Utilities.GetBitsFromByte(data, 4, 7);
				bool counterSteps = Utilities.GetBoolFromByte(data, 3);
				uint divisionRatioFrequency = Utilities.GetBitsFromByte(data, 0, 2);
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).ShiftClockFrequency = shiftClockFrequency;
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).CounterSteps = counterSteps;
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).DivisionRatioFrequency = divisionRatioFrequency;
			}
			else if (address == 0xFF23)
			{
				bool initialize = Utilities.GetBoolFromByte(data, 7);
				if (initialize)
				{
					APU.Instance.Channels[3].Initialize();
				}
				bool counterContinuousSelection = Utilities.GetBoolFromByte(data, 6);
				((NoiseGeneratorChannel)APU.Instance.Channels[3]).CounterContinuousSelection = counterContinuousSelection;
			}
			else if (address == 0xFF24)
			{
				bool vinLeftOn = Utilities.GetBoolFromByte(data, 7);
				uint leftOutputVolume = Utilities.GetBitsFromByte(data, 4, 6);
				bool vinRightOn = Utilities.GetBoolFromByte(data, 3);
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
				bool allSoundOn = Utilities.GetBoolFromByte(data, 7);
				if (allSoundOn)
				{
					APU.Instance.On();
				}
				else
				{
					APU.Instance.Off();
				}
				APU.Instance.Channels[0].SoundOn = Utilities.GetBoolFromByte(data, 0);
				APU.Instance.Channels[1].SoundOn = Utilities.GetBoolFromByte(data, 1);
				APU.Instance.Channels[2].SoundOn = Utilities.GetBoolFromByte(data, 2);
				APU.Instance.Channels[3].SoundOn = Utilities.GetBoolFromByte(data, 3);
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
				// NOTE: Ignore?
				//GameBoy.DebugOutput += "Register 0xFF44 is read-only!\n";
				//MainForm.Pause();
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
			else if (address == 0xFF4D)
			{
				if (ROM.Instance.CGBCompatible || ROM.Instance.CGBOnly)
				{
					CPU.Instance.DoubleSpeedArmed = true;
				}
				else
				{
					GameBoy.DebugOutput += $"Writing to CGB register in non-CGB game: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
			}
			else if (address == 0xFF4F)
			{
				if (ROM.Instance.CGBCompatible || ROM.Instance.CGBOnly)
				{
					// TODO: CGB support.
					GameBoy.DebugOutput += $"Writing to unimplemented CGB register: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
				else
				{
					GameBoy.DebugOutput += $"Writing to CGB register in non-CGB game: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
			}
			else if (address == 0xFF68)
			{
				if (ROM.Instance.CGBCompatible || ROM.Instance.CGBOnly)
				{
					// TODO: CGB support.
					GameBoy.DebugOutput += $"Writing to unimplemented CGB register: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
				else
				{
					GameBoy.DebugOutput += $"Writing to CGB register in non-CGB game: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
			}
			else if (address == 0xFF69)
			{
				if (ROM.Instance.CGBCompatible || ROM.Instance.CGBOnly)
				{
					// TODO: CGB support.
					GameBoy.DebugOutput += $"Writing to unimplemented CGB register: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
				else
				{
					GameBoy.DebugOutput += $"Writing to CGB register in non-CGB game: 0x{address:X4}!\n";
					//MainForm.Pause();
				}
			}
			else if (address >= 0xFF71 && address <= 0xFF7F)
			{
				// NOTE: Ignore?
				//GameBoy.DebugOutput += $"Writing to undocumented register: 0x{address:X4}!\n";
				//MainForm.Pause();
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
