namespace GBSharp
{
	internal partial class CPU
	{
		// Add value to result, potentially setting any F flags appropriately.
		void Add(ref ushort result, ushort value, bool setZ = true, bool setH = true, bool setCY = true)
		{
			ushort newResult = (ushort)(result + value);
			if (setZ)
			{
				Z = newResult == 0x0000;
			}
			N = false;
			// Check for overflow.
			if (setH)
			{
				H = newResult < result;
			}
			if (setCY)
			{
				CY = newResult < result;
			}
			result = newResult;
		}

		// Add value to result, potentially setting any F flags appropriately.
		void Add(ref byte result, byte value, bool setZ = true, bool setH = true, bool setCY = true)
		{
			byte newResult = (byte)(result + value);
			if (setZ)
			{
				Z = newResult == 0x00;
			}
			N = false;
			// Check for overflow.
			if (setH)
			{
				H = newResult < result;
			}
			if (setCY)
			{
				CY = newResult < result;
			}
			result = newResult;
		}

		// Subtract value from result, potentially setting any F flags appropriately.
		void Sub(ref ushort result, ushort value, bool setZ = true, bool setH = true, bool setCY = true)
		{
			ushort newResult = (ushort)(result - value);
			if (setZ)
			{
				Z = newResult == 0x0000;
			}
			N = true;
			// Check for underflow.
			if (setH)
			{
				H = newResult > result;
			}
			if (setCY)
			{
				CY = newResult > result;
			}
			result = newResult;
		}

		// Subtract value from result, potentially setting any F flags appropriately.
		void Sub(ref byte result, byte value, bool setZ = true, bool setH = true, bool setCY = true)
		{
			byte newResult = (byte)(result - value);
			if (setZ)
			{
				Z = newResult == 0x0000;
			}
			N = true;
			// Check for underflow.
			if (setH)
			{
				H = newResult > result;
			}
			if (setCY)
			{
				CY = newResult > result;
			}
			result = newResult;
		}

		// Run an instruction and return how many cycles elapsed.
		private void HandleOpcode(byte instruction, out uint cycles)
		{
			cycles = 0;

			switch (instruction)
			{
				case 0x00:      // NOP
					{
						PrintOpcode(instruction, "NOP");
						PC++;
						cycles++;
					}
					break;

				case 0x01:      // LD BC, d16
					{
						C = Memory.Instance.Read(PC + 1);
						B = Memory.Instance.Read(PC + 2);
						ushort d16 = (ushort)((B << 8) + C);
						PrintOpcode(instruction, $"LD BC, 0x{d16:X4}");
						PC += 3;
						cycles += 3;
					}
					break;

				case 0x03:      // INC BC
					{
						ushort bc = (ushort)((B << 8) + C);
						bc++;
						B = (byte)((bc & 0xFF00) >> 8);
						C = (byte)(bc & 0x00FF);
						PrintOpcode(instruction, "INC BC");
						PC++;
						cycles += 2;
					}
					break;

				case 0x04:      // INC B
					{
						Add(ref B, 1, true, true, false);
						PrintOpcode(instruction, "INC B");
						PC++;
						cycles++;
					}
					break;

				case 0x05:      // DEC B
					{
						Sub(ref B, 1, true, true, false);
						PrintOpcode(instruction, "DEC B");
						PC++;
						cycles++;
					}
					break;

				case 0x06:      // LD B, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						B = d8;
						PrintOpcode(instruction, $"LD B, 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x07:      // RLCA
					{
						CY = (byte)(A & 0x80) == 0x80;
						A = (byte)(A << 1);
						A |= (byte)(CY ? 0x01 : 0x00);
						Z = false;
						N = false;
						H = false;
						PrintOpcode(instruction, "RLCA");
						PC++;
						cycles++;
					}
					break;

				case 0x09:      // ADD HL, BC
					{
						ushort bc = (ushort)((B << 8) + C);
						Add(ref HL, bc, false);
						PrintOpcode(instruction, "ADD HL, BC");
						PC++;
						cycles += 2;
					}
					break;

				case 0x0A:      // LD A, (BC)
					{
						ushort bc = (ushort)((B << 8) + C);
						A = Memory.Instance.Read(bc);
						PrintOpcode(instruction, "LD A, (BC)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x0B:      // DEC BC
					{
						ushort bc = (ushort)((B << 8) + C);
						bc--;
						B = (byte)((bc & 0xFF00) >> 8);
						C = (byte)(bc & 0x00FF);
						PrintOpcode(instruction, "DEC BC");
						PC++;
						cycles += 2;
					}
					break;

				case 0x0C:      // INC C
					{
						Add(ref C, 1, true, true, false);
						PrintOpcode(instruction, "INC C");
						PC++;
						cycles++;
					}
					break;

				case 0x0D:      // DEC C
					{
						Sub(ref C, 1, true, true, false);
						PrintOpcode(instruction, "DEC C");
						PC++;
						cycles++;
					}
					break;

				case 0x0E:      // LD C, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						C = d8;
						PrintOpcode(instruction, $"LD C, 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x0F:      // RCCA
					{
						CY = (byte)(A & 0x01) == 0x01;
						A = (byte)(A >> 1);
						A |= (byte)(CY ? 0x80 : 0x00);
						Z = false;
						N = false;
						H = false;
						PrintOpcode(instruction, "RCCA");
						PC++;
						cycles++;
					}
					break;

				case 0x11:      // LD DE, d16
					{
						E = Memory.Instance.Read(PC + 1);
						D = Memory.Instance.Read(PC + 2);
						ushort d16 = (ushort)((D << 8) + E);
						PrintOpcode(instruction, $"LD DE, 0x{d16:X4}");
						PC += 3;
						cycles += 3;
					}
					break;

				case 0x12:      // LD (DE), A
					{
						ushort de = (ushort)((D << 8) + E);
						Memory.Instance.Write(de, A);
						PrintOpcode(instruction, "LD (DE), A");
						PC++;
						cycles += 2;
					}
					break;

				case 0x13:      // INC DE
					{
						ushort de = (ushort)((D << 8) + E);
						de++;
						D = (byte)((de & 0xFF00) >> 8);
						E = (byte)(de & 0x00FF);
						PrintOpcode(instruction, "INC DE");
						PC++;
						cycles += 2;
					}
					break;

				case 0x14:      // INC D
					{
						Add(ref D, 1, true, true, false);
						PrintOpcode(instruction, "INC D");
						PC++;
						cycles++;
					}
					break;

				case 0x15:      // DEC D
					{
						Sub(ref D, 1, true, true, false);
						PrintOpcode(instruction, "DEC D");
						PC++;
						cycles++;
					}
					break;

				case 0x16:      // LD D, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						D = d8;
						PrintOpcode(instruction, $"LD D, 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x18:      // JR s8
					{
						sbyte s8 = (sbyte)(Memory.Instance.Read(PC + 1) + 2);
						ushort newPC = (ushort)(PC + s8);
						PrintOpcode(instruction, $"JR 0x{newPC:X4}");
						PC = newPC;
						cycles += 3;
					}
					break;

				case 0x19:      // ADD HL, DE
					{
						ushort de = (ushort)((D << 8) + E);
						Add(ref HL, de, false);
						PrintOpcode(instruction, "ADD HL, DE");
						PC++;
						cycles += 2;
					}
					break;

				case 0x1A:      // LD A, (DE)
					{
						ushort de = (ushort)((D << 8) + E);
						A = Memory.Instance.Read(de);
						PrintOpcode(instruction, "LD A, (DE)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x1C:      // INC E
					{
						Add(ref E, 1, true, true, false);
						PrintOpcode(instruction, "INC E");
						PC++;
						cycles++;
					}
					break;

				case 0x1D:      // DEC E
					{
						Sub(ref E, 1, true, true, false);
						PrintOpcode(instruction, "DEC E");
						PC++;
						cycles++;
					}
					break;

				case 0x1E:      // LD E, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						E = d8;
						PrintOpcode(instruction, $"LD E, 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x20:      // JR NZ, s8
					{
						sbyte s8 = (sbyte)(Memory.Instance.Read(PC + 1) + 2);
						ushort newPC = (ushort)(PC + s8);
						PrintOpcode(instruction, $"JR NZ, 0x{newPC:X4}");
						if (!Z)
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

				case 0x21:      // LD HL, d16
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort d16 = (ushort)(higher + lower);
						HL = d16;
						PrintOpcode(instruction, $"LD HL, 0x{d16:X4}");
						PC += 3;
						cycles += 3;
					}
					break;

				case 0x22:      // LD (HL+), A
					{
						Memory.Instance.Write(HL, A);
						HL++;
						PrintOpcode(instruction, "LD (HL+), A");
						PC++;
						cycles += 2;
					}
					break;

				case 0x23:      // INC HL
					{
						HL++;
						PrintOpcode(instruction, "INC HL");
						PC++;
						cycles += 2;
					}
					break;

				case 0x26:      // LD H, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						byte h = d8;
						byte l = (byte)(HL & 0x00FF);
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, $"LD H, 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x28:      // JR Z, s8
					{
						sbyte s8 = (sbyte)(Memory.Instance.Read(PC + 1) + 2);
						ushort newPC = (ushort)(PC + s8);
						PrintOpcode(instruction, $"JR Z, 0x{newPC:X4}");
						if (Z)
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

				case 0x29:      // ADD HL, HL
					{
						Add(ref HL, HL, false, true, true);
						PrintOpcode(instruction, "ADD HL, HL");
						PC++;
						cycles += 2;
					}
					break;

				case 0x2A:      // LD A, (HL+)
					{
						A = Memory.Instance.Read(HL);
						HL++;
						PrintOpcode(instruction, "LD A, (HL+)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x2B:      // DEC HL
					{
						HL--;
						PrintOpcode(instruction, "DEC HL");
						PC++;
						cycles += 2;
					}
					break;

				case 0x2F:      // CPL
					{
						A = (byte)(~A);
						N = true;
						H = true;
						PrintOpcode(instruction, "CPL");
						PC++;
						cycles++;
					}
					break;

				case 0x30:      // JR NC, s8
					{
						sbyte s8 = (sbyte)(Memory.Instance.Read(PC + 1) + 2);
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
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort d16 = (ushort)(higher + lower);
						SP = d16;
						PrintOpcode(instruction, $"LD SP, 0x{d16:X4}");
						PC += 3;
						cycles += 3;
					}
					break;

				case 0x32:      // LD (HL-), A
					{
						Memory.Instance.Write(HL, A);
						HL--;
						PrintOpcode(instruction, "LD (HL-), A");
						PC++;
						cycles += 2;
					}
					break;

				case 0x34:      // INC (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Add(ref d8, 1, true, true, false);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "INC (HL)");
						PC++;
						cycles += 3;
					}
					break;

				case 0x35:      // DEC (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Sub(ref d8, 1, true, true, false);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "DEC (HL)");
						PC++;
						cycles += 3;
					}
					break;

				case 0x36:      // LD (HL), d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, $"LD (HL), 0x{d8:2}");
						PC += 2;
						cycles += 3;
					}
					break;

				case 0x37:      // SCF
					{
						N = false;
						H = false;
						CY = true;
						PrintOpcode(instruction, "SCF");
						PC++;
						cycles++;
					}
					break;

				case 0x38:      // JR C, s8
					{
						sbyte s8 = (sbyte)(Memory.Instance.Read(PC + 1) + 2);
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

				case 0x3A:      // LD A, (HL-)
					{
						A = Memory.Instance.Read(HL);
						HL--;
						PrintOpcode(instruction, "LD A, (HL-)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x3C:      // INC A
					{
						Add(ref A, 1, true, true, false);
						PrintOpcode(instruction, "INC A");
						PC++;
						cycles++;
					}
					break;

				case 0x3D:      // DEC A
					{
						Sub(ref A, 1, true, true, false);
						PrintOpcode(instruction, "DEC A");
						PC++;
						cycles++;
					}
					break;

				case 0x3E:      // LD A, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						A = d8;
						PrintOpcode(instruction, $"LD A, 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x3F:      // CCF
					{
						N = false;
						H = false;
						CY = !CY;
						PrintOpcode(instruction, "CCF");
						PC++;
						cycles++;
					}
					break;

				case 0x41:      // LD B, C
					{
						B = C;
						PrintOpcode(instruction, "LD B, C");
						PC++;
						cycles++;
					}
					break;

				case 0x44:      // LD B, H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						B = h;
						PrintOpcode(instruction, "LD B, H");
						PC++;
						cycles++;
					}
					break;

				case 0x45:      // LD B, L
					{
						byte l = (byte)(HL & 0x00FF);
						B = l;
						PrintOpcode(instruction, "LD B, L");
						PC++;
						cycles++;
					}
					break;

				case 0x46:      // LD B, (HL)
					{
						B = Memory.Instance.Read(HL);
						PrintOpcode(instruction, "LD B, (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x47:      // LD B, A
					{
						B = A;
						PrintOpcode(instruction, "LD B, A");
						PC++;
						cycles++;
					}
					break;

				case 0x48:      // LD C, B
					{
						C = B;
						PrintOpcode(instruction, "LD C, B");
						PC++;
						cycles++;
					}
					break;

				case 0x4B:      // LD C, E
					{
						C = E;
						PrintOpcode(instruction, "LD C, E");
						PC++;
						cycles++;
					}
					break;

				case 0x4D:      // LD C, L
					{
						byte l = (byte)(HL & 0x00FF);
						C = l;
						PrintOpcode(instruction, "LD C, L");
						PC++;
						cycles++;
					}
					break;

				case 0x4E:      // LD C, (HL)
					{
						C = Memory.Instance.Read(HL);
						PrintOpcode(instruction, "LD C, (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x4F:      // LD C, A
					{
						C = A;
						PrintOpcode(instruction, "LD C, A");
						PC++;
						cycles++;
					}
					break;

				case 0x54:      // LD D, H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						D = h;
						PrintOpcode(instruction, "LD D, H");
						PC++;
						cycles++;
					}
					break;

				case 0x56:      // LD D, (HL)
					{
						D = Memory.Instance.Read(HL);
						PrintOpcode(instruction, "LD D, (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x57:      // LD D, A
					{
						D = A;
						PrintOpcode(instruction, "LD D, A");
						PC++;
						cycles++;
					}
					break;

				case 0x58:      // LD E, B
					{
						E = B;
						PrintOpcode(instruction, "LD E, B");
						PC++;
						cycles++;
					}
					break;

				case 0x59:      // LD E, C
					{
						E = C;
						PrintOpcode(instruction, "LD E, C");
						PC++;
						cycles++;
					}
					break;

				case 0x5A:      // LD E, D
					{
						E = D;
						PrintOpcode(instruction, "LD E, D");
						PC++;
						cycles++;
					}
					break;

				case 0x5D:      // LD E, L
					{
						byte l = (byte)(HL & 0x00FF);
						E = l;
						PrintOpcode(instruction, "LD E, L");
						PC++;
						cycles++;
					}
					break;

				case 0x5E:      // LD E, (HL)
					{
						E = Memory.Instance.Read(HL);
						PrintOpcode(instruction, "LD E, (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x5F:      // LD E, A
					{
						E = A;
						PrintOpcode(instruction, "LD E, A");
						PC++;
						cycles++;
					}
					break;

				case 0x61:      // LD H, C
					{
						byte h = C;
						byte l = (byte)(HL & 0x00FF);
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD H, C");
						PC++;
						cycles++;
					}
					break;

				case 0x66:      // LD H, (HL)
					{
						byte h = Memory.Instance.Read(HL);
						byte l = (byte)(HL & 0x00FF);
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD H, (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x67:      // LD H, A
					{
						byte h = A;
						byte l = (byte)(HL & 0x00FF);
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD H, A");
						PC++;
						cycles++;
					}
					break;

				case 0x68:      // LD L, B
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = B;
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD L, B");
						PC++;
						cycles++;
					}
					break;

				case 0x69:      // LD L, C
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = C;
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD L, C");
						PC++;
						cycles++;
					}
					break;

				case 0x6A:      // LD L, D
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = D;
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD L, D");
						PC++;
						cycles++;
					}
					break;

				case 0x6B:      // LD L, E
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = E;
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD L, E");
						PC++;
						cycles++;
					}
					break;

				case 0x6E:      // LD L, (HL)
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = Memory.Instance.Read(HL);
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD L, (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x6F:      // LD L, A
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = A;
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD L, A");
						PC++;
						cycles++;
					}
					break;

				case 0x70:      // LD (HL), B
					{
						Memory.Instance.Write(HL, B);
						PrintOpcode(instruction, "LD (HL), B");
						PC++;
						cycles += 2;
					}
					break;

				case 0x71:      // LD (HL), C
					{
						Memory.Instance.Write(HL, C);
						PrintOpcode(instruction, "LD (HL), C");
						PC++;
						cycles += 2;
					}
					break;

				case 0x72:      // LD (HL), D
					{
						Memory.Instance.Write(HL, D);
						PrintOpcode(instruction, "LD (HL), D");
						PC++;
						cycles += 2;
					}
					break;

				case 0x73:      // LD (HL), E
					{
						Memory.Instance.Write(HL, E);
						PrintOpcode(instruction, "LD (HL), E");
						PC++;
						cycles += 2;
					}
					break;

				case 0x76:      // HALT
					{
						_halted = true;
						PrintOpcode(instruction, "HALT");
						PC++;
						cycles++;
					}
					break;

				case 0x77:      // LD (HL), A
					{
						Memory.Instance.Write(HL, A);
						PrintOpcode(instruction, "LD (HL), A");
						PC++;
						cycles += 2;
					}
					break;

				case 0x78:      // LD A, B
					{
						A = B;
						PrintOpcode(instruction, "LD A, B");
						PC++;
						cycles++;
					}
					break;

				case 0x79:      // LD A, C
					{
						A = C;
						PrintOpcode(instruction, "LD A, C");
						PC++;
						cycles++;
					}
					break;

				case 0x7A:      // LD A, D
					{
						A = D;
						PrintOpcode(instruction, "LD A, D");
						PC++;
						cycles++;
					}
					break;

				case 0x7B:      // LD A, E
					{
						A = E;
						PrintOpcode(instruction, "LD A, E");
						PC++;
						cycles++;
					}
					break;

				case 0x7C:      // LD A, H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						A = h;
						PrintOpcode(instruction, "LD A, H");
						PC++;
						cycles++;
					}
					break;

				case 0x7D:      // LD A, L
					{
						byte l = (byte)(HL & 0x00FF);
						A = l;
						PrintOpcode(instruction, "LD A, L");
						PC++;
						cycles++;
					}
					break;

				case 0x7E:      // LD A, (HL)
					{
						A = Memory.Instance.Read(HL);
						PrintOpcode(instruction, "LD A, (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x80:      // ADD A, B
					{
						Add(ref A, B);
						PrintOpcode(instruction, "ADD A, B");
						PC++;
						cycles++;
					}
					break;

				case 0x81:      // ADD A, C
					{
						Add(ref A, C);
						PrintOpcode(instruction, "ADD A, C");
						PC++;
						cycles++;
					}
					break;

				case 0x82:      // ADD A, D
					{
						Add(ref A, D);
						PrintOpcode(instruction, "ADD A, D");
						PC++;
						cycles++;
					}
					break;

				case 0x83:      // ADD A, E
					{
						Add(ref A, E);
						PrintOpcode(instruction, "ADD A, E");
						PC++;
						cycles++;
					}
					break;

				case 0x84:      // ADD A, H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						Add(ref A, h);
						PrintOpcode(instruction, "ADD A, H");
						PC++;
						cycles++;
					}
					break;

				case 0x85:      // ADD A, L
					{
						byte l = (byte)(HL & 0x00FF);
						Add(ref A, l);
						PrintOpcode(instruction, "ADD A, L");
						PC++;
						cycles++;
					}
					break;

				case 0x86:      // ADD A, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Add(ref A, d8);
						PrintOpcode(instruction, "ADD A, (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0x87:      // ADD A, A
					{
						Add(ref A, A);
						PrintOpcode(instruction, "ADD A, A");
						PC++;
						cycles++;
					}
					break;

				case 0x90:      // SUB B
					{
						Sub(ref A, B);
						PrintOpcode(instruction, "SUB B");
						PC++;
						cycles++;
					}
					break;

				case 0x91:      // SUB C
					{
						Sub(ref A, C);
						PrintOpcode(instruction, "SUB C");
						PC++;
						cycles++;
					}
					break;

				case 0x92:      // SUB D
					{
						Sub(ref A, D);
						PrintOpcode(instruction, "SUB D");
						PC++;
						cycles++;
					}
					break;

				case 0xA0:      // AND B
					{
						A &= B;
						Z = A == 0x00;
						N = false;
						H = true;
						CY = false;
						PrintOpcode(instruction, "AND B");
						PC++;
						cycles++;
					}
					break;

				case 0xA1:      // AND C
					{
						A &= C;
						Z = A == 0x00;
						N = false;
						H = true;
						CY = false;
						PrintOpcode(instruction, "AND C");
						PC++;
						cycles++;
					}
					break;

				case 0xA6:      // AND (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						A &= d8;
						Z = A == 0x00;
						N = false;
						H = true;
						CY = false;
						PrintOpcode(instruction, "AND (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0xA7:      // AND A
					{
						A &= A;
						Z = A == 0x00;
						N = false;
						H = true;
						CY = false;
						PrintOpcode(instruction, "AND A");
						PC++;
						cycles++;
					}
					break;

				case 0xA9:      // XOR C
					{
						A ^= C;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "XOR C");
						PC++;
						cycles++;
					}
					break;

				case 0xAA:      // XOR D
					{
						A ^= D;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "XOR D");
						PC++;
						cycles++;
					}
					break;

				case 0xAE:      // XOR (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						A ^= d8;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "XOR (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0xAF:      // XOR A
					{
						A ^= A;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "XOR A");
						PC++;
						cycles++;
					}
					break;

				case 0xB0:      // OR B
					{
						A |= B;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "OR B");
						PC++;
						cycles++;
					}
					break;

				case 0xB1:      // OR C
					{
						A |= C;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "OR C");
						PC++;
						cycles++;
					}
					break;

				case 0xB2:      // OR D
					{
						A |= D;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "OR D");
						PC++;
						cycles++;
					}
					break;

				case 0xB3:      // OR E
					{
						A |= E;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "OR E");
						PC++;
						cycles++;
					}
					break;

				case 0xB4:      // OR H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						A |= h;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "OR H");
						PC++;
						cycles++;
					}
					break;

				case 0xB5:      // OR L
					{
						byte l = (byte)(HL & 0x00FF);
						A |= l;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "OR L");
						PC++;
						cycles++;
					}
					break;

				case 0xB6:      // OR (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						A |= d8;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "OR (HL)");
						PC++;
						cycles++;
					}
					break;

				case 0xB7:      // OR A
					{
						A |= A;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "OR A");
						PC++;
						cycles++;
					}
					break;

				case 0xB8:      // CP B
					{
						int cp = A - B;
						Z = cp == 0;
						N = true;
						H = cp < 0;
						CY = cp < 0;
						PrintOpcode(instruction, "CP B");
						PC++;
						cycles++;
					}
					break;

				case 0xB9:      // CP C
					{
						int cp = A - C;
						Z = cp == 0;
						N = true;
						H = cp < 0;
						CY = cp < 0;
						PrintOpcode(instruction, "CP C");
						PC++;
						cycles++;
					}
					break;

				case 0xBA:      // CP D
					{
						int cp = A - D;
						Z = cp == 0;
						N = true;
						H = cp < 0;
						CY = cp < 0;
						PrintOpcode(instruction, "CP D");
						PC++;
						cycles++;
					}
					break;

				case 0xBB:      // CP E
					{
						int cp = A - E;
						Z = cp == 0;
						N = true;
						H = cp < 0;
						CY = cp < 0;
						PrintOpcode(instruction, "CP E");
						PC++;
						cycles++;
					}
					break;

				case 0xBC:      // CP H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						int cp = A - h;
						Z = cp == 0;
						N = true;
						H = cp < 0;
						CY = cp < 0;
						PrintOpcode(instruction, "CP H");
						PC++;
						cycles++;
					}
					break;

				case 0xBD:      // CP L
					{
						byte l = (byte)(HL & 0x00FF);
						int cp = A - l;
						Z = cp == 0;
						N = true;
						H = cp < 0;
						CY = cp < 0;
						PrintOpcode(instruction, "CP L");
						PC++;
						cycles++;
					}
					break;

				case 0xBE:      // CP (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						int cp = A - d8;
						Z = cp == 0;
						N = true;
						H = cp < 0;
						CY = cp < 0;
						PrintOpcode(instruction, "CP (HL)");
						PC++;
						cycles += 2;
					}
					break;

				case 0xC0:      // RET NZ
					{
						PrintOpcode(instruction, "RET NZ");
						if (!Z)
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

				case 0xC1:      // POP BC
					{
						C = Memory.Instance.Read(SP);
						SP++;
						B = Memory.Instance.Read(SP);
						SP++;
						PrintOpcode(instruction, "POP BC");
						PC++;
						cycles += 3;
					}
					break;

				case 0xC2:      // JP NZ, a16
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						PrintOpcode(instruction, $"JP NZ, 0x{a16:4}");
						if (!Z)
						{
							PC = a16;
							cycles += 4;
						}
						else
						{
							PC += 3;
							cycles += 3;
						}
					}
					break;

				case 0xC3:      // JP a16
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						PrintOpcode(instruction, $"JP 0x{a16:X4}");
						PC = a16;
						cycles += 4;
					}
					break;

				case 0xC4:      // CALL NZ, a16
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						PrintOpcode(instruction, $"CALL NZ, 0x{a16:X4}");
						if (!Z)
						{
							ushort nextPC = (ushort)(PC + 3);
							byte pcHigher = (byte)((nextPC & 0xFF00) >> 8);
							Memory.Instance.Write(SP - 1, pcHigher);
							byte pcLower = (byte)(nextPC & 0x00FF);
							Memory.Instance.Write(SP - 2, pcLower);
							SP -= 2;
							PC = a16;
							cycles += 6;
						}
						else
						{
							PC += 3;
							cycles += 3;
						}
					}
					break;

				case 0xC5:      // PUSH BC
					{
						SP--;
						Memory.Instance.Write(SP, B);
						SP--;
						Memory.Instance.Write(SP, C);
						PrintOpcode(instruction, "PUSH BC");
						PC++;
						cycles += 4;
					}
					break;

				case 0xC6:      // ADD A, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						Add(ref A, d8);
						PrintOpcode(instruction, $"ADD A, 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0xC8:      // RET Z
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

				case 0xC9:      // RET
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

				case 0xCA:      // JP Z, a16
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						PrintOpcode(instruction, $"JP Z, 0x{a16:4}");
						if (Z)
						{
							PC = a16;
							cycles += 4;
						}
						else
						{
							PC += 3;
							cycles += 3;
						}
					}
					break;

				case 0xCB:      // NOTE: CB is a prefix for additional opcodes.
					{
						byte nextInstruction = Memory.Instance.Read(PC + 1);
						Handle16BitOpcode(nextInstruction, out cycles);
					}
					break;

				case 0xCC:      // CALL Z, a16
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						PrintOpcode(instruction, $"CALL Z, 0x{a16:X4}");
						if (Z)
						{
							ushort nextPC = (ushort)(PC + 3);
							byte pcHigher = (byte)((nextPC & 0xFF00) >> 8);
							Memory.Instance.Write(SP - 1, pcHigher);
							byte pcLower = (byte)(nextPC & 0x00FF);
							Memory.Instance.Write(SP - 2, pcLower);
							SP -= 2;
							PC = a16;
							cycles += 6;
						}
						else
						{
							PC += 3;
							cycles += 3;
						}
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
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						PrintOpcode(instruction, $"CALL 0x{a16:X4}");
						PC = a16;
						cycles += 6;
					}
					break;

				case 0xCE:      // ADC A, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						d8 += (byte)(CY ? 1 : 0);
						Add(ref A, d8);
						PrintOpcode(instruction, "ADC A, 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0xD0:      // RET NC
					{
						PrintOpcode(instruction, "RET NC");
						if (!CY)
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

				case 0xD1:      // POP DE
					{
						E = Memory.Instance.Read(SP);
						SP++;
						D = Memory.Instance.Read(SP);
						SP++;
						PrintOpcode(instruction, "POP DE");
						PC++;
						cycles += 3;
					}
					break;

				case 0xD4:      // CALL NC, a16
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						PrintOpcode(instruction, $"CALL NC, 0x{a16:X4}");
						if (!CY)
						{
							ushort nextPC = (ushort)(PC + 3);
							byte pcHigher = (byte)((nextPC & 0xFF00) >> 8);
							Memory.Instance.Write(SP - 1, pcHigher);
							byte pcLower = (byte)(nextPC & 0x00FF);
							Memory.Instance.Write(SP - 2, pcLower);
							SP -= 2;
							PC = a16;
							cycles += 6;
						}
						else
						{
							PC += 3;
							cycles += 3;
						}
					}
					break;

				case 0xD5:      // PUSH DE
					{
						SP--;
						Memory.Instance.Write(SP, D);
						SP--;
						Memory.Instance.Write(SP, E);
						PrintOpcode(instruction, "PUSH DE");
						PC++;
						cycles += 4;
					}
					break;

				case 0xD6:      // SUB d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						Sub(ref A, d8);
						PrintOpcode(instruction, "SUB 0x{d8:2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0xD8:      // RET C
					{
						PrintOpcode(instruction, "RET C");
						if (CY)
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

				case 0xD9:      // RETI
					{
						byte lower = Memory.Instance.Read(SP);
						SP++;
						ushort higher = (ushort)(Memory.Instance.Read(SP) << 8);
						SP++;
						IME = true;
						PrintOpcode(instruction, "RETI");
						PC = (ushort)(higher + lower);
						cycles += 4;
					}
					break;

				case 0xDA:      // JP C, a16
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						PrintOpcode(instruction, $"JP C, 0x{a16:4}");
						if (CY)
						{
							PC = a16;
							cycles += 4;
						}
						else
						{
							PC += 3;
							cycles += 3;
						}
					}
					break;

				case 0xE0:      // LD (a8), A
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = 0xFF00;
						Memory.Instance.Write(higher + lower, A);
						PrintOpcode(instruction, $"LD (0x{lower:X2}), A");
						PC += 2;
						cycles += 3;
					}
					break;

				case 0xE1:      // POP HL
					{
						byte l = Memory.Instance.Read(SP);
						SP++;
						byte h = Memory.Instance.Read(SP);
						SP++;
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "POP HL");
						PC++;
						cycles += 3;
					}
					break;

				case 0xE2:      // LD (C), A
					{
						byte lower = C;
						ushort higher = 0xFF00;
						ushort a16 = (ushort)(higher + lower);
						Memory.Instance.Write(a16, A);
						PrintOpcode(instruction, $"LD (0x{a16:X4}), A");
						PC++;
						cycles += 2;
					}
					break;

				case 0xE5:      // PUSH HL
					{
						SP--;
						byte h = (byte)((HL & 0xFF00) >> 8);
						Memory.Instance.Write(SP, h);
						SP--;
						byte l = (byte)(HL & 0x00FF);
						Memory.Instance.Write(SP, l);
						PrintOpcode(instruction, "PUSH HL");
						PC++;
						cycles += 4;
					}
					break;

				case 0xE6:      // AND d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						A &= d8;
						Z = A == 0x00;
						N = false;
						H = true;
						CY = false;
						PrintOpcode(instruction, $"AND 0x{d8:2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0xE9:      // JP HL
					{
						PrintOpcode(instruction, $"JP HL");
						PC = HL;
						cycles++;
					}
					break;

				case 0xEA:      // LD (a16), A
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						Memory.Instance.Write(a16, A);
						PrintOpcode(instruction, $"LD (0x{a16:X4}), A");
						PC += 3;
						cycles += 4;
					}
					break;

				case 0xEE:      // XOR d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						A ^= d8;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "XOR 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0xF0:      // LD A, (a8)
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = 0xFF00;
						A = Memory.Instance.Read(higher + lower);
						PrintOpcode(instruction, $"LD A, (0x{lower:X2})");
						PC += 2;
						cycles += 3;
					}
					break;

				case 0xF1:      // POP AF
					{
						byte f = Memory.Instance.Read(SP);
						SetF(f);
						SP++;
						A = Memory.Instance.Read(SP);
						SP++;
						PrintOpcode(instruction, "POP AF");
						PC++;
						cycles += 3;
					}
					break;

				case 0xF2:      // LD A, (C)
					{
						byte lower = C;
						ushort higher = 0xFF00;
						ushort a16 = (ushort)(higher + lower);
						A = Memory.Instance.Read(a16);
						PrintOpcode(instruction, $"LD A, (0x{a16:X4})");
						PC++;
						cycles += 2;
					}
					break;

				case 0xF3:      // DI
					{
						IME = false;
						PrintOpcode(instruction, "DI");
						PC++;
						cycles++;
					}
					break;

				case 0xF5:      // PUSH AF
					{
						SP--;
						Memory.Instance.Write(SP, A);
						SP--;
						byte f = GetF();
						Memory.Instance.Write(SP, f);
						PrintOpcode(instruction, "PUSH AF");
						PC++;
						cycles += 4;
					}
					break;

				case 0xF6:      // OR d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						A |= d8;
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, $"OR 0x{d8:2}");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0xF8:      // LD HL, SP+s8
					{
						sbyte s8 = (sbyte)Memory.Instance.Read(PC + 1);
						HL = (ushort)(SP + s8);
						Z = false;
						N = false;
						H = s8 < 0;
						CY = s8 < 0;
						PrintOpcode(instruction, $"LD HL, SP+0x{s8:2}");
						PC += 2;
						cycles += 3;
					}
					break;

				case 0xF9:      // LD SP, HL
					{
						SP = HL;
						PrintOpcode(instruction, "LD SP, HL");
						PC++;
						cycles += 2;
					}
					break;

				case 0xFA:      // LD A, (a16)
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						A = Memory.Instance.Read(a16);
						PrintOpcode(instruction, $"LD A, (0x{a16:X4})");
						PC += 3;
						cycles += 4;
					}
					break;

				case 0xFB:      // EI
					{
						IME = true;
						PrintOpcode(instruction, "EI");
						PC++;
						cycles++;
					}
					break;

				case 0xFE:      // CP d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						int cp = A - d8;
						Z = cp == 0;
						N = true;
						H = cp < 0;
						CY = cp < 0;
						PrintOpcode(instruction, $"CP 0x{d8:X2}");
						PC += 2;
						cycles += 2;
					}
					break;

				default:
					{
						GameBoy.DebugOutput += $"[0x{PC:X4}] Unimplemented opcode: 0x{instruction:X2}!\n";
						MainForm.Pause();
					}
					break;
			}
		}

		// Run a 16-bit instruction and return how many cycles elapsed.
		private void Handle16BitOpcode(byte instruction, out uint cycles)
		{
			cycles = 0;

			switch (instruction)
			{
				case 0x11:      // RL C
					{
						C = (byte)(C << 1);
						C |= (byte)(CY ? 0x01 : 0x00);
						Z = C == 0x00;
						N = false;
						H = false;
						PrintOpcode(instruction, "RL C");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x22:      // SLA D
					{
						CY = (byte)(D & 0x80) == 0x80;
						D = (byte)(D << 1);
						Z = D == 0x00;
						N = false;
						H = false;
						PrintOpcode(instruction, "SLA D");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x23:      // SLA E
					{
						CY = (byte)(E & 0x80) == 0x80;
						E = (byte)(E << 1);
						Z = E == 0x00;
						N = false;
						H = false;
						PrintOpcode(instruction, "SLA E");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x24:      // SLA H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = (byte)(HL & 0x00FF);
						CY = (byte)(h & 0x80) == 0x80;
						h = (byte)(h << 1);
						HL = (ushort)((h << 8) + l);
						Z = h == 0x00;
						N = false;
						H = false;
						PrintOpcode(instruction, "SLA H");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x25:      // SLA L
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = (byte)(HL & 0x00FF);
						CY = (byte)(l & 0x80) == 0x80;
						l = (byte)(l << 1);
						HL = (ushort)((h << 8) + l);
						Z = l == 0x00;
						N = false;
						H = false;
						PrintOpcode(instruction, "SLA L");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x37:      // SWAP A
					{
						byte lower = (byte)(A & 0x0F);
						byte higher = (byte)(A & 0xF0);
						A = (byte)((higher >> 4) + (lower << 4));
						Z = A == 0x00;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "SWAP A");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x38:      // SRL B
					{
						CY = (byte)(B & 0x01) == 0x01;
						B = (byte)(B >> 1);
						Z = B == 0x00;
						N = false;
						H = false;
						PrintOpcode(instruction, "SRL B");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x3A:      // SRL D
					{
						CY = (byte)(D & 0x01) == 0x01;
						D = (byte)(D >> 1);
						Z = D == 0x00;
						N = false;
						H = false;
						PrintOpcode(instruction, "SRL D");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x3B:      // SRL E
					{
						CY = (byte)(E & 0x01) == 0x01;
						E = (byte)(E >> 1);
						Z = E == 0x00;
						N = false;
						H = false;
						PrintOpcode(instruction, "SRL E");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x3F:      // SRL A
					{
						CY = (byte)(A & 0x01) == 0x01;
						A = (byte)(A >> 1);
						Z = A == 0x00;
						N = false;
						H = false;
						PrintOpcode(instruction, "SRL A");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x47:      // BIT 0, A
					{
						byte bit = Utilities.GetBitsFromByte(A, 0, 0);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 0, A");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x4E:      // BIT 1, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						byte bit = Utilities.GetBitsFromByte(d8, 1, 1);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 1, (HL)");
						PC += 2;
						cycles += 3;
					}
					break;

				case 0x4F:      // BIT 1, A
					{
						byte bit = Utilities.GetBitsFromByte(A, 1, 1);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 1, A");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x57:      // BIT 2, A
					{
						byte bit = Utilities.GetBitsFromByte(A, 2, 2);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 2, A");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x5F:      // BIT 3, A
					{
						byte bit = Utilities.GetBitsFromByte(A, 3, 3);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 3, A");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x74:      // BIT 6, H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte bit = Utilities.GetBitsFromByte(h, 6, 6);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 6, H");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x76:      // BIT 6, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						byte bit = Utilities.GetBitsFromByte(d8, 6, 6);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 6, (HL)");
						PC += 2;
						cycles += 3;
					}
					break;

				case 0x77:      // BIT 6, A
					{
						byte bit = Utilities.GetBitsFromByte(A, 6, 6);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 6, A");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x7A:      // BIT 7, D
					{
						byte bit = Utilities.GetBitsFromByte(D, 7, 7);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 7, D");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x7B:      // BIT 7, E
					{
						byte bit = Utilities.GetBitsFromByte(E, 7, 7);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 7, E");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x7C:      // BIT 7, H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte bit = Utilities.GetBitsFromByte(h, 7, 7);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 7, H");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x7D:      // BIT 7, L
					{
						byte l = (byte)(HL & 0x00FF);
						byte bit = Utilities.GetBitsFromByte(l, 7, 7);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 7, L");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x7E:      // BIT 7, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						byte bit = Utilities.GetBitsFromByte(d8, 7, 7);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 7, (HL)");
						PC += 2;
						cycles += 3;
					}
					break;

				case 0x7F:      // BIT 7, A
					{
						byte bit = Utilities.GetBitsFromByte(A, 7, 7);
						Z = bit == 0x00;
						N = false;
						H = true;
						PrintOpcode(instruction, "BIT 7, A");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0x86:      // RES 0, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Utilities.SetBitsInByte(ref d8, 0x00, 0, 0);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "RES 0, (HL)");
						PC += 2;
						cycles += 4;
					}
					break;

				case 0x8E:      // RES 1, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Utilities.SetBitsInByte(ref d8, 0x00, 1, 1);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "RES 1, (HL)");
						PC += 2;
						cycles += 4;
					}
					break;

				case 0x96:      // RES 2, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Utilities.SetBitsInByte(ref d8, 0x00, 2, 2);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "RES 2, (HL)");
						PC += 2;
						cycles += 4;
					}
					break;

				case 0x9E:      // RES 3, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Utilities.SetBitsInByte(ref d8, 0x00, 3, 3);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "RES 3, (HL)");
						PC += 2;
						cycles += 4;
					}
					break;

				case 0xBC:      // RES 7, H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = (byte)(HL & 0x00FF);
						Utilities.SetBitsInByte(ref h, 0x00, 7, 7);
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "RES 7, H");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0xBE:      // RES 7, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Utilities.SetBitsInByte(ref d8, 0x00, 7, 7);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "RES 7, (HL)");
						PC += 2;
						cycles += 4;
					}
					break;

				case 0xCE:      // SET 1, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Utilities.SetBitsInByte(ref d8, 0x01, 1, 1);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "SET 1, (HL)");
						PC += 2;
						cycles += 4;
					}
					break;

				case 0xCF:      // SET 1, A
					{
						Utilities.SetBitsInByte(ref A, 0x01, 1, 1);
						PrintOpcode(instruction, "SET 1, A");
						PC += 2;
						cycles += 2;
					}
					break;

				case 0xD6:      // SET 2, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Utilities.SetBitsInByte(ref d8, 0x01, 2, 2);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "SET 2, (HL)");
						PC += 2;
						cycles += 4;
					}
					break;

				case 0xDE:      // SET 3, (HL)
					{
						byte d8 = Memory.Instance.Read(HL);
						Utilities.SetBitsInByte(ref d8, 0x01, 3, 3);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, "SET 3, (HL)");
						PC += 2;
						cycles += 4;
					}
					break;

				case 0xF4:      // SET 6, H
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = (byte)(HL & 0x00FF);
						Utilities.SetBitsInByte(ref h, 0x01, 6, 6);
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "SET 6, H");
						PC += 2;
						cycles += 2;
					}
					break;

				default:
					{
						GameBoy.DebugOutput += $"[0x{PC:X4}] Unimplemented opcode: 0xCB{instruction:X2}!\n";
						MainForm.Pause();
					}
					break;
			}
		}
	}
}
