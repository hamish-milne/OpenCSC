using OpenCompiler;
using System;

namespace OpenCSC
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var input = "public class // Something here until \n /* block comment \n multiline */ keyword // Now a line comment";
			var lexer = new DefaultLexer()
			{
				new Identifier(),
				new BlockBraceOpen(),
				new BlockBraceClose(),
				new ParenOpen(),
				new ParenClose(),
				new Semicolon(),
				new Ampersand(),
				new Pipe(),
				new AND(),
				new OR(),
				new Comma(),
				new LineComment(),
				new BlockComment()
			};
			lexer.SetInput(input);
			try
			{
				var result = lexer.Run();
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