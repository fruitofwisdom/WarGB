namespace GBSharp
{
	internal class Graphics
	{
		// Dots run at full clock speed, i.e., 4 dots per CPU cycle. Every 456 cycles, we increment LY, possibly
		// trigger v-blank, etc.
		private const uint kDotsPerLine = 456;
		private const uint kVBlankLine = 144;
		private const uint kLinesPerFrame = 154;
		public const uint kCyclesPerFrame = kDotsPerLine / 4 * kLinesPerFrame;

		private uint Dot;

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

		// The LCD y-coordinate (F44)
		public byte LY { get; private set; }

		// The LY compare value (FF45)
		public byte LYC;

		public byte BGPaletteData;
		public byte OBJPaletteData0;
		public byte OBJPaletteData1;

		private static Graphics? _instance;
		public static Graphics Instance
		{
			get
			{
				_instance ??= new Graphics();
				return _instance;
			}
		}

		public Graphics()
		{
			Dot = 0;

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

			LY = 0x00;
			LYC = 0x00;

			BGPaletteData = 0;
			OBJPaletteData0 = 0;
			OBJPaletteData1 = 0;
		}

		public void Update()
		{
			// A "dot" is how long it takes to render one pixel and there are 4 dots per CPU cycle, usually.
			// TODO: Support CGB double-speed mode also?
			Dot = CPU.Instance.Cycles * 4;

			byte newLY = (byte)(Dot / kDotsPerLine % kLinesPerFrame);
			//uint cyclesToVBlank = kVBlankLine * kDotsPerLine / 4 - CPU.Instance.Cycles;

			// Set the PPU mode correctly.
			if (newLY >= kVBlankLine)
			{
				// Vertical blank.
				PPUMode = 0x01;
			}
			else if (Dot % kDotsPerLine < 80)
			{
				// OAM scan.
				PPUMode = 0x02;
			}
			else if (Dot % kDotsPerLine < 252)
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
