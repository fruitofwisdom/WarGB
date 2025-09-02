namespace GBSharp
{
	internal class PPU
	{
		internal struct Pixel
		{
			public int Color = 0;
			// These two are needed for priority handling.
			public int X = 0;
			public int ObjAddress = 0x0000;
			public SGB.Palette SGBPalette = new();

			public Pixel() { }
		}

		public const int kWidth = 160;
		public const int kHeight = 144;
		// Represents each pixel of the LCD, where the data is the palette color.
		public Pixel[,] LCDBackBuffer = new Pixel[kWidth, kHeight];
		public Pixel[,] LCDFrontBuffer = new Pixel[kWidth, kHeight];

		// Emulator rendering options.
		public bool ShouldRenderBackground = true;
		public bool ShouldRenderWindow = true;
		public bool ShouldRenderObjects = true;

		// Every 456 dots, we increment LY, possibly trigger v-blank, etc.
		public const uint kDotsPerLine = 456;
		private const uint kVBlankLine = 144;
		public const uint kLinesPerFrame = 154;
		//public const uint kCyclesPerFrame = kDotsPerLine / 4 * kLinesPerFrame;

		private uint Dots;
		// Count objects rendered per scanline for accuracy.
		private int _objectsRendered;

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
		private int _wly;       // the window's LY

		// The SGB options.
		public int ScreenMask;

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
			_objectsRendered = 0;

			for (int x = 0; x < kWidth; ++x)
			{
				for (int y = 0; y < kHeight; ++y)
				{
					LCDBackBuffer[x, y] = new Pixel();
					LCDFrontBuffer[x, y] = new Pixel();
				}
			}

			Dots = 0;
			_objectsRendered = 0;

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
			_wly = 0;

			ScreenMask = 0;
		}

		// Update one dot of the PPU and return if rendering occurred.
		public bool Update()
		{
			bool didRender = false;

			if (!LCDEnabled)
			{
				return didRender;
			}

			Dots++;

			if (Dots >= kDotsPerLine * kLinesPerFrame)
			{
				Dots -= kDotsPerLine * kLinesPerFrame;
			}
			byte newLY = (byte)(Dots / kDotsPerLine);

			if (newLY == LY)		// The scanline hasn't changed yet.
			{
				// Handle the correct PPU mode.
				if (LY >= kVBlankLine)
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
				else if (Dots % kDotsPerLine < 376)
				{
					// Drawing pixels.
					PPUMode = 0x03;

					// Rendering this scanline begins now.
					if (_lastPPUMode != PPUMode)
					{
						Render();
						didRender = true;
					}
				}
				else
				{
					// Horizontal blank.
					PPUMode = 0x00;

					// A STAT interrupt has occurred.
					if (_lastPPUMode != PPUMode && Mode0IntSelect)
					{
						if (GameBoy.ShouldLogOpcodes)
						{
							GameBoy.LogOutput += $"[{Dots}, {LY}] A STAT interrupt occurred with newLY={newLY}.\n";
						}

						CPU.Instance.IF |= 0x02;
					}
				}

				_lastPPUMode = PPUMode;
			}
			else		// We started rendering a new scanline.
			{
				LY = newLY;

				// Reset the window's internal line counter too.
				if (LY == 0)
				{
					_wly = 0;
				}

				if (LY == kVBlankLine)
				{
					if (GameBoy.ShouldLogOpcodes)
					{
						GameBoy.LogOutput += $"[{Dots}, {LY}] A v-blank interrupt occurred.\n";
					}

					// When done, copy the back buffer to the front buffer.
					Array.Copy(LCDBackBuffer, LCDFrontBuffer, LCDFrontBuffer.Length);
					for (int x = 0; x < kWidth; ++x)
					{
						for (int y = 0; y < kHeight; ++y)
						{
							LCDBackBuffer[x, y] = new Pixel();
						}
					}

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

			return didRender;
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

			// LY is reset when LCD is disabled.
			if (!LCDEnabled)
			{
				Dots = 0;
				LY = 0x00;
			}
		}

		public byte GetSTAT()
		{
			byte stat = 0x80;

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

		public void Render()
		{
			if (!LCDEnabled)
			{
				return;
			}

			// Handle the SGB screen mask.
			if (ScreenMask == 1)
			{
				// Freeze the screen.
				Array.Copy(LCDFrontBuffer, LCDBackBuffer, LCDBackBuffer.Length);
				return;
			}
			else if (ScreenMask == 2)
			{
				// Turn the screen black. (Handled in LCDControl.)
				return;
			}
			else if (ScreenMask == 3)
			{
				// Turn the screen white. (Handled in LCDControl.)
				return;
			}

			// Draw the background by iterating over each of its tiles.
			if (ShouldRenderBackground && BGWindowEnable)
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
						if (x < -32)
						{
							x += 256;
						}
						if (y < -32)
						{
							y += 256;
						}
						RenderTile(tileAddress, x, y, BGPaletteData);
					}
				}
			}

			RenderWindow();

			// Draw the objects by iterating over each of their tiles.
			if (ShouldRenderObjects && OBJEnabled)
			{
				// TODO: Handle variable dot rendering speeds?
				_objectsRendered = 0;
				for (int objAddress = 0xFE00; objAddress <= 0xFE9C; objAddress += 0x04)
				{
					byte byte1 = Memory.Instance.Read(objAddress);
					int y = byte1;
					byte byte2 = Memory.Instance.Read(objAddress + 1);
					int x = byte2;

					// Objects with a Y of 0 or greater than 160 are hidden.
					if (y == 0 || y >= 160)
					{
						continue;
					}

					// Enforce 10 objects-per-scanline limitation.
					if (_objectsRendered == 10)
					{
						continue;
					}

					// The X and Y values are actually offset.
					y -= 16;
					x -= 8;
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
					// TODO: Also handle priority of opaque pixels by OAM location.
					bool priority = Utilities.GetBoolFromByte(attributes, 7);
					bool yFlip = Utilities.GetBoolFromByte(attributes, 6);
					bool xFlip = Utilities.GetBoolFromByte(attributes, 5);
					byte objPaletteData = Utilities.GetBoolFromByte(attributes, 4) ? OBJPaletteData1 : OBJPaletteData0;

					// Render tile(s) for 8x8 or 8x16 mode.
					bool rendered = false;
					if (!OBJSize)
					{
						rendered = RenderTile(tileAddress, x, y, objPaletteData, true, true, xFlip, yFlip, priority, objAddress);
					}
					else
					{
						rendered = RenderTile(tileAddress, x, yFlip ? y + 8 : y, objPaletteData, true, true, xFlip, yFlip, priority, objAddress);

						// In 8x16 mode, also render the next tile immediately below.
						tileAddress = 0x8000 + (tileNumber | 0x01) * 16;
						rendered = RenderTile(tileAddress, x, yFlip ? y : y + 8, objPaletteData, true, true, xFlip, yFlip, priority, objAddress);
					}

					if (rendered)
					{
						_objectsRendered++;
					}
				}
			}
		}

		// Render the window layer, if it's enabled.
		private void RenderWindow()
		{
			// Draw the window as well.
			if (ShouldRenderWindow && WindowEnabled && BGWindowEnable)
			{
				bool renderedWindow = false;

				for (int tileY = 0; tileY < 32; ++tileY)
				{
					for (int tileX = 0; tileX < 32; ++tileX)
					{
						// The window uses its own internal line counter to look up which tiles to render.
						int tileLookupY = tileY - (LY - _wly - WY) / 8;

						int mapAddress = WindowTileMapArea ? 0x9C00 : 0x9800;
						mapAddress += tileLookupY * 32 + tileX;

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
						bool didRender = RenderTile(tileAddress, x, y, BGPaletteData);
						if (didRender && GameBoy.ShouldLogOpcodes)
						{
							GameBoy.LogOutput += $"Rendered 0x{mapAddress:X4} at ({x}, {y}) for ({tileX}, {tileLookupY}).\tLY = {LY} and _wly = {_wly}.\n";
						}
						renderedWindow |= didRender;
					}
				}

				// The internal window line counter only increments if the window was actually rendered this scanline.
				if (renderedWindow)
				{
					_wly++;
				}
			}
		}

		// Draw an individual tile with data from an address at a location with a palette.
		private bool RenderTile(int tileAddress, int x, int y, byte palette,
			bool useOamLimit = false, bool transparency = false, bool xFlip = false, bool yFlip = false, bool priority = false, int objAddress = 0x000)
		{
			// Exit early if we wouldn't render on this particular scanline.
			if (LY < y || LY > y + 7)
			{
				return false;
			}

			// Exit early if the tile is completely off-screen, but it still affects the scanline limit.
			if ((x < -8 || x >= kWidth) && useOamLimit)
			{
				return true;
			}

			bool rendered = false;

			// Draw each tile, pixel by pixel.
			for (int pixelY = 0; pixelY < 8; ++pixelY)
			{
				for (int pixelX = 0; pixelX < 8; ++pixelX)
				{
					int lcdX = xFlip ? x + 7 - pixelX : x + pixelX;
					int lcdY = yFlip ? y + 7 - pixelY : y + pixelY;

					// Only render the line that corresponds to the current LY.
					if (lcdY != LY)
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
								if (LCDBackBuffer[lcdX, lcdY].ObjAddress == 0x0000 ||
									LCDBackBuffer[lcdX, lcdY].X > x ||		// TODO: Only applies in non-CGB mode.
									(LCDBackBuffer[lcdX, lcdY].X == x && LCDBackBuffer[lcdX, lcdY].ObjAddress > objAddress))
								{
									LCDBackBuffer[lcdX, lcdY].Color = lcdColor;
									LCDBackBuffer[lcdX, lcdY].X = x;
									LCDBackBuffer[lcdX, lcdY].ObjAddress = objAddress;
									if (SGB.Instance.Enabled)
									{
										LCDBackBuffer[lcdX, lcdY].SGBPalette = SGB.Instance.GetPaletteAt(lcdX, lcdY);
									}
								}
								rendered = true;
							}
							else
							{
								// If priority is on, only render above BG color 0.
								int bgColor0 = Utilities.GetBitsFromByte(BGPaletteData, 0, 1);
								if (LCDBackBuffer[lcdX, lcdY].Color == bgColor0)
								{
									if (LCDBackBuffer[lcdX, lcdY].ObjAddress == 0x0000 ||
										LCDBackBuffer[lcdX, lcdY].X > x ||       // TODO: Only applies in non-CGB mode.
										(LCDBackBuffer[lcdX, lcdY].X == x && LCDBackBuffer[lcdX, lcdY].ObjAddress > objAddress))
									{
										LCDBackBuffer[lcdX, lcdY].Color = lcdColor;
										LCDBackBuffer[lcdX, lcdY].X = x;
										LCDBackBuffer[lcdX, lcdY].ObjAddress = objAddress;
										if (SGB.Instance.Enabled)
										{
											LCDBackBuffer[lcdX, lcdY].SGBPalette = SGB.Instance.GetPaletteAt(lcdX, lcdY);
										}
									}
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
