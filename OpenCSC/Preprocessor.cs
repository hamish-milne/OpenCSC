using System;
using System.Collections.Generic;
using System.Text;
using OpenCompiler;

namespace OpenCSC
{
	public class If : Keyword
	{
		public override Substring Value
		{
			get { return "if"; }
		}
	}

	public class Else : Keyword
	{
		public override Substring Value
		{
			get { return "else"; }
		}
	}

	public abstract class PPDirective : Keyword
	{
	}

	public class Elif : PPDirective
	{
		public override Substring Value
		{
			get { return "elif"; }
		}
	}

	public class Endif : PPDirective
	{
		public override Substring Value
		{
			get { return "endif"; }
		}
	}

	public class Define : PPDirective
	{
		public override Substring Value
		{
			get { return "define"; }
		}
	}

	public class Undef : PPDirective
	{
		public override Substring Value
		{
			get { return "undef"; }
		}
	}

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
		protected IList<LexerItemInfo> input;
		protected List<bool> conditionStack = new List<bool>();

		void IInput<PreprocessorSettings>.SetInput(PreprocessorSettings value)
		{
			Settings = value;
		}

		public override void SetInput(IList<LexerItemInfo> input)
		{
			this.input = input;
		}

		public virtual bool ParseExpression(int position)
		{
			return false;
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
				throw new CodeException(directives[num] + " was unexpected",
					directives[num].Line, directives[num].Column);
		}

		public virtual void ProcessDirective(IList<LexerItemInfo> directives, bool pastFirstSymbol, int line, int column)
		{
			if (directives == null)
				throw new ArgumentNullException("directives");
			if (directives.Count == 0)
				throw new CodeException("Preprocessor directive expected", line, column);
			var first = directives[0];
			var firstItem = first.Item;
			if (firstItem is Region || first.Item is Endregion)
				return;
			if (firstItem is Define || firstItem is Undef)
			{
				if (directives.Count < 2)
					throw new CodeException("Expecting an identifier", first.Line, first.Column);
				var item = directives[1].Item as Word;
				if (item == null)
					throw new CodeException(directives[1].Item + " is not an identifier",
						directives[1].Line, directives[1].Column);
				CheckExcessParams(directives, 2);
				if (!IncludeCode())
					return;
				if (firstItem is Define)
					Settings.Conditions.Add(item.Value);
				else
					while (Settings.Conditions.Remove(item.Value)) { }
			}
			else if (firstItem is If)
			{

			}
			else if (firstItem is Elif)
			{

			}
			else if (firstItem is Else)
			{

			}
			else if (firstItem is Endif)
			{
				CheckExcessParams(directives, 1);
				conditionStack.RemoveAt(conditionStack.Count - 1);
			}
			else
			{
				throw new CodeException("Preprocessor directive expected", first.Line, first.Column);
			}
		}

		public virtual bool IgnoreToken(LexerItem token)
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
						throw new CodeException(
							"Preprocessor directives must be the first non-whitespace character on a line",
							item.Line, item.Column);
					}
					tokens.Clear();
					for (int j = i + 1; j < input.Count; j++)
					{
						var newItem = input[j];
						if (newItem.Line != item.Line)
							break;
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
			return ret;
		}
	}
}
