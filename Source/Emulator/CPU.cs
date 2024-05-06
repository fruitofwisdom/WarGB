namespace GBSharp
{
	internal partial class CPU
	{
		private bool NeedToStop;
		private bool Playing;
		private bool StepRequested;

		public static bool ShouldPrintOpcodes = false;

		public uint Frequency { get; private set; }
		public uint Cycles { get; private set; }

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
			NeedToStop = false;
			Playing = false;
			StepRequested = false;

			// NOTE: Original DMG CPU frequency is 1.05 MHz.
			// TODO: Support CGB double-speed mode also?
			Frequency = 4194304;
			Cycles = 0;

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

		// The CPU runs in its own thread.
		public void Run()
		{
			// NOTE: We skip any validation or BIOS handling.
			Thread.CurrentThread.Name = "GB# CPU";
			MainForm.PrintDebugMessage("Ready to play " + ROM.Instance.Title + "!\n");

			while (true)
			{
				// The thread needs to close.
				if (NeedToStop)
				{
					return;
				}

				// Do nothing if we're paused, unless a step was requested.
				if (!Playing && !StepRequested)
				{
					Thread.Sleep(1);
					continue;
				}

				// Read and execute the next CPU instruction.
				byte instruction = Memory.Instance.Read(PC);
				HandleOpcode(instruction);

				// Let the LCD update too.
				// TODO: Update the PPU independently so that dots are more accurate than relying on CPU cycles?
				Graphics.Instance.Update();

				if (IME)
				{
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
						Cycles += 5;
						MainForm.PrintDebugMessage("A v-blank interrupt occurred.\n");
					}
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
						Cycles += 5;
						MainForm.PrintDebugMessage("A LCD interrupt occurred.\n");
					}
					// TODO: Handle other interrupt flags.
				}

				// Prevent cycles overflowing.
				if (Cycles >= Graphics.kCyclesPerFrame)
				{
					Cycles -= Graphics.kCyclesPerFrame;

					// TODO: When is appropriate to sleep for performance?
					Thread.Sleep(1);
				}

				if (StepRequested)
				{
					Playing = false;
					StepRequested = false;
				}
			}
		}

		// Stop the thread.
		public void Stop()
		{
			NeedToStop = true;
		}

		public void Play()
		{
			Playing = true;
		}

		public void Pause()
		{
			Playing = false;
		}

		public void Step()
		{
			StepRequested = true;
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
