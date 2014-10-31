using System;
using System.Collections.Generic;
using System.Text;
using OpenCompiler;

namespace OpenCSC
{
	/// <summary>
	/// 'If' statement
	/// </summary>
	public class If : Keyword, IDirective
	{
		public override Substring Value
		{
			get { return "if"; }
		}

		public virtual void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			var ret = parent.ParseExpression(directives, 1, directives.Count);
			parent.ConditionStack.Add(new ConditionStackElement(ret));
		}
	}

	/// <summary>
	/// 'Else' statement
	/// </summary>
	public class Else : Keyword, IDirective
	{
		public override Substring Value
		{
			get { return "else"; }
		}

		public virtual void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			parent.CheckExcessParams(directives, 1);
			var cs = parent.ConditionStack;
			bool valid = false;
			ConditionStackElement elem;
			int lastPos = cs.Count - 1;
			if (lastPos >= 0)
			{
				elem = cs[lastPos];
				valid = !elem.Final;
			}
			if (!valid)
				parent.Output.Errors.Add(new UnexpectedDirective(directives[0]));
			cs[lastPos] = ConditionStackElement.DoElse(cs[lastPos]);
		}
	}


	/// <summary>
	/// Else-if directive
	/// </summary>
	public class Elif : Directive
	{
		public override Substring Value
		{
			get { return "elif"; }
		}

		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			var lastPos = parent.ConditionStack.Count - 1;
			var ret = parent.ParseExpression(directives, 1, directives.Count);
			parent.ConditionStack[lastPos] = ConditionStackElement.DoIf(ret, parent.ConditionStack[lastPos]);
		}
	}

	/// <summary>
	/// Used to end 'if', 'else' and 'elif' blocks
	/// </summary>
	public class Endif : Directive
	{
		public override Substring Value
		{
			get { return "endif"; }
		}

		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			parent.CheckExcessParams(directives, 1);
			if (parent.ConditionStack.Count < 1)
				parent.Output.Errors.Add(new UnexpectedDirective(directives[0]));
			else
				parent.ConditionStack.RemoveAt(parent.ConditionStack.Count - 1);
		}
	}

	public abstract class DefineBase : Directive
	{
		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			if (directives.Count < 2)
			{
				parent.Output.Errors.Add(new InvalidPreprocessorExpression(directives[0]));
				return;
			}
			var item = directives[1].Item as Word;
			if (item == null)
			{
				parent.Output.Errors.Add(new InvalidPreprocessorExpression(directives[1]));
				return;
			}
			parent.CheckExcessParams(directives, 2);
			if (!parent.IncludeCode)
				return;
			var conditions = parent.Settings.Defines;
			DoFunction(parent);
		}

		protected abstract void DoFunction(Preprocessor parent);
	}

	/// <summary>
	/// Used to define conditional compilation symbols
	/// </summary>
	public class Define : DefineBase
	{
		public override Substring Value
		{
			get { return "define"; }
		}

		protected override void DoFunction(Preprocessor parent)
		{
			var conditions = parent.Settings.Defines;
			if (!conditions.Contains(Value))
				conditions.Add(Value);
		}
	}

	/// <summary>
	/// Used to undefine conditional compilation symbols
	/// </summary>
	public class Undef : DefineBase
	{
		public override Substring Value
		{
			get { return "undef"; }
		}

		protected override void DoFunction(Preprocessor parent)
		{
			var conditions = parent.Settings.Defines;
			while (conditions.Remove(Value))
			{
			}
		}
	}

	public abstract class OutputDirective : Directive
	{
		public Substring TextValue;

		public override Token CheckPresence(Lexer lexer)
		{
			var baseRet = (OutputDirective)base.CheckPresence(lexer);
			bool valid = false;
			if (baseRet != null && lexer.Processed.Count > 0)
			{
				var lastToken = lexer.Processed[lexer.Processed.Count - 1];
				if (lastToken.Item is Hash)
				{
					valid = true;
				}
				else if (lastToken.Item is Whitespace && lexer.Processed.Count > 1)
				{
					if (lexer.Processed[lexer.Processed.Count - 2].Item is Hash)
						valid = true;
				}
			}
			if (valid)
			{
				lexer.EatWhitespace();
				baseRet.TextValue = lexer.EatUntil('\n', false);
			}
			return baseRet;
		}
	}

	/// <summary>
	/// Outputs
	/// </summary>
	public class Warning : OutputDirective
	{
		public override Substring Value
		{
			get { return "warning"; }
		}

		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			if(directives[0].Item is Pragma)
			{

			} else
				parent.Output.Errors.Add(new UserWarning(TextValue, directives[0]));
		}
	}

	public abstract class SetWarningLevel : Keyword
	{
		public abstract WarningLevel Level { get; }
	}

	public class Disable : SetWarningLevel
	{
		public override WarningLevel Level
		{
			get { return WarningLevel.Disabled; }
		}

		public override Substring Value
		{
			get { return "disable"; }
		}
	}

	public class Restore : SetWarningLevel
	{
		public override WarningLevel Level
		{
			get { return WarningLevel.Normal; }
		}

		public override Substring Value
		{
			get { return "restore"; }
		}
	}

	public class Error : OutputDirective
	{
		public override Substring Value
		{
			get { return "error"; }
		}

		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			parent.Output.Errors.Add(new UserError(TextValue, directives[0]));
		}
	}

	public class Line : Directive
	{
		public override Substring Value
		{
			get { return "line"; }
		}

		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			throw new NotImplementedException();
		}
	}

	public class Region : Directive
	{
		public override Substring Value
		{
			get { return "region"; }
		}

		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
		}
	}

	public class Endregion : Directive
	{
		public override Substring Value
		{
			get { return "endregion"; }
		}

		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
		}
	}

	public class Pragma : Directive
	{
		public override Substring Value
		{
			get { return "pragma"; }
		}

		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			if(directives.Count < 2)
			{
				parent.Output.Errors.Add(new UnrecognizedPragma(directives[0]));
			}
			else
			{
				var first = directives[1];
				var firstItem = first.Item;
				var directive = firstItem as IDirective;
				if (directive != null)
					directive.RunDirective(parent, directives);
				else
					parent.Output.Errors.Add(new UnrecognizedPragma(first));
			}
		}
	}

	public class Checksum : Directive
	{
		public override Substring Value
		{
			get { return "checksum"; }
		}

		public override void RunDirective(Preprocessor parent, IList<TokenInfo> directives)
		{
			throw new NotImplementedException();
		}
	}

	public class CSharpPreprocessor : Preprocessor
	{
		protected PreprocessorSettings settings;
		protected CompilerOutput output;
		protected IList<TokenInfo> input;
		protected List<ConditionStackElement> conditionStack;
		protected bool pastFirstSymbol;

		public override bool PastFirstSymbol
		{
			get { return pastFirstSymbol; }
		}

		public override IList<ConditionStackElement> ConditionStack
		{
			get
			{
				if (conditionStack == null)
					conditionStack = new List<ConditionStackElement>();
				return conditionStack;
			}
		}

		public override PreprocessorSettings Settings
		{
			get
			{
				if (settings == null)
					settings = new DefaultPreprocessorSettings();
				return settings;
			}
			protected set
			{
				settings = value;
			}
		}

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

		public override void SetInput(IList<TokenInfo> input)
		{
			this.input = input;
		}
		
		protected enum ExpressionValue
		{
			None, False, True, AND, OR
		}

		protected struct ExpressionValueInfo
		{
			public ExpressionValue Value;
			public int Column;
			public int Length;

			public ExpressionValueInfo(ExpressionValue value, int column, int length)
			{
				this.Value = value;
				this.Column = column;
				this.Length = length;
			}
		}
		
		public override bool ParseExpression(IList<TokenInfo> directives, int start, int end)
		{
			if(directives == null)
				throw new ArgumentNullException("directives");
			if (start >= directives.Count)
				throw new ArgumentOutOfRangeException("start");
			if(end - start < 1)
			{
				Output.Errors.Add(new InvalidPreprocessorExpression(directives[start]));
				return false;
			}
			int parenLocation = 0;
			int parenDepth = 0;
			var expressionList = new List<ExpressionValueInfo>();
			for (int i = start; i < end && i < directives.Count; i++)
			{
				var item = directives[i];
				if (item.Item is ParenOpen)
				{
					if (parenDepth == 0)
						parenLocation = i;
					parenDepth++;
				}
				else if (item.Item is ParenClose)
				{
					parenDepth--;
					if (parenDepth == 0)
						expressionList.Add(new ExpressionValueInfo(
							ParseExpression(directives, parenLocation + 1, i)
							? ExpressionValue.True : ExpressionValue.False,
							item.Column, item.Item.Length));
				}
				else
				{
					if (parenDepth != 0)
						continue;
					var word = item.Item as Word;
					if (word != null)
						expressionList.Add(new ExpressionValueInfo(
							Settings.Defines.Contains(word.Value)
							? ExpressionValue.True : ExpressionValue.False,
							item.Column, item.Item.Length));
					else if (item.Item is AND)
						expressionList.Add(new ExpressionValueInfo(
							ExpressionValue.AND, item.Column, item.Item.Length));
					else if (item.Item is OR)
						expressionList.Add(new ExpressionValueInfo(
							ExpressionValue.OR, item.Column, item.Item.Length));
					else
						Output.Errors.Add(new InvalidPreprocessorExpression(item));
				}
			}
			return EvaluateExpression(expressionList, directives[start].Line);
		}
		
		protected virtual bool EvaluateExpression(IList<ExpressionValueInfo> expressionList, int line)
		{
			var value = ExpressionValue.None;
			var op = ExpressionValue.None;
			for (int i = 0; i < expressionList.Count; i++)
			{
				var exp = expressionList[i];
				var v = exp.Value;
				if (v == ExpressionValue.False || v == ExpressionValue.True)
				{
					if (value != ExpressionValue.None && op == ExpressionValue.None)
					{
						Output.Errors.Add(new EndOfLineExpected(line, exp.Column, exp.Length));
						return false;
					}
					else if (value == ExpressionValue.None)
						value = v;
					else
					{
						if (op == ExpressionValue.AND)
							value = (value == ExpressionValue.True) && (v == ExpressionValue.True) ?
								ExpressionValue.True : ExpressionValue.False;
						else
							value = (value == ExpressionValue.True) || (v == ExpressionValue.True) ?
								ExpressionValue.True : ExpressionValue.False;
						op = ExpressionValue.None;
					}
				}
				else
				{
					if (value == ExpressionValue.None)
					{
						Output.Errors.Add(new InvalidPreprocessorExpression(line, exp.Column, exp.Length));
						return false;
					}
					op = v;
				}
			}
			if (op != ExpressionValue.None)
				Output.Errors.Add(new InvalidPreprocessorExpression(line, 0, 1));
			return (value == ExpressionValue.True);
		}

		public override bool IncludeCode
		{
			get
			{
				for (int i = 0; i < ConditionStack.Count; i++)
					if (!ConditionStack[i].Enabled)
						return false;
				return true;
			}
		}

		public override bool CheckExcessParams(IList<TokenInfo> directives, int num)
		{
			if (directives.Count > num)
				Output.Errors.Add(new UnexpectedDirective(directives[num]));
			return directives.Count < num;
		}
		
		public override bool IgnoreToken(Token token)
		{
			return token is Whitespace || token is Comment;
		}

		public void ProcessDirective(IList<TokenInfo> directives, bool pastFirstSymbol, int line, int column)
		{
			if (directives == null)
				throw new ArgumentNullException("directives");
			if (directives.Count == 0)
			{
				Output.Errors.Add(new DirectiveExpected(line, column, 1));
				return;
			}

			var first = directives[0];
			var firstItem = first.Item;
			var directive = firstItem as IDirective;
			if (directive != null)
				directive.RunDirective(this, directives);
			else
				Output.Errors.Add(new DirectiveExpected(first));
		}

		public override IList<TokenInfo> Run()
		{
			if (input == null)
				throw new InvalidOperationException("No input");

			var ret = new List<TokenInfo>(input.Count);
			bool pastFirstSymbol = false;
			var tokens = new List<TokenInfo>();
			for (int i = 0; i < input.Count; i++)
			{
				var item = input[i];
				if (item.Item is Hash)
				{
					int startPos = i;
					while (startPos-- > 0)
					{
						var testItem = input[startPos];
						if (testItem.Item is Whitespace)
							continue;
						if (testItem.Line != item.Line)
							break;
						Output.Errors.Add(new NotFirstCharacter(item));
					}
					tokens.Clear();
					for (int j = i + 1; j < input.Count; j++)
					{
						var newItem = input[j];
						if (newItem.Line != item.Line)
							break;
						i++;
						if (newItem.Item is Whitespace)
							continue;
						tokens.Add(newItem);
					}
					ProcessDirective(tokens, pastFirstSymbol, item.Line, item.Column);
				}
				else if (!IgnoreToken(item.Item))
				{
					pastFirstSymbol = true;
					if (IncludeCode)
						ret.Add(item);						
				}
			}
			if (ConditionStack.Count > 0)
			{
				var last = input[input.Count - 1];
				Output.Errors.Add(new EndifExpected(last));
			}
			return ret;
		}
	}
}