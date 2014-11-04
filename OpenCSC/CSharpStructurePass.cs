using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using OpenCompiler;

namespace OpenCSC
{
	public abstract class ReservedKeyword : Keyword
	{
	}

	public class Using : Keyword, IScopeModifier
	{
		public override Substring Value
		{
			get { return "using"; }
		}

		public virtual void RunScopeModifier(StructurePass parent)
		{
			var name = parent[1].Item as Keyword;
			if (name == null)
				parent.AddError(new IdentifierExpected(parent[1]));
			else if (name is ReservedKeyword)
				parent.AddError(new UnexpectedKeyword(parent[1]));
			else
			{

			}
		}
	}

	public struct AssemblyInfo
	{
		public Assembly Assembly;
		public string Alias;
	}

	public class CSharpStructurePass : StructurePass
	{
		protected CompilerOutput output;
		protected IList<TokenInfo> input;

		public override void SetInput(IList<TokenInfo> input)
		{
			this.input = input;
		}

		public override CompilerOutput Output
		{
			get
			{
				if (output == null)
					output = new DefaultCompilerOutput();
				return output;
			}
			set { output = value; }
		}

		public override IList<TypeStructure> Run()
		{
			if (input == null)
				throw new InvalidOperationException("Input is null");
			
		}
	}
}
