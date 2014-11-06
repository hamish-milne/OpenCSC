using System;
using System.Collections.Generic;
using System.Text;
using OpenCompiler;

namespace OpenCSC
{
	public class SemicolonExpected : DefaultCompilerError
	{
		public override int Number
		{
			get { return 1002; }
		}

		public override Substring Message
		{
			get { return "; expected"; }
		}

		public SemicolonExpected(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public SemicolonExpected(TokenInfo item)
			: base(item)
		{
		}
	}

	public class UnexpectedKeyword : DefaultCompilerError
	{
		public override int Number
		{
			get { return 1041; }
		}

		public Keyword Keyword;

		public override Substring Message
		{
			get
			{
				return Keyword == null ? "Identifier expected" :
					("Identifier expected; '" + Keyword.Value + "' is a keyword");
			}
		}

		public UnexpectedKeyword(int line, int column, int length, Keyword keyword)
			: base(line, column, length)
		{
			Keyword = keyword;
		}

		public UnexpectedKeyword(TokenInfo item)
			: base(item)
		{
			Keyword = item.Item as Keyword;
		}
	}

	public class ExternAliasError : DefaultCompilerError
	{
		public override int Number
		{
			get { return 439; }
		}

		public override Substring Message
		{
			get { return "An extern alias declaration must precede all other elements defined in the namespace"; }
		}

		public ExternAliasError(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public ExternAliasError(TokenInfo item)
			: base(item)
		{
		}
	}

	public class TypeExpected : DefaultCompilerError
	{
		public override int Number
		{
			get { return 1022; }
		}

		public override Substring Message
		{
			get { return "Type or namespace definition, or end-of-file expected"; }
		}

		public TypeExpected(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public TypeExpected(TokenInfo item)
			: base(item)
		{
		}
	}
}
