namespace GBSharp
{
	internal class Graphics
	{
		public const uint kLinesPerFrame = 154;
		public uint CyclesPerLine { get; private set; }

		// The LCD control flags and related registers.
		private bool LCDEnabled;
		private bool WindowTileMapArea;
		private bool WindowEnabled;
		private bool BGWindowTileDataArea;
		private bool BGTileMapArea;
		private bool OBJSize;
		private bool OBJEnabled;
		private bool BGWindowEnable;

		public byte LY;		// LCDC y-coordinate
		// TODO: The other LCD display registers.

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
			CyclesPerLine = (uint)(CPU.Instance.Frequency / 1000.0f * 0.10875f);

			LCDEnabled = true;
			WindowTileMapArea = false;
			WindowEnabled = false;
			BGWindowTileDataArea = true;
			BGTileMapArea = false;
			OBJSize = false;
			OBJEnabled = false;
			BGWindowEnable = true;

			LY = 0;

			BGPaletteData = 0;
			OBJPaletteData0 = 0;
			OBJPaletteData1 = 0;
		}

		public void Update()
		{
			// TODO: Update LCD controller another way?
			// 144 lines at 0.10875 lines per millisecond then 10 lines of v-blank.
			// Every 456 cycles, we increment LY, possibly trigger v-blank, etc.
			byte newLY = (byte)(CPU.Instance.Cycles / CyclesPerLine % kLinesPerFrame);
			if (newLY != LY)
			{
				LY = newLY;

				// V-blank begins at line 144
				if (LY == 144)
				{
					// Set the v-blank IF flag.
					CPU.Instance.IF |= 0x01;
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
	}
}
