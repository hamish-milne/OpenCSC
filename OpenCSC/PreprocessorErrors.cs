using System;
using System.Collections.Generic;
using System.Text;
using OpenCompiler;

namespace OpenCSC
{
	public class InvalidPreprocessorExpression : DefaultCompilerError
	{
		public override Substring Message
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

		public InvalidPreprocessorExpression(TokenInfo item)
			: base(item)
		{
		}
	}

	public class EndOfLineExpected : DefaultCompilerError
	{
		public override Substring Message
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

		public EndOfLineExpected(TokenInfo item)
			: base(item)
		{
		}
	}

	public class EndOfLineExpectedWarning : DefaultCompilerWarning
	{
		public override Substring Message
		{
			get { return "Single-line comment or end-of-line expected"; }
		}

		public override int Number
		{
			get { return 1696; }
		}

		public EndOfLineExpectedWarning(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public EndOfLineExpectedWarning(TokenInfo item)
			: base(item)
		{
		}
	}

	public class IdentifierExpected : DefaultCompilerError
	{
		public override Substring Message
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

		public IdentifierExpected(TokenInfo item)
			: base(item)
		{
		}
	}

	public class EndifExpected : DefaultCompilerError
	{
		public override Substring Message
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

		public EndifExpected(TokenInfo item)
			: base(item)
		{
		}
	}

	public class UnexpectedDirective : DefaultCompilerError
	{
		public override Substring Message
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

		public UnexpectedDirective(TokenInfo item)
			: base(item)
		{
		}
	}

	public class DirectiveExpected : DefaultCompilerError
	{
		public override Substring Message
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

		public DirectiveExpected(TokenInfo item)
			: base(item)
		{
		}
	}

	public class NotFirstCharacter : DefaultCompilerError
	{
		public override Substring Message
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

		public NotFirstCharacter(TokenInfo item)
			: base(item)
		{
		}
	}

	public class UserError : DefaultCompilerError
	{
		protected Substring message;

		public override Substring Message
		{
			get { return message; }
		}

		public override int Number
		{
			get { return 1029; }
		}

		public UserError(Substring message, int line, int column, int length)
			: base(line, column, length)
		{
			this.message = message;
		}

		public UserError(Substring message, TokenInfo item)
			: base(item)
		{
			this.message = message;
		}
	}

	public class UserWarning : UserError
	{
		protected ErrorLevel errorLevel = ErrorLevel.Warning;

		public override ErrorLevel ErrorLevel
		{
			get { return errorLevel; }
		}

		public override void TreatAsError()
		{
			errorLevel = ErrorLevel.Error;
		}

		public override int Number
		{
			get { return 1030; }
		}

		public UserWarning(Substring message, int line, int column, int length)
			: base(message, line, column, length)
		{
		}

		public UserWarning(Substring message, TokenInfo item)
			: base(message, item)
		{
		}
	}

	public class UnrecognizedPragma : DefaultCompilerWarning
	{
		public override int Number
		{
			get { return 1633; }
		}

		public override Substring Message
		{
			get { return "Unrecognized #pragma directive"; }
		}

		public UnrecognizedPragma(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public UnrecognizedPragma(TokenInfo item)
			: base(item)
		{
		}
	}
	
	public class ExpectedDisableOrRestore : DefaultCompilerWarning
	{
		public override int Number
		{
			get { return 1634; }
		}

		public override Substring Message
		{
			get { return "Expected disable or restore"; }
		}

		public ExpectedDisableOrRestore(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public ExpectedDisableOrRestore(TokenInfo item)
			: base(item)
		{
		}
	}

	public class InvalidNumber : DefaultCompilerWarning
	{
		public override int Number
		{
			get { return 1692; }
		}

		public override Substring Message
		{
			get { return "Invalid number"; }
		}

		public InvalidNumber(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public InvalidNumber(TokenInfo item)
			: base(item)
		{
		}
	}
}
