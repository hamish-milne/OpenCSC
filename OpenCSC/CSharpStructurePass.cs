using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using OpenCompiler;

namespace OpenCSC
{
	public class Namespace : Keyword
	{
		public override Substring Value
		{
			get { return "namespace"; }
		}
	}
	
	public class Using : Keyword, IStructureItem
	{
		public override Substring Value
		{
			get { return "using"; }
		}

		public virtual void RunStructureItem(StructurePass parent)
		{
			int advanceby = 2;
			var name = parent[1].Item as Keyword;
			if (name == null)
				parent.AddError(new IdentifierExpected(parent[1]));
			else if (name is ReservedKeyword)
				parent.AddError(new UnexpectedKeyword(parent[1]));
			else if (!(parent[2].Item is Semicolon))
			{
				advanceby++;
				// If the third item is '=', it's an alias statement
				if (parent[2].Item is Equals)
				{
					var source = parent[3].Item as Keyword;
					if (source == null)
						parent.AddError(new IdentifierExpected(parent[3]));
					else if (source is ReservedKeyword)
						parent.AddError(new UnexpectedKeyword(parent[3]));
					else if (!(parent[4].Item is Semicolon))
						parent.AddError(new SemicolonExpected(parent[3]));
					else
					{
						parent.Aliases.Add(new Alias(source.Value, name.Value, 0, parent.Position + 5));
						advanceby++;
					}
				}
				else
					parent.AddError(new SemicolonExpected(parent[2]));
			}
			else
			{
				parent.Aliases.Add(new Alias(name.Value, "", 0, parent.Position + 3));
				advanceby++;
			}
			parent.Advance(advanceby);
		}
	}

	public class AliasKeyword : Keyword
	{
		public override Substring Value
		{
			get { return "alias"; }
		}
	}

	public class Extern : Keyword, IStructureItem
	{
		public override Substring Value
		{
			get { return "extern"; }
		}

		public virtual void RunStructureItem(StructurePass parent)
		{
			int advanceBy = 2;
			if (parent.PositionInScope > 0)
				parent.AddError(new ExternAliasError(parent[0]));
			if (parent[1].Item is AliasKeyword)
			{
				advanceBy++;
				var name = parent[2].Item as Keyword;
				if(name != null)
				{
					advanceBy++;
				}
			}
			else
				parent.AddError(null);
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
