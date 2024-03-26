namespace GBSharp
{
	internal partial class CPU
	{
		void HandleOpcode(byte instruction)
		{
			switch (instruction)
			{
				case 0x00:      // NOP
					{
						PrintOpcode(instruction, "NOP");
						PC++;
						Cycles++;
					}
					break;

				case 0x01:      // LD BC, d16
					{
						C = Memory.Instance.Read(PC + 1);
						B = Memory.Instance.Read(PC + 2);
						ushort d16 = (ushort)((B << 8) + C);
						PrintOpcode(instruction, $"LD BC, 0x{d16:X4}");
						PC += 3;
						Cycles += 3;
					}
					break;

				case 0x04:      // INC B
					{
						B++;
						Z = B == 0;
						N = false;
						// TODO: H?
						PrintOpcode(instruction, "INC B");
						PC++;
						Cycles++;
					}
					break;

				case 0x05:      // DEC B
					{
						B--;
						Z = B == 0;
						N = true;
						// TODO: H?
						PrintOpcode(instruction, "DEC B");
						PC++;
						Cycles++;
					}
					break;

				case 0x06:      // LD B, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						B = d8;
						PrintOpcode(instruction, $"LD B, 0x{d8:X2}");
						PC += 2;
						Cycles += 2;
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
						Cycles += 2;
					}
					break;

				case 0x0D:      // DEC C
					{
						C--;
						Z = C == 0;
						N = true;
						// TODO: H?
						PrintOpcode(instruction, "DEC C");
						PC++;
						Cycles++;
					}
					break;

				case 0x0E:      // LD C, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						C = d8;
						PrintOpcode(instruction, $"LD C, 0x{d8:X2}");
						PC += 2;
						Cycles += 2;
					}
					break;

				case 0x11:      // LD DE, d16
					{
						E = Memory.Instance.Read(PC + 1);
						D = Memory.Instance.Read(PC + 2);
						ushort d16 = (ushort)((D << 8) + E);
						PrintOpcode(instruction, $"LD DE, 0x{d16:X4}");
						PC += 3;
						Cycles += 3;
					}
					break;

				case 0x12:      // LD (DE), A
					{
						ushort de = (ushort)((D << 8) + E);
						Memory.Instance.Write(de, A);
						PrintOpcode(instruction, "LD (DE), A");
						PC++;
						Cycles += 2;
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
						Cycles += 2;
					}
					break;

				case 0x18:      // JR s8
					{
						sbyte s8 = (sbyte)(Memory.Instance.Read(PC + 1) + 2);
						ushort newPC = (ushort)(PC + s8);
						PrintOpcode(instruction, $"JR 0x{newPC:X4}");
						PC = newPC;
						Cycles += 3;
					}
					break;

				case 0x1A:      // LD A, (DE)
					{
						ushort de = (ushort)((D << 8) + E);
						A = Memory.Instance.Read(de);
						PrintOpcode(instruction, "LD A, (DE)");
						PC++;
						Cycles += 2;
					}
					break;

				case 0x1C:      // INC E
					{
						E++;
						Z = E == 0;
						N = false;
						// TODO: H?
						PrintOpcode(instruction, "INC E");
						PC++;
						Cycles++;
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
							Cycles += 3;
						}
						else
						{
							PC += 2;
							Cycles += 2;
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
						Cycles += 3;
					}
					break;

				case 0x22:      // LD (HL+), A
					{
						Memory.Instance.Write(HL, A);
						HL++;
						PrintOpcode(instruction, "LD (HL+), A");
						PC++;
						Cycles += 2;
					}
					break;

				case 0x23:      // INC HL
					{
						HL++;
						PrintOpcode(instruction, "INC HL");
						PC++;
						Cycles += 2;
					}
					break;

				case 0x2A:      // LD A, (HL+)
					{
						A = Memory.Instance.Read(HL);
						HL++;
						PrintOpcode(instruction, "LD A, (HL+)");
						PC++;
						Cycles += 2;
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
							Cycles += 3;
						}
						else
						{
							PC += 2;
							Cycles += 2;
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
						Cycles += 3;
					}
					break;

				case 0x36:      // LD (HL), d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						Memory.Instance.Write(HL, d8);
						PrintOpcode(instruction, $"LD (HL), 0x{d8:2}");
						PC += 2;
						Cycles += 3;
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
							Cycles += 3;
						}
						else
						{
							PC += 2;
							Cycles += 2;
						}
					}
					break;

				case 0x3E:      // LD A, d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						A = d8;
						PrintOpcode(instruction, $"LD A, 0x{d8:X2}");
						PC += 2;
						Cycles += 2;
					}
					break;

				case 0x56:      // LD D, (HL)
					{
						D = Memory.Instance.Read(HL);
						PrintOpcode(instruction, "LD D, (HL)");
						PC++;
						Cycles += 2;
					}
					break;

				case 0x57:      // LD D, A
					{
						D = A;
						PrintOpcode(instruction, "LD D, A");
						PC++;
						Cycles++;
					}
					break;

				case 0x5E:      // LD E, (HL)
					{
						E = Memory.Instance.Read(HL);
						PrintOpcode(instruction, "LD E, (HL)");
						PC++;
						Cycles += 2;
					}
					break;

				case 0x66:      // LD H, (HL)
					{
						byte h = Memory.Instance.Read(HL);
						byte l = (byte)(HL & 0x00FF);
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD H, (HL)");
						PC++;
						Cycles += 2;
					}
					break;

				case 0x6F:      // LD L, A
					{
						byte h = (byte)((HL & 0xFF00) >> 8);
						byte l = A;
						HL = (ushort)((h << 8) + l);
						PrintOpcode(instruction, "LD L, A");
						PC++;
						Cycles++;
					}
					break;

				case 0x72:      // LD (HL), D
					{
						Memory.Instance.Write(HL, D);
						PrintOpcode(instruction, "LD (HL), D");
						PC++;
						Cycles += 2;
					}
					break;

				case 0x78:      // LD A, B
					{
						A = B;
						PrintOpcode(instruction, "LD A, B");
						PC++;
						Cycles++;
					}
					break;

				case 0x7D:      // LD A, L
					{
						byte l = (byte)(HL & 0x00FF);
						A = l;
						PrintOpcode(instruction, "LD A, L");
						PC++;
						Cycles++;
					}
					break;

				case 0xAF:      // XOR A
					{
						A ^= A;
						Z = A == 0;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "XOR A");
						PC++;
						Cycles++;
					}
					break;

				case 0xB1:      // OR C
					{
						A |= C;
						Z = A == 0;
						N = false;
						H = false;
						CY = false;
						PrintOpcode(instruction, "OR C");
						PC++;
						Cycles++;
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
						Cycles += 3;
					}
					break;

				case 0xC3:      // JP a16
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = (ushort)(Memory.Instance.Read(PC + 2) << 8);
						ushort a16 = (ushort)(higher + lower);
						PrintOpcode(instruction, $"JP 0x{a16:X4}");
						PC = a16;
						Cycles += 4;
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
						Cycles += 4;
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
							Cycles += 5;
						}
						else
						{
							PC++;
							Cycles += 2;
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
						Cycles += 4;
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
						Cycles += 6;
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
						Cycles += 3;
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
						Cycles += 4;
					}
					break;

				case 0xE0:      // LD (a8), A
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = 0xFF00;
						Memory.Instance.Write(higher + lower, A);
						PrintOpcode(instruction, $"LD (0x{lower:X2}), A");
						PC += 2;
						Cycles += 3;
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
						Cycles += 3;
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
						Cycles += 4;
					}
					break;

				case 0xE6:      // AND d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						A &= d8;
						Z = A == 0;
						N = false;
						H = true;
						CY = false;
						PrintOpcode(instruction, $"AND 0x{d8:2}");
						PC += 2;
						Cycles += 3;
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
						Cycles += 4;
					}
					break;

				case 0xF0:      // LD A, (a8)
					{
						byte lower = Memory.Instance.Read(PC + 1);
						ushort higher = 0xFF00;
						A = Memory.Instance.Read(higher + lower);
						PrintOpcode(instruction, $"LD A, (0x{lower:X2})");
						PC += 2;
						Cycles += 3;
					}
					break;

				case 0xF3:      // DI
					{
						IME = false;
						PrintOpcode(instruction, "DI");
						PC++;
						Cycles++;
					}
					break;

				case 0xFE:      // CP d8
					{
						byte d8 = Memory.Instance.Read(PC + 1);
						int cp = A - d8;
						Z = cp == 0;
						// TODO: H?
						CY = cp < 0;
						PrintOpcode(instruction, $"CP 0x{d8:X2}");
						PC += 2;
						Cycles += 2;
					}
					break;

				default:
					{
						MainForm.PrintDebugMessage($"[0x{PC:X4}] Unimplemented opcode: 0x{instruction:X2}!\n");
						MainForm.Pause();
					}
					break;
			}
		}
	}
}
