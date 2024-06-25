namespace GBSharp
{
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
	}
}
