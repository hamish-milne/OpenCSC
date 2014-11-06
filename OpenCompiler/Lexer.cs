using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OpenCompiler
{
	/// <summary>
	/// References a portion of a string using start and length indices
	/// </summary>
	/// <remarks>
	/// This allows for efficient comparing and hashing of small sections
	/// of a single large string
	/// </remarks>
	public struct Substring : IEquatable<Substring>
	{
		private string source;
		private int start, length;

		/// <summary>
		/// Initialises the structure using the entire source string
		/// </summary>
		/// <param name="source">The source string</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="source"/> is null</exception>
		public Substring(string source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			this.source = source;
			start = 0;
			length = source.Length;
		}

		/// <summary>
		/// The length of the substring
		/// </summary>
		public int Length
		{
			get { return length; }
		}

		/// <summary>
		/// Gets a character in the substring
		/// </summary>
		/// <param name="i">The offset from the start</param>
		/// <returns>The character at index <paramref name="i"/></returns>
		/// <exception cref="IndexOutOfRangeException"><paramref name="i"/>
		/// is greater than or equal to <see cref="Length"/></exception>
		public char this[int i]
		{
			get { return source[start + i]; }
		}

		/// <summary>
		/// Initialises the structure using the given subsection of the source string
		/// </summary>
		/// <param name="source">The source string</param>
		/// <param name="start">The position in <paramref name="source"/> to start from</param>
		/// <param name="length">The number of characters</param>
		public Substring(string source, int start, int length)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (start < 0)
				throw new ArgumentOutOfRangeException("start");
			if (length > source.Length)
				throw new ArgumentOutOfRangeException("length");
			this.source = source;
			this.start = start;
			this.length = length;
		}

		/// <summary>
		/// Checks if two substrings are equal
		/// </summary>
		/// <param name="other">The substring to check this one against</param>
		/// <returns><c>true</c> if they are equal, otherwise <c>false</c></returns>
		public bool Equals(Substring other)
		{
			if (other.source == null)
				return (source == null);
			if (length != other.length)
				return false;
			for (int i = 0; i < length; i++)
				if (source[start + i] != other.source[other.start + i])
					return false;
			return true;
		}

		/// <summary>
		/// Converts this substring to a full string
		/// </summary>
		/// <returns>A full string representation</returns>
		public override string ToString()
		{
			if (source == null)
				return "";
			if (start == 0 && length == source.Length)
				return source;
			if (length < 1)
				return String.Empty;
			return source.Substring(start, length);
		}

		/// <summary>
		/// An operator wrapper for <see cref="Equals(Substring)"/>
		/// </summary>
		/// <param name="a">The first object</param>
		/// <param name="b">The second object</param>
		/// <returns><c>true</c> if <paramref name="a"/> and <paramref name="b"/> are equal,
		/// otherwise <c>false</c></returns>
		public static bool operator ==(Substring a, Substring b)
		{
			return a.Equals(b);
		}

		/// <summary>
		/// An operator wrapper for <see cref="Equals(Substring)"/>
		/// </summary>
		/// <param name="a">The first object</param>
		/// <param name="b">The second object</param>
		/// <returns><c>false</c> if <paramref name="a"/> and <paramref name="b"/> are equal,
		/// otherwise <c>true</c></returns>
		public static bool operator !=(Substring a, Substring b)
		{
			return !a.Equals(b);
		}

		/// <summary>
		/// Returns a substring trimmed at the start and end
		/// </summary>
		/// <param name="fromStart">The number of characters to remove from the start</param>
		/// <param name="fromEnd">The number of characters to remove from the end</param>
		/// <returns>The trimmed substring</returns>
		public Substring Trim(int fromStart, int fromEnd)
		{
			return new Substring(source, start + fromStart, (length - fromStart) - fromEnd);
		}

		/// <summary>
		/// Gets a unique hash code for this substring
		/// </summary>
		/// <returns>A unique hash code for this substring</returns>
		public override unsafe int GetHashCode()
		{
			if (source == null)
				return 0;
			if (start < 0 ||
				length < 0 ||
				start >= source.Length ||
				length > source.Length - start)
				throw new InvalidOperationException("Invalid substring: " + start + ", " + length + ", " + source.Length);
			int hash = 5381;
			fixed (char* c = source)
			{
				var ptr = c + start;
				var end = ptr + length;
				while (ptr++ != end)
					hash = ((hash << 5) + hash) ^ *ptr;
			}
			return hash;
		}

		/// <summary>
		/// Checks if two objects are equal
		/// </summary>
		/// <param name="obj">The other object</param>
		/// <returns><c>true</c> if they are equal, otherwise <c>false</c></returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Substring))
				return false;
			var other = (Substring)obj;
			return Equals(other);
		}

		/// <summary>
		/// Converts a string to a substring automatically
		/// </summary>
		/// <param name="str">The string object</param>
		/// <returns>The equivalent substring</returns>
		public static implicit operator Substring(string str)
		{
			return new Substring(str);
		}

		/// <summary>
		/// Converts a substring to a string explicitly
		/// </summary>
		/// <param name="str">The substring object</param>
		/// <returns>The full string representation</returns>
		public static explicit operator string(Substring str)
		{
			return str.ToString();
		}
	}

	/// <summary>
	/// A lexed code item, or a template for one
	/// </summary>
	public abstract class Token : IComparable<Token>
	{
		/// <summary>
		/// The first character for this item, or '\0' if it can vary
		/// </summary>
		/// <remarks>
		/// You should override this property if you can, as it makes lexing more efficient
		/// </remarks>
		public virtual char StartChar { get { return '\0'; } }

		/// <summary>
		/// Whether this item takes preceding whitespace into account
		/// </summary>
		/// <remarks>Defaults to (StartChar == '\0')</remarks>
		public virtual bool UseWhitespace { get { return (StartChar == '\0'); } }

		/// <summary>
		/// Higher numbers mean a higher priority. Higher priority items are lexed first
		/// </summary>
		public virtual int Priority { get { return 0; } }

		public virtual int CompareTo(Token other)
		{
			if (other == null)
				return -1;
			return other.Priority - Priority;
		}

		public abstract int Length { get; }

		/// <summary>
		/// Checks if the given item is present at the current point in the lexer content.
		/// </summary>
		/// <param name="lexer">The current lexer object</param>
		/// <returns>The lexed item, or <c>null</c> if none exists at this point</returns>
		public abstract Token CheckPresence(Lexer lexer);

		/// <summary>
		/// Gets a lexed item of the required type. By default, returns <c>this</c>
		/// </summary>
		/// <returns>A Token of the required type</returns>
		public virtual Token Create()
		{
			return this;
		}

		/// <summary>
		/// Gets a lexed item with the given substring argument
		/// </summary>
		/// <param name="argument">The portion of code for which this item is for</param>
		/// <returns>A Token representing the given portion of code</returns>
		public virtual Token Create(Substring argument)
		{
			return Create();
		}
	}

	/// <summary>
	/// Represents one or more whitespace characters
	/// </summary>
	public class Whitespace : Token
	{
		public override bool UseWhitespace
		{
			get { return true; }
		}

		public override int Length
		{
			get { return 1; }
		}

		public override Token CheckPresence(Lexer lexer)
		{
			if (lexer.EatWhitespace() > 0)
				return this;
			return null;
		}
	}

	/// <summary>
	/// Base class for single character items
	/// </summary>
	public abstract class SingleChar : Token
	{
		/// <inheritdoc/>
		public override Token CheckPresence(Lexer lexer)
		{
			if (lexer.Current == StartChar)
			{
				lexer.Advance();
				return this;
			}
			return null;
		}

		public override int Length
		{
			get { return 1; }
		}
	}

	/// <summary>
	/// Thrown when the lexer unexpectedly reaches the end of the file
	/// </summary>
	public class EndOfFileException : CodeException
	{
		/// <summary>
		/// Creates a new instance with the given message
		/// </summary>
		/// <param name="message">The textual message</param>
		public EndOfFileException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new instance with the default message
		/// </summary>
		public EndOfFileException()
			: base("End of file reached")
		{
		}
	}

	/// <summary>
	/// Stores a Token, plus line and column information
	/// </summary>
	public struct TokenInfo
	{
		/// <summary>
		/// The Token in question
		/// </summary>
		public Token Item;

		public Substring File;

		/// <summary>
		/// The line the item is found on
		/// </summary>
		public int Line;

		/// <summary>
		/// The column the line is found on
		/// </summary>
		public int Column;

		public bool DebugHide;

		/// <summary>
		/// Creates a new object
		/// </summary>
		/// <param name="item"></param>
		/// <param name="line"></param>
		/// <param name="column"></param>
		public TokenInfo(Token item, Substring file, int line, int column, bool debugHide)
		{
			Item = item;
			File = file;
			Line = line;
			Column = column;
			DebugHide = debugHide;
		}

		/// <summary>
		/// Provides a string representation of this item
		/// </summary>
		/// <returns>(<see cref="Line"/>, <see cref="Column"/>) <see cref="Item"/></returns>
		public override string ToString()
		{
			return File + ": (" + Line + ", " + Column + ") " + Item == null ? "null" : Item.ToString();
		}
	}

	public struct LexerInput
	{
		public string FileName;
		public string Content;

		public LexerInput(string filename, string content)
		{
			FileName = filename;
			Content = content;
		}
	}

	/// <summary>
	/// Takes in a raw source file, and outputs a list of objects that can be easily parsed
	/// into code structures
	/// </summary>
	public abstract class Lexer : Pipeline<LexerInput, IList<TokenInfo>>, IEnumerable<Token>
	{
		/// <summary>
		/// The current position of the lexer in the source
		/// </summary>
		public abstract int Position { get; protected set; }

		/// <summary>
		/// The current line number
		/// </summary>
		public abstract int Line { get; }

		/// <summary>
		/// The current column number
		/// </summary>
		public abstract int Column { get; }

		/// <summary>
		/// The list of processed tokens
		/// </summary>
		public abstract IList<TokenInfo> Processed { get; }

		/// <summary>
		/// Advances the position within the content by one character
		/// </summary>
		public virtual void Advance()
		{
			Advance(1);
		}

		/// <summary>
		/// Advances the position within the content
		/// </summary>
		/// <param name="i">The number of characters to advance by</param>
		public abstract void Advance(int i);

		/// <summary>
		/// Gets a character in the stream, relative to the current position
		/// </summary>
		/// <param name="i">The offset relative to the position</param>
		/// <returns>The character, or '\0' if the index is out of range</returns>
		public abstract char this[int i] { get; }

		/// <summary>
		/// Advances the position such that the current character is not whitespace
		/// </summary>
		/// <returns>The number of whitespace characters advanced by</returns>
		public abstract int EatWhitespace();

		/// <summary>
		/// The current character in the stream
		/// </summary>
		public virtual char Current
		{
			get { return this[0]; }
		}

		/// <summary>
		/// The list of item templates that can be lexed
		/// </summary>
		public abstract IList<Token> Templates { get; }

		/// <summary>
		/// Adds an item template to the list
		/// </summary>
		/// <param name="item"></param>
		public virtual void Add(Token item)
		{
			Templates.Add(item);
		}

		/// <summary>
		/// Gets a substring within the content
		/// </summary>
		/// <param name="position">The position within the content (not relative)</param>
		/// <param name="length">The number of characters</param>
		/// <returns>The substring</returns>
		/// <exception cref="ArgumentOutOfRangeException">One of the provided arguments was out of range</exception>
		public abstract Substring GetSubstring(int position, int length);

		/// <summary>
		/// Advances the position until the given character is found (and one beyond it)
		/// </summary>
		/// <param name="c">The character to find</param>
		/// <param name="throwEOF">If <c>true</c> and the lexer reaches the end of the stream without
		/// finding the character, throw an <see cref="EndOfFileException"/></param>
		/// <returns>The substring between here and the found character (not including it)</returns>
		public abstract Substring EatUntil(char c, bool throwEOF);

		/// <summary>
		/// Gets an enumerator to iterate through the template list
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerator<Token> GetEnumerator()
		{
			return Templates.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>
	/// A standard lexer implementation that uses a string input
	/// </summary>
	public class DefaultLexer : Lexer
	{
		protected CompilerInput input;
		protected CompilerOutput output;

		public override CompilerOutput Output
		{
			get
			{
				if (output == null)
					output = new DefaultCompilerOutput();
				return output;
			}
			set
			{
				output = value;
			}
		}

		public override CompilerInput Input
		{
			get
			{
				if (input == null)
					input = new DefaultCompilerInput();
				return input;
			}
			set
			{
				input = value;
			}
		}

		/// <summary>
		/// Direct access to the string content
		/// </summary>
		protected string content;

		protected string fileName;

		/// <summary>
		/// Direct access to the position
		/// </summary>
		protected int position, line, column;

		/// <summary>
		/// Direct access to the template list
		/// </summary>
		protected List<Token> templates;

		/// <summary>
		/// Direct access to the processing list
		/// </summary>
		protected List<TokenInfo> processed;

		/// <summary>
		/// Sets the string input, removing null bytes and converting line endings to LF for simplicity
		/// </summary>
		/// <param name="input">The string input</param>
		/// <exception cref="ArgumentNullException"><paramref name="input"/> is null</exception>
		public override void SetInput(LexerInput input)
		{
			var content = input.Content;
			if (content == null)
				throw new ArgumentNullException("input");
			var sb = new StringBuilder(content.Length);
			bool cr = false;
			for (int i = 0; i < content.Length; i++)
			{
				var c = content[i];
				// Replace CR with LF
				if (c == '\r')
				{
					cr = true;
					c = '\n';
				}
				else if (cr)
				{
					// Replace CRLF with LF
					cr = false;
					if (c == '\n')
						continue;
				}
				// Remove null chars (null signifies end of stream here)
				if (c == '\0')
					continue;
				sb.Append(c);
			}
			this.content = sb.ToString();
			fileName = input.FileName;
			Position = 0;
			line = 0;
			column = 0;
		}

		/// <summary>
		/// Lexes the string
		/// </summary>
		/// <returns>A list of lexed item information</returns>
		/// <exception cref="InvalidOperationException">There are no templates or no input</exception>
		/// <exception cref="CodeException">Use this for user errors</exception>
		/// <exception cref="EndOfFileException">The lexer unexpectedly reached the end of the stream</exception>
		public override IList<TokenInfo> Run()
		{
			// Input validation
			if (templates == null)
				throw new InvalidOperationException("No item list");
			if (content == null)
				throw new InvalidOperationException("No input");

			// Sort by priority
			templates.Sort();

			// Add items to successive dictionaries, if they have start characters
			// If not, or they use whitespace, add them to normal lists to iterate
			// through manually.
			int oldPriority = 0;
			var charMaps = new List<Dictionary<char, Token>>();
			Dictionary<char, Token> thisMap = null;
			var manual = new List<Token>();
			var whitespace = new List<Token>();
			for (int i = 0; i < templates.Count; i++)
			{
				var item = templates[i];
				if (item.StartChar == '\0' || item.UseWhitespace)
				{
					(item.UseWhitespace ? whitespace : manual).Add(item);
					continue;
				}
				if (i == 0 || item.Priority < oldPriority)
				{
					thisMap = new Dictionary<char, Token>();
					charMaps.Add(thisMap);
				}
				oldPriority = item.Priority;
				try { thisMap.Add(item.StartChar, item); }
				catch { (item.UseWhitespace ? whitespace : manual).Add(item); }
			}

			// Iterate through content
			Processed.Clear();
			position = 0;
			int numWhitespace = 0;
			while (position < content.Length)
			{
				int startPos = position;
				int rLine = Line;
				int rCol = Column;
				Token toAdd = null;

				// First check items which use whitespace
				// (easier this way, but not quite as efficient)
				for (int i = 0; i < whitespace.Count; i++)
					if (CheckSingle(whitespace[i], ref toAdd))
						break;

				// If that didn't work, check each dictionary in order
				if (toAdd == null)
				{
					numWhitespace = EatWhitespace();
					if (position < content.Length)
					{
						rLine = Line;
						rCol = Column;
						var c = content[position];
						for (int i = 0; i < charMaps.Count; i++)
						{
							thisMap = charMaps[i];
							Token toCheck;
							if (!thisMap.TryGetValue(c, out toCheck))
								continue;
							if (CheckSingle(toCheck, ref toAdd))
								break;
						}
					}
				}

				// If that didn't work, check the normal manual items
				if (toAdd == null)
				{
					for (int i = 0; i < manual.Count; i++)
						if (CheckSingle(manual[i], ref toAdd))
							break;
				}

				// If an item was found, add it to the list
				if (toAdd != null)
				{
					Processed.Add(new TokenInfo(toAdd, fileName, Line, Column, false));
					// If it has zero length (no characters eaten) an infinite loop is possible,
					// so we should throw here
					if (position == startPos)
						throw new CodeException(toAdd + " has zero length");
				}
				else
				{
					// Otherwise, something's happened. Either the end of the file, which is fine
					// or there was an unexpected character, in which case we throw
					EatWhitespace();
					if (position < content.Length)
					{
						Output.Errors.Add(new UnexpectedCharacterError(content[position], line, column));
						position++;
					}
					else
					{
						break;
					}
				}
			}
			return Processed;
		}

		private bool CheckSingle(Token toCheck, ref Token toAdd)
		{
			var preCheck = position;
			toAdd = toCheck.CheckPresence(this);
			if (toAdd == null)
				position = preCheck;
			return (toAdd != null);
		}

		/// <inheritdoc/>
		public override int Position
		{
			get { return position; }
			protected set { position = value; }
		}

		/// <inheritdoc/>
		public override int Line
		{
			get { return line; }
		}

		/// <inheritdoc/>
		public override int Column
		{
			get { return column; }
		}

		public override IList<TokenInfo> Processed
		{
			get
			{
				if (processed == null)
					processed = new List<TokenInfo>();
				return processed;
			}
		}

		/// <inheritdoc/>
		public override IList<Token> Templates
		{
			get
			{
				if (templates == null)
					templates = new List<Token>();
				return templates;
			}
		}

		/// <inheritdoc/>
		public override Substring GetSubstring(int start, int length)
		{
			return new Substring(content, start, length);
		}

		/// <inheritdoc/>
		public override int EatWhitespace()
		{
			int startPos = Position;
			while (Char.IsWhiteSpace(Current) && Position < content.Length)
				Advance();
			return Position - startPos;
		}

		/// <inheritdoc/>
		public override Substring EatUntil(char c, bool throwEOF)
		{
			int startPos = Position;
			while (Current != c && Position < content.Length)
				Advance();
			if (throwEOF && Position >= content.Length)
				throw new EndOfFileException();
			return new Substring(content, startPos, Position - startPos);
		}

		private void NewLine()
		{
			line++;
			column = 0;
		}

		/// <inheritdoc/>
		public override void Advance()
		{
			if (content[position] == '\n')
				NewLine();
			position++;
		}

		/// <inheritdoc/>
		public override void Advance(int i)
		{
			while (i-- > 0)
				Advance();
		}

		/// <inheritdoc/>
		public override char this[int i]
		{
			get { return (position + i) < content.Length ? content[position + i] : '\0'; }
		}

		/// <inheritdoc/>
		public override char Current
		{
			get
			{
				if (position < content.Length)
					return content[position];
				return '\0';
			}
		}
	}
}