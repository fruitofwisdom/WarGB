namespace GBSharp
{
	internal class SGB
	{
		internal struct Packet
		{
			public byte[] _data;

			public Packet()
			{
				_data = new byte[16];
			}
		}

		// Emulator SGB options.
		public bool Allowed = false;

		public bool Enabled { get; private set; }

		// Are we receiving data from the game?
		public bool Receiving { get; private set; }
		private int _numBitsReceived;

		// A transmission is a series of packets.
		private List<Packet> _packets = [];
		private int _currentPacket = 0;
		private int _transmissionLength;

		private static SGB? _instance;
		public static SGB Instance
		{
			get
			{
				_instance ??= new SGB();
				return _instance;
			}
		}

		public SGB()
		{
			Reset();
		}

		public void Reset()
		{
			// Retain emulator options.
			//Allowed = false;

			Enabled = false;

			Receiving = false;
			_numBitsReceived = 0;

			_packets.Clear();
			_currentPacket = 0;
			_transmissionLength = 0;
		}

		public void StartReceiving()
		{
			// SGB functionality is turned off.
			if (!Allowed)
			{
				return;
			}

			Receiving = true;
			_numBitsReceived = 0;

			_packets.Add(new Packet());
		}

		public void SendBit(byte bit)
		{
			if (_numBitsReceived < 128)
			{
				int whichByte = _numBitsReceived / 8;
				_packets[_currentPacket]._data[whichByte] |= (byte)(bit << (_numBitsReceived % 8));

				_numBitsReceived++;
			}
			else
			{
				// After 128 bits, this packet is complete.
				Receiving = false;
				_numBitsReceived = 0;

				// The first packet contains the header.
				if (_currentPacket == 0)
				{
					_transmissionLength = _packets[0]._data[0] & 0x07;
				}

				_currentPacket++;
				if (_currentPacket == _transmissionLength)
				{
					byte commandCode = (byte)((_packets[0]._data[0] & 0xF8) >> 3);
					HandleTransmission(commandCode);

					// This transmission is complete.
					_packets.Clear();
					_currentPacket = 0;
					_transmissionLength = 0;
				}
			}
		}

		private void HandleTransmission(byte commandCode)
		{
			switch (commandCode)
			{
				case 0x00:		// PAL01
					{
						// TODO: Implement.
						GameBoy.DebugOutput += "Finish implementing command code PAL01 (0x00).\n";
					}
					break;

				case 0x04:		// ATTR_BLK
					{
						// TODO: Implement.
						GameBoy.DebugOutput += "Finish implementing command code ATTR_BLK (0x04).\n";
					}
					break;

				case 0x06:		// ATTR_DIV
					{
						// TODO: Implement.
						GameBoy.DebugOutput += "Finish implementing command code ATTR_DIV (0x06).\n";
					}
					break;

				case 0x07:		// ATTR_CHR
					{
						// TODO: Implement.
						GameBoy.DebugOutput += "Finish implementing command code ATTR_CHR (0x07).\n";
					}
					break;

				case 0x08:		// SOUND
					{
						// TODO: Support SGB sound?
					}
					break;

				case 0x09:		// SOU_TRN
					{
						// TODO: Support SGB sound?
					}
					break;

				case 0x0A:		// PAL_SET
					{
						byte flags = _packets[0]._data[9];
						// Cancel the screen mask.
						if ((flags & 0x40) == 0x040)
						{
							PPU.Instance.ScreenMask = 0;
						}

						// TODO: Implement.
						GameBoy.DebugOutput += "Finish implementing command code PAL_SET (0x0A).\n";
					}
					break;

				case 0x0B:		// PAL_TRN
					{
						// TODO: Implement.
						GameBoy.DebugOutput += "Finish implementing command code PAL_TRN (0x0B).\n";
					}
					break;

				case 0x0C:		// ATRC_EN
					{
						// NOTE: Ignore?
					}
					break;

				case 0x0E:		// ICON_EN
					{
						// NOTE: Ignore?
					}
					break;

				case 0x0F:		// DATA_SND
					{
						// TODO: Anything?
						GameBoy.DebugOutput += "Finish implementing command code DATA_SND (0x0F)?\n";
					}
					break;

				case 0x11:		// MLT_REQ
					{
						// This is traditionally used to detect SGB support.
						Enabled = true;
					}
					break;

				case 0x13:		// CHR_TRN
					{
						// TODO: Support SGB border?
					}
					break;

				case 0x14:		// PCT_TRN
					{
						// TODO: Support SGB border?
					}
					break;

				case 0x15:		// ATTR_TRN
					{
						// TODO: Implement.
						GameBoy.DebugOutput += "Finish implementing command code ATTR_TRN (0x15).\n";
					}
					break;

				case 0x17:		// MASK_EN
					{
						// Set the screen mask.
						PPU.Instance.ScreenMask = _packets[0]._data[1];
					}
					break;

				case 0x19:		// PAL_PRI
					{
						// TODO: Anything?
						GameBoy.DebugOutput += "Finish implementing command code PAL_PRI (0x19)?\n";
					}
					break;

				default:
					{
						GameBoy.DebugOutput += $"Read an unimplemented command code: 0x{commandCode:X2}!\n";
						MainForm.Pause();
					}
					break;
			}
		}
	}
}
