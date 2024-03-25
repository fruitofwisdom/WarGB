namespace GBSharp
{
	internal class Utilities
	{
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
