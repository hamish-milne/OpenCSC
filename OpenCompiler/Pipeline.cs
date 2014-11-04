using System;
using System.Collections;
using System.Collections.Generic;

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

	/// <summary>
	/// Base class for a user code exception
	/// </summary>
	public class CodeException : Exception
	{
		public CodeException()
			: base("The compilation could not be completed")
		{
		}

		public CodeException(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Accepts an input of type <typeparamref name="T"/>
	/// </summary>
	/// <typeparam name="T">The type of input to accept</typeparam>
	public interface IInput<T>
	{
		/// <summary>
		/// Sets the pipeline stage to use the provided input
		/// </summary>
		/// <param name="input">The input object</param>
		void SetInput(T input);
	}

	/// <summary>
	/// Provides an output of type <typeparamref name="T"/>
	/// </summary>
	/// <typeparam name="T">The type of output to provide</typeparam>
	public interface IOutput<T>
	{
		/// <summary>
		/// Executes the pipeline stage to provide the required output
		/// </summary>
		/// <returns>The output of this stage</returns>
		T Run();
	}

	/// <summary>
	/// Base class for compiler input
	/// </summary>
	public abstract class CompilerInput
	{
		/// <summary>
		/// A list of source files to compile
		/// </summary>
		public abstract IList<string> SourceFiles { get; }

		/// <summary>
		/// The desired output location
		/// </summary>
		public abstract string OutputLocation { get; set; }

		public abstract ICollection<Substring> Defines { get; }

		public abstract IDictionary<LineError, WarningLevel> Warnings { get; }
	}

	/// <summary>
	/// Base class for compiler output
	/// </summary>
	public abstract class CompilerOutput
	{
		/// <summary>
		/// <c>true</c> if the compilation was a success, otherwise <c>false</c>
		/// </summary>
		public abstract bool Success { get; }

		/// <summary>
		/// The collection of compiler output lines (errors, warnings and information)
		/// </summary>
		public abstract IList<CompilerError> Errors { get; }
	}

	public class DefaultCompilerInput : CompilerInput
	{
		protected IList<string> sourceFiles;
		protected string outputLocation;
		protected ICollection<Substring> defines;
		protected IDictionary<LineError, WarningLevel> warnings;

		public override IList<string> SourceFiles
		{
			get
			{
				if (sourceFiles == null)
					sourceFiles = new List<string>();
				return sourceFiles;
			}
		}

		public override string OutputLocation
		{
			get { return outputLocation; }
			set { outputLocation = value; }
		}

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

	public class DefaultCompilerOutput : CompilerOutput
	{
		protected IList<CompilerError> errors;

		public override bool Success
		{
			get
			{
				for (int i = 0; i < Errors.Count; i++)
				{
					var e = Errors[i];
					if (e != null && e.ErrorLevel == ErrorLevel.Error)
						return false;
				}
				return true;
			}
		}

		public override IList<CompilerError> Errors
		{
			get
			{
				if (errors == null)
					errors = new List<CompilerError>();
				return errors;
			}
		}
	}

	public static class Cache<T> where T : class, new()
	{
		public static readonly T Instance = new T();
	}

	/// <summary>
	/// Base class for a pipeline stage, accepting input and providing output
	/// </summary>
	/// <typeparam name="TInput">The type of input to accept</typeparam>
	/// <typeparam name="TOutput">Thhe type of output to provide</typeparam>
	public abstract class Pipeline<TInput, TOutput> : IInput<TInput>, IOutput<TOutput>
	{
		public static T Get<T>() where T : class, new()
		{
			return Cache<T>.Instance;
		}

		public abstract CompilerOutput Output { get; set; }

		public abstract CompilerInput Input { get; set; }

		/// <summary>
		/// Sets the pipeline stage to use the provided input
		/// </summary>
		/// <param name="input">The input object</param>
		public abstract void SetInput(TInput input);

		/// <summary>
		/// Executes the pipeline stage to provide the required output
		/// </summary>
		/// <returns>The output of this stage</returns>
		public abstract TOutput Run();

		public virtual void AddError(CompilerError error)
		{
			if (error == null)
				throw new ArgumentNullException("error");
			WarningLevel level;
			Input.Warnings.TryGetValue(new LineError(error.Line, error.Number), out level);
			if (level == WarningLevel.Error)
				error.TreatAsError();
			if (level != WarningLevel.Disabled)
				Output.Errors.Add(error);
		}
	}

	/// <summary>
	/// Base class for a chained pipeline that allows an intemediary stage between TMiddle and TOutput
	/// </summary>
	/// <typeparam name="TInput">The type of input to accept</typeparam>
	/// <typeparam name="TMiddle">The intermediary output</typeparam>
	/// <typeparam name="TOutput">The type of final output to provide</typeparam>
	public abstract class ChainedOutputPipeline<TInput, TMiddle, TOutput> : Pipeline<TInput, TOutput>, IOutput<TMiddle>
	{
		/// <summary>
		/// The inner pipeline stage
		/// </summary>
		public abstract Pipeline<TMiddle, TOutput> Chain
		{
			get;
			set;
		}

		TMiddle IOutput<TMiddle>.Run()
		{
			return RunStage();
		}

		/// <summary>
		/// Gets the intermediary output to provide to the chained stage
		/// </summary>
		/// <returns>The intermediary output</returns>
		public abstract TMiddle RunStage();

		/// <summary>
		/// Executes the intermediary stage, and passes the result to the chained item
		/// </summary>
		/// <returns>The final output</returns>
		public override TOutput Run()
		{
			Chain.SetInput(RunStage());
			return Chain.Run();
		}
	}

	/// <summary>
	/// Base class for a chained pipeline that allows an intemediary stage between TInput and TMiddle
	/// </summary>
	/// <typeparam name="TInput">The type of input to accept</typeparam>
	/// <typeparam name="TMiddle">The intermediary output</typeparam>
	/// <typeparam name="TOutput">The type of final output to provide</typeparam>
	public abstract class ChainedInputPipeline<TInput, TMiddle, TOutput> : Pipeline<TInput, TOutput>, IInput<TMiddle>
	{
		/// <summary>
		/// The inner pipeline stage
		/// </summary>
		public abstract Pipeline<TInput, TMiddle> Chain
		{
			get;
			set;
		}

		void IInput<TMiddle>.SetInput(TMiddle input)
		{
			SetStageInput(input);
		}

		/// <summary>
		/// Directly sets the inner stage input
		/// </summary>
		/// <param name="input">The input to set</param>
		public abstract void SetStageInput(TMiddle input);

		/// <summary>
		/// Sets the raw input, passing it right away to the chained input stage
		/// </summary>
		/// <param name="input">The input to set</param>
		public override void SetInput(TInput input)
		{
			Chain.SetInput(input);
			SetStageInput(Chain.Run());
		}
	}
}