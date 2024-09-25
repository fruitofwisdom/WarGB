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
		// TODO: Implement the timer and interrupt.
		public byte TIMA;
		public byte TMA;
		// TODO: Implement timer control.
		//public byte TAC;

		private bool _halted;

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
			A = 0x00;
			Z = true;
			N = false;
			H = true;
			CY = true;
			B = 0x00;
			C = 0x00;
			D = 0x00;
			E = 0x00;
			HL = 0x0000;
			PC = 0x0100;
			SP = 0xFFFE;

			IF = 0x00;
			IE = 0x00;
			IME = false;

			// TODO: Correct initial values?
			Divider = 0;
			DIV = 0x00;
			_divApu = 0;
			TIMA = 0x00;
			TMA = 0x00;
			// TODO: Implement timer control.
			//TAC = 0x00;

			_halted = false;
		}

		// Step through one instruction and return the number of cycles elapsed.
		public uint Step()
		{
			uint cycles;

			// If an interrupt was triggered, handle it as our next instruction.
			bool interruptHandled = HandleInterrupt(out cycles);

			// Handle the next opcode, unless we just handled an interrupt or are halted.
			if (!interruptHandled && !_halted)
			{
				byte instruction = Memory.Instance.Read(PC);
				HandleOpcode(instruction, out cycles);
			}

			return cycles;
		}

		// Returns if an interrupt was triggered and handled and how many cycles elapsed.
		private bool HandleInterrupt(out uint cycles)
		{
			bool interruptHandled = false;
			cycles = 0;

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
					interruptHandled = true;
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
					interruptHandled = true;
					_halted = false;
				}
				// TODO: Handle other interrupt flags.
			}

			return interruptHandled;
		}

		public void UpdateDividerAndTimer()
		{
			byte previousDiv = DIV;

			Divider++;
			// DIV is the top 8 bits of the internal divider.
			DIV = (byte)(Divider >> 8);

            // TODO: Implement the timer and interrupt.

			// Update the APU's DIV-APU-based events.
            if ((previousDiv & 0x08) == 0x08 && (DIV & 0x08) == 0x00)
            {
				_divApu++;
				Sound.Instance.UpdateDiv(_divApu);
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
				string output = $"[0x{PC:X4} {Memory.Instance.ROMBank}] 0x{instruction:X2}: " + opcode;
				for (int i = output.Length; i < 40; ++i)
				{
					output += " ";
				}
				output += $"A=0x{A:X2}, F=0x{GetF():X2}, BC=0x{B:X2}{C:X2}, DE=0x{D:X2}{E:X2}, HL=0x{HL:X4}, SP=0x{SP:X4}\n";
				GameBoy.LogOutput += output;
			}
		}
	}
}
