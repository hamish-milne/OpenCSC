using System;
using System.Collections.Generic;
using System.Text;
using OpenCompiler;

namespace OpenCSC
{
	public interface IDirective
	{
		Preprocessor.Directive GetDirective();
	}

	/// <summary>
	/// 'If' statement
	/// </summary>
	public class If : Keyword, IDirective
	{
		public override Substring Value
		{
			get { return "if"; }
		}

		public virtual Preprocessor.Directive GetDirective()
		{
			return Cache<Preprocessor.If>.Instance;
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

		public virtual Preprocessor.Directive GetDirective()
		{
			return Cache<Preprocessor.If>.Instance;
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
		protected List<ConditionStackElement> conditionStack = new List<ConditionStackElement>();

		protected struct ConditionStackElement
		{
			public bool Enabled;
			public bool HasEnabled;
			public bool Final;

			public ConditionStackElement(bool enabled)
			{
				Enabled = enabled;
				HasEnabled = enabled;
				Final = false;
			}

			public ConditionStackElement(bool enabled, bool hasEnabled, bool final)
			{
				Enabled = enabled;
				HasEnabled = hasEnabled;
				Final = final;
			}

			public static ConditionStackElement DoIf(bool enabled, ConditionStackElement prev)
			{
				return new ConditionStackElement(enabled && !prev.HasEnabled, enabled || prev.HasEnabled, prev.Final);
			}

			public static ConditionStackElement DoElse(ConditionStackElement prev)
			{
				return new ConditionStackElement(!prev.HasEnabled, true, true);
			}
		}

		/// <summary>
		/// Base class for preprocessor directives
		/// </summary>
		public abstract class Directive : Keyword
		{
			public abstract void RunDirective(Preprocessor parent, IList<LexerItemInfo> directives);
		}

		/// <summary>
		/// If directive
		/// </summary>
		public class If : Directive
		{
			public override Substring Value
			{
				get { return "if"; }
			}

			public override void RunDirective(Preprocessor parent, IList<LexerItemInfo> directives)
			{
				var ret = parent.ParseExpression(directives, 1, directives.Count);
				parent.conditionStack.Add(new ConditionStackElement(ret));
			}
		}

		/// <summary>
		/// Else directive
		/// </summary>
		public class Else : Directive
		{
			public override Substring Value
			{
				get { return "else"; }
			}

			public override void RunDirective(Preprocessor parent, IList<LexerItemInfo> directives)
			{
				parent.CheckExcessParams(directives, 1);
				var cs = parent.conditionStack;
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

			public override void RunDirective(Preprocessor parent, IList<LexerItemInfo> directives)
			{
				var lastPos = parent.conditionStack.Count - 1;
				var ret = parent.ParseExpression(directives, 1, directives.Count);
				parent.conditionStack[lastPos] = ConditionStackElement.DoIf(ret, parent.conditionStack[lastPos]);
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

			public override void RunDirective(Preprocessor parent, IList<LexerItemInfo> directives)
			{
				parent.CheckExcessParams(directives, 1);
				if (parent.conditionStack.Count < 1)
					parent.Output.Errors.Add(new UnexpectedDirective(directives[0]));
				else
					parent.conditionStack.RemoveAt(parent.conditionStack.Count - 1);
			}
		}

		public abstract class DefineBase : Directive
		{
			public override void RunDirective(Preprocessor parent, IList<LexerItemInfo> directives)
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
				if (!parent.IncludeCode())
					return;
				var conditions = parent.Settings.Conditions;
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
				var conditions = parent.Settings.Conditions;
				if (!conditions.Contains(Value))
					conditions.Add(Value);
			}
		}

		/// <summary>
		/// Used to undefine conditional compilation symbols
		/// </summary>
		public class Undef : Directive
		{
			public override Substring Value
			{
				get { return "undef"; }
			}

			protected override void DoFunction(Preprocessor parent)
			{
				var conditions = parent.Settings.Conditions;
				while (conditions.Remove(Value))
				{
				}
			}
		}

		/*public abstract class OutputDirective : Directive
		{


			public override LexerItem CheckPresence(Lexer lexer)
			{
				var baseRet = base.CheckPresence(lexer);
				if (baseRet != null)
				{
					lexer.EatWhitespace();
					lexer.EatUntil('\n', false);
				}
			}
		}*/

		/// <summary>
		/// Outputs
		/// </summary>
		public class Warning : Directive
		{
			public override Substring Value
			{
				get { return "warning"; }
			}

			



			public override void RunDirective(Preprocessor parent, IList<LexerItemInfo> directives)
			{
				
			}
		}

		public class Error : Directive
		{
			public override Substring Value
			{
				get { return "error"; }
			}
		}

		public class Line : Directive
		{
			public override Substring Value
			{
				get { return "line"; }
			}
		}

		public class Region : Directive
		{
			public override Substring Value
			{
				get { return "region"; }
			}

			public override void RunDirective(Preprocessor parent, IList<LexerItemInfo> directives)
			{
			}
		}

		public class Endregion : Directive
		{
			public override Substring Value
			{
				get { return "endregion"; }
			}

			public override void RunDirective(Preprocessor parent, IList<LexerItemInfo> directives)
			{
			}
		}

		public class Pragma : Directive
		{
			public override Substring Value
			{
				get { return "pragma"; }
			}
		}

		public class Checksum : Directive
		{
			public override Substring Value
			{
				get { return "checksum"; }
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

		protected bool IncludeCode()
		{
			for (int i = 0; i < conditionStack.Count; i++)
				if (!conditionStack[i].Enabled)
					return false;
			return true;
		}

		protected void CheckExcessParams(IList<LexerItemInfo> directives, int num)
		{
			if (directives.Count > num)
				Output.Errors.Add(new UnexpectedDirective(directives[num]));
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
			var directive = firstItem as Directive;
			if (directive == null)
			{
				var iface = firstItem as IDirective;
				if (iface != null)
					directive = iface.GetDirective();
			}
			if (directive != null)
				directive.RunDirective(this, directives);
			else
				Output.Errors.Add(new DirectiveExpected(first));
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
						Output.Errors.Add(new NotFirstCharacter(item));
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
				else if (!IgnoreToken(item.Item))
				{
					pastFirstSymbol = true;
					if (IncludeCode())
						ret.Add(item);						
				}
			}
			if (conditionStack.Count > 0)
			{
				var last = input[input.Count - 1];
				Output.Errors.Add(new EndifExpected(last));
			}
			return ret;
		}
	}
}