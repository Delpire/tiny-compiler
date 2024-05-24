using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyCompiler
{
    public enum TokenType
    {
        EOF,
        Newline,
        Number,
        Ident,
        String,
        Label,
        Goto,
        Print,
        Input,
        Let,
        If,
        Then,
        Endif,
        While,
        Repeat,
        EndWhile,
        EQ,
        Plus,
        Minus,
        Asterisk,
        Slash,
        EqEq,
        NotEq,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
    }

    public class Token
    {
        private static Dictionary<string, TokenType> _keywords = new()
        {
            { "LABEL", TokenType.Label },
            { "GOTO", TokenType.Goto },
            { "PRINT", TokenType.Print },
            { "INPUT", TokenType.Input },
            { "LET", TokenType.Let },
            { "IF", TokenType.If },
            { "THEN", TokenType.Then },
            { "ENDIF", TokenType.Endif },
            { "WHILE", TokenType.While },
            { "REPEAT", TokenType.Repeat },
            { "ENDWHILE", TokenType.EndWhile },
        };

        public string Text { get; }
        public TokenType Kind { get; }

        public Token(char text, TokenType kind) : this(text.ToString(), kind) { }

        public Token(string text, TokenType kind)
        { 
            Text = text;
            Kind = kind;
        }

        public static bool TryGetKeyWord(string text, out TokenType tokenType)
        {
            return _keywords.TryGetValue(text.ToUpper(), out tokenType);
        }
    }
}
