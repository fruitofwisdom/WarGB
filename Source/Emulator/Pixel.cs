namespace WarGB
{
	internal struct Pixel
	{
		// The brush index (basically color).
		public int Color = 0;

		// These two are needed for priority handling.
		public int X = 0;
		public int ObjAddress = 0x0000;

		// Palette data, if it's available.
		public Palette Palette = new();

		public Pixel() { }
	}

	internal struct Palette
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

		// Return a Color from the given ushort 5-bit color data.
		public static Color GetColorFromData(ushort colorData)
		{
			Color color;

			// Convert each channel from 5-bit to 8-bit RGB.
			int red = (int)((float)(ushort)(colorData & 0x001F) / 0x001F * 255);
			int green = (int)((float)(ushort)((colorData & 0x03E0) >> 5) / 0x001F * 255);
			int blue = (int)((float)(ushort)((colorData & 0x7C00) >> 10) / 0x001F * 255);
			color = Color.FromArgb(255, red, green, blue);

			return color;
		}
	}
}
