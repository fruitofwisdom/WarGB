using System.Media;

namespace GBSharp
{
	internal class CPU
	{
		public bool Playing;

		// NOTE: Original DMG CPU frequency is 1.05 MHz.
		// TODO: Support CGB double-speed mode also?
		private uint Frequency = 4194304;		// four oscillations per
		//private uint Frequency = 8388608;		// double-speed mode oscillations

		// The AF register.
		private byte A;
		// TODO: This is actually just four flags: Z, N, H, and CY.
		private byte F;
		// The BC register.
		private byte B;		// higher byte
		private byte C;		// lower byte
		// The DE register.
		private byte D;
		private byte E;
		// The HL register.
		private ushort HL;
		// The stack pointer.
		private ushort SP;
		// The program counter.
		private ushort PC;

		// The interrupt flags.
		private bool IF;		// interrupt request flag
		private bool IE;		// interrupt enable flag
		public bool IME;		// interrupt master enable flag

		// TODO: Add DIV, timer (TIMA, TMA, TAC) registers.
		// TODO: Add LCD controller et al?
		// TODO: Add sound synthesis, control, waveform RAM, etc?

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
			A = 0x00;
			F = 0x00;
			B = 0x00;
			C = 0x00;
			D = 0x00;
			E = 0x00;
			HL = 0x0000;
			SP = 0xFFFE;
			PC = 0x0100;

			// The interrupt flags.
			// TODO: Should these just reference their memory address? What about IME?
			IF = false;
			IE = false;
			IME = false;
		}

		public void Play()
		{
			if (ROM.Instance.Data is null)
			{
				return;
			}

			// NOTE: We skip any validation or BIOS handling.
			Initialize();
			Playing = true;
			int cycles = 0;
			while (Playing)
			{
				// TODO: Interrupt handling here?

				byte instruction = ROM.Instance.Data[PC];
				switch (instruction)
				{
					case 0x00:      // NOP
						PC++;
						cycles++;
						break;

						/*
					case 0x01:      // LD BC, d16
						C = ROM.Instance.Data[PC + 1];
						B = ROM.Instance.Data[PC + 2];
						PC += 3;
						cycles += 3;
						break;
						*/

					case 0x31:      // LD SP, d16
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = (ushort)(ROM.Instance.Data[PC + 2] << 8);
							SP = (ushort)(higher + lower);
							PC += 3;
							cycles += 3;
						}
						break;

					case 0x3E:      // LD A, d8
						A = ROM.Instance.Data[PC + 1];
						PC += 2;
						cycles += 2;
						break;

						/*
					case 0x50:      // DEC B
						B--;
						PC++;
						cycles++;
						break;

					case 0x66:      // LD H, (HL)
						ushort memory = (ushort)(Memory.Instance.Read(HL) << 8);
						// NOTE: H is the higher byte of register HL.
						HL = memory;
						PC++;
						cycles += 2;
						break;
						*/

					case 0xC3:      // JP a16
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = (ushort)(ROM.Instance.Data[PC + 2] << 8);
							PC = (ushort)(higher + lower);
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
							PC = (ushort)(higher + lower);
							cycles += 6;
						}
						break;

					case 0xE0:      // LD (a8), A
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = 0xFF00;
							Memory.Instance.Write(higher + lower, A);
							PC += 2;
							cycles += 3;
						}
						break;

					case 0xF0:      // LD A, (a8)
						{
							byte lower = ROM.Instance.Data[PC + 1];
							ushort higher = 0xFF00;
							A = Memory.Instance.Read(higher + lower);
							PC += 2;
							cycles += 3;
						}
						break;

					case 0xF3:      // DI
						IME = false;
						PC++;
						cycles++;
						break;

					default:
						break;
				}

				// PC went out of bounds.
				if (PC >= ROM.Instance.Data.Length)
				{
					Playing = false;
				}

				// TODO: Sleep once we've accumulated enough cycles to match the CPU speed?
				//if (cycles >= Frequency / 60)
				Thread.Sleep(0);
			}
		}
	}
}
