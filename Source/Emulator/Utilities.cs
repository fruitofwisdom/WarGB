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
			SetHAndCY(result, newResult, setH, setCY);
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
			SetHAndCY(result, newResult, setH, setCY);
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
			SetHAndCY(result, newResult, setH, setCY);
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
			SetHAndCY(result, newResult, setH, setCY);
			result = newResult;
		}

		// Set H and CY flags based on which bits overflowed between newValue and oldValue.
		void SetHAndCY(ushort oldValue, ushort newValue, bool setH = true, bool setCY = true)
		{
			byte oldNibble1 = (byte)(oldValue & 0x000F);
			byte oldNibble2 = (byte)((oldValue & 0x00F0) >> 4);
			byte oldNibble3 = (byte)((oldValue & 0x0F00) >> 8);
			byte oldNibble4 = (byte)((oldValue & 0xF000) >> 12);
			byte newNibble1 = (byte)(newValue & 0x000F);
			byte newNibble2 = (byte)((newValue & 0x00F0) >> 4);
			byte newNibble3 = (byte)((newValue & 0x0F00) >> 8);
			byte newNibble4 = (byte)((newValue & 0xF000) >> 12);

			// Addition occurred last, check for overflow.
			if (!N)
			{
				/*
				// Check for overflow.
				if (setH)
				{
					H = (ushort)((newValue & 0x0FFF) + (oldValue & 0x0FFF)) > 0x0FFF;
				}
				if (setCY)
				{
					CY = newValue < oldValue;
				}
				*/
				if (setH)
				{
					H = newNibble1 < oldNibble1 || newNibble3 < oldNibble3;
				}
				if (setCY)
				{
					CY = newNibble2 < oldNibble2 || newNibble4 < oldNibble4 || newValue < oldValue;
				}
			}
			// Subtraction occurred last, check for underflow.
			else
			{
				/*
				// Check for underflow.
				if (setH)
				{
					H = (ushort)((newValue & 0x0FFF) - (oldValue & 0x0FFF)) > 0x0FFF;
				}
				if (setCY)
				{
					CY = newValue > oldValue;
				}
				*/
				if (setH)
				{
					H = newNibble1 > oldNibble1 || newNibble3 > oldNibble3;
				}
				if (setCY)
				{
					CY = newNibble2 > oldNibble2 || newNibble4 > oldNibble4 || newValue > oldValue;
				}
			}
		}

		// Set H and CY flags based on which bits overflowed between newValue and oldValue.
		void SetHAndCY(byte oldValue, byte newValue, bool setH = true, bool setCY = true)
		{
			byte oldNibble1 = (byte)(oldValue & 0x0F);
			byte oldNibble2 = (byte)((oldValue & 0xF0) >> 4);
			byte newNibble1 = (byte)(newValue & 0x0F);
			byte newNibble2 = (byte)((newValue & 0xF0) >> 4);

			// Addition occurred last, check for overflow.
			if (!N)
			{
				/*
				// Check for overflow.
				if (setH)
				{
					H = (byte)((newValue & 0x0F) + (oldValue & 0x0F)) > 0x0F;
				}
				if (setCY)
				{
					CY = newValue < oldValue;
				}
				*/
				if (setH)
				{
					H = newNibble1 < oldNibble1;
				}
				if (setCY)
				{
					CY = newNibble2 < oldNibble2 || newValue < oldValue;
				}
			}
			// Subtraction occurred last, check for underflow.
			else
			{
				/*
				// Check for underflow.
				if (setH)
				{
					H = (byte)((newValue & 0x0F) - (oldValue & 0x0F)) > 0x0F;
				}
				if (setCY)
				{
					CY = newValue > oldValue;
				}
				*/
				if (setH)
				{
					H = newNibble1 > oldNibble1;
				}
				if (setCY)
				{
					CY = newNibble2 > oldNibble2 || newValue > oldValue;
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
