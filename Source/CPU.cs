namespace GBSharp
{
	internal class CPU
	{
		private bool NeedToStop;
		private bool Playing;
		private bool StepRequested;

		private bool ShouldPrintOpcodes = false;

		// NOTE: Original DMG CPU frequency is 1.05 MHz.
		// TODO: Support CGB double-speed mode also?
		private const uint Frequency = 4194304;		// cycles per second

		// The accumulator register.
		private byte A;
		// The auxiliary registers.
		// The flag register is actually four flags.
		private byte F;
		private bool Z;		// set to 1 when the result of an operation is 0, otherwise reset
		private bool N;		// set to 1 following execution of the subtraction instruction, regardless of the result
		private bool H;		// set to 1 when an operation results in carrying from or borrowing to bit 3
		private bool CY;	// set to 1 when an operation results in carrying from or borrowing to bit 7
		// BC, DE, and HL are register pairs.
		private byte B;		// higher byte
		private byte C;		// lower byte
		private byte D;
		private byte E;
		private ushort HL;
		// The program counter.
		private ushort PC;
		// The stack pointer.
		private ushort SP;

		// TODO: The port/mode registers.

		// TODO: The bank control registers for CGB?

		// The interrupt flags.
		public bool IF;		// interrupt request flag (also 0xFF0F)
		public bool IE;		// interrupt enable flag (also 0xFFFF)
		private bool IME;	// interrupt master enable flag

		// TODO: The LCD display registers.
		public byte LY;		// LCDC y-coordinate

		// TODO: The sound registers.

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
			Initialize();
		}

		// Reset the CPU's registers and flags.
		private void Initialize()
		{
			NeedToStop = false;
			Playing = false;
			StepRequested = false;

			A = 0x00;
			F = 0xB0;
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

			// The interrupt flags.
			// TODO: Should these just reference their memory address? What about IME?
			IF = false;
			IE = false;
			IME = false;
		}

		// The CPU runs in its own thread.
		public void Run()
		{
			if (ROM.Instance.Data is null)
			{
				return;
			}

			// NOTE: We skip any validation or BIOS handling.
			Thread.CurrentThread.Name = "GB# CPU";
			Initialize();
			uint cycles = 0;
			MainForm.PrintDebugMessage("Initialized.\n");

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

				// TODO: Interrupt handling here?

				byte instruction = ROM.Instance.Data[PC];
				switch (instruction)
				{
					case 0x00:      // NOP
						{
							PrintOpcode(instruction, "NOP");
							PC++;
							cycles++;
						}
						break;

						/*
					case 0x01:      // LD BC, d16
						{
							C = ROM.Instance.Data[PC + 1];
							B = ROM.Instance.Data[PC + 2];
							PC += 3;
							cycles += 3;
						}
						break;
						*/

					case 0x30:		// JR NC, s8
						{
							sbyte s8 = (sbyte)(ROM.Instance.Data[PC + 1] + 2);
							ushort newPC = (ushort)(PC + s8);
							PrintOpcode(instruction, $"JR NC, 0x{newPC:X4}");
							if (!CY)
							{
								PC = newPC;
								cycles += 3;
							}
							else
							{
								PC += 2;
								cycles += 2;
							}
						}
						break;

					case 0x31:      // LD SP, d16
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = (ushort)(ROM.Instance.Data[PC + 2] << 8);
							ushort d16 = (ushort)(higher + lower);
							PrintOpcode(instruction, $"LD SP, 0x{d16:X4}");
							SP = d16;
							PC += 3;
							cycles += 3;
						}
						break;

					case 0x38:      // JR C, s8
						{
							sbyte s8 = (sbyte)(ROM.Instance.Data[PC + 1] + 2);
							ushort newPC = (ushort)(PC + s8);
							PrintOpcode(instruction, $"JR C, 0x{newPC:X4}");
							if (CY)
							{
								PC = newPC;
								cycles += 3;
							}
							else
							{
								PC += 2;
								cycles += 2;
							}
						}
						break;

					case 0x3E:      // LD A, d8
						{
							byte d8 = ROM.Instance.Data[PC + 1];
							PrintOpcode(instruction, $"LD A, 0x{d8:X2}");
							A = d8;
							PC += 2;
							cycles += 2;
						}
						break;

						/*
					case 0x50:      // DEC B
						{
							B--;
							PC++;
							cycles++;
						}
						break;

					case 0x66:      // LD H, (HL)
						{
							ushort memory = (ushort)(Memory.Instance.Read(HL) << 8);
							// NOTE: H is the higher byte of register HL.
							HL = memory;
							PC++;
							cycles += 2;
						}
						break;
						*/

					case 0xC3:      // JP a16
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = (ushort)(ROM.Instance.Data[PC + 2] << 8);
							ushort a16 = (ushort)(higher + lower);
							PrintOpcode(instruction, $"JP 0x{a16:X4}");
							PC = a16;
							cycles += 4;
						}
						break;

					case 0xCD:      // CALL a16
						{
							ushort nextPC = (ushort)(PC + 3);
							byte pcHigher = (byte)((nextPC & 0xFF00) >> 8);
							Memory.Instance.Write(SP - 1, pcHigher);
							byte pcLower = (byte)(nextPC & 0x00FF);
							Memory.Instance.Write(SP - 2, pcLower);
							SP -= 2;
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = (ushort)(ROM.Instance.Data[PC + 2] << 8);
							ushort a16 = (ushort)(higher + lower);
							PrintOpcode(instruction, $"CALL 0x{a16:X4}");
							PC = a16;
							cycles += 6;
						}
						break;

					case 0xE0:      // LD (a8), A
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = 0xFF00;
							PrintOpcode(instruction, $"LD (0x{lower:X2}), A");
							Memory.Instance.Write(higher + lower, A);
							PC += 2;
							cycles += 3;
						}
						break;

					case 0xF0:      // LD A, (a8)
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = 0xFF00;
							PrintOpcode(instruction, $"LD A, (0x{lower:X2})");
							A = Memory.Instance.Read(higher + lower);
							PC += 2;
							cycles += 3;
						}
						break;

					case 0xF3:      // DI
						{
							PrintOpcode(instruction, "DI");
							IME = false;
							PC++;
							cycles++;
						}
						break;

					case 0xFE:      // CP d8
						{
							byte d8 = ROM.Instance.Data[PC + 1];
							PrintOpcode(instruction, $"CP 0x{d8:X2}");
							int cp = A - d8;
							Z = cp == 0;
							CY = cp < 0;
							PC += 2;
							cycles += 2;
						}
						break;

					default:
						MainForm.PrintDebugMessage($"Unimplemented opcode: 0x{instruction:X2}!\n");
						MainForm.Pause();
						break;
				}

				// TODO: Update LCD controller another way?
				// 144 lines at 0.10875 lines per millisecond then 10 lines of v-blank.
				// Every 456 cycles, we increment LY, possibly trigger v-blank, etc.
				const uint cyclesPerLine = (uint)(Frequency / 1000.0f * 0.10875f);
				byte newLY = (byte)(cycles / cyclesPerLine % 154);
				MainForm.PrintDebugStatus("LY: " + LY);
				if (newLY != LY)
				{
					LY = newLY;

					// V-blank begins at line 144 through line 153
					if (LY == 144)
					{
						// TODO: Set the v-blank interrupt flag.
						MainForm.PrintDebugMessage("A v-blank occurred.\n");
					}

					// TODO: When is appropriate to sleep for performance?
					Thread.Sleep(1);
				}

				// TODO: Handle interrupts.
				if (IME)
				{
					// TODO: If IF flags match IE flags, trigger an interrupt.
				}

				// Prevent cycles overflowing.
				if (cycles >= cyclesPerLine * 154)
				{
					cycles -= cyclesPerLine * 154;
				}

				// PC went out of bounds.
				if (PC >= ROM.Instance.Data.Length)
				{
					Playing = false;
					MainForm.PrintDebugMessage("PC went out of bounds!\n");
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

		private void PrintOpcode(byte instruction, string opcode)
		{
			if (ShouldPrintOpcodes)
			{
				MainForm.PrintDebugMessage($"[0x{PC:X4}] 0x{instruction:X2}: " + opcode + "\n");
			}
		}
	}
}
