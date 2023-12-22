using System.Media;

namespace GBSharp
{
	internal class CPU
	{
		public bool Playing;

		// NOTE: Original Game Boy CPU frequency.
		private uint Frequency = 4194304;

		// The AF register.
		private byte A;
		// TODO: This is actually just four flags.
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
			// TODO: Initialize to the stack area of memory.
			SP = 0x0000;
			PC = 0x0100;
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
				byte instruction = ROM.Instance.Data[PC];
				switch (instruction)
				{
					case 0x00:      // NOP
						PC++;
						cycles++;
						break;

					case 0x01:      // LD BC, d16
						C = ROM.Instance.Data[PC + 1];
						B = ROM.Instance.Data[PC + 2];
						PC += 3;
						cycles += 3;
						break;

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

					case 0xC3:      // JP a16
						byte lower = ROM.Instance.Data[PC + 1];
						ushort higher = (ushort)(ROM.Instance.Data[PC + 2] << 8);
						PC = (ushort)(higher + lower);
						cycles += 4;
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
