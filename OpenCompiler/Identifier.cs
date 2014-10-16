using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenCompiler
{
	/// <summary>
	/// Base class for a word - an identifier or fixed keyword
	/// </summary>
	public abstract class Word : Token
	{
		/// <summary>
		/// The text itself
		/// </summary>
		public abstract Substring Value { get; }

		/// <summary>
		/// Whitespace doesn't matter
		/// </summary>
		public override bool UseWhitespace
		{
			get { return false; }
		}

		/// <summary>
		/// Words should have a lower priority than normal
		/// </summary>
		public override int Priority
		{
			get { return -1; }
		}

		public override int Length
		{
			get { return Value.Length; }
		}

		/// <summary>
		/// Checks if the given character is valid for this word type
		/// </summary>
		/// <remarks>
		/// By default, alphanumeric characters and underscored are accepted,
		/// but the first character cannot be a number
		/// </remarks>
		/// <param name="c">The character to check</param>
		/// <param name="firstChar"><c>true</c> if this is the first character in the word</param>
		/// <returns><c>true</c> if the character is valid, otherwise <c>false</c></returns>
		public virtual bool IsValidChar(char c, bool firstChar)
		{
			if (c == '_')
				return true;
			return firstChar ? Char.IsLetter(c) : Char.IsLetterOrDigit(c);
		}

		/// <summary>
		/// Checks for the presence of any word
		/// </summary>
		/// <param name="lexer">The lexer object</param>
		/// <returns>A created word item, or <c>null</c></returns>
		public override Token CheckPresence(Lexer lexer)
		{
			bool first = true;
			int startPos = lexer.Position;
			while (IsValidChar(lexer.Current, first))
			{
				lexer.Advance();
				first = false;
			}
			if (lexer.Position == startPos)
				return null;
			var endPos = lexer.Position;
			return Create(lexer.GetSubstring(startPos, endPos - startPos));
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return GetType().FullName + ": " + Value.ToString();
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var word = obj as Word;
			if (word == null)
				return false;
			return Value == word.Value;
		}
	}

	/// <summary>
	/// A standard user-code identifier
	/// </summary>
	public class Identifier : Word
	{
		/// <summary>
		/// Direct access to the string value
		/// </summary>
		protected Substring stringValue;

		/// <summary>
		/// Gets the value of this identifier
		/// </summary>
		public override Substring Value
		{
			get { return stringValue; }
		}

		/// <summary>
		/// Variable identifiers should have an even lower priority than other words
		/// </summary>
		public override int Priority
		{
			get { return base.Priority - 1; }
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		public Identifier()
			: this(new Substring())
		{
		}

		/// <summary>
		/// Creates a new instance with the given value
		/// </summary>
		/// <param name="value">The string value</param>
		public Identifier(Substring value)
		{
			stringValue = value;
		}

		/// <summary>
		/// Not supported. An identifier must have a string value
		/// </summary>
		/// <returns></returns>
		public override Token Create()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Creates a new instance with the given value
		/// </summary>
		/// <param name="argument">The string value</param>
		/// <returns>An Identifier instance</returns>
		public override Token Create(Substring argument)
		{
			return new Identifier(argument);
		}
	}

	/// <summary>
	/// A keyword with a fixed value
	/// </summary>
	public abstract class Keyword : Word
	{
		/// <summary>
		/// Creates a keyword for the given substring
		/// </summary>
		/// <param name="argument">The string value</param>
		/// <returns>This object</returns>
		/// <exception cref="ArgumentException">The argument was not
		/// equal to the keyword value</exception>
		public override Token Create(Substring argument)
		{
			if (argument == Value)
				return this;
			throw new ArgumentException("Keyword value is fixed");
		}

		/// <summary>
		/// Checks for this keyword by checking for the exact sequence of characters
		/// in the lexer.
		/// </summary>
		/// <param name="lexer">The lexer object</param>
		/// <returns>This object, or <c>null</c></returns>
		public override Token CheckPresence(Lexer lexer)
		{
			for (int i = 0; i < Value.Length; i++)
				if (lexer[i] != Value[i])
					return null;
			lexer.Advance(Value.Length);
			return this;
		}
	}

	/// <summary>
	/// A template item that returns keywords or identifiers as necessary
	/// </summary>
	public class WordTemplate : Word, IEnumerable
	{
		/// <summary>
		/// Direct access to the map of strings to keywords
		/// </summary>
		protected Dictionary<Substring, Word> keywordMap
			= new Dictionary<Substring, Word>(0);

		/// <summary>
		/// Direct access to the default word type
		/// </summary>
		protected Word defaultWord;

		/// <summary>
		/// Adds a keyword to the map
		/// </summary>
		/// <param name="word">The keyword to add</param>
		public void Add(Word word)
		{
			if (word == null)
				throw new ArgumentNullException("word");
			keywordMap[word.Value] = word;
		}

		/// <summary>
		/// The default word type to create, if a keyword was not found
		/// </summary>
		public virtual Word Default
		{
			get { return defaultWord; }
			set { defaultWord = value; }
		}

		/// <summary>
		/// Not supported
		/// </summary>
		public override Substring Value
		{
			get { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Not supported
		/// </summary>
		/// <returns></returns>
		public override Token Create()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Finds the string value in the keyword map, or uses the default word
		/// </summary>
		/// <param name="argument">The string value</param>
		/// <returns>Usually a keyword or identifier as necessary</returns>
		public override Token Create(Substring argument)
		{
			Word word;
			if (keywordMap.TryGetValue(argument, out word))
				return word.Create();
			if (defaultWord == null)
				throw new InvalidOperationException("No default word set");
			return defaultWord.Create(argument);
		}

		/// <summary>
		/// Creates a new instance with a given default word
		/// </summary>
		/// <param name="defaultWord">The default word type. Usually an Identifier</param>
		public WordTemplate(Word defaultWord)
		{
			Default = defaultWord;
		}

		public IEnumerator GetEnumerator()
		{
			return keywordMap.GetEnumerator();
		}
	}
}