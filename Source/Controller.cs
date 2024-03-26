namespace GBSharp
{
	internal class Controller
	{
		// Which inputs are being read.
		public bool SelectButtons;
		public bool SelectDpad;

		// The state of the inputs.
		public bool A;
		public bool B;
		public bool Select;
		public bool Start;
		public bool Right;
		public bool Left;
		public bool Up;
		public bool Down;

		private static Controller? _instance;
		public static Controller Instance
		{
			get
			{
				_instance ??= new Controller();
				return _instance;
			}
		}

		public Controller()
		{
			SelectButtons = false;
			SelectDpad = false;

			A = false;
			B = false;
			Select = false;
			Start = false;
			Right = false;
			Left = false;
			Up = false;
			Down = false;
		}

		// Return the inputs as the register FF00.
		public byte ReadFromRegister()
		{
			byte data = 0x00;

			if (SelectButtons)
			{
				data |= 0x20;
				data |= (byte)(A ? 0x00 : 0x01);
				data |= (byte)(B ? 0x00 : 0x02);
				data |= (byte)(Select ? 0x00 : 0x04);
				data |= (byte)(Start ? 0x00 : 0x08);
			}
			else if (SelectDpad)
			{
				data |= 0x10;
				data |= (byte)(Right ? 0x00 : 0x01);
				data |= (byte)(Left ? 0x00 : 0x02);
				data |= (byte)(Up ? 0x00 : 0x04);
				data |= (byte)(Down ? 0x00 : 0x08);
			}
			else
			{
				// All buttons released.
				data |= 0x0F;
			}

			return data;
		}
	}
}
