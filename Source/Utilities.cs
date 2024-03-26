namespace GBSharp
{
	internal class Utilities
	{
		// Returns the bits (as a byte) from a portion of another byte. For example, data of 0xE3 (0b11100011) from
		// bits 5 to 7 would return 0x07 (0b00000111).
		public static byte GetBitsFromByte(byte data, int firstBit, int lastBit)
		{
			int numBits = lastBit - firstBit + 1;
			byte offset = (byte)(Math.Pow(2, lastBit + 1) - 1);
			byte xor = (byte)(Math.Pow(2, firstBit) - 1);
			offset ^= xor;
			byte bits = (byte)((data & offset) >> firstBit);
			return bits;
		}
	}
}
