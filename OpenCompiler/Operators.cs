namespace OpenCompiler
{
	/// <summary>
	/// Base class for a single-character operator
	/// </summary>
	public abstract class Operator : SingleChar { }

	/// <summary>
	/// The dot operator, used for object resolution
	/// </summary>
	public class Dot : Operator
	{
		public override char StartChar
		{
			get { return '.'; }
		}
	}

	/// <summary>
	/// Semicolons, used to end statements
	/// </summary>
	public class Semicolon : Operator
	{
		public override char StartChar
		{
			get { return ';'; }
		}
	}

	/// <summary>
	/// Colon, used in C++ for namespace resolution, and others
	/// for ternary statements
	/// </summary>
	public class Colon : Operator
	{
		public override char StartChar
		{
			get { return ':'; }
		}
	}

	/// <summary>
	/// Comma, used to separate statements or arguments
	/// </summary>
	public class Comma : Operator
	{
		public override char StartChar
		{
			get { return ','; }
		}
	}

	/// <summary>
	/// Hyphen, used for arithmetic negation or subtraction
	/// </summary>
	public class Hyphen : Operator
	{
		public override char StartChar
		{
			get { return '-'; }
		}
	}

	/// <summary>
	/// Plus, used for arithmetic addition
	/// </summary>
	public class Plus : Operator
	{
		public override char StartChar
		{
			get { return '+'; }
		}
	}

	/// <summary>
	/// Forward slash, used for divide
	/// </summary>
	public class ForwardSlash : Operator
	{
		public override char StartChar
		{
			get { return '/'; }
		}
	}

	/// <summary>
	/// Backslash, used for escape sequences
	/// </summary>
	public class Backslash : Operator
	{
		public override char StartChar
		{
			get { return '\\'; }
		}
	}

	/// <summary>
	/// At sign, used for verbatim literals and use of reserved keywords as identifiers
	/// </summary>
	public class At : Operator
	{
		public override char StartChar
		{
			get { return '@'; }
		}
	}

	/// <summary>
	/// Equals, used for assignment
	/// </summary>
	public class Equals : Operator
	{
		public override char StartChar
		{
			get { return '='; }
		}
	}

	/// <summary>
	/// Tilde, used for bitwise negation
	/// </summary>
	public class Tilde : Operator
	{
		public override char StartChar
		{
			get { return '~'; }
		}
	}

	/// <summary>
	/// Ampersand, used for bitwise AND
	/// </summary>
	public class Ampersand : Operator
	{
		public override char StartChar
		{
			get { return '&'; }
		}
	}

	/// <summary>
	/// Pipe, used for bitwise OR
	/// </summary>
	public class Pipe : Operator
	{
		public override char StartChar
		{
			get { return '|'; }
		}
	}

	/// <summary>
	/// Percent, used for the modulo function
	/// </summary>
	public class Percent : Operator
	{
		public override char StartChar
		{
			get { return '%'; }
		}
	}

	/// <summary>
	/// Asterisk, used for multiplication
	/// </summary>
	public class Asterisk : Operator
	{
		public override char StartChar
		{
			get { return '*'; }
		}
	}

	/// <summary>
	/// Caret, used for XOR
	/// </summary>
	public class Caret : Operator
	{
		public override char StartChar
		{
			get { return '^'; }
		}
	}

	/// <summary>
	/// Exclamation, used for boolean negation
	/// </summary>
	public class Exclamation : Operator
	{
		public override char StartChar
		{
			get { return '!'; }
		}
	}

	/// <summary>
	/// Question mark, used for nullable types and the ternary operator
	/// </summary>
	public class Question : Operator
	{
		public override char StartChar
		{
			get { return '?'; }
		}
	}

	/// <summary>
	/// The 'less than' conditional
	/// </summary>
	public class LessThan : Operator
	{
		public override char StartChar
		{
			get { return '<'; }
		}
	}

	/// <summary>
	/// The 'greater than' conditional
	/// </summary>
	public class GreaterThan : Operator
	{
		public override char StartChar
		{
			get { return '>'; }
		}
	}

	/// <summary>
	/// Hash, used for preprocessor directives
	/// </summary>
	public class Hash : Operator
	{
		public override char StartChar
		{
			get { return '#'; }
		}
	}
}