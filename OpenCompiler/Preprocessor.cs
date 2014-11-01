using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OpenCompiler
{

#if !NET35
	public class HashSet<T> : ICollection<T>
	{
		protected Dictionary<T, bool> dict =
			new Dictionary<T, bool>();
		protected int count;

		protected class Enumerator : IEnumerator<T>
		{
			IEnumerator<KeyValuePair<T, bool>> internalEnumerator;
			T current;

			public T Current
			{
				get { return current; }
			}

			public bool MoveNext()
			{
				bool ret;
				do
				{
					ret = internalEnumerator.MoveNext();
				} while (ret && !internalEnumerator.Current.Value);
				return ret;
			}

			public void Reset()
			{
				current = default(T);
				internalEnumerator.Reset();
			}

			object IEnumerator.Current
			{
				get { return current; }
			}

			public Enumerator(HashSet<T> set)
			{
				internalEnumerator = set.dict.GetEnumerator();
			}

			public void Dispose()
			{
				internalEnumerator.Dispose();
				current = default(T);
			}
		}

		public int Count
		{
			get { return count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Add(T item)
		{
			bool ret;
			dict.TryGetValue(item, out ret);
			if (!ret)
			{
				dict[item] = true;
				count++;
			}
			return !ret;
		}

		public bool Remove(T item)
		{
			bool ret;
			dict.TryGetValue(item, out ret);
			if (ret)
			{
				dict[item] = false;
				count--;
			}
			return ret;
		}

		public void Clear()
		{
			dict.Clear();
			count = 0;
		}

		public bool Contains(T item)
		{
			bool ret;
			dict.TryGetValue(item, out ret);
			return ret;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			foreach (var pair in dict)
				if (pair.Value)
					array[arrayIndex++] = pair.Key;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

	}
#endif


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

	public abstract class PreprocessorSettings
	{
		public abstract ICollection<Substring> Defines { get; }
		public abstract IDictionary<LineError, WarningLevel> Warnings { get; }
	}

	public class DefaultPreprocessorSettings : PreprocessorSettings
	{
		protected ICollection<Substring> defines;
		protected IDictionary<LineError, WarningLevel> warnings;

		public override ICollection<Substring> Defines
		{
			get
			{
				if (defines == null)
					defines = new HashSet<Substring>();
				return defines;
			}
		}

		public override IDictionary<LineError, WarningLevel> Warnings
		{
			get
			{
				if (warnings == null)
					warnings = new Dictionary<LineError, WarningLevel>();
				return warnings;
			}
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
		Pipeline<IList<TokenInfo>, IList<TokenInfo>>,
		IInput<PreprocessorSettings>
	{
		public abstract PreprocessorSettings Settings { get; protected set; }

		public abstract IDictionary<int, WarningLevel> CurrentWarningLevels { get; }

		public abstract bool PastFirstSymbol { get; }

		public virtual void AddError(CompilerError error)
		{
			if (error == null)
				throw new ArgumentNullException("error");
			WarningLevel level;
			Settings.Warnings.TryGetValue(new LineError(error.Line, error.Number), out level);
			if (level == WarningLevel.Error)
				error.TreatAsError();
			if (level != WarningLevel.Disabled)
				Output.Errors.Add(error);
		}

		public abstract IList<ConditionStackElement> ConditionStack { get; }

		public abstract bool ParseExpression(IList<TokenInfo> directives, int start, int end);

		public abstract bool CheckExcessParams(IList<TokenInfo> directives, int num);

		public abstract bool IncludeCode { get; }

		public abstract bool IgnoreToken(Token item);

		void IInput<PreprocessorSettings>.SetInput(PreprocessorSettings settings)
		{
			Settings = settings;
		}
	}
}
