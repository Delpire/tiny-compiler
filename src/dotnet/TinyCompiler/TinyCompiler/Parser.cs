namespace TinyCompiler
{
    public class Parser
    {
        private Lexer _lexer;
        private Token _currentToken;
        private Token _peekToken;

        private HashSet<Token> _symbols = new();
        private HashSet<Token> _labelsDeclared = new();
        private HashSet<Token> _labelesGotoed = new();

        public Parser(Lexer lexer)
        {
            _lexer = lexer;

            NextToken();
            NextToken();
        }

        public bool CheckToken(TokenType kind)
        {
            return kind == _currentToken.Kind;
        }

        public bool CheckPeek(TokenType kind)
        {
            return kind == _peekToken.Kind;
        }

        public void Match(TokenType kind)
        {
            if (!CheckToken(kind))
            {
                Abort($"Expected: '{kind}'. Recieved: {_currentToken.Kind}");
            }

            NextToken();
        }

        public void NextToken()
        {
            _currentToken = _peekToken;
            _peekToken = _lexer.GetToken();
        }

        public void Abort(string message)
        {
            Console.WriteLine($"Parsing error: {message}");
            throw new FormatException(message);
        }

        // program ::= {statement}
        public void Program()
        {
            Console.WriteLine("PROGRAM");

            while (CheckToken(TokenType.Newline))
            {
                NextToken();
            }

            while (!CheckToken(TokenType.EOF))
            {
                Statement();
            }

            foreach (var label in _labelesGotoed)
            {
                if (!_labelsDeclared.Contains(label))
                {
                    Abort($"Attempting to GOTO undeclared label: {label.Text}");
                }
            }
        }

        public void Statement()
        {
            // "PRINT" (expression | string) nl
            if (CheckToken(TokenType.Print))
            {
                Console.WriteLine("STATEMENT-PRINT");
                NextToken();

                if (CheckToken(TokenType.String))
                {
                    NextToken();
                }
                else
                {
                    Expression();
                }
            }
            // "IF" comparison "THEN" nl {statement} "ENDIF" nl
            else if (CheckToken(TokenType.If))
            {
                Console.WriteLine("STATEMENT-IF");
                NextToken();
                Comparison();

                Match(TokenType.Then);
                NewLine();

                while (!CheckToken(TokenType.Endif))
                {
                    Statement();
                }

                Match(TokenType.Endif);
            }
            // "WHILE" comparison "REPEAT" nl {statement nl} "ENDWHILE" nl
            else if (CheckToken(TokenType.While))
            {
                Console.WriteLine("STATEMENT-WHILE");

                NextToken();
                Comparison();

                Match(TokenType.Repeat);
                NewLine();

                while (!CheckToken(TokenType.EndWhile))
                {
                    Statement();
                }

                Match(TokenType.EndWhile);
            }
            // "LABEL" ident nl
            else if (CheckToken(TokenType.Label))
            {
                Console.WriteLine("STATEMENT-LABEL");

                NextToken();

                if (_labelsDeclared.Contains(_currentToken))
                {
                    Abort($"Label already exists: {_currentToken.Text}");
                }

                _labelsDeclared.Add(_currentToken);

                Match(TokenType.Ident);
            }
            // "GOTO" ident nl
            else if (CheckToken(TokenType.Goto))
            {
                Console.WriteLine("STATEMENT-GOTO");
                NextToken();
                _labelesGotoed.Add(_currentToken);
                Match(TokenType.Ident);
            }
            // "LET" ident "=" expression nl
            else if (CheckToken(TokenType.Let))
            {
                Console.WriteLine("STATEMENT-LET");

                NextToken();
                if (_symbols.Contains(_currentToken))
                {
                    _symbols.Add(_currentToken);
                }

                Match(TokenType.Ident);
                Match(TokenType.EQ);
                Expression();
            }
            // "Input" ident nl
            else if (CheckToken(TokenType.Input))
            {
                Console.WriteLine("STATEMENT-INPUT");

                NextToken();
                if (_symbols.Contains(_currentToken))
                {
                    _symbols.Add(_currentToken);
                }

                Match(TokenType.Ident);
            }
            else
            {
                Abort($"Invalid statement at {_currentToken.Text} ({_currentToken.Kind})");
            }



            NewLine();
        }

        // expression ::= term {("-" | "+") term}
        public void Expression()
        {
            Console.WriteLine("EXPRESSION");

            Term();
            while (CheckToken(TokenType.Plus) || CheckToken(TokenType.Minus))
            {
                NextToken();
                Term();
            }
        }

        // comparison ::= expression (("==" | "!=" | ">" | ">=" | "<" | "<=")) expression
        public void Comparison()
        {
            Console.WriteLine("COMPARISON");
            Expression();

            if (IsComparisonOperator())
            {
                NextToken();
                Expression();
            }
            else
            {
                Abort($"Expected comparison at: {_currentToken.Text}");
            }

            while (IsComparisonOperator())
            {
                NextToken();
                Expression();
            }
        }

        // term ::= unary {("/" | "*") unary}
        public void Term()
        {
            Console.WriteLine("TERM");

            Unary();
            while (CheckToken(TokenType.Slash) || CheckToken(TokenType.Asterisk))
            {
                NextToken();
                Unary();
            }
        }

        // unary ::= ["+" | "-"] primary
        public void Unary()
        {
            Console.WriteLine("UNARY");

            if(CheckToken(TokenType.Plus) || CheckToken(TokenType.Minus))
            {
                NextToken();
            }
            Primary();
        }

        public void Primary()
        {
            Console.WriteLine($"PRIMARY ({_currentToken.Text})");

            if (CheckToken(TokenType.Number))
            {
                NextToken();
            }
            else if (CheckToken(TokenType.Ident))
            {
                if (!_symbols.Contains(_currentToken))
                {
                    Abort($"Referencing variable before assignment: {_currentToken.Text}");
                }

                NextToken();
            }
            else
            {
                Abort($"Unexpected token at {_currentToken.Text}");
            }
        }

        public bool IsComparisonOperator()
        {
            return CheckToken(TokenType.EqEq) 
                || CheckToken(TokenType.NotEq) 
                || CheckToken(TokenType.GreaterThan) 
                || CheckToken(TokenType.GreaterThanOrEqual)
                || CheckToken(TokenType.LessThan)
                || CheckToken(TokenType.LessThanOrEqual);
        }

        public void NewLine()
        {
            Console.WriteLine("NEWLINE");
            Match(TokenType.Newline);
            while (CheckToken(TokenType.Newline))
            {
                NextToken();
            }
        }
    }
}
