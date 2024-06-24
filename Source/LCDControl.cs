namespace GBSharp
{
	public partial class LCDControl : UserControl
	{
		// The four shades of green we'll use for the Game Boy's LCD.
		private readonly SolidBrush[] _brushes;

		public LCDControl()
		{
			InitializeComponent();

			// Some green colors, from lightest to darkest.
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
			e.Graphics.Clear(_brushes[0].Color);

			// Read from the PPU's front buffer and render to our Graphics object.
			int scale = Size.Width / PPU.kWidth;
			for (int x = 0; x < PPU.kWidth; ++x)
			{
				for (int y = 0; y < PPU.kHeight; ++y)
				{
					int brush = PPU.Instance.LCDFrontBuffer[x, y];
					// Don't bother rendering the clear color again.
					if (brush != 0)
					{
						e.Graphics.FillRectangle(_brushes[brush], x * scale, y * scale, scale, scale);
					}
				}
			}
		}
	}
}
