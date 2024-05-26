using TinyCompiler;

Console.WriteLine("Tiny Compiler");

//if (args.Length != 2)
//{
//    Console.WriteLine("Error: Compiler needs a source file as an argument.");
//}

string source = File.ReadAllText("hello.tiny");

try
{
    Lexer lexer = new Lexer(source);
    Emitter emitter = new Emitter("out.c");
    Parser parser = new Parser(lexer, emitter);
    parser.Program();
    emitter.WriteFile();
}
catch (Exception ex)
{

}
Console.WriteLine("Parsing complete");
Console.ReadLine();