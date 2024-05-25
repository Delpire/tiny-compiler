using TinyCompiler;

Console.WriteLine("Tiny Compiler");

//if (args.Length != 2)
//{
//    Console.WriteLine("Error: Compiler needs a source file as an argument.");
//}

string source = File.ReadAllText("hello.tiny");

Lexer lexer = new Lexer(source);
Parser parser = new Parser(lexer);
try
{
    parser.Program();
}
catch (Exception ex)
{

}
Console.WriteLine("Parsing complete");
Console.ReadLine();