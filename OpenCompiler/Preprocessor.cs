using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OpenCompiler
{
	public interface IDirective
	{
		void RunDirective(Preprocessor pp, IList<TokenInfo> directives);
	}

	/// <summary>
	/// Base class for preprocessor directives
	/// </summary>
	public abstract class Directive : Keyword, IDirective
	{
		public abstract void RunDirective(Preprocessor parent, IList<TokenInfo> directives);
	}

	public enum WarningLevel
	{
		Error = -1,
		Normal = 0,
		Disabled = 1
	}

	public struct LineError
	{
		public int Line;
		public int Error;

		public LineError(int line, int error)
		{
			Line = line;
			Error = error;
		}

		public override int GetHashCode()
		{
			return (509 * Line) + Error; 
		}

		public override bool Equals(object obj)
		{
			if(obj is LineError)
			{
				var o = (LineError)obj;
				return (o.Line == Line && o.Error == Error);
			}
			return false;
		}
	}

	public struct ConditionStackElement
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

	public abstract class Preprocessor :
		Pipeline<IList<TokenInfo>, IList<TokenInfo>>
	{
		public abstract IDictionary<int, WarningLevel> CurrentWarningLevels { get; }

		public abstract bool PastFirstSymbol { get; }

		public abstract int ApparentLineNumber { get; set; }

		public abstract Substring ApparentFileName { get; set; }

		public abstract bool DebugHide { get; set; }

		public abstract IList<ConditionStackElement> ConditionStack { get; }

		public abstract bool ParseExpression(IList<TokenInfo> directives, int start, int end);

		public abstract bool CheckExcessParams(IList<TokenInfo> directives, int num);

		public abstract bool IncludeCode { get; }

		public abstract bool IgnoreToken(Token item);
	}
}
