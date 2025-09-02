namespace GBSharp
{
	public partial class LCDControl : UserControl
	{
		public bool UseOriginalGreen = true;
		public bool WithGhosting = false;

		// The four shades of green we'll use for the Game Boy's LCD.
		private readonly SolidBrush[] _originalGreenBrushes;
		// The four shades we'll use for a black and white Game Boy.
		private readonly SolidBrush[] _blackAndWhiteBrushes;

		// Used for ghosting.
		private int[,] _lastColor = new int[PPU.kWidth, PPU.kHeight];

		public LCDControl()
		{
			InitializeComponent();

			// Some green colors, from lightest to darkest.
			_originalGreenBrushes =
			[
				new(Color.GreenYellow),
				new(Color.LimeGreen),
				new(Color.Green),
				new(Color.DarkGreen)
			];

			// Some black and whites too.
			_blackAndWhiteBrushes =
			[
				new(Color.White),
				new(Color.LightGray),
				new(Color.Gray),
				new(Color.Black),
			];

			Array.Clear(_lastColor);
		}

		private void LCDControl_Paint(object sender, PaintEventArgs e)
		{
			Color clearColor = UseOriginalGreen ? _originalGreenBrushes[0].Color : _blackAndWhiteBrushes[0].Color;
			if (SGB.Instance.Enabled)
			{
				// The SGB clear color is the first color of the first palette.
				clearColor = SGB.Instance.Palettes[0].Colors[0];
			}
			e.Graphics.Clear(clearColor);

			// Read from the PPU's front buffer and render to our Graphics object.
			int scale = Size.Width / PPU.kWidth;
			for (int x = 0; x < PPU.kWidth; ++x)
			{
				for (int y = 0; y < PPU.kHeight; ++y)
				{
					Brush brush;
					int brushIndex = PPU.Instance.LCDFrontBuffer[x, y].Color;

					// SGB screen masks 2 and 3 are just solid colors.
					if (PPU.Instance.ScreenMask == 2)
					{
						brush = new SolidBrush(Color.Black);
						e.Graphics.FillRectangle(brush, x * scale, y * scale, scale, scale);
						continue;
					}
					else if (PPU.Instance.ScreenMask == 3)
					{
						brush = new SolidBrush(Color.White);
						e.Graphics.FillRectangle(brush, x * scale, y * scale, scale, scale);
						continue;
					}

					// When original green ghosting is enabled, "ramp" down the color.
					if (WithGhosting)
					{
						if (brushIndex < _lastColor[x, y])
						{
							brushIndex = _lastColor[x, y] - 1;
						}
					}

					if (brushIndex != 0)		// Don't bother rendering the clear color again.
					{
						if (SGB.Instance.Enabled)
						{
							brush = new SolidBrush(PPU.Instance.LCDFrontBuffer[x, y].SGBPalette.Colors[brushIndex]);
						}
						else
						{
							brush = UseOriginalGreen ? _originalGreenBrushes[brushIndex] : _blackAndWhiteBrushes[brushIndex];
						}
						e.Graphics.FillRectangle(brush, x * scale, y * scale, scale, scale);
					}

					_lastColor[x, y] = brushIndex;
				}
			}
		}
	}
}
