using System.Collections.Generic;

namespace OpenCompiler
{
	/// <summary>
	/// Base class for any literal value
	/// </summary>
	public abstract class Literal : Token
	{
		public abstract object Value { get; }
	}

	/// <summary>
	/// A numeric literal
	/// </summary>
	public class NumberLiteral : Literal
	{
		/// <summary>
		/// The zero value, cached here for efficiency
		/// </summary>
		public static readonly NumberLiteral Zero = new NumberLiteral(0);

		/// <summary>
		/// Direct access to HasPoint
		/// </summary>
		protected bool hasPoint;

		/// <summary>
		/// Direct access to NumberValue
		/// </summary>
		protected double value;

		/// <summary>
		/// Direct access to Value
		/// </summary>
		protected object boxed;

		/// <summary>
		/// Direct access to NumberBase
		/// </summary>
		protected int numberBase;

		protected int length = 1;

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="value">The value of the literal</param>
		/// <param name="hasPoint">Whether it has a (decimal) point</param>
		/// <param name="numberBase">The recorded number base (10 for decimal)</param>
		public NumberLiteral(double value, bool hasPoint, int numberBase)
		{
			this.value = value;
			this.hasPoint = hasPoint;
			this.numberBase = numberBase;
		}

		/// <summary>
		/// Creates a new decimal integer literal
		/// </summary>
		/// <param name="value">The value of the literal</param>
		public NumberLiteral(double value)
			: this(value, false, 10)
		{
		}

		/// <summary>
		/// Whether it has a (decimal) point
		/// </summary>
		public virtual bool HasPoint
		{
			get { return hasPoint; }
		}

		/// <summary>
		/// The value of the literal
		/// </summary>
		public override object Value
		{
			get
			{
				if (boxed == null)
					boxed = value;
				return boxed;
			}
		}

		public override int Length
		{
			get { return length; }
		}

		/// <summary>
		/// The numeric literal value
		/// </summary>
		public virtual double NumberValue
		{
			get { return value; }
		}

		/// <summary>
		/// Don't use whitespace
		/// </summary>
		public override bool UseWhitespace
		{
			get { return false; }
		}

		/// <summary>
		/// The recorded number base (10 for decimal)
		/// </summary>
		public virtual int NumberBase
		{
			get { return numberBase; }
		}

		/// <summary>
		/// The normal map of characters to digit values
		/// </summary>
		/// <remarks>
		/// Values less than zero mark the location of the point
		/// </remarks>
		public static readonly Dictionary<char, int> NormalDigitMap
			= new Dictionary<char, int>()
			{
				{ '0', 0 },
				{ '1', 1 },
				{ '2', 2 },
				{ '3', 3 },
				{ '4', 4 },
				{ '5', 5 },
				{ '6', 6 },
				{ '7', 7 },
				{ '8', 8 },
				{ '9', 9 },
				{ 'a', 10 },
				{ 'A', 10 },
				{ 'b', 11 },
				{ 'B', 11 },
				{ 'c', 12 },
				{ 'C', 12 },
				{ 'd', 13 },
				{ 'D', 13 },
				{ 'e', 14 },
				{ 'E', 14 },
				{ 'f', 15 },
				{ 'F', 15 },
				{ '.', -1 },
			};

		/// <summary>
		/// The currently used digit map. Normally the <see cref="NormalDigitMap"/>
		/// </summary>
		/// <remarks>
		/// Values less than zero mark the location of the point
		/// </remarks>
		public virtual IDictionary<char, int> DigitMap
		{
			get { return NormalDigitMap; }
		}

		/// <summary>
		/// Uses prefixes to get the number base of the literal
		/// </summary>
		/// <remarks>
		/// '0x' for hex, '0b' for binary and '0' for octal
		/// </remarks>
		/// <param name="lexer">The lexer object</param>
		/// <param name="hasPoint">Set if a (decimal) point is found</param>
		/// <returns>The number base, or 0 for zero, or -1 for invalid</returns>
		public virtual int GetNumberBase(Lexer lexer, out bool hasPoint)
		{
			int numberBase = 10;
			hasPoint = false;
			if (lexer.Current == '0')
			{
				lexer.Advance();
				switch (lexer.Current)
				{
					case 'X':
					case 'x':
						lexer.Advance();
						numberBase = 16;
						break;
					case 'b':
						lexer.Advance();
						numberBase = 2;
						break;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						numberBase = 8;
						break;
					case '.':
						hasPoint = true;
						break;
					default:
						lexer.Advance();
						return 0;
				}
			}
			else
			{
				var digitValue = -1;
				DigitMap.TryGetValue(lexer.Current, out digitValue);
				if (digitValue < 0)
					return -1;
			}
			return numberBase;
		}

		/// <summary>
		/// Returns zero
		/// </summary>
		/// <returns><see cref="Zero"/></returns>
		public override Token Create()
		{
			return Zero;
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="value">The value of the literal</param>
		/// <param name="hasPoint">Whether it has a (decimal) point</param>
		/// <param name="numberBase">The recorded number base (10 for decimal)</param>
		/// <returns>The created instance</returns>
		public virtual Token Create(double value, bool hasPoint, int numberBase)
		{
			return new NumberLiteral(value, hasPoint, numberBase);
		}

		/// <summary>
		/// Gets a number literal from the lexer
		/// </summary>
		/// <param name="lexer">The lexer object</param>
		/// <returns>A new numeric literal, <see cref="Zero"/>, or <c>null</c></returns>
		public override Token CheckPresence(Lexer lexer)
		{
			bool hasPoint;
			int numberBase = GetNumberBase(lexer, out hasPoint);
			if (numberBase == 0)
				return Create();
			else if (numberBase < 0)
				return null;
			int charValue;
			float multiplier = 1f;
			double totalValue = 0.0;
			while (DigitMap.TryGetValue(lexer.Current, out charValue) && charValue < numberBase)
			{
				if (charValue < 0)
				{
					if (hasPoint)
						break;
					else
						hasPoint = true;
				}
				else
				{
					if (hasPoint)
					{
						multiplier /= numberBase;
						totalValue += charValue * multiplier;
					}
					else
					{
						totalValue *= numberBase;
						totalValue += charValue;
					}
				}
			}
			return Create(totalValue, hasPoint, numberBase);
		}
	}
}