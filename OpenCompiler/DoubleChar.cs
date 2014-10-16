namespace OpenCompiler
{
	/// <summary>
	/// Base class for lexer items with two of the same character
	/// </summary>
	public abstract class TwoChar : SingleChar
	{
		/// <summary>
		/// Should be a higher priority than single character items
		/// </summary>
		public override int Priority
		{
			get { return base.Priority + 1; }
		}

		public abstract char SecondChar { get; }

		/// <summary>
		/// Checks for a double character item
		/// </summary>
		/// <param name="lexer">The lexer object</param>
		/// <returns>This double character item, or <c>null</c></returns>
		public override Token CheckPresence(Lexer lexer)
		{
			if (lexer.Current == StartChar && lexer[1] == SecondChar)
			{
				lexer.Advance(2);
				return this;
			}
			return null;
		}
	}

	/// <summary>
	/// Base class for double character operators
	/// </summary>
	public abstract class DoubleOperator : TwoChar
	{
		public override char SecondChar
		{
			get { return StartChar; }
		}
	}

	/// <summary>
	/// C-style boolean AND operator
	/// </summary>
	public class AND : DoubleOperator
	{
		/// <summary>
		/// An ampersand '&'
		/// </summary>
		public override char StartChar
		{
			get { return '&'; }
		}
	}

	/// <summary>
	/// C-style boolean OR operator
	/// </summary>
	public class OR : DoubleOperator
	{
		/// <summary>
		/// A pipe '|'
		/// </summary>
		public override char StartChar
		{
			get { return '|'; }
		}
	}

	/// <summary>
	/// The equality conditional
	/// </summary>
	public class Equality : DoubleOperator
	{
		/// <summary>
		/// '='
		/// </summary>
		public override char StartChar
		{
			get { return '='; }
		}
	}

	/// <summary>
	/// The increment operator
	/// </summary>
	public class Increment : DoubleOperator
	{
		/// <summary>
		/// '+'
		/// </summary>
		public override char StartChar
		{
			get { return '+'; }
		}
	}

	/// <summary>
	/// The decrement operator
	/// </summary>
	public class Decrement : DoubleOperator
	{
		/// <summary>
		/// '-'
		/// </summary>
		public override char StartChar
		{
			get { return '-'; }
		}
	}

	/// <summary>
	/// The struct pointer resolution operator
	/// </summary>
	public class StructPtr : TwoChar
	{
		/// <summary>
		/// '-'
		/// </summary>
		public override char StartChar
		{
			get { return '-'; }
		}

		/// <summary>
		/// '&gt'
		/// </summary>
		public override char SecondChar
		{
			get { return '>'; }
		}
	}

	/// <summary>
	/// The inequality conditional
	/// </summary>
	public class Inequality : TwoChar
	{
		/// <summary>
		/// '!'
		/// </summary>
		public override char StartChar
		{
			get { return '!'; }
		}

		/// <summary>
		/// '='
		/// </summary>
		public override char SecondChar
		{
			get { return '='; }
		}
	}

	/// <summary>
	/// The 'greater than or equal to' conditional
	/// </summary>
	public class GreaterThanOrEqual : TwoChar
	{
		/// <summary>
		/// '&gt'
		/// </summary>
		public override char StartChar
		{
			get { return '>'; }
		}

		/// <summary>
		/// '='
		/// </summary>
		public override char SecondChar
		{
			get { return '='; }
		}
	}

	/// <summary>
	/// The 'less than or equal to' conditional
	/// </summary>
	public class LessThanOrEqual : TwoChar
	{
		/// <summary>
		/// '&lt'
		/// </summary>
		public override char StartChar
		{
			get { return '<'; }
		}

		/// <summary>
		/// '='
		/// </summary>
		public override char SecondChar
		{
			get { return '='; }
		}
	}
}