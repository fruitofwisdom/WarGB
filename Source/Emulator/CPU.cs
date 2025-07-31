namespace GBSharp
{
	internal partial class CPU
	{
		// The accumulator register.
		private byte A;
		// The auxiliary registers.
		// The flag register is actually four flags.
		private bool Z;         // set to 1 when the result of an operation is 0, otherwise reset
		private bool N;         // set to 1 following execution of the subtraction instruction, regardless of the result
		private bool H;         // set to 1 when an operation results in carrying from or borrowing to bit 3
		private bool CY;        // set to 1 when an operation results in carrying from or borrowing to bit 7
		// BC, DE, and HL are register pairs.
		private byte B;         // higher byte
		private byte C;         // lower byte
		private byte D;
		private byte E;
		private ushort HL;
		// The program counter.
		public ushort PC { get; private set; }
		// The stack pointer.
		private ushort SP;

		// The interrupt flags.
		public byte IF;         // interrupt request flag (also 0xFF0F)
		public byte IE;         // interrupt enable flag (also 0xFFFF)
		private bool IME;       // interrupt master enable flag

		// The divider and timer registers.
		public ushort Divider;
		public byte DIV;
		private ushort _divApu;
		public byte TIMA;
		public byte TMA;
		public bool TimerEnabled;
		public byte TimerClockSelect;
		private bool _timerOverflowPending;

		// TODO: Implement the link cable?
		private const ushort kSerialTransferTime = 128;
		private ushort _serialTransferTimeRemaining;

		// CGB double speed.
		public bool DoubleSpeed { get; private set; }
		public bool DoubleSpeedArmed;
		private const ushort kDoubleSpeedArmedTime = 2050;      // M-cycles
		private ushort _doubleSpeedArmedTimeRemaining;

		// Other CPU states.
		private bool _halted;		// from the HALT opcode
		private bool _stopped;		// from the STOP opcode
		private bool _was16BitOpcode;

		private static CPU? _instance;
		public static CPU Instance
		{
			get
			{
				_instance ??= new CPU();
				return _instance;
			}
		}

		public CPU()
		{
			Reset();
		}

		// Reset the CPU's registers and flags.
		public void Reset()
		{
			// Initial CPU state after the boot ROM.
			A = 0x11; //0x01;		// old no$gmb values
			Z = true;
			N = false;
			H = false; //true;
			CY = false; //true;
			B = 0x00;
			C = 0x00; //0x13;
			D = 0xFF; //0x00;
			E = 0x56; //0xD8;
			HL = 0x000D; //0x014D;
			PC = 0x0100;
			SP = 0xFFFE;

			IF = 0x00;
			IE = 0x00;
			IME = false;

			Divider = 0xAB00;
			DIV = 0;
			_divApu = 0;
			TIMA = 0x00;
			TMA = 0x00;
			TimerEnabled = false;
			TimerClockSelect = 0x00;
			_timerOverflowPending = false;

			_serialTransferTimeRemaining = kSerialTransferTime;

			DoubleSpeed = false;
			DoubleSpeedArmed = false;
			_doubleSpeedArmedTimeRemaining = kDoubleSpeedArmedTime;

			_halted = false;
			_stopped = false;
			_was16BitOpcode = false;

			// TODO: Set up sets of registers another way?
			if (ROM.Instance.SGBCompatible)
			{
				A = 0xFF;
				C = 0x14;
			}
		}

		// Step through one instruction and return the number of cycles elapsed.
		public uint Step()
		{
			uint cycles = 0;

			// It takes some time before double speed mode actually begins.
			if (DoubleSpeedArmed && _doubleSpeedArmedTimeRemaining > 0)
			{
				_doubleSpeedArmedTimeRemaining--;
				if (_doubleSpeedArmedTimeRemaining == 0)
				{
					DoubleSpeed = !DoubleSpeed;
					DoubleSpeedArmed = false;
					_doubleSpeedArmedTimeRemaining = kDoubleSpeedArmedTime;
					_stopped = false;
				}
			}

			if (_stopped)
			{
				return cycles;
			}

			// Handle the opcode.
			if (!_halted)
			{
				byte instruction = Memory.Instance.Read(PC);
				HandleOpcode(instruction, out cycles);
			}

			// Then handle any interrupts. NOTE: This order is important.
			cycles += HandleInterrupt();

			return cycles;
		}

		// Returns if an interrupt was triggered and handled and how many cycles elapsed.
		private uint HandleInterrupt()
		{
			uint cycles = 0;

			if (IME)
			{
				// Handle a v-blank interrupt.
				if ((byte)(IE & 0x01) == 0x01 && (byte)(IF & 0x01) == 0x01)
				{
					IME = false;
					Utilities.SetBitsInByte(ref IF, 0, 0, 0);
					byte pcHigher = (byte)((PC & 0xFF00) >> 8);
					Memory.Instance.Write(SP - 1, pcHigher);
					byte pcLower = (byte)(PC & 0x00FF);
					Memory.Instance.Write(SP - 2, pcLower);
					SP -= 2;
					PC = 0x0040;
					cycles = 5;
					_halted = false;
				}
				// Handle an LCD interrupt.
				else if ((byte)(IE & 0x02) == 0x02 && (byte)(IF & 0x02) == 0x02)
				{
					IME = false;
					Utilities.SetBitsInByte(ref IF, 0, 1, 1);
					byte pcHigher = (byte)((PC & 0xFF00) >> 8);
					Memory.Instance.Write(SP - 1, pcHigher);
					byte pcLower = (byte)(PC & 0x00FF);
					Memory.Instance.Write(SP - 2, pcLower);
					SP -= 2;
					PC = 0x0048;
					cycles = 5;
					_halted = false;
				}
				// Handle a timer interrupt.
				else if ((byte)(IE & 0x04) == 0x04 && (byte)(IF & 0x04) == 0x04)
				{
					IME = false;
					Utilities.SetBitsInByte(ref IF, 0, 2, 2);
					byte pcHigher = (byte)((PC & 0xFF00) >> 8);
					Memory.Instance.Write(SP - 1, pcHigher);
					byte pcLower = (byte)(PC & 0x00FF);
					Memory.Instance.Write(SP - 2, pcLower);
					SP -= 2;
					PC = 0x0050;
					cycles = 5;
					_halted = false;
				}
				else if ((byte)(IE & 0x08) == 0x08 && (byte)(IF & 0x08) == 0x08)
				{
					IME = false;
					Utilities.SetBitsInByte(ref IF, 0, 3, 3);
					byte pcHigher = (byte)((PC & 0xFF00) >> 8);
					Memory.Instance.Write(SP - 1, pcHigher);
					byte pcLower = (byte)(PC & 0x00FF);
					Memory.Instance.Write(SP - 2, pcLower);
					SP -= 2;
					PC = 0x0058;
					cycles = 5;
					_halted = false;
				}
				else if ((byte)(IE & 0x10) == 0x10 && (byte)(IF & 0x10) == 0x10)
				{
					IME = false;
					Utilities.SetBitsInByte(ref IF, 0, 4, 4);
					byte pcHigher = (byte)((PC & 0xFF00) >> 8);
					Memory.Instance.Write(SP - 1, pcHigher);
					byte pcLower = (byte)(PC & 0x00FF);
					Memory.Instance.Write(SP - 2, pcLower);
					SP -= 2;
					PC = 0x0060;
					cycles = 5;
					_halted = false;
				}
			}
			else
			{
				// NOTE: If interrupts are disabled, but an interrupt is triggered while the CPU
				// is halted, unhalt the CPU, but don't handle the interrupt.
				if (_halted && IF != 0x00)
				{
					_halted = false;
				}
			}

			return cycles;
		}

		// Update the divider and timer every CPU cycle (M-cycle).
		public void UpdateDividerAndTimer()
		{
			byte previousDiv = DIV;

			Divider++;
			// DIV is the top 8 bits of the internal divider.
			DIV = (byte)(Divider >> 8);

			if (_timerOverflowPending)
			{
				if (GameBoy.ShouldLogOpcodes)
				{
					GameBoy.LogOutput += $"[0x{CPU.Instance.PC:X4} {Memory.Instance.ROMBank}] A timer interrupt occurred.\n";
				}

				// Set the timer IF flag.
				IF |= 0x04;
				_timerOverflowPending = false;
			}

			if (TimerEnabled)
			{
				ushort clockSelectCycles = 0;
				switch (TimerClockSelect)
				{
					case 0x00:
						clockSelectCycles = 256;
						break;
					case 0x01:
						clockSelectCycles = 4;
						break;
					case 0x02:
						clockSelectCycles = 16;
						break;
					case 0x03:
						clockSelectCycles = 64;
						break;
				}

				// Every certain number of M-cycles, increment TIMA and check for a timer interrupt.
				if ((Divider % clockSelectCycles) == 0)
				{
					TIMA++;
					if (TIMA == 0x00)
					{
						// NOTE: The timer interrupt flag isn't set until one M-cycle after this overflow.
						_timerOverflowPending = true;

						TIMA = TMA;
					}
				}
			}

			// If a transfer is requested and we are running the internal clock.
			if (Memory.Instance.SerialTransferEnabled && Memory.Instance.SerialClockSelect)
			{
				if (_serialTransferTimeRemaining > 0)
				{
					_serialTransferTimeRemaining--;
				}

				if (_serialTransferTimeRemaining == 0)
				{
					// TODO: Implement the link cable?
					Memory.Instance.SerialData = 0xFF;

					Memory.Instance.SerialTransferEnabled = false;
					_serialTransferTimeRemaining = kSerialTransferTime;

					if (GameBoy.ShouldLogOpcodes)
					{
						GameBoy.LogOutput += "A serial interrupt occurred.\n";
					}

					// Set the serial IF flag.
					IF |= 0x08;
				}
			}

			// Update the APU's DIV-APU-based events.
			// TODO: This should be 0x08, but 0x04 works more accurately maybe?
			if ((previousDiv & 0x04) == 0x04 && (DIV & 0x04) == 0x00)
            {
				_divApu++;
				APU.Instance.UpdateDiv(_divApu);
			}
		}

		private byte GetF()
		{
			byte f = 0x00;

			f |= (byte)(Z ? 0x80 : 0x00);
			f |= (byte)(N ? 0x40 : 0x00);
			f |= (byte)(H ? 0x20 : 0x00);
			f |= (byte)(CY ? 0x10 : 0x00);

			return f;
		}

		private void SetF(byte f)
		{
			Z = Utilities.GetBitsFromByte(f, 7, 7) != 0x00;
			N = Utilities.GetBitsFromByte(f, 6, 6) != 0x00;
			H = Utilities.GetBitsFromByte(f, 5, 5) != 0x00;
			CY = Utilities.GetBitsFromByte(f, 4, 4) != 0x00;
		}

		private void PrintOpcode(byte instruction, string opcode)
		{
			if (GameBoy.ShouldLogOpcodes)
			{
				string output = $"[0x{PC:X4} {Memory.Instance.ROMBank}]";
				if (_was16BitOpcode)
				{
					output += $" 0xCB{instruction:X2}: " + opcode;
				}
				else
				{
					output += $"   0x{instruction:X2}: " + opcode;
				}
				for (int i = output.Length; i < 40; ++i)
				{
					output += " ";
				}
				byte d8 = Memory.Instance.Read(HL);
				output += $"A=0x{A:X2}, F=0x{GetF():X2}, BC=0x{B:X2}{C:X2}, DE=0x{D:X2}{E:X2}, HL=0x{HL:X4} (0x{d8:X2}), SP=0x{SP:X4}\n";
				GameBoy.LogOutput += output;
			}
		}
	}
}
