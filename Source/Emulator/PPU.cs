namespace GBSharp
{
	internal class PPU
	{
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

			byte newLY = (byte)(Dots / kDotsPerLine % kLinesPerFrame);

			// Set the PPU mode correctly.
			if (newLY >= kVBlankLine)
			{
				// Vertical blank.
				PPUMode = 0x01;
			}
			else if (Dots % kDotsPerLine < 80)
			{
				// OAM scan.
				PPUMode = 0x02;
			}
			else if (Dots % kDotsPerLine < 252)
			{
				// Drawing pixels.
				// TODO: Handle variable dot rendering speeds?
				PPUMode = 0x11;
			}
			else
			{
				// Horizontal blank.
				PPUMode = 0x00;
			}

			// Did we start rendering a new scanline?
			if (newLY != LY)
			{
				LY = newLY;

				// Check for a v-blank interrupt.
				if (LY == kVBlankLine)
				{
					// Set the v-blank IF flag.
					CPU.Instance.IF |= 0x01;
				}

				// Check for a LYC == LY STAT interrupt.
				LYCEqualsLY = LYC == LY;
				if (LYCEqualsLY && LYCIntSelect)
				{
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
			LCDEnabled = Utilities.GetBitsFromByte(lcdc, 7, 7) != 0x00;
			WindowTileMapArea = Utilities.GetBitsFromByte(lcdc, 6, 6) != 0x00;
			WindowEnabled = Utilities.GetBitsFromByte(lcdc, 5, 5) != 0x00;
			BGWindowTileDataArea = Utilities.GetBitsFromByte(lcdc, 4, 4) != 0x00;
			BGTileMapArea = Utilities.GetBitsFromByte(lcdc, 3, 3) != 0x00;
			OBJSize = Utilities.GetBitsFromByte(lcdc, 2, 2) != 0x00;
			OBJEnabled = Utilities.GetBitsFromByte(lcdc, 1, 1) != 0x00;
			BGWindowEnable = Utilities.GetBitsFromByte(lcdc, 0, 0) != 0x00;
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
			LYCIntSelect = Utilities.GetBitsFromByte(stat, 6, 6) != 0x00;
			Mode2IntSelect = Utilities.GetBitsFromByte(stat, 5, 5) != 0x00;
			Mode1IntSelect = Utilities.GetBitsFromByte(stat, 4, 4) != 0x00;
			Mode0IntSelect = Utilities.GetBitsFromByte(stat, 3, 3) != 0x00;
			LYCEqualsLY = Utilities.GetBitsFromByte(stat, 2, 2) != 0x00;
			PPUMode = Utilities.GetBitsFromByte(stat, 0, 1);
		}

		public void Render(Graphics graphics, SolidBrush[] brushes, int scale)
		{
			// TODO: Move this after clearing?
			if (!LCDEnabled)
			{
				return;
			}

			graphics.Clear(brushes[0].Color);

			// Draw the background by iterating over each of its tiles.
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

					RenderTile(graphics, brushes, scale, tileAddress, tileX * 8 + SCX, tileY * 8 + SCY, BGPaletteData);
				}
			}

			// Draw the window as well.
			if (WindowEnabled)
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
						RenderTile(graphics, brushes, scale, tileAddress, tileX * 8 + WX - 7, tileY * 8 + WY, BGPaletteData);
					}
				}
			}

			// Draw the objects by iterating over each of their tiles.
			// TODO: Enforce 10 objects-per-scanline limitation?
			for (int objAddress = 0xFE00; objAddress <= 0xFE9C; objAddress += 0x04)
			{
				int y = Memory.Instance.Read(objAddress) - 16;
				int x = Memory.Instance.Read(objAddress + 1) - 8;
				byte tileNumber = Memory.Instance.Read(objAddress + 2);
				int tileAddress = 0x8000 + tileNumber * 16;
				byte attributes = Memory.Instance.Read(objAddress + 3);
				// TODO: Priority, x-flip, and y-flip.
				byte objPaletteData = Utilities.GetBitsFromByte(attributes, 4, 4) == 0x20 ? OBJPaletteData1 : OBJPaletteData0;

				RenderTile(graphics, brushes, scale, tileAddress, x, y, objPaletteData, true);

				// In 8x16 mode, also render the next tile immediately below.
				if (OBJSize)
				{
					tileAddress += 16;
					RenderTile(graphics, brushes, scale, tileAddress, x, y + 8, objPaletteData, true);
				}
			}
		}

		// Draw an individual tile with data from an address at a location with a palette.
		private void RenderTile(Graphics graphics, SolidBrush[] brushes, int scale,
			int tileAddress, int x, int y, byte palette, bool transparency = false)
		{
			if (tileAddress == 0)
			{
				return;
			}

			// Draw each tile, pixel by pixel.
			for (int pixelY = 0; pixelY < 8; ++pixelY)
			{
				for (int pixelX = 0; pixelX < 8; ++pixelX)
				{
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
						int brush = Utilities.GetBitsFromByte(palette, colorId * 2, colorId * 2 + 1);
						int lcdX = x + pixelX;
						int lcdY = y + pixelY;
						// TODO: Handle -x and -y wrapping around?
						graphics.FillRectangle(brushes[brush], lcdX * scale, lcdY * scale, scale, scale);
					}
				}
			}
		}
	}
}
