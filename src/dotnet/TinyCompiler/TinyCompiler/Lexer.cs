using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TinyCompiler
{
    public class Lexer
    {
        private string _source;
        private char _currentChar;
        private int _currentCharIndex = -1;

        public const char EndOfFile = '\0';

        public char CurrentChar { get => _currentChar; }

        public Lexer(string source)
        {
            _source = source + Environment.NewLine;
            NextChar();
        }

        public void NextChar()
        {
            _currentCharIndex++;
            if (_currentCharIndex >= _source.Length)
            {
                _currentChar = EndOfFile;
            }
            else
            {
                _currentChar = _source[_currentCharIndex];
            }
        }

        public char Peek()
        {
            if (_currentCharIndex + 1 >= _source.Length)
            {
                return EndOfFile;
            }

            return _source[_currentCharIndex + 1];
        }

        public void Abort(string message)
        {
            Console.WriteLine($"Lexing error: {message}");
        }

        public void SkipWhitespace()
        {
            while (CurrentChar == ' ' || CurrentChar == '\t' || CurrentChar == '\r')
            {
                NextChar();
            }
        }

        public void SkipComment()
        {
            if (CurrentChar != '#')
            {
                return;
            }

            while (CurrentChar != '\n')
            {
                NextChar();
            }
        }

        public Token GetToken()
        {
            SkipWhitespace();
            SkipComment();

            Token token;
            switch (CurrentChar)
            {
                case '+':
                    token = new Token(CurrentChar, TokenType.Plus);
                    break;
                case '-':
                    token = new Token(CurrentChar, TokenType.Minus);
                    break;
                case '*':
                    token = new Token(CurrentChar, TokenType.Asterisk);
                    break;
                case '/':
                    token = new Token(CurrentChar, TokenType.Slash);
                    break;
                case '\n':
                    token = new Token(CurrentChar, TokenType.Newline);
                    break;
                case '\0':
                    token = new Token(CurrentChar, TokenType.EOF);
                    break;
                case '=':

                    if (Peek() == '=')
                    {
                        var previous = CurrentChar;
                        NextChar();
                        token = new Token($"{previous}{CurrentChar}", TokenType.EqEq);
                    }
                    else
                    {
                        token = new Token(CurrentChar, TokenType.EQ);
                    }

                    break;
                case '>':

                    if (Peek() == '=')
                    {
                        var previous = CurrentChar;
                        NextChar();
                        token = new Token($"{previous}{CurrentChar}", TokenType.GreaterThanOrEqual);
                    }
                    else
                    {
                        token = new Token(CurrentChar, TokenType.GreaterThan);
                    }

                    break;
                case '<':

                    if (Peek() == '=')
                    {
                        var previous = CurrentChar;
                        NextChar();
                        token = new Token($"{previous}{CurrentChar}", TokenType.LessThanOrEqual);
                    }
                    else
                    {
                        token = new Token(CurrentChar, TokenType.LessThan);
                    }

                    break;
                case '!':

                    if (Peek() == '=')
                    {
                        var previous = CurrentChar;
                        NextChar();
                        token = new Token($"{previous}{CurrentChar}", TokenType.NotEq);
                    }
                    else
                    {
                        Abort($"Expected !=. Got !{Peek()}");
                        throw new FormatException("");
                    }

                    break;
                case '\"':
                    NextChar();
                    var startIndex = _currentCharIndex;

                    while (CurrentChar != '\"')
                    {
                        if (CurrentChar == '\r' ||  CurrentChar == '\n' || CurrentChar == '\t' || CurrentChar == '\\' || CurrentChar == '%')
                        {
                            Abort($"Illegal character in string: {CurrentChar}");
                            throw new FormatException("");
                        }
                        NextChar();
                    }

                    string text = _source.Substring(startIndex, _currentCharIndex - startIndex);
                    token = new Token(text, TokenType.String);

                    break;

                default:

                    if (Char.IsNumber(CurrentChar))
                    {
                        var numberStartIndex = _currentCharIndex;

                        while (Char.IsNumber(Peek()))
                        {
                            NextChar();
                        }

                        if (Peek() == '.')
                        {
                            NextChar();

                            if (!Char.IsNumber(Peek()))
                            {
                                Abort($"Numbers with a decimal must have a digit following the period.");
                                throw new FormatException("");
                            }

                            while (Char.IsNumber(Peek()))
                            {
                                NextChar();
                            }
                        }

                        string number = _source.Substring(numberStartIndex, _currentCharIndex - numberStartIndex);
                        token = new Token(number, TokenType.Number);

                    }
                    else if (Char.IsLetter(CurrentChar))
                    {
                        var identOrKeyWordStartIndex = _currentCharIndex;

                        while (Char.IsLetter(CurrentChar))
                        {
                            NextChar();
                        }

                        string identOrKeyWord = _source.Substring(identOrKeyWordStartIndex, _currentCharIndex - identOrKeyWordStartIndex);

                        if (Token.TryGetKeyWord(identOrKeyWord, out TokenType tokenType))
                        {
                            token = new Token(identOrKeyWord, tokenType);
                        }
                        else
                        {
                            token = new Token(identOrKeyWord, TokenType.Ident);
                        }
                    }
                    else
                    {
                        Abort($"Unknown token: '{CurrentChar}'");
                        throw new FormatException("");
                    }

                    break;
            }

            NextChar();
            return token;
        }
    }
}
