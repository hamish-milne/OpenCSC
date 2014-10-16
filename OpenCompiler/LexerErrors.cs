using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCompiler
{
	public class UnexpectedCharacterError : CompilerError
	{
		protected int line;
		protected int column;

		public override ErrorLevel ErrorLevel
		{
			get { return ErrorLevel.Error; }
		}

		public override int Number
		{
			get { return 1519; }
		}

		public override Substring Message
		{
			get { return "Unexpected character: " + Character; }
		}

		public override int Line
		{
			get { return line; }
		}

		public override int Column
		{
			get { return column; }
		}

		public override int Length
		{
			get { return 1; }
		}

		public virtual char Character { get; set; }

		public UnexpectedCharacterError(char character, int line, int column)
		{
			Character = character;
			this.line = line;
			this.column = column;
		}
	}
}
