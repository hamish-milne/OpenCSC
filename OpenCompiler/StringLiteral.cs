using System.Text;

namespace OpenCompiler
{
	public class UnrecognizedEscapeSequence : DefaultCompilerError
	{
		public UnrecognizedEscapeSequence(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public override string Message
		{
			get { return "Unrecognized escape sequence"; }
		}

		public override int Number
		{
			get { return 1009; }
		}
	}

	public class NewlineInConstant : DefaultCompilerError
	{
		public NewlineInConstant(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public override string Message
		{
			get { return "Newline in constant"; }
		}

		public override int Number
		{
			get { return 1010; }
		}
	}

	/// <summary>
	/// Base class for a quoted (string, character etc.) literal
	/// </summary>
	public abstract class QuotedLiteral : Literal
	{
		/// <summary>
		/// Direct access to the string value
		/// </summary>
		protected Substring stringValue = "";

		/// <summary>
		/// The string value
		/// </summary>
		public virtual Substring StringValue
		{
			get { return stringValue; }
		}

		/// <summary>
		/// The string value
		/// </summary>
		public override object Value
		{
			get { return StringValue.ToString(); }
		}

		public override int Length
		{
			get { return StringValue.Length; }
		}

		/// <summary>
		/// Gets whether newlines are allowed in the literal
		/// </summary>
		public virtual bool AllowNewline
		{
			get { return false; }
		}

		/// <summary>
		/// The character used for quoting the literal
		/// </summary>
		public virtual char QuoteChar
		{
			get { return StartChar; }
		}

		/// <summary>
		/// Gets the integer value of a hexadecimal character,
		/// used in unicode escape sequences
		/// </summary>
		/// <param name="c">The character</param>
		/// <returns>Its hexadecimal value, or -1 for invalid</returns>
		public virtual int GetHexValue(char c)
		{
			if (c >= '0' && c <= '9')
				return (int)(c - '0');
			if (c >= 'A' && c <= 'F')
				return (int)(c - 'A') + 10;
			if (c >= 'a' && c <= 'f')
				return (int)(c - 'a') + 10;
			return -1;
		}

		/// <summary>
		/// Parses a single character, including escape sequences
		/// </summary>
		/// <param name="lexer">The lexer object</param>
		/// <param name="c">The found character</param>
		/// <returns><c>true</c> if the character is valid, or <c>false</c> if this is the end of the literal</returns>
		/// <exception cref="LexerException">There was an invalid escape sequence</exception>
		/// <exception cref="EndOfFileException">The literal was incomplete by the end of the file</exception>
		public virtual bool ParseChar(Lexer lexer, out char c)
		{
			lexer.Advance();
			c = lexer.Current;
			if (c == '\\')
			{
				lexer.Advance();
				c = lexer.Current;
				switch (c)
				{
					case 'n':
						c = '\n';
						break;
					case 'r':
						c = '\r';
						break;
					case 't':
						c = '\t';
						break;
					case 'f':
						c = '\f';
						break;
					case '0':
						c = '\0';
						break;
					case '\\':
					case '"':
					case '\'':
						break;
					case 'u':
						int total = 0;
						for (int i = 0; i < 4; i++)
						{
							lexer.Advance();
							var v = GetHexValue(lexer.Current);
							if (v == -1)
							{
								lexer.Output.Errors.Add(new UnrecognizedEscapeSequence(lexer.Line, lexer.Column - i - 1, i + 1));
								break;
							}
							total = (total << 4) + v;
						}
						c = (char)total;
						break;
					default:
						lexer.Output.Errors.Add(new UnrecognizedEscapeSequence(lexer.Line, lexer.Column - 1, 2));
						break;
				}
				return true;
			}
			else if (c == QuoteChar)
			{
				lexer.Advance();
				return false;
			}
			else if (c == '\n' && !AllowNewline)
				lexer.Output.Errors.Add(new NewlineInConstant(lexer.Line, lexer.Column, 1));
			else if (c == '\0')
				throw new EndOfFileException("Incomplete string literal. Expecting `" + QuoteChar + "'");
			return true;
		}

		/// <summary>
		/// Checks for a quoted literal
		/// </summary>
		/// <param name="lexer">The lexer object</param>
		/// <returns>A new string literal, or <c>null</c></returns>
		public override LexerItem CheckPresence(Lexer lexer)
		{
			if (lexer.Current != StartChar)
				return null;
			var sb = new StringBuilder();
			char c;
			while (ParseChar(lexer, out c))
				sb.Append(c);
			return Create(sb.ToString());
		}
	}

	/// <summary>
	/// A single quoted literal, used normally for single characters
	/// </summary>
	public class CharLiteral : QuotedLiteral
	{
		/// <summary>
		/// An empty literal
		/// </summary>
		public static readonly CharLiteral Empty = new CharLiteral("");

		/// <summary>
		/// Single quotes
		/// </summary>
		public override char StartChar
		{
			get { return '\''; }
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="value">The string value</param>
		public CharLiteral(Substring value)
		{
			stringValue = value;
		}

		/// <summary>
		/// Returns an empty instance
		/// </summary>
		/// <returns><see cref="Empty"/></returns>
		public override LexerItem Create()
		{
			return Empty;
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="argument">The string value</param>
		/// <returns>The created instance</returns>
		public override LexerItem Create(Substring argument)
		{
			return new CharLiteral(argument);
		}
	}

	/// <summary>
	/// A double quoted literal, used normally for strings
	/// </summary>
	public class StringLiteral : QuotedLiteral
	{
		/// <summary>
		/// An empty literal
		/// </summary>
		public static readonly StringLiteral Empty = new StringLiteral("");

		/// <summary>
		/// Double quotes
		/// </summary>
		public override char StartChar
		{
			get { return '\"'; }
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="value">The string value</param>
		public StringLiteral(Substring value)
		{
			stringValue = value;
		}

		/// <summary>
		/// Returns an empty instance
		/// </summary>
		/// <returns><see cref="Empty"/></returns>
		public override LexerItem Create()
		{
			return Empty;
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="argument">The string value</param>
		/// <returns>The created instance</returns>
		public override LexerItem Create(Substring argument)
		{
			return new StringLiteral(argument);
		}
	}

	/// <summary>
	/// A C#-style verbatim/multiline literal
	/// </summary>
	public class VerbatimLiteral : StringLiteral
	{
		/// <summary>
		/// An empty literal
		/// </summary>
		new public static readonly VerbatimLiteral Empty = new VerbatimLiteral("");

		/// <summary>
		/// '@' for verbatim literals
		/// </summary>
		public override char StartChar
		{
			get { return '@'; }
		}

		/// <summary>
		/// The normal double quotes
		/// </summary>
		public override char QuoteChar
		{
			get { return base.StartChar; }
		}

		/// <summary>
		/// <c>true</c> for verbatim literals
		/// </summary>
		public override bool AllowNewline
		{
			get { return true; }
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="value">The string value</param>
		public VerbatimLiteral(Substring value)
			: base(value)
		{
		}

		/// <summary>
		/// Returns an empty instance
		/// </summary>
		/// <returns><see cref="Empty"/></returns>
		public override LexerItem Create()
		{
			return Empty;
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="argument">The string value</param>
		/// <returns>The created instance</returns>
		public override LexerItem Create(Substring argument)
		{
			return new StringLiteral(argument);
		}

		/// <summary>
		/// Parses a single character, escaping only double quotes with `""'
		/// </summary>
		/// <inheritdoc/>
		public override bool ParseChar(Lexer lexer, out char c)
		{
			lexer.Advance();
			c = lexer.Current;
			if (c == '"')
			{
				lexer.Advance();
				return (lexer.Current == '"');
			}
			else if (c == '\0')
				throw new EndOfFileException("Incomplete string literal. Expecting `" + QuoteChar + "'");
			return true;
		}
	}
}