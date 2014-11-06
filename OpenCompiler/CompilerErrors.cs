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
		public abstract Substring Message { get; }
		public abstract int Line { get; }
		public abstract int Column { get; }
		public abstract int Length { get; }

		public virtual void TreatAsError()
		{
		}

		public override string ToString()
		{
			return "FILENAME HERE" +
				'(' + Line + ',' + Column + ',' + Line + ',' + (Column + Length) + "): "
				+ (ErrorLevel == ErrorLevel.Error ? "error" : "warning")
				+ ' ' + Number + ": " + Message;
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

		public DefaultCompilerError(TokenInfo item)
		{
			this.line = item.Line;
			this.column = item.Column;
			this.length = item.Item.Length;
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

		public override Substring Message
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

	public class DefaultCompilerWarning : DefaultCompilerError
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

		public DefaultCompilerWarning(int line, int column, int length)
			: base(line, column, length)
		{
		}

		public DefaultCompilerWarning(TokenInfo item)
			: base(item)
		{
		}
	}
}
