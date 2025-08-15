namespace GBSharp
{
	internal class SGB
	{
		internal struct Packet
		{
			public byte[] _data;

			public Packet()
			{
				_data = new byte[16];
			}
		}

		public struct Palette
		{
			public Color[] Colors;

			public Palette()
			{
				Colors =
				[
					Color.White,
					Color.White,
					Color.White,
					Color.White
				];
			}
		}

		// Emulator SGB options.
		public bool Allowed = false;

		public bool Enabled { get; private set; }

		// Are we receiving data from the game?
		public bool Receiving { get; private set; }
		private int _numBitsReceived;

		// A transmission is a series of packets.
		private readonly List<Packet> _packets = [];
		private int _currentPacket = 0;
		private int _transmissionLength;

		// Data for the palettes in SGB memory.
		private readonly byte[] _paletteData;
		// Actual palettes currently in use.
		public Palette[] Palettes = new Palette[4];

		// Data for the attribute files in SGB memory.
		private readonly byte[] _attributeFileData;
		// Actual attribute file in use.
		private int _attributeFileNumber;
		// The 20x18 grid of attribute characters (mapping tiles to palettes).
		private readonly int[,] _attributeChars;

		public static bool ShouldLogPackets = false;

		private static SGB? _instance;
		public static SGB Instance
		{
			get
			{
				_instance ??= new SGB();
				return _instance;
			}
		}

		public SGB()
		{
			_paletteData = new byte[4096];				// 512 palettes at 8 bytes each
			_attributeFileData = new byte[4050];		// 45 attributes at 90 bytes each
			_attributeChars = new int[PPU.kWidth / 8, PPU.kHeight / 8];

			Reset();
		}

		public void Reset()
		{
			// Retain emulator options.
			//Allowed = false;

			Enabled = false;

			Receiving = false;
			_numBitsReceived = 0;

			_packets.Clear();
			_currentPacket = 0;
			_transmissionLength = 0;

			Array.Clear(_paletteData);
			for (int i = 0; i < Palettes.Length; ++i)
			{
				Palettes[i] = new Palette();
			}

			Array.Clear(_attributeFileData);
			_attributeFileNumber = 0;
			Array.Clear(_attributeChars);
		}

		public void StartReceiving()
		{
			// SGB functionality is turned off.
			if (!Allowed)
			{
				return;
			}

			Receiving = true;
			_numBitsReceived = 0;

			_packets.Add(new Packet());
		}

		public void SendBit(byte bit)
		{
			if (_numBitsReceived < 128)
			{
				int whichByte = _numBitsReceived / 8;
				_packets[_currentPacket]._data[whichByte] |= (byte)(bit << (_numBitsReceived % 8));

				_numBitsReceived++;
			}
			else
			{
				// After 128 bits, this packet is complete.
				Receiving = false;
				_numBitsReceived = 0;

				// The first packet contains the header.
				if (_currentPacket == 0)
				{
					_transmissionLength = _packets[0]._data[0] & 0x07;
				}

				_currentPacket++;
				if (_currentPacket == _transmissionLength)
				{
					byte commandCode = (byte)((_packets[0]._data[0] & 0xF8) >> 3);
					HandleTransmission(commandCode);

					// This transmission is complete.
					_packets.Clear();
					_currentPacket = 0;
					_transmissionLength = 0;
				}
			}
		}

		private void HandleTransmission(byte commandCode)
		{
			switch (commandCode)
			{
				case 0x00:		// PAL01
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet PAL01 (0x00).\n";
						}

						// Read the color data from the packet.
						ushort color0 = (ushort)(_packets[0]._data[1] + (ushort)(_packets[0]._data[2] << 8));
						ushort palette0Color1 = (ushort)(_packets[0]._data[3] + (ushort)(_packets[0]._data[4] << 8));
						ushort palette0Color2 = (ushort)(_packets[0]._data[5] + (ushort)(_packets[0]._data[6] << 8));
						ushort palette0Color3 = (ushort)(_packets[0]._data[7] + (ushort)(_packets[0]._data[8] << 8));
						ushort palette1Color1 = (ushort)(_packets[0]._data[9] + (ushort)(_packets[0]._data[10] << 8));
						ushort palette1Color2 = (ushort)(_packets[0]._data[11] + (ushort)(_packets[0]._data[12] << 8));
						ushort palette1Color3 = (ushort)(_packets[0]._data[13] + (ushort)(_packets[0]._data[14] << 8));

						// Update the in-use Palettes based on the new colors.
						Palettes[0].Colors[0] = GetColorFromData(color0);
						Palettes[0].Colors[1] = GetColorFromData(palette0Color1);
						Palettes[0].Colors[2] = GetColorFromData(palette0Color2);
						Palettes[0].Colors[3] = GetColorFromData(palette0Color3);
						Palettes[1].Colors[0] = GetColorFromData(color0);
						Palettes[1].Colors[1] = GetColorFromData(palette1Color1);
						Palettes[1].Colors[2] = GetColorFromData(palette1Color2);
						Palettes[1].Colors[3] = GetColorFromData(palette1Color3);
					}
					break;

				case 0x01:		// PAL23
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet PAL23 (0x01).\n";
						}

						// Read the color data from the packet.
						ushort color0 = (ushort)(_packets[0]._data[1] + (ushort)(_packets[0]._data[2] << 8));
						ushort palette2Color1 = (ushort)(_packets[0]._data[3] + (ushort)(_packets[0]._data[4] << 8));
						ushort palette2Color2 = (ushort)(_packets[0]._data[5] + (ushort)(_packets[0]._data[6] << 8));
						ushort palette2Color3 = (ushort)(_packets[0]._data[7] + (ushort)(_packets[0]._data[8] << 8));
						ushort palette3Color1 = (ushort)(_packets[0]._data[9] + (ushort)(_packets[0]._data[10] << 8));
						ushort palette3Color2 = (ushort)(_packets[0]._data[11] + (ushort)(_packets[0]._data[12] << 8));
						ushort palette3Color3 = (ushort)(_packets[0]._data[13] + (ushort)(_packets[0]._data[14] << 8));

						// Update the in-use Palettes based on the new colors.
						Palettes[2].Colors[0] = GetColorFromData(color0);
						Palettes[2].Colors[1] = GetColorFromData(palette2Color1);
						Palettes[2].Colors[2] = GetColorFromData(palette2Color2);
						Palettes[2].Colors[3] = GetColorFromData(palette2Color3);
						Palettes[3].Colors[0] = GetColorFromData(color0);
						Palettes[3].Colors[1] = GetColorFromData(palette3Color1);
						Palettes[3].Colors[2] = GetColorFromData(palette3Color2);
						Palettes[3].Colors[3] = GetColorFromData(palette3Color3);
					}
					break;

				case 0x04:		// ATTR_BLK
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet ATTR_BLK (0x04).\n";
						}

						HandleAttrBlk();
					}
					break;

				case 0x05:		// ATTR_LIN
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet ATTR_LIN (0x05).\n";
						}

						HandleAttrLin();
					}
					break;

				case 0x06:		// ATTR_DIV
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet ATTR_DIV (0x06).\n";
						}

						HandleAttrDiv();
					}
					break;

				case 0x07:		// ATTR_CHR
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet ATTR_CHR (0x07).\n";
						}

						HandleAttrChr();
					}
					break;

				case 0x08:		// SOUND
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet SOUND (0x08). Unimplemented.\n";
						}

						// TODO: Support SGB sound?
					}
					break;

				case 0x09:		// SOU_TRN
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet SOU_TRN (0x09). Unimplemented.\n";
						}

						// TODO: Support SGB sound?
					}
					break;

				case 0x0A:		// PAL_SET
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet PAL_SET (0x0A):";
						}

						ushort higher = (ushort)(_packets[0]._data[2] << 8);
						ushort lower = _packets[0]._data[1];
						ushort palette0 = (ushort)(higher + lower);
						higher = (ushort)(_packets[0]._data[4] << 8);
						lower = _packets[0]._data[3];
						ushort palette1 = (ushort)(higher + lower);
						higher = (ushort)(_packets[0]._data[6] << 8);
						lower = _packets[0]._data[5];
						ushort palette2 = (ushort)(higher + lower);
						higher = (ushort)(_packets[0]._data[8] << 8);
						lower = _packets[0]._data[7];
						ushort palette3 = (ushort)(higher + lower);

						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += $" {palette0}, {palette1}, {palette2}, {palette3}";
						}

						byte flags = _packets[0]._data[9];
						bool cancelScreenMask = (flags & 0x40) == 0x40;
						if (cancelScreenMask)
						{
							// Cancel the screen mask.
							PPU.Instance.ScreenMask = 0;
						}
						bool applyATF = (flags & 0x80) == 0x80;
						int atfNumber = flags & 0x3F;
						if (applyATF)
						{
							// Apply the attribute file.
							if (atfNumber != _attributeFileNumber)
							{
								_attributeFileNumber = atfNumber;
								ApplyAttributeFile();
							}

							if (ShouldLogPackets)
							{
								GameBoy.DebugOutput += $", ATF: {_attributeFileNumber}";
							}
						}

						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "\n";
						}

						// Update the in-use Palettes based on the new palette indices.
						Palettes[0] = GetPaletteFromPaletteData(palette0);
						Palettes[1] = GetPaletteFromPaletteData(palette1);
						Palettes[2] = GetPaletteFromPaletteData(palette2);
						Palettes[3] = GetPaletteFromPaletteData(palette3);
					}
					break;

				case 0x0B:		// PAL_TRN
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet PAL_TRN (0x0B).\n";
						}

						// Immediately transfer the palette data from VRAM starting at 0x8800.
						Array.Copy(Memory.Instance.VRAM, 0x0800, _paletteData, 0, _paletteData.Length);
					}
					break;

				case 0x0C:		// ATRC_EN
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet ATRC_EN (0x0C). Unimplemented.\n";
						}

						// NOTE: Ignore?
					}
					break;

				case 0x0E:		// ICON_EN
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet ICON_EN (0x0E). Unimplemented.\n";
						}

						// NOTE: Ignore?
					}
					break;

				case 0x0F:		// DATA_SND
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet DATA_SND (0x0F). Unimplemented.\n";
						}

						// TODO: Anything?
					}
					break;

				case 0x11:		// MLT_REQ
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet MLT_REQ (0x11).\n";
						}

						// This is traditionally used to detect SGB support.
						Enabled = true;
					}
					break;

				case 0x13:		// CHR_TRN
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet CHR_TRN (0x13). Unimplemented.\n";
						}

						// TODO: Support SGB border?
					}
					break;

				case 0x14:		// PCT_TRN
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet PCT_TRN (0x14). Unimplemented.\n";
						}

						// TODO: Support SGB border?
					}
					break;

				case 0x15:		// ATTR_TRN
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet ATTR_TRN (0x15).\n";
						}

						// Immediately transfer the attribute files from VRAM starting at 0x8800.
						Array.Copy(Memory.Instance.VRAM, 0x0800, _attributeFileData, 0, _attributeFileData.Length);
					}
					break;

				case 0x16:      // ATTR_SET
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet ATTR_SET (0x16): ";
						}

						bool cancelScreenMask = (_packets[0]._data[0] & 0x40) == 0x40;
						if (cancelScreenMask)
						{
							// Cancel the screen mask.
							PPU.Instance.ScreenMask = 0;
						}
						int atfNumber = _packets[0]._data[0] & 0x3F;
						if (atfNumber != _attributeFileNumber)
						{
							// Apply the attribute file.
							_attributeFileNumber = atfNumber;
							ApplyAttributeFile();
						}

						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += $"{_attributeFileNumber}\n";
						}

					}
					break;

				case 0x17:		// MASK_EN
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += $"Received SGB packet MASK_EN (0x17): {_packets[0]._data[1]}\n";
						}

						// Set the screen mask.
						PPU.Instance.ScreenMask = _packets[0]._data[1];
					}
					break;

				case 0x19:		// PAL_PRI
					{
						if (ShouldLogPackets)
						{
							GameBoy.DebugOutput += "Received SGB packet PAL_PRI (0x19). Unimplemented.\n";
						}

						// NOTE: Ignore?
					}
					break;

				default:
					{
						GameBoy.DebugOutput += $"Read an unimplemented command code: 0x{commandCode:X2}!\n";
						MainForm.Pause();
					}
					break;
			}
		}

		// Apply the current attribute file to the attribute characters.
		private void ApplyAttributeFile()
		{
			Array.Clear(_attributeChars);
			for (int y = 0; y < PPU.kHeight / 8; ++y)
			{
				for (int x = 0; x < PPU.kWidth / 8; ++x)
				{
					// Each byte is 4 palettes, 20 chars per 5 bytes, 90 bytes total.
					int offset = _attributeFileNumber * 90 + (x + y * PPU.kWidth / 8) / 4;
					byte data = _attributeFileData[offset];
					int shift = (3 - (x % 4)) * 2;
					int palette = (data & (0x03 << shift)) >> shift;
					_attributeChars[x, y] = palette;
				}
			}
		}

		// Returns the Palette to use at an x and y.
		public Palette GetPaletteAt(int x, int y)
		{
			return Palettes[_attributeChars[x / 8, y / 8]];
		}

		// Return a Color from the given ushort 5-bit color data.
		private Color GetColorFromData(ushort colorData)
		{
			Color color;

			// Convert each channel from 5-bit to 8-bit RGB.
			int red = (int)((float)(ushort)(colorData & 0x001F) / 0x001F * 255);
			int green = (int)((float)(ushort)((colorData & 0x03E0) >> 5) / 0x001F * 255);
			int blue = (int)((float)(ushort)((colorData & 0x7C00) >> 10) / 0x001F * 255);
			color = Color.FromArgb(255, red, green, blue);

			return color;
		}

		// Return a new Palette from the palette data.
		private Palette GetPaletteFromPaletteData(ushort palette)
		{
			Palette newPalette = new();

			// Read the color data from the palette data.
			ushort color0 = (ushort)(_paletteData[palette * 8] + (ushort)(_paletteData[palette * 8 + 1] << 8));
			ushort color1 = (ushort)(_paletteData[palette * 8 + 2] + (ushort)(_paletteData[palette * 8 + 3] << 8));
			ushort color2 = (ushort)(_paletteData[palette * 8 + 4] + (ushort)(_paletteData[palette * 8 + 5] << 8));
			ushort color3 = (ushort)(_paletteData[palette * 8 + 6] + (ushort)(_paletteData[palette * 8 + 7] << 8));

			// Convert each color from 5-bit to 8-bit RGB.
			newPalette.Colors[0] = GetColorFromData(color0);
			newPalette.Colors[1] = GetColorFromData(color1);
			newPalette.Colors[2] = GetColorFromData(color2);
			newPalette.Colors[3] = GetColorFromData(color3);

			return newPalette;
		}

		private void HandleAttrBlk()
		{
			int numDataSets = _packets[0]._data[1];

			// Read each data set and apply its rectangle.
			int packet = 0;
			int data = 2;
			for (int i = 0; i < numDataSets; ++i)
			{
				// Each data set comes consecutively in packets, but after the first two bytes of the first packet.
				bool changeInside = (_packets[packet]._data[data] & 0x01) == 0x01;
				bool changeSurrouding = (_packets[packet]._data[data] & 0x02) == 0x02;
				bool changeOutside = (_packets[packet]._data[data] & 0x04) == 0x04;
				if (++data > 15) { packet++; data = 0; }
				int paletteInside = _packets[packet]._data[data] & 0x03;
				int paletteSurrounding = (_packets[packet]._data[data] & 0x0C) >> 2;
				int paletteOutside = (_packets[packet]._data[data] & 0x30) >> 4;
				if (++data > 15) { packet++; data = 0; }
				int left = _packets[packet]._data[data];
				if (++data > 15) { packet++; data = 0; }
				int top = _packets[packet]._data[data];
				if (++data > 15) { packet++; data = 0; }
				int right = _packets[packet]._data[data];
				if (++data > 15) { packet++; data = 0; }
				int bottom = _packets[packet]._data[data];
				if (++data > 15) { packet++; data = 0; }

				for (int x = 0; x < PPU.kWidth / 8; ++x)
				{
					for (int y = 0; y < PPU.kHeight / 8; ++y)
					{
						if ((x > left && x < right && y > top && y < bottom) && changeInside)
						{
							_attributeChars[x, y] = paletteInside;
						}

						if ((x < left || x > right || y < top || y > bottom) && changeOutside)
						{
							_attributeChars[x, y] = paletteOutside;
						}

						if (((x == left || x == right) && y >= top && y <= bottom) ||
							((y == top || y == bottom) && x >= left && x <= right))
						{
							_attributeChars[x, y] = changeSurrouding ? paletteSurrounding :
								(changeInside ? paletteInside : paletteOutside);
						}
					}
				}
			}
		}

		private void HandleAttrLin()
		{
			int numDataSets = _packets[0]._data[1];

			// Read each data set and apply its line.
			int packet = 0;
			int data = 2;
			for (int i = 0; i < numDataSets; ++i)
			{
				int lineNumber = _packets[packet]._data[data] & 0x1F;
				int palette = (_packets[packet]._data[data] & 0x60) >> 5;
				bool horizontal = (_packets[packet]._data[data] & 0x80) == 0x80;

				for (int j = 0; j < (horizontal ? PPU.kWidth / 8 : PPU.kHeight / 8); ++j)
				{
					if (horizontal)
					{
						_attributeChars[j, lineNumber] = palette;
					}
					else
					{
						_attributeChars[lineNumber, j] = palette;
					}
				}

				if (++data > 15) { packet++; data = 0; }
			}
		}

		private void HandleAttrDiv()
		{
			int bottomRightPalette = _packets[0]._data[1] & 0x03;
			int topLeftPalette = (_packets[0]._data[1] & 0x0C) >> 2;
			int divisionPalette = (_packets[0]._data[1] & 0x30) >> 4;
			bool horizontal = (_packets[0]._data[1] & 0x40) == 0x40;
			int xOrY = _packets[0]._data[2];

			// Apply the division.
			for (int x = 0; x < PPU.kWidth / 8; ++x)
			{
				for (int y = 0; y < PPU.kHeight / 8; ++y)
				{
					if (x < xOrY && !horizontal)
					{
						_attributeChars[x, y] = topLeftPalette;
					}
					if (y < xOrY && horizontal)
					{
						_attributeChars[x, y] = topLeftPalette;
					}
					if (x == xOrY && !horizontal)
					{
						_attributeChars[x, y] = divisionPalette;
					}
					if (y == xOrY && horizontal)
					{
						_attributeChars[x, y] = divisionPalette;
					}
					if (x > xOrY && !horizontal)
					{
						_attributeChars[x, y] = bottomRightPalette;
					}
					if (y > xOrY && horizontal)
					{
						_attributeChars[x, y] = bottomRightPalette;
					}
				}
			}
		}

		private void HandleAttrChr()
		{
			int x = _packets[0]._data[1];
			int y = _packets[0]._data[2];
			int numDataSets = _packets[0]._data[3] + (_packets[0]._data[4] << 8);
			if (numDataSets > 360)
			{
				numDataSets = 360;
			}
			bool topToBottom = _packets[0]._data[5] == 0x01;

			// Read each data set and apply its character.
			int packet = 0;
			int data = 6;
			for (int i = 0; i < numDataSets; ++i)
			{
				int shift = (3 - (i % 4)) * 2;
				int palette =(_packets[packet]._data[data] & (0x03 << shift)) >> shift;
				_attributeChars[x, y] = palette;
				if (i % 4 == 3)
				{
					data++;
				}
				if (data > 15)
				{
					packet++; data = 0;
				}
				if (topToBottom)
				{
					if (++y >= PPU.kHeight / 8)
					{
						y = 0;
						x++;
					}
				}
				else
				{
					if (++x >= PPU.kWidth / 8)
					{
						x = 0;
						y++;
					}
				}
			}
		}
	}
}
