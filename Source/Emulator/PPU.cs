namespace GBSharp
{
	internal class PPU
	{
		// Every 456 dots, we increment LY, possibly trigger v-blank, etc.
		public const uint kDotsPerLine = 456;
		private const uint kVBlankLine = 144;
		public const uint kLinesPerFrame = 154;
		public const uint kCyclesPerFrame = kDotsPerLine / 4 * kLinesPerFrame;

		private uint Dots;

		// The LCDC register control flags (FF40)
		public bool LCDEnabled;
		private bool WindowTileMapArea;
		private bool WindowEnabled;
		public bool BGWindowTileDataArea;
		public bool BGTileMapArea;
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
		public uint SCY;
		public uint SCX;

		// The LCD y-coordinate (F44)
		public byte LY { get; private set; }

		// The LY compare value (FF45)
		public byte LYC;

		// The palette data (FF47, FF48, and FF49)
		public byte BGPaletteData;
		public byte OBJPaletteData0;
		public byte OBJPaletteData1;

		// The window Y and X values (FF4A and FF4B)
		public uint WY;
		public uint WX;

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
	}
}
