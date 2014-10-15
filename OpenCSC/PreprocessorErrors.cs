using System;
using System.Collections.Generic;
using System.Text;
using OpenCompiler;

namespace OpenCSC
{
	public class InvalidPreprocessorExpression : DefaultCompilerError
	{
		public override string Message
		{
			get { return "Invalid preprocessor expression"; }
		}

		public override int Number
		{
			get { return 1517; }
		}

		public InvalidPreprocessorExpression(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public InvalidPreprocessorExpression(LexerItemInfo item)
			: base(item)
		{
		}
	}

	public class EndOfLineExpected : DefaultCompilerError
	{
		public override string Message
		{
			get { return "Single-line comment or end-of-line expected"; }
		}

		public override int Number
		{
			get { return 1025; }
		}

		public EndOfLineExpected(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public EndOfLineExpected(LexerItemInfo item)
			: base(item)
		{
		}
	}

	public class IdentifierExpected : DefaultCompilerError
	{
		public override string Message
		{
			get { return "Identifier expected"; }
		}

		public override int Number
		{
			get { return 1001; }
		}

		public IdentifierExpected(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public IdentifierExpected(LexerItemInfo item)
			: base(item)
		{
		}
	}

	public class EndifExpected : DefaultCompilerError
	{
		public override string Message
		{
			get { return "#endif directive expected"; }
		}

		public override int Number
		{
			get { return 1027; }
		}

		public EndifExpected(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public EndifExpected(LexerItemInfo item)
			: base(item)
		{
		}
	}

	public class UnexpectedDirective : DefaultCompilerError
	{
		public override string Message
		{
			get { return "Unexpected preprocessor directive"; }
		}

		public override int Number
		{
			get { return 1028; }
		}

		public UnexpectedDirective(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public UnexpectedDirective(LexerItemInfo item)
			: base(item)
		{
		}
	}

	public class DirectiveExpected : DefaultCompilerError
	{
		public override string Message
		{
			get { return "Preprocessor directive expected"; }
		}

		public override int Number
		{
			get { return 1024; }
		}

		public DirectiveExpected(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public DirectiveExpected(LexerItemInfo item)
			: base(item)
		{
		}
	}

	public class NotFirstCharacter : DefaultCompilerError
	{
		public override string Message
		{
			get { return "Preprocessor directives must appear as the first non-whitespace character on a line"; }
		}

		public override int Number
		{
			get { return 1040; }
		}

		public NotFirstCharacter(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public NotFirstCharacter(LexerItemInfo item)
			: base(item)
		{
		}
	}
}
