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
			_scale = Size.Width / 160;
			PPU.Instance.Render(e.Graphics, _brushes, _scale);
		}
	}
}
