using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCompiler
{
	public struct BigNumber
	{
		static double doubleExp;
		static decimal decimalExp;
		static float floatExp;

		static BigNumber()
		{
			doubleExp = Math.Pow(2.0, 64.0);
			floatExp = (float)doubleExp;
			decimalExp = (decimal)doubleExp;
		}

		bool sign;
		ulong mantissa;
		ulong[] extraMantissa;
		int exponent;

		public double AsDouble()
		{
			double value = mantissa;
			if (extraMantissa != null)
				for (int i = 0; i < extraMantissa.Length; i++)
					value = (value * doubleExp) + extraMantissa[i];
			if(exponent != 0)
				value *= Math.Pow(2.0, exponent);
			if(sign) value *= -1.0;
			return value;
		}

		public double AsFloat()
		{
			float value = mantissa;
			if (extraMantissa != null)
				for (int i = 0; i < extraMantissa.Length; i++)
					value = (value * floatExp) + extraMantissa[i];
			if (exponent != 0)
				value *= (float)Math.Pow(2.0, exponent);
			if (sign) value *= -1.0f;
			return value;
		}

		public decimal AsDecimal()
		{
			decimal value = mantissa;
			if (extraMantissa != null)
				for (int i = 0; i < extraMantissa.Length; i++)
					value = (value * decimalExp) + extraMantissa[i];
			if (exponent != 0)
				value *= (decimal)Math.Pow(2.0, exponent);
			if (sign) value *= -1m;
			return value;
		}

		public int AsInt()
		{
			if (extraMantissa != null || exponent != 0)
				throw new InvalidCastException();
			return sign ? (int)mantissa : -(int)mantissa;
		}

		public long AsLong()
		{
			if(extraMantissa != null || exponent != 0)
				throw new InvalidCastException();
			return sign ? (long)mantissa : -(long)mantissa;
		}

		public ulong AsUlong()
		{
			if (sign || extraMantissa != null || exponent != 0)
				throw new InvalidCastException();
			return mantissa;
		}

		public uint AsUint()
		{
			if (sign || extraMantissa != null || exponent != 0)
				throw new InvalidCastException();
			return (uint)mantissa;
		}

		public byte AsByte()
		{
			if (sign || extraMantissa != null || exponent != 0)
				throw new InvalidCastException();
			return (byte)mantissa;
		}

		public sbyte AsSbyte()
		{
			if (extraMantissa != null || exponent != 0)
				throw new InvalidCastException();
			return sign ? (sbyte)mantissa : (sbyte)-(long)mantissa;
		}
	}
}
