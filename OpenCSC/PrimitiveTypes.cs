using System;
using System.Collections.Generic;
using System.Text;
using OpenCompiler;

namespace OpenCSC
{
	public abstract class PrimitiveType : Keyword
	{
		public abstract Type Type { get; }
	}

	public class VoidKeyword : PrimitiveType
	{
		public override Type Type
		{
			get { return typeof(void); }
		}

		public override Substring Value
		{
			get { return "void"; }
		}
	}

	public class Int32Keyword : PrimitiveType
	{
		public override Type Type
		{
			get { return typeof(int); }
		}

		public override Substring Value
		{
			get { return "int"; }
		}
	}

	public class Int64Keyword : PrimitiveType
	{
		public override Type Type
		{
			get { return typeof(long); }
		}

		public override Substring Value
		{
			get { return "long"; }
		}
	}

	public class UInt32Keyword : PrimitiveType
	{
		public override Type Type
		{
			get { return typeof(uint); }
		}

		public override Substring Value
		{
			get { return "uint"; }
		}
	}

	public class UInt64Keyword : PrimitiveType
	{
		public override Type Type
		{
			get { return typeof(ulong); }
		}

		public override Substring Value
		{
			get { return "ulong"; }
		}
	}

	public class ByteKeyword : PrimitiveType
	{
		public override Type Type
		{
			get { return typeof(byte); }
		}

		public override Substring Value
		{
			get { return "byte"; }
		}
	}

	public class SByteKeyword : PrimitiveType
	{
		public override Type Type
		{
			get { return typeof(sbyte); }
		}

		public override Substring Value
		{
			get { return "sbyte"; }
		}
	}

	public class SingleKeyword : PrimitiveType
	{
		public override Type Type
		{
			get { return typeof(float); }
		}

		public override Substring Value
		{
			get { return "float"; }
		}
	}

	public class DoubleKeyword : PrimitiveType
	{
		public override Type Type
		{
			get { return typeof(double); }
		}

		public override Substring Value
		{
			get { return "double"; }
		}
	}
}
