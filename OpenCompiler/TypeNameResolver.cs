using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;

namespace OpenCompiler
{
	public abstract class TypeNameResolver
	{
		public abstract Type GetType(Substring fullName);
	}

	public struct ResolveType
	{
		public Substring FullName;
		public Type Type;
	}

	public abstract class MemberStructure
	{
		protected TypeNameResolver resolver;
		protected Substring name;
		protected IList<Keyword> modifiers;

		public virtual TypeNameResolver Resolver
		{
			get { return resolver; }
		}

		public virtual IList<Keyword> Modifiers
		{
			get
			{
				if (modifiers == null)
					modifiers = new List<Keyword>();
				return modifiers;
			}
		}

		public virtual Substring Name
		{
			get { return name; }
			set { name = value; }
		}

		public abstract void Update();
	}

	public abstract class TypeStructure : MemberStructure
	{
		public abstract IList<MemberStructure> Members { get; }
		public abstract Type RealType { get; set; }
	}

	public abstract class FieldStructure : MemberStructure
	{
		public abstract ResolveType Type { get; set; }
	}

	public abstract class PropertyStructure : MemberStructure
	{
		public abstract ResolveType Type { get; }
		public abstract IList<TokenInfo> Body { get; }
	}

	public abstract class MethodStructure : MemberStructure
	{
		public abstract ResolveType ReturnType { get; set; }
		public abstract IList<ParamStructure> Params { get; }
		public abstract IList<TokenInfo> Body { get; }
	}

	public abstract class ParamStructure : MemberStructure
	{
		public abstract ResolveType Type { get; set; }
	}
}
