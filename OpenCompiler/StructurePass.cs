using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCompiler
{
	public interface IStructureItem
	{
		void RunStructureItem(StructurePass parent);
	}

	public struct Alias
	{
		public Substring Source, Destination;

		public Alias(Substring source, Substring destination)
		{
			Source = source;
			Destination = destination;
		}
	}

	public abstract class StructurePass : Pipeline<IList<TokenInfo>, IList<TypeStructure>>
	{
		public abstract IList<Substring> Scope { get; }

		public abstract IList<Alias> Aliases { get; }

		public abstract TokenInfo this[int i] { get; }

		public abstract int Position { get; }

		public virtual void Advance()
		{
			Advance(1);
		}

		public abstract void Advance(int i);

		public abstract void RegisterCloseBrace(Brace open);
	}
}
