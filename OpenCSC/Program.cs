using OpenCompiler;
using System;

namespace OpenCSC
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var input = 
@"
#define somethingElse
test1
#if something
test2
# elif (somethingElse ) && something
test3
#else
test4
";
			var lexer = new DefaultLexer()
			{
				new Define(),
				new If(),
				new Else(),
				new Elif(),
				new Endif(),
				new Identifier(),
				new Hash(),
				new OR(),
				new AND(),
				new ParenOpen(),
				new ParenClose(),
			};
			lexer.SetInput(input);
			try
			{
				var result = lexer.Run();
				foreach (var item in result)
				{
					Console.WriteLine(item);
				}
				Console.WriteLine("===");
				var preproc = new Preprocessor();
				preproc.SetInput(result);
				result = preproc.Run();
				foreach (var item in result)
				{
					Console.WriteLine(item);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			Console.Read();
		}
	}
}