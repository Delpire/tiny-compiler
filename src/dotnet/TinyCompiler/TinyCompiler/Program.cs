// See https://aka.ms/new-console-template for more information
using TinyCompiler;

Console.WriteLine("Hello, World!");

string source = "IF+-123 foo*THEN/";
Lexer lexer = new Lexer(source);

var token = lexer.GetToken();
while (token.Kind != TokenType.EOF)
{
    Console.WriteLine(token.Kind);
    Console.WriteLine(token.Text);
    token = lexer.GetToken();
}