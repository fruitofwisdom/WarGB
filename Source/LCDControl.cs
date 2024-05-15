namespace GBSharp
{
	public partial class LCDControl : UserControl
	{
		// The four shades of green we'll use for the Game Boy's LCD.
		private readonly SolidBrush[] _brushes;

		private int _scale = 1;

		public LCDControl()
		{
			InitializeComponent();

			_brushes =
			[
				new(Color.GreenYellow),
				new(Color.LimeGreen),
				new(Color.Green),
				new(Color.DarkGreen)
			];

		}

		private void LCDControl_Paint(object sender, PaintEventArgs e)
		{
			if (!PPU.Instance.LCDEnabled)
			{
				return;
			}

			_scale = Size.Width / 160;
			e.Graphics.Clear(Color.GreenYellow);

			// Draw the background by iterating over each of its tiles.
			for (int tileY = 0; tileY < 32; ++tileY)
			{
				for (int tileX = 0; tileX < 32; ++tileX)
				{
					int mapAddress = PPU.Instance.BGTileMapArea ? 0x9C00 : 0x9800;
					mapAddress += tileY * 32 + tileX;

					// Each tile is 16 bytes, but can be addressed in one of two ways.
					int tileAddress;
					if (PPU.Instance.BGWindowTileDataArea)
					{
						byte tileNumber = Memory.Instance.Read(mapAddress);
						tileAddress = 0x8000 + tileNumber * 16;
					}
					else
					{
						sbyte tileNumber = (sbyte)Memory.Instance.Read(mapAddress);
						tileAddress = 0x9000 + tileNumber * 16;
					}

					RenderTile(e.Graphics, tileAddress, tileX * 8, tileY * 8, PPU.Instance.BGPaletteData);
				}
			}

			// Draw the objects by iterating over each of their tiles.
			// TODO: Enforce 10 objects-per-scanline limitation?
			for (int objAddress = 0xFE00; objAddress  <= 0xFE9C; objAddress += 0x04)
			{
				int y = Memory.Instance.Read(objAddress) - 16;
				int x = Memory.Instance.Read(objAddress + 1) - 8;
				byte tileNumber = Memory.Instance.Read(objAddress + 2);
				int tileAddress = 0x8000 + tileNumber * 16;
				byte attributes = Memory.Instance.Read(objAddress + 3);
				// TODO: Priority, x-flip, and y-flip.
				byte objPaletteData = Utilities.GetBitsFromByte(attributes, 4, 4) == 0x20 ? PPU.Instance.OBJPaletteData1 : PPU.Instance.OBJPaletteData0;

				RenderTile(e.Graphics, tileAddress, x, y, objPaletteData);

				// In 8x16 mode, also render the next tile immediately below.
				if (PPU.Instance.OBJSize)
				{
					tileAddress += 16;
					RenderTile(e.Graphics, tileAddress, x, y + 8, objPaletteData);
				}
			}

			// TODO: Draw the window.
		}

		// Draw an individual tile with data from an address at a location with a palette.
		void RenderTile(Graphics graphics, int tileAddress, int x, int y, byte palette)
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

					// Look up the correct palette color and draw that pixel.
					int brush = Utilities.GetBitsFromByte(palette, colorId * 2, colorId * 2 + 1);
					int lcdX = x + pixelX;
					int lcdY = y + pixelY;
					graphics.FillRectangle(_brushes[brush], lcdX * _scale, lcdY * _scale, _scale, _scale);
				}
			}
		}
	}
}
