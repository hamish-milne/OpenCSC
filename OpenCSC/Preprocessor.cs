using System;
using System.Collections.Generic;
using System.Text;
using OpenCompiler;

namespace OpenCSC
{
	/// <summary>
	/// 'If' statement
	/// </summary>
	public class If : Keyword
	{
		public override Substring Value
		{
			get { return "if"; }
		}
	}

	/// <summary>
	/// 'Else' statement
	/// </summary>
	public class Else : Keyword
	{
		public override Substring Value
		{
			get { return "else"; }
		}
	}

	/// <summary>
	/// Base class for preprocessor directives
	/// </summary>
	public abstract class PPDirective : Keyword
	{
	}

	/// <summary>
	/// Else-if directive
	/// </summary>
	public class Elif : PPDirective
	{
		public override Substring Value
		{
			get { return "elif"; }
		}
	}

	/// <summary>
	/// Used to end 'if', 'else' and 'elif' blocks
	/// </summary>
	public class Endif : PPDirective
	{
		public override Substring Value
		{
			get { return "endif"; }
		}
	}

	/// <summary>
	/// Used to define conditional compilation symbols
	/// </summary>
	public class Define : PPDirective
	{
		public override Substring Value
		{
			get { return "define"; }
		}
	}

	/// <summary>
	/// Used to undefine conditional compilation symbols
	/// </summary>
	public class Undef : PPDirective
	{
		public override Substring Value
		{
			get { return "undef"; }
		}
	}

	/// <summary>
	/// Outputs
	/// </summary>
	public class Warning : PPDirective
	{
		public override Substring Value
		{
			get { return "warning"; }
		}
	}

	public class Error : PPDirective
	{
		public override Substring Value
		{
			get { return "error"; }
		}
	}

	public class Line : PPDirective
	{
		public override Substring Value
		{
			get { return "line"; }
		}
	}

	public class Region : PPDirective
	{
		public override Substring Value
		{
			get { return "region"; }
		}
	}

	public class Endregion : PPDirective
	{
		public override Substring Value
		{
			get { return "endregion"; }
		}
	}

	public class Pragma : PPDirective
	{
		public override Substring Value
		{
			get { return "pragma"; }
		}
	}

	public class Checksum : PPDirective
	{
		public override Substring Value
		{
			get { return "checksum"; }
		}
	}

	public enum WarningLevel
	{
		Error = -1,
		Normal = 0,
		Disabled = 1
	}

	public class PreprocessorSettings
	{
		protected ICollection<Substring> conditions;

		protected ICollection<int> warningDisable;

		protected ICollection<int> warningErrors;

		public virtual ICollection<Substring> Conditions
		{
			get
			{
				if (conditions == null)
					conditions = new List<Substring>();
				return conditions;
			}
		}

		public virtual ICollection<int> WarningDisable
		{
			get
			{
				if (warningDisable == null)
					warningDisable = new List<int>();
				return warningDisable;
			}
		}

		public virtual ICollection<int> WarningErrors
		{
			get
			{
				if (warningErrors == null)
					warningErrors = new List<int>();
				return warningErrors;
			}
		}
	}
	
	public class Preprocessor : Pipeline<IList<LexerItemInfo>, IList<LexerItemInfo>>, IInput<PreprocessorSettings>
	{
		public PreprocessorSettings Settings;
		protected CompilerOutput output;
		protected IList<LexerItemInfo> input;
		protected List<bool> conditionStack = new List<bool>();
		protected Dictionary<LexerItem, Action> actions;
		protected bool switchOn;
		protected bool elseUsed;

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

		protected delegate void Action(IList<LexerItemInfo> directives);

		public Preprocessor()
		{
			actions = new Dictionary<LexerItem, Action>()
			{
				{ new Define(), Define },
				{ new Undef(), Define },
				{ new If(), If },
				{ new Elif(), Elif },
				{ new Else(), Else },
				{ new Endif(), Endif },
				{ new Region(), Nop },
				{ new Endregion(), Nop },
			};
		}

		void IInput<PreprocessorSettings>.SetInput(PreprocessorSettings value)
		{
			Settings = value;
		}

		public override void SetInput(IList<LexerItemInfo> input)
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
		
		public virtual bool ParseExpression(IList<LexerItemInfo> directives, int start, int end)
		{
			if(directives == null)
				throw new ArgumentNullException("directives");
			if (start >= directives.Count)
				throw new ArgumentOutOfRangeException("start");
			if(end - start < 1)
			{
				Output.Errors.Add(new InvalidPreprocessorExpression(directives[start].Line, directives[start].Column, 1));
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
							Settings.Conditions.Contains(word.Value)
							? ExpressionValue.True : ExpressionValue.False,
							item.Column, item.Item.Length));
					else if (item.Item is AND)
						expressionList.Add(new ExpressionValueInfo(
							ExpressionValue.AND, item.Column, item.Item.Length));
					else if (item.Item is OR)
						expressionList.Add(new ExpressionValueInfo(
							ExpressionValue.OR, item.Column, item.Item.Length));
					else
						Output.Errors.Add(new InvalidPreprocessorExpression(item.Line, item.Column, item.Item.Length));
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

		protected bool IncludeCode()
		{
			for (int i = 0; i < conditionStack.Count; i++)
				if (!conditionStack[i])
					return false;
			return true;
		}

		protected void CheckExcessParams(IList<LexerItemInfo> directives, int num)
		{
			if (directives.Count > num)
				Output.Errors.Add(new UnexpectedDirective(
					directives[num].Line, directives[num].Column, directives[num].Item.Length));
		}

		protected void Nop(IList<LexerItemInfo> directives)
		{
		}

		protected void If(IList<LexerItemInfo> directives)
		{
			var ret = ParseExpression(directives, 1, directives.Count);
			switchOn = ret;
			conditionStack.Add(ret);
		}

		protected void Else(IList<LexerItemInfo> directives)
		{
			CheckExcessParams(directives, 1);
			if (conditionStack.Count < 1 || elseUsed)
				Output.Errors.Add(new UnexpectedDirective(
					directives[0].Line, directives[0].Column, directives[0].Item.Length));
			conditionStack[conditionStack.Count - 1] = !switchOn;
			elseUsed = true;
		}
		
		protected void Elif(IList<LexerItemInfo> directives)
		{
			if (!switchOn)
			{
				var ret = ParseExpression(directives, 1, directives.Count);
				switchOn = ret;
				conditionStack[conditionStack.Count - 1] = ret;
			}
			else
			{
				conditionStack[conditionStack.Count - 1] = false;
			}
		}

		protected void Define(IList<LexerItemInfo> directives)
		{
			if (directives.Count < 2)
			{
				Output.Errors.Add(new InvalidPreprocessorExpression(
					directives[0].Line, directives[0].Column, directives[0].Item.Length));
				return;
			}
			var item = directives[1].Item as Word;
			if (item == null)
			{
				Output.Errors.Add(new InvalidPreprocessorExpression(
					directives[1].Line, directives[1].Column, directives[1].Item.Length));
				return;
			}
			CheckExcessParams(directives, 2);
			if (!IncludeCode())
				return;
			if (directives[0].Item is Define)
				Settings.Conditions.Add(item.Value);
			else
				while (Settings.Conditions.Remove(item.Value)) { }
		}

		protected void Endif(IList<LexerItemInfo> directives)
		{
			CheckExcessParams(directives, 1);
			conditionStack.RemoveAt(conditionStack.Count - 1);
			switchOn = false;
		}


		protected virtual void ProcessDirective(IList<LexerItemInfo> directives, bool pastFirstSymbol, int line, int column)
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
			Action action;
			actions.TryGetValue(firstItem, out action);
			if (action != null)
				action(directives);
			else
				Output.Errors.Add(new DirectiveExpected(first.Line, first.Column, firstItem.Length));
		}
		
		protected virtual bool IgnoreToken(LexerItem token)
		{
			return token is WhitespaceToken || token is Comment;
		}

		public override IList<LexerItemInfo> Run()
		{
			if (input == null)
				throw new InvalidOperationException("No input");
			if (Settings == null)
				Settings = new PreprocessorSettings();

			var ret = new List<LexerItemInfo>(input.Count);
			bool pastFirstSymbol = false;
			var tokens = new List<LexerItemInfo>();
			for (int i = 0; i < input.Count; i++)
			{
				var item = input[i];
				if (item.Item is Hash)
				{
					int startPos = i;
					while (startPos-- > 0)
					{
						var testItem = input[startPos];
						if (testItem.Item is WhitespaceToken)
							continue;
						if (testItem.Line != item.Line)
							break;
						Output.Errors.Add(new NotFirstCharacter(item.Line, item.Column, item.Item.Length));
					}
					tokens.Clear();
					for (int j = i + 1; j < input.Count; j++)
					{
						var newItem = input[j];
						if (newItem.Line != item.Line)
							break;
						i++;
						if (newItem.Item is WhitespaceToken)
							continue;
						tokens.Add(newItem);
					}
					ProcessDirective(tokens, pastFirstSymbol, item.Line, item.Column);
				}
				else
				{
					if (!IgnoreToken(item.Item))
						pastFirstSymbol = true;
					if(IncludeCode())
						ret.Add(item);
				}
			}
			if (conditionStack.Count > 0)
			{
				var last = input[input.Count - 1];
				Output.Errors.Add(new EndifExpected(last.Line, last.Column, last.Item.Length));
			}
			return ret;
		}
	}
}