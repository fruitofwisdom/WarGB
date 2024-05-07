namespace GBSharp
{
	internal partial class CPU
	{
		public static bool ShouldPrintOpcodes = false;

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
			//F = 0xB0;			// TODO: Just use the other flags?
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
		}

		// Step through one instruction and return the number of cycles elapsed.
		public uint Step()
		{
			uint cycles;

			// If an interrupt was triggered, handle it as our next instruction.
			bool interruptHandled = HandleInterrupt(out cycles);

			// Otherwise, run the next opcode as normal.
			if (!interruptHandled)
			{
				byte instruction = Memory.Instance.Read(PC);
				cycles = HandleOpcode(instruction);
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
					MainForm.PrintDebugMessage("A v-blank interrupt occurred.\n");
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
					MainForm.PrintDebugMessage("A LCD interrupt occurred.\n");
				}
				// TODO: Handle other interrupt flags.
			}

			return interruptHandled;
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
			if (ShouldPrintOpcodes)
			{
				MainForm.PrintDebugMessage($"[0x{PC:X4}] 0x{instruction:X2}: " + opcode + "\n");
			}
		}
	}
}
