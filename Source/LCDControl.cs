using WarGB.Properties;

namespace WarGB
{
	public partial class LCDControl : UserControl
	{
		public bool UseOriginalGreen = true;
		public bool WithGhosting = false;

		public float LCDScale = 1.0f;

		// The four shades of green we'll use for the Game Boy's LCD.
		private readonly SolidBrush[] _originalGreenBrushes;
		// The four shades we'll use for a black and white Game Boy.
		private readonly SolidBrush[] _blackAndWhiteBrushes;

		// Used for ghosting.
		private readonly int[,] _lastBrushIndex = new int[PPU.kWidth, PPU.kHeight];

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

			Array.Clear(_lastBrushIndex);
		}

		private void LCDControl_Paint(object sender, PaintEventArgs e)
		{
			LCDScale = (float)Size.Width / PPU.kWidth;

			// SGB screen masks 2 and 3 are just solid colors.
			if (PPU.Instance.ScreenMask == 2)
			{
				e.Graphics.Clear(Color.Black);
				return;
			}
			else if (PPU.Instance.ScreenMask == 3)
			{
				e.Graphics.Clear(Color.White);
				return;
			}

			// Clear the screen with the appropriate color.
			Color clearColor = UseOriginalGreen ? _originalGreenBrushes[0].Color : _blackAndWhiteBrushes[0].Color;
			if (SGB.Instance.Enabled)
			{
				clearColor = SGB.Instance.ClearColor;
			}
			e.Graphics.Clear(clearColor);

			// Read from the PPU's front buffer and render to our Graphics object.
			for (int x = 0; x < PPU.kWidth; ++x)
			{
				for (int y = 0; y < PPU.kHeight; ++y)
				{
					Brush brush;
					int brushIndex = PPU.Instance.LCDFrontBuffer[x, y].Color;

					if (SGB.Instance.Enabled)
					{
						brush = new SolidBrush(PPU.Instance.LCDFrontBuffer[x, y].SGBPalette.Colors[brushIndex]);
					}
					else if (CPU.Instance.IsCGB && (ROM.Instance.CGBCompatible || ROM.Instance.CGBOnly))
					{
						Color color = PPU.Instance.LCDFrontBuffer[x, y].CGBPalette.Colors[brushIndex];

						// If we want accurate colors, wash the color out some.
						if (Settings.Default.AccurateColors)
						{
							// TODO: Improve this algorithm?
							int newR = Math.Min((int)(color.R * 1.42f), 255);
							int newG = Math.Min((int)(color.G * 1.42f), 255);
							int newB = Math.Min((int)(color.B * 1.42f), 255);
							color = Color.FromArgb(color.A, newR, newG, newB);
						}

						brush = new SolidBrush(color);
					}
					else
					{
						// When original green ghosting is enabled, "ramp" down the color.
						if (WithGhosting)
						{
							if (brushIndex < _lastBrushIndex[x, y])
							{
								brushIndex = _lastBrushIndex[x, y] - 1;
							}
							_lastBrushIndex[x, y] = brushIndex;
						}

						// No need to render this pixel again.
						if (brushIndex == 0)
						{
							continue;
						}
						else
						{
							brush = UseOriginalGreen ? _originalGreenBrushes[brushIndex] : _blackAndWhiteBrushes[brushIndex];
						}
					}

					// Draw the pixel.
					e.Graphics.FillRectangle(brush, x * LCDScale, y * LCDScale, LCDScale, LCDScale);
				}
			}
		}
	}
}
