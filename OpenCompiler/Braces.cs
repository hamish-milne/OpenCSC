namespace OpenCompiler
{
	/// <summary>
	/// Curly braces, used normally to delimit blocks of code
	/// </summary>
	public abstract class BlockBrace : Brace
	{
		/// <inheritdoc/>
		public override bool IsBraceType(Brace brace)
		{
			return brace is BlockBrace;
		}

		/// <inheritdoc/>
		public override bool IsOpenBrace(Brace open)
		{
			return open is BlockBraceOpen;
		}

		/// <inheritdoc/>
		public override bool IsCloseBrace(Brace close)
		{
			return close is BlockBraceClose;
		}
	}

	/// <inheritdoc/>
	public class BlockBraceOpen : BlockBrace
	{
		/// <summary>
		/// '{'
		/// </summary>
		public override char StartChar
		{
			get { return '{'; }
		}
	}

	/// <inheritdoc/>
	public class BlockBraceClose : BlockBrace
	{
		/// <summary>
		/// '}'
		/// </summary>
		public override char StartChar
		{
			get { return '}'; }
		}
	}

	/// <summary>
	/// Parentheses, used for function definitions and calls
	/// </summary>
	public abstract class Paren : Brace
	{
		/// <inheritdoc/>
		public override bool IsBraceType(Brace brace)
		{
			return brace is Paren;
		}

		/// <inheritdoc/>
		public override bool IsOpenBrace(Brace open)
		{
			return open is ParenOpen;
		}

		/// <inheritdoc/>
		public override bool IsCloseBrace(Brace close)
		{
			return close is ParenClose;
		}
	}

	/// <inheritdoc/>
	public class ParenOpen : Paren
	{
		/// <summary>
		/// '('
		/// </summary>
		public override char StartChar
		{
			get { return '('; }
		}
	}

	/// <inheritdoc/>
	public class ParenClose : Paren
	{
		/// <summary>
		/// ')'
		/// </summary>
		public override char StartChar
		{
			get { return ')'; }
		}
	}

	/// <summary>
	/// Square brackets, used for array definitions and indexers
	/// </summary>
	public abstract class Bracket : Brace
	{
		/// <inheritdoc/>
		public override bool IsBraceType(Brace brace)
		{
			return brace is Bracket;
		}

		/// <inheritdoc/>
		public override bool IsCloseBrace(Brace close)
		{
			return close is BracketClose;
		}

		/// <inheritdoc/>
		public override bool IsOpenBrace(Brace open)
		{
			return open is BracketOpen;
		}
	}

	/// <inheritdoc/>
	public class BracketOpen : Bracket
	{
		/// <summary>
		/// '['
		/// </summary>
		public override char StartChar
		{
			get { return '['; }
		}
	}

	/// <inheritdoc/>
	public class BracketClose : Bracket
	{
		/// <summary>
		/// ']'
		/// </summary>
		public override char StartChar
		{
			get { return ']'; }
		}
	}

	/// <summary>
	/// Angle brackets, used in C# generics
	/// </summary>
	public abstract class Angle : Brace
	{
		/// <inheritdoc/>
		public override bool IsBraceType(Brace brace)
		{
			return brace is Angle;
		}

		/// <inheritdoc/>
		public override bool IsCloseBrace(Brace close)
		{
			return close is AngleClose;
		}

		/// <inheritdoc/>
		public override bool IsOpenBrace(Brace open)
		{
			return open is AngleOpen;
		}
	}

	public class AngleOpen : Angle
	{
		/// <summary>
		/// '&lt'
		/// </summary>
		public override char StartChar
		{
			get { return '<'; }
		}
	}

	public class AngleClose : Angle
	{
		/// <summary>
		/// '&gt'
		/// </summary>
		public override char StartChar
		{
			get { return '>'; }
		}
	}
}