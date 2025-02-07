namespace GBSharp
{
	internal class PPU
	{
		// An Emulator option. Enable slower, but scanline-accurate rendering.
		public bool Accurate = true;
		private int _objectsRendered = 0;

		public const int kWidth = 160;
		public const int kHeight = 144;
		// Represents each pixel of the LCD, where the data is the palette color.
		public int[,] LCDBackBuffer = new int[kWidth, kHeight];
		public int[,] LCDFrontBuffer = new int[kWidth, kHeight];

		// Every 456 dots, we increment LY, possibly trigger v-blank, etc.
		public const uint kDotsPerLine = 456;
		private const uint kVBlankLine = 144;
		public const uint kLinesPerFrame = 154;
		//public const uint kCyclesPerFrame = kDotsPerLine / 4 * kLinesPerFrame;

		private uint Dots;

		// The LCDC register control flags (FF40)
		private bool LCDEnabled;
		private bool WindowTileMapArea;
		private bool WindowEnabled;
		private bool BGWindowTileDataArea;
		private bool BGTileMapArea;
		private bool OBJSize;
		private bool OBJEnabled;
		private bool BGWindowEnable;

		// The STAT LCD status flags (FF41)
		private bool LYCIntSelect;
		private bool Mode2IntSelect;
		private bool Mode1IntSelect;
		private bool Mode0IntSelect;
		private bool LYCEqualsLY;
		private byte PPUMode;
		private byte _lastPPUMode;

		// The scroll Y and X values (FF42 and FF43)
		public int SCY;
		public int SCX;

		// The LCD y-coordinate (F44)
		public byte LY { get; private set; }

		// The LY compare value (FF45)
		public byte LYC;

		// The palette data (FF47, FF48, and FF49)
		public byte BGPaletteData;
		public byte OBJPaletteData0;
		public byte OBJPaletteData1;

		// The window Y and X values (FF4A and FF4B)
		public int WY;
		public int WX;

		private static PPU? _instance;
		public static PPU Instance
		{
			get
			{
				_instance ??= new PPU();
				return _instance;
			}
		}

		public PPU()
		{
			Reset();
		}

		public void Reset()
		{
			// Retain emulator options.
			//Accurate = true;
			_objectsRendered = 0;

			Clear();

			Dots = 0;

			LCDEnabled = true;
			WindowTileMapArea = false;
			WindowEnabled = false;
			BGWindowTileDataArea = true;
			BGTileMapArea = false;
			OBJSize = false;
			OBJEnabled = false;
			BGWindowEnable = true;

			LYCIntSelect = false;
			Mode2IntSelect = false;
			Mode1IntSelect = false;
			Mode0IntSelect = false;
			LYCEqualsLY = false;
			PPUMode = 0x00;
			_lastPPUMode = 0x00;

			SCY = 0;
			SCX = 0;

			LY = 0x00;
			LYC = 0x00;

			BGPaletteData = 0;
			OBJPaletteData0 = 0;
			OBJPaletteData1 = 0;

			WY = 0;
			WX = 0;
		}

		public void Update()
		{
			Dots++;

			if (Dots >= kDotsPerLine * kLinesPerFrame)
			{
				Dots -= kDotsPerLine * kLinesPerFrame;
			}
			byte newLY = (byte)(Dots / kDotsPerLine);

			if (newLY >= kVBlankLine)
			{
				// Vertical blank.
				PPUMode = 0x01;

				// A STAT interrupt has occurred.
				if (_lastPPUMode != PPUMode && Mode1IntSelect)
				{
					CPU.Instance.IF |= 0x02;
				}
			}
			else if (Dots % kDotsPerLine < 80)
			{
				// OAM scan.
				PPUMode = 0x02;

				// A STAT interrupt has occurred.
				if (_lastPPUMode != PPUMode && Mode2IntSelect)
				{
					CPU.Instance.IF |= 0x02;
				}
			}
			else if (Dots % kDotsPerLine < 252)
			{
				// Drawing pixels.
				PPUMode = 0x03;

				// Actually render to LCD data.
				if (Accurate && _lastPPUMode != PPUMode && LY < kVBlankLine)
				{
					Render();
				}
			}
			else
			{
				// Horizontal blank.
				PPUMode = 0x00;

				// A STAT interrupt has occurred.
				if (_lastPPUMode != PPUMode && Mode0IntSelect)
				{
					CPU.Instance.IF |= 0x02;
				}
			}
			_lastPPUMode = PPUMode;

			// Did we start rendering a new scanline?
			if (newLY != LY)
			{
				LY = newLY;

				if (LY == kVBlankLine)
				{
					// Actually render to LCD data.
					if (!Accurate)
					{
						Render();
					}

					// When done, copy the back buffer to the front buffer.
					Array.Copy(LCDBackBuffer, LCDFrontBuffer, LCDBackBuffer.Length);
					Array.Clear(LCDBackBuffer);

					// Set the v-blank IF flag.
					CPU.Instance.IF |= 0x01;
				}

				// Check for a LYC == LY STAT interrupt.
				LYCEqualsLY = LYC == LY;
				if (LYCEqualsLY && LYCIntSelect)
				{
					if (GameBoy.ShouldLogOpcodes)
					{
						GameBoy.LogOutput += $"[{Dots}, {LY}] A STAT interrupt occurred with LYC={LYC}.\n";
					}

					// Set the LCD IF flag.
					CPU.Instance.IF |= 0x02;
				}
			}
		}

		public byte GetLCDC()
		{
			byte lcdc = 0x00;

			lcdc |= (byte)(LCDEnabled ? 0x80 : 0x00);
			lcdc |= (byte)(WindowTileMapArea ? 0x40 : 0x00);
			lcdc |= (byte)(WindowEnabled ? 0x20 : 0x00);
			lcdc |= (byte)(BGWindowTileDataArea ? 0x10 : 0x00);
			lcdc |= (byte)(BGTileMapArea ? 0x08 : 0x00);
			lcdc |= (byte)(OBJSize ? 0x04 : 0x00);
			lcdc |= (byte)(OBJEnabled ? 0x02 : 0x00);
			lcdc |= (byte)(BGWindowEnable ? 0x01 : 0x00);

			return lcdc;
		}

		public void SetLCDC(byte lcdc)
		{
			LCDEnabled = Utilities.GetBoolFromByte(lcdc, 7);
			WindowTileMapArea = Utilities.GetBoolFromByte(lcdc, 6);
			WindowEnabled = Utilities.GetBoolFromByte(lcdc, 5);
			BGWindowTileDataArea = Utilities.GetBoolFromByte(lcdc, 4);
			BGTileMapArea = Utilities.GetBoolFromByte(lcdc, 3);
			OBJSize = Utilities.GetBoolFromByte(lcdc, 2);
			OBJEnabled = Utilities.GetBoolFromByte(lcdc, 1);
			BGWindowEnable = Utilities.GetBoolFromByte(lcdc, 0);
		}

		public byte GetSTAT()
		{
			byte stat = 0x00;

			stat |= (byte)(LYCIntSelect ? 0x40 : 0x00);
			stat |= (byte)(Mode2IntSelect ? 0x20 : 0x00);
			stat |= (byte)(Mode1IntSelect ? 0x10 : 0x00);
			stat |= (byte)(Mode0IntSelect ? 0x08 : 0x00);
			stat |= (byte)(LYCEqualsLY ? 0x04 : 0x00);
			stat |= PPUMode;

			return stat;
		}

		public void SetSTAT(byte stat)
		{
			LYCIntSelect = Utilities.GetBoolFromByte(stat, 6);
			Mode2IntSelect = Utilities.GetBoolFromByte(stat, 5);
			Mode1IntSelect = Utilities.GetBoolFromByte(stat, 4);
			Mode0IntSelect = Utilities.GetBoolFromByte(stat, 3);
			LYCEqualsLY = Utilities.GetBoolFromByte(stat, 2);
			PPUMode = Utilities.GetBitsFromByte(stat, 0, 1);
		}

		private void Clear()
		{
			Array.Clear(LCDBackBuffer);
			Array.Clear(LCDFrontBuffer);
		}

		public void Render()
		{
			if (!LCDEnabled)
			{
				return;
			}

			// Draw the background by iterating over each of its tiles.
			if (BGWindowEnable)
			{
				for (int tileY = 0; tileY < 32; ++tileY)
				{
					for (int tileX = 0; tileX < 32; ++tileX)
					{
						int mapAddress = BGTileMapArea ? 0x9C00 : 0x9800;
						mapAddress += tileY * 32 + tileX;

						// Each tile is 16 bytes, but can be addressed in one of two ways.
						int tileAddress;
						if (BGWindowTileDataArea)
						{
							byte tileNumber = Memory.Instance.Read(mapAddress);
							tileAddress = 0x8000 + tileNumber * 16;
						}
						else
						{
							sbyte tileNumber = (sbyte)Memory.Instance.Read(mapAddress);
							tileAddress = 0x9000 + tileNumber * 16;
						}

						int x = tileX * 8 - SCX;
						int y = tileY * 8 - SCY;
						// Background tiles can wrap around.
						if (x < 0)
						{
							x += 256;
						}
						if (y < 0)
						{
							y += 256;
						}
						RenderTile(tileAddress, x, y, BGPaletteData);
					}
				}
			}

			// Draw the window as well.
			if (WindowEnabled && BGWindowEnable)
			{
				for (int tileY = 0; tileY < 32; ++tileY)
				{
					for (int tileX = 0; tileX < 32; ++tileX)
					{
						int mapAddress = WindowTileMapArea ? 0x9C00 : 0x9800;
						mapAddress += tileY * 32 + tileX;

						// Each tile is 16 bytes, but can be addressed in one of two ways.
						int tileAddress;
						if (BGWindowTileDataArea)
						{
							byte tileNumber = Memory.Instance.Read(mapAddress);
							tileAddress = 0x8000 + tileNumber * 16;
						}
						else
						{
							sbyte tileNumber = (sbyte)Memory.Instance.Read(mapAddress);
							tileAddress = 0x9000 + tileNumber * 16;
						}

						// NOTE: The window has a 7 pixel x-offset.
						int x = tileX * 8 + WX - 7;
						int y = tileY * 8 + WY;
						RenderTile(tileAddress, x, y, BGPaletteData);
					}
				}
			}

			if (OBJEnabled)
			{
				// Draw the objects by iterating over each of their tiles.
				// TODO: Handle variable dot rendering speeds?
				_objectsRendered = 0;
				for (int objAddress = 0xFE00; objAddress <= 0xFE9C; objAddress += 0x04)
				{
					byte byte1 = Memory.Instance.Read(objAddress);
					int y = byte1;

					// Objects with a Y of 0 or 160 are hidden.
					if (y == 0 || y == 160)
					{
						continue;
					}

					// Enforce 10 objects-per-scanline limitation.
					if (Accurate && _objectsRendered == 10)
					{
						continue;
					}

					// The X and Y values are actually offset.
					y -= 16;
					byte byte2 = Memory.Instance.Read(objAddress + 1);
					int x = byte2 - 8;
					byte tileNumber = Memory.Instance.Read(objAddress + 2);
					int tileAddress = 0x8000;
					if (OBJSize)
					{
						tileAddress += (tileNumber & 0xFE) * 16;
					}
					else
					{
						tileAddress += tileNumber * 16;
					}
					byte attributes = Memory.Instance.Read(objAddress + 3);
					bool priority = Utilities.GetBoolFromByte(attributes, 7);
					bool yFlip = Utilities.GetBoolFromByte(attributes, 6);
					bool xFlip = Utilities.GetBoolFromByte(attributes, 5);
					byte objPaletteData = Utilities.GetBoolFromByte(attributes, 4) ? OBJPaletteData1 : OBJPaletteData0;

					// Render tile(s) for 8x8 or 8x16 mode.
					bool rendered = false;
					if (!OBJSize)
					{
						rendered = RenderTile(tileAddress, x, y, objPaletteData, true, xFlip, yFlip, priority);
					}
					else
					{
						rendered = RenderTile(tileAddress, x, yFlip ? y + 8 : y, objPaletteData, true, xFlip, yFlip, priority);

						// In 8x16 mode, also render the next tile immediately below.
						tileAddress = 0x8000 + (tileNumber | 0x01) * 16;
						rendered = RenderTile(tileAddress, x, yFlip ? y : y + 8, objPaletteData, true, xFlip, yFlip, priority);
					}

					if (rendered)
					{
						_objectsRendered++;
					}
				}
			}
		}

		// Draw an individual tile with data from an address at a location with a palette.
		private bool RenderTile(int tileAddress, int x, int y, byte palette,
			bool transparency = false, bool xFlip = false, bool yFlip = false, bool priority = false)
		{
			bool rendered = false;

			// Exit early if we wouldn't render on this particular scanline.
			if (Accurate && (y <= LY - 8 || y > LY))
			{
				return rendered;
			}

			// Draw each tile, pixel by pixel.
			for (int pixelY = 0; pixelY < 8; ++pixelY)
			{
				for (int pixelX = 0; pixelX < 8; ++pixelX)
				{
					int lcdX = xFlip ? x + 7 - pixelX : x + pixelX;
					int lcdY = yFlip ? y + 7 - pixelY : y + pixelY;

					// Only render the line that corresponds to the current LY.
					if (Accurate && lcdY != LY)
					{
						continue;
					}

					// Each line in the tile is two bytes (each pixel is 2-bits of color).
					int pixelAddress = tileAddress + pixelY * 2;
					byte byte1 = Memory.Instance.Read(pixelAddress);
					byte byte2 = Memory.Instance.Read(pixelAddress + 1);
					byte colorId = Utilities.GetBitsFromByte(byte1, 7 - pixelX, 7 - pixelX);
					colorId += (byte)(Utilities.GetBitsFromByte(byte2, 7 - pixelX, 7 - pixelX) << 1);

					// Look up the correct palette color and draw that pixel. If transparency is enabled, then
					// color 0 is not drawn.
					if (!(transparency && colorId == 0))
					{
						int lcdColor = Utilities.GetBitsFromByte(palette, colorId * 2, colorId * 2 + 1);
						if (lcdX >= 0 && lcdX < kWidth && lcdY >= 0 && lcdY < kHeight)
						{
							if (!priority)
							{
								LCDBackBuffer[lcdX, lcdY] = lcdColor;
								rendered = true;
							}
							else
							{
								// If priority is on, only render above BG color 0.
								int bgColor0 = Utilities.GetBitsFromByte(BGPaletteData, 0, 1);
								if (LCDBackBuffer[lcdX, lcdY] == bgColor0)
								{
									LCDBackBuffer[lcdX, lcdY] = lcdColor;
									rendered = true;
								}
							}
						}
					}
				}
			}

			return rendered;
		}
	}
}
