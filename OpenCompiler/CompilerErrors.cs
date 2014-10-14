using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCompiler
{
	public enum ErrorLevel
	{
		Info, Warning, Error
	}

	public abstract class CompilerError
	{
		public abstract ErrorLevel ErrorLevel { get; }
		public abstract int Number { get; }
		public abstract string Message { get; }
		public abstract int Line { get; }
		public abstract int Column { get; }
		public abstract int Length { get; }

		public override string ToString()
		{
			return Number + ": " + Message + " at line " + Line + ", column " + Column;
		}
	}

	public class DefaultCompilerError : CompilerError
	{
		protected int length;
		protected int line;
		protected int column;

		public DefaultCompilerError(int line, int column, int length)
		{
			this.line = line;
			this.column = column;
			this.length = length;
		}

		public override int Length
		{
			get { return length; }
		}

		public override int Line
		{
			get { return line; }
		}

		public override int Column
		{
			get { return column; }
		}

		public override string Message
		{
			get { return "An error occurred"; }
		}

		public override ErrorLevel ErrorLevel
		{
			get { return ErrorLevel.Error; }
		}

		public override int Number
		{
			get { return 0; }
		}
	}
}
