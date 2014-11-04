using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCompiler
{
	public interface IScopeModifier
	{
		void RunScopeModifier(StructurePass parent);
	}

	public abstract class StructurePass : Pipeline<IList<TokenInfo>, IList<TypeStructure>>
	{
		public abstract IList<Substring> Scope { get; }

		public abstract IList<Substring> Aliases { get; }

		public abstract TokenInfo this[int i] { get; }

		public virtual void Advance()
		{
			Advance(1);
		}

		public abstract void Advance(int i);

		public abstract void RegisterCloseBrace(Brace open);
	}
}
