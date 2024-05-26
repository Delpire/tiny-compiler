using TinyCompiler;

namespace TinyCompiler.Tests
{
    public class LexerTests
    {
        [Theory]
        [InlineData("+", TokenType.Plus)]
        [InlineData("-", TokenType.Minus)]
        [InlineData("*", TokenType.Asterisk)]
        [InlineData("/", TokenType.Slash)]
        [InlineData(">", TokenType.GreaterThan)]
        [InlineData(">=", TokenType.GreaterThanOrEqual)]
        [InlineData("<", TokenType.LessThan)]
        [InlineData("<=", TokenType.LessThanOrEqual)]
        [InlineData("!=", TokenType.NotEq)]
        [InlineData("=", TokenType.EQ)]
        [InlineData("==", TokenType.EqEq)]
        [InlineData("3", TokenType.Number)]
        [InlineData("3.0", TokenType.Number)]
        [InlineData("ABC", TokenType.Ident)]
        [InlineData("LABEL", TokenType.Label)]
        [InlineData("GOTO", TokenType.Goto)]
        [InlineData("PRINT", TokenType.Print)]
        [InlineData("INPUT", TokenType.Input)]
        [InlineData("LET", TokenType.Let)]
        [InlineData("IF", TokenType.If)]
        [InlineData("THEN", TokenType.Then)]
        [InlineData("ENDIF", TokenType.Endif)]
        [InlineData("WHILE", TokenType.While)]
        [InlineData("REPEAT", TokenType.Repeat)]
        [InlineData("ENDWHILE", TokenType.EndWhile)]
        public void SimpleSource_LexterReturnsExpectedToken(string source, TokenType expectedKind)
        {
            Lexer lexer = new Lexer(source);
            var token = lexer.GetToken();

            Assert.Equal(source, token.Text);
            Assert.Equal(expectedKind, token.Kind);
        }

        [Fact]
        public void SourceContainsString_LexterReturnsExpectedToken()
        {
            string source = "\"mystring\"";
            string expectedText = "mystring";
            Lexer lexer = new Lexer(source);
            var token = lexer.GetToken();

            Assert.Equal(expectedText, token.Text);
            Assert.Equal(TokenType.String, token.Kind);
        }

        [Fact]
        public void SourceContainsMultipleTokens_LexerReturnsExpectedTokens()
        {
            string source = "+-* LET IF";
            Lexer lexer = new Lexer(source);

            Assert.Equal(TokenType.Plus, lexer.GetToken().Kind);
            Assert.Equal(TokenType.Minus, lexer.GetToken().Kind);
            Assert.Equal(TokenType.Asterisk, lexer.GetToken().Kind);
            Assert.Equal(TokenType.Let, lexer.GetToken().Kind);
            Assert.Equal(TokenType.If, lexer.GetToken().Kind);
        }

        [Fact]
        public void SourceContainsComments_LexerReturnsExpectedTokens()
        {
            string source = "+ - #TEST Comment -\n + -";
            Lexer lexer = new Lexer(source);
            int numberOfTokens = 0;
            while (lexer.GetToken().Kind != TokenType.EOF)
            {
                numberOfTokens++;
            }

            // Expected to find the first two tokens, but skip everything after the comment.
            // Then expected to be able to get the new line, two tokens, and EOF.
            Assert.Equal(6, numberOfTokens);
        }

        [Theory]
        [InlineData("!")]
        [InlineData("4.")]
        [InlineData("\"\r")]
        [InlineData("\"\n")]
        [InlineData("\"\t")]
        [InlineData("\"\\")]
        [InlineData("\"%")]
        [InlineData("\f")]
        public void InvalidCharacters_ThrowsFormatException(string source)
        {
            Lexer lexer = new Lexer(source);

            Assert.Throws<FormatException>(() => lexer.GetToken());
        }
    }
}