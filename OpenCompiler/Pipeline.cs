using System;
using System.Collections.Generic;

namespace OpenCompiler
{
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
		public abstract string OutputLocation { get; }
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

	/// <summary>
	/// Base class for a pipeline stage, accepting input and providing output
	/// </summary>
	/// <typeparam name="TInput">The type of input to accept</typeparam>
	/// <typeparam name="TOutput">Thhe type of output to provide</typeparam>
	public abstract class Pipeline<TInput, TOutput> : IInput<TInput>, IOutput<TOutput>
	{
		public abstract CompilerOutput Output { get; set; }

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