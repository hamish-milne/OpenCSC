namespace OpenCompiler
{
	/// <summary>
	/// Base class for comments
	/// </summary>
	public abstract class Comment : Token
	{
		/// <summary>
		/// The raw commented text
		/// </summary>
		public Substring CommentedText;

		/// <summary>
		/// Comments should have a high priority to avoid confusion with operators
		/// </summary>
		public override int Priority
		{
			get { return 3; }
		}

		/// <summary>
		/// Displays the commented string
		/// </summary>
		/// <returns>A formatted string</returns>
		public override string ToString()
		{
			return GetType().FullName + ": " + CommentedText.ToString();
		}
	}

	/// <summary>
	/// C-style line comments
	/// </summary>
	public class LineComment : Comment
	{
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public LineComment()
			: this(new Substring())
		{
		}

		/// <summary>
		/// Creates a new instance with the given text
		/// </summary>
		/// <param name="commentedText">The commented text</param>
		public LineComment(Substring commentedText)
		{
			CommentedText = commentedText;
		}

		/// <summary>
		/// The character used to mark comments
		/// </summary>
		public override char StartChar
		{
			get { return '/'; }
		}

		/// <summary>
		/// The required repetitions of <see cref="StartChar"/>
		/// </summary>
		public virtual int NumChars
		{
			get { return 2; }
		}

		public override int Length
		{
			get { return CommentedText.Length + NumChars; }
		}

		/// <summary>
		/// Checks for a line comment
		/// </summary>
		/// <param name="lexer">The lexer object</param>
		/// <returns>A line comment item, or <c>null</c></returns>
		public override Token CheckPresence(Lexer lexer)
		{
			lexer.EatWhitespace();
			for (int i = 0; i < NumChars; i++)
				if (lexer[i] != StartChar)
					return null;
			lexer.Advance(NumChars);
			return new LineComment(lexer.EatUntil('\n', false));
		}
	}

	/// <summary>
	/// Perl and python style line comments
	/// </summary>
	public class HashComment : LineComment
	{
		/// <inheritdoc/>
		public HashComment()
			: base()
		{
		}

		/// <inheritdoc/>
		public HashComment(Substring text)
			: base(text)
		{
		}

		/// <inheritdoc/>
		public override char StartChar
		{
			get { return '#'; }
		}

		/// <inheritdoc/>
		public override int NumChars
		{
			get { return 1; }
		}
	}

	/// <summary>
	/// C-style multi-line comments
	/// </summary>
	public class BlockComment : Comment
	{
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public BlockComment()
			: this(new Substring())
		{
		}

		/// <summary>
		/// Creates a new instance with the given text
		/// </summary>
		/// <param name="commentedText">The commented text</param>
		public BlockComment(Substring commentedText)
		{
			CommentedText = commentedText;
		}

		/// <summary>
		/// The first character to mark a block comment (or the second to end it)
		/// </summary>
		public override char StartChar
		{
			get { return '/'; }
		}

		/// <summary>
		/// The second character to mark a block comment (or the first to end it)
		/// </summary>
		public virtual char SecondChar
		{
			get { return '*'; }
		}

		public override int Length
		{
			get { return CommentedText.Length + 4; }
		}

		/// <summary>
		/// Checks for a block comment
		/// </summary>
		/// <param name="lexer">The lexer object</param>
		/// <returns>A block comment item, or <c>null</c></returns>
		public override Token CheckPresence(Lexer lexer)
		{
			if (lexer.Current == StartChar && lexer[1] == SecondChar)
			{
				lexer.Advance(2);
				Substring ret;
				do
				{
					ret = lexer.EatUntil(SecondChar, true);
				} while (lexer[1] != StartChar);
				lexer.Advance(2);
				return new BlockComment(ret);
			}
			return null;
		}
	}
}