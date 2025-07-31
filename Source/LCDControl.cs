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
			e.Graphics.Clear(clearColor);

			// Read from the PPU's front buffer and render to our Graphics object.
			int scale = Size.Width / PPU.kWidth;
			for (int x = 0; x < PPU.kWidth; ++x)
			{
				for (int y = 0; y < PPU.kHeight; ++y)
				{
					int brushIndex = PPU.Instance.LCDFrontBuffer[x, y].Color;

					// When ghosting is enabled, ramp down the color.
					if (WithGhosting)
					{
						if (brushIndex < _lastColor[x, y])
						{
							brushIndex = _lastColor[x, y] - 1;
						}
					}

					// Don't bother rendering the clear color again.
					if (brushIndex != 0)
					{
						Brush brush = UseOriginalGreen ? _originalGreenBrushes[brushIndex] : _blackAndWhiteBrushes[brushIndex];
						e.Graphics.FillRectangle(brush, x * scale, y * scale, scale, scale);
					}

					_lastColor[x, y] = brushIndex;
				}
			}
		}
	}
}
