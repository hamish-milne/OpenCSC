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
		public int Scope, Position;

		public Alias(Substring source, Substring destination, int scope, int position)
		{
			Source = source;
			Destination = destination;
			Scope = scope;
			Position = position;
		}
	}

	public struct ScopeItem
	{
		public Keyword Keyword;
		public Substring Name;
		public int Position;

		public ScopeItem(Keyword keyword, Substring name, int position)
		{
			Keyword = keyword;
			Name = name;
			Position = position;
		}
	}

	public abstract class StructurePass : Pipeline<IList<TokenInfo>, IList<TypeStructure>>
	{
		public abstract IList<ScopeItem> Scope { get; }

		public abstract IList<Alias> Aliases { get; }

		public abstract TokenInfo this[int i] { get; }

		public abstract int Position { get; }

		public virtual int PositionInScope
		{
			get
			{
				if (Scope.Count < 1)
					return Position;
				return Position - Scope[Scope.Count - 1].Position;
			}
		}

		public virtual void Advance()
		{
			Advance(1);
		}

		public abstract void Advance(int i);

		public abstract void RegisterCloseBrace(Brace open);
	}
}
