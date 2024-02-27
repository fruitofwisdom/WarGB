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
		// TODO: Just use the other flags?
		//private byte F;
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
		public byte IF;		// interrupt request flag (also 0xFF0F)
		public byte IE;		// interrupt enable flag (also 0xFFFF)
		private bool IME;	// interrupt master enable flag

		// The LCD display registers.
		public byte LCDC;
		public byte LY;		// LCDC y-coordinate
		// TODO: The other LCD display registers.

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
			//F = 0xB0;		// TODO: Just use the other flags?
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

			LCDC = 0x91;
			LY = 0;
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
					case 0x00:		// NOP
						{
							PrintOpcode(instruction, "NOP");
							PC++;
							cycles++;
						}
						break;

					case 0x01:		// LD BC, d16
						{
							C = ROM.Instance.Data[PC + 1];
							B = ROM.Instance.Data[PC + 2];
							ushort d16 = (ushort)(B << 8 + C);
							PrintOpcode(instruction, $"LD BC, 0x{d16:X4}");
							PC += 3;
							cycles += 3;
						}
						break;

					case 0x0B:		// DEC BC
						{
							ushort bc = (ushort)(B << 8 + C);
							bc--;
							B = (byte)((bc & 0xFF00) >> 8);
							C = (byte)(bc & 0x00FF);
							PrintOpcode(instruction, "DEC BC");
							PC++;
							cycles += 2;
						}
						break;

					case 0x21:		// LD HL, d16
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = (ushort)(ROM.Instance.Data[PC + 2] << 8);
							ushort d16 = (ushort)(higher + lower);
							PrintOpcode(instruction, $"LD HL, 0x{d16:X4}");
							HL = d16;
							PC += 3;
							cycles += 3;
						}
						break;

					case 0x23:		// INC HL
						{
							HL++;
							PrintOpcode(instruction, "INC HL");
							PC++;
							cycles += 2;
						}
						break;

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

					case 0x31:		// LD SP, d16
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

					case 0x38:		// JR C, s8
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

					case 0x3E:		// LD A, d8
						{
							byte d8 = ROM.Instance.Data[PC + 1];
							PrintOpcode(instruction, $"LD A, 0x{d8:X2}");
							A = d8;
							PC += 2;
							cycles += 2;
						}
						break;

					case 0x57:		// LD D, A
						{
							D = A;
							PrintOpcode(instruction, "LD D, A");
							PC++;
							cycles++;
						}
						break;

						/*
					case 0x66:		// LD H, (HL)
						{
							ushort memory = (ushort)(Memory.Instance.Read(HL) << 8);
							// NOTE: H is the higher byte of register HL.
							HL = memory;
							PC++;
							cycles += 2;
						}
						break;
						*/

					case 0x72:		// LD (HL), D
						{
							Memory.Instance.Write(HL, D);
							PrintOpcode(instruction, "LD (HL), D");
							PC++;
							cycles += 2;
						}
						break;

					case 0x78:		// LD A, B
						{
							A = B;
							PrintOpcode(instruction, "LD A, B");
							PC++;
							cycles++;
						}
						break;

					case 0xB1:		// OR C
						{
							A |= C;
							Z = A == 0x00;
							PrintOpcode(instruction, "OR C");
							PC++;
							cycles++;
						}
						break;

					case 0xC3:		// JP a16
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = (ushort)(ROM.Instance.Data[PC + 2] << 8);
							ushort a16 = (ushort)(higher + lower);
							PrintOpcode(instruction, $"JP 0x{a16:X4}");
							PC = a16;
							cycles += 4;
						}
						break;

					case 0xC8:		// RET Z
						{
							PrintOpcode(instruction, "RET Z");
							if (Z)
							{
								byte lower = Memory.Instance.Read(SP);
								SP++;
								ushort higher = (ushort)(Memory.Instance.Read(SP) << 8);
								SP++;
								PC = (ushort)(higher + lower);
								cycles += 5;
							}
							else
							{
								PC++;
								cycles += 2;
							}
						}
						break;

					case 0xC9:		// RET
						{
							byte lower = Memory.Instance.Read(SP);
							SP++;
							ushort higher = (ushort)(Memory.Instance.Read(SP) << 8);
							SP++;
							PrintOpcode(instruction, "RET");
							PC = (ushort)(higher + lower);
							cycles += 4;
						}
						break;

					case 0xCD:		// CALL a16
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

					case 0xE6:		// AND d8
						{
							byte d8 = ROM.Instance.Data[PC + 1];
							PrintOpcode(instruction, $"AND 0x{d8:2}");
							A &= d8;
							PC += 2;
							cycles += 3;
						}
						break;

					case 0xE0:		// LD (a8), A
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = 0xFF00;
							PrintOpcode(instruction, $"LD (0x{lower:X2}), A");
							Memory.Instance.Write(higher + lower, A);
							PC += 2;
							cycles += 3;
						}
						break;

					case 0xF0:		// LD A, (a8)
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = 0xFF00;
							PrintOpcode(instruction, $"LD A, (0x{lower:X2})");
							A = Memory.Instance.Read(higher + lower);
							PC += 2;
							cycles += 3;
						}
						break;

					case 0xF3:		// DI
						{
							PrintOpcode(instruction, "DI");
							IME = false;
							PC++;
							cycles++;
						}
						break;

					case 0xFE:		// CP d8
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
				const uint linesPerFrame = 154;
				byte newLY = (byte)(cycles / cyclesPerLine % linesPerFrame);
				if (newLY != LY)
				{
					LY = newLY;

					// V-blank begins at line 144
					if (LY == 144)
					{
						// Set the v-blank IF flag.
						IF |= 0x01;
					}
				}

				if (IME)
				{
					if ((byte)(IF & 0x01) == 0x01 && (byte)(IE & 0x01) == 0x01)
					{
						// TODO: Handle the v-blank interrupt.
						// NOP();
						// NOP();
						// CALL(0x0040);
						// Should total 5 cycles.
						MainForm.PrintDebugMessage("A v-blank interrupt occurred.\n");
					}
					// TODO: Handle other interrupt flags.
				}

				// Prevent cycles overflowing.
				if (cycles >= cyclesPerLine * linesPerFrame)
				{
					cycles -= cyclesPerLine * linesPerFrame;

					// TODO: When is appropriate to sleep for performance?
					Thread.Sleep(1);
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
