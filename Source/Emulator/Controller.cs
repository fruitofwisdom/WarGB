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

		public void TriggerJoypadInterrupt()
		{
			if (GameBoy.ShouldLogOpcodes)
			{
				GameBoy.LogOutput += "A joypad interrupt occurred.\n";
			}

			// Set the joypad interrupt flag.
			CPU.Instance.IF |= 0x10;
		}

		// Return the inputs as the register FF00.
		public byte ReadFromRegister()
		{
			byte data = 0xC0;

			if (SelectButtons)
			{
				data |= 0x10;
				data |= (byte)(A ? 0x00 : 0x01);
				data |= (byte)(B ? 0x00 : 0x02);
				data |= (byte)(Select ? 0x00 : 0x04);
				data |= (byte)(Start ? 0x00 : 0x08);

				if (SGB.Instance.Enabled && SGB.Instance.ActivePlayer() != 0xFF)
				{
					// TODO: Add a mapping for additional players?
					data |= 0x3F;
				}
			}
			else if (SelectDpad)
			{
				data |= 0x20;
				data |= (byte)(Right ? 0x00 : 0x01);
				data |= (byte)(Left ? 0x00 : 0x02);
				data |= (byte)(Up ? 0x00 : 0x04);
				data |= (byte)(Down ? 0x00 : 0x08);

				if (SGB.Instance.Enabled && SGB.Instance.ActivePlayer() != 0xFF)
				{
					// TODO: Add a mapping for additional players?
					data |= 0x3F;
				}
			}
			else
			{
				if (SGB.Instance.Enabled)
				{
					// Return which controller is active.
					data = SGB.Instance.ActivePlayer();
				}
				else
				{
					// All buttons released.
					data |= 0x3F;
				}
			}

			return data;
		}
	}
}
