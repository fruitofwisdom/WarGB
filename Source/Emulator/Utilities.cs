namespace GBSharp
{
	internal partial class CPU
	{
		// Add value to result, potentially setting any F flags appropriately.
		void Add(ref ushort result, ushort value, bool setZ = true, bool setH = true, bool setCY = true)
		{
			ushort newResult = (ushort)(result + value);
			if (setZ)
			{
				Z = newResult == 0x0000;
			}
			N = false;
			SetHAndCY(result, value, setH, setCY);
			result = newResult;
		}

		// Add value to result, potentially setting any F flags appropriately.
		void Add(ref byte result, byte value, bool setZ = true, bool setH = true, bool setCY = true)
		{
			byte newResult = (byte)(result + value);
			if (setZ)
			{
				Z = newResult == 0x00;
			}
			N = false;
			SetHAndCY(result, value, setH, setCY);
			result = newResult;
		}

		// Subtract value from result, potentially setting any F flags appropriately.
		void Sub(ref ushort result, ushort value, bool setZ = true, bool setH = true, bool setCY = true)
		{
			ushort newResult = (ushort)(result - value);
			if (setZ)
			{
				Z = newResult == 0x0000;
			}
			N = true;
			SetHAndCY(result, value, setH, setCY);
			result = newResult;
		}

		// Subtract value from result, potentially setting any F flags appropriately.
		void Sub(ref byte result, byte value, bool setZ = true, bool setH = true, bool setCY = true)
		{
			byte newResult = (byte)(result - value);
			if (setZ)
			{
				Z = newResult == 0x00;
			}
			N = true;
			SetHAndCY(result, value, setH, setCY);
			result = newResult;
		}

		// Set H and CY flags based on which bits overflowed between newValue and oldValue.
		void SetHAndCY(ushort oldValue, ushort newValue, bool setH = true, bool setCY = true)
		{
			// Addition occurred last, check for overflow.
			if (!N)
			{
				if (setH)
				{
					H = ((oldValue & 0x0FFF) + (newValue & 0x0FFF)) > 0x0FFF;
				}
				if (setCY)
				{
					CY = oldValue + newValue > 0xFFFF;
				}
			}
			// Subtraction occurred last, check for underflow.
			else
			{
				if (setH)
				{
					H = ((oldValue & 0x0FFF) - (newValue & 0x0FFF)) < 0x0000;
				}
				if (setCY)
				{
					CY = oldValue - newValue < 0x0000;
				}
			}
		}

		// Set H and CY flags based on which bits overflowed between newValue and oldValue.
		void SetHAndCY(byte oldValue, byte newValue, bool setH = true, bool setCY = true)
		{
			// Addition occurred last, check for overflow.
			if (!N)
			{
				if (setH)
				{
					H = ((oldValue & 0x0F) + (newValue & 0x0F)) > 0x0F;
				}
				if (setCY)
				{
					CY = oldValue + newValue > 0xFF;
				}
			}
			// Subtraction occurred last, check for underflow.
			else
			{
				if (setH)
				{
					H = ((oldValue & 0x0F) - (newValue & 0x0F)) < 0x00;
				}
				if (setCY)
				{
					CY = oldValue - newValue < 0x00;
				}
			}
		}
	}

	internal class Utilities
	{
		// Returns the bits (as a byte) from a portion of another byte. For example, data of 0xE3 (0b11100011) from
		// bits 5 to 7 would return 0x07 (0b00000111).
		public static byte GetBitsFromByte(byte data, int firstBit, int lastBit)
		{
			byte offset = (byte)((1 << (lastBit + 1)) - 1);
			byte xor = (byte)((1 << firstBit) - 1);
			offset ^= xor;
			byte bits = (byte)((data & offset) >> firstBit);
			return bits;
		}

		// Sets the bits in a portion of a byte to another bit (as a byte). For example, data of 0x03 (0b00000011) and
		// value of 0x7 from bits 5 to 7 would become 0xE3 (0b11100011).
		public static void SetBitsInByte(ref byte data, byte value, int firstBit, int lastBit)
		{
			byte or = (byte)(value << firstBit);
			// Get all the bits we will touch and clear them.
			byte offset = (byte)(((1 << (lastBit - firstBit + 1)) - 1) << firstBit);
			byte opposite = (byte)(~offset);
			data &= opposite;
			// Assign value in their place.
			data |= or;
		}

		// Returns a bool that represents whether a particular bit is set. For example, data of 0x42 (0b00101010) and
		// bit 3 would be true.
		public static bool GetBoolFromByte(byte data, int bit)
		{
			byte bitmask = (byte)(1 << bit);
			return (data & bitmask) != 0x00;
		}
	}
}
