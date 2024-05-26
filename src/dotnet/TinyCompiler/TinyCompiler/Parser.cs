namespace TinyCompiler
{
    public class Parser
    {
        private Lexer _lexer;
        private Emitter _emitter;
        private Token _currentToken;
        private Token _peekToken;

        private HashSet<Token> _symbols = new();
        private HashSet<Token> _labelsDeclared = new();
        private HashSet<Token> _labelesGotoed = new();

        public Parser(Lexer lexer, Emitter emitter)
        {
            _lexer = lexer;
            _emitter = emitter;

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
            _emitter.HeaderLine("#include <stdio.h>");
            _emitter.HeaderLine("int main(void)");
            _emitter.HeaderLine("{");


            while (CheckToken(TokenType.Newline))
            {
                NextToken();
            }

            while (!CheckToken(TokenType.EOF))
            {
                Statement();
            }

            _emitter.EmitLine("return 0;");
            _emitter.EmitLine("}");

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
                NextToken();

                if (CheckToken(TokenType.String))
                {
                    _emitter.EmitLine($"printf(\"{_currentToken.Text}\\n\");");
                    NextToken();
                }
                else
                {
                    _emitter.Emit("printf(\"%.2f\\n\", (float)(");
                    Expression();
                    _emitter.EmitLine("));");
                }
            }
            // "IF" comparison "THEN" nl {statement} "ENDIF" nl
            else if (CheckToken(TokenType.If))
            {
                NextToken();
                _emitter.Emit("if (");
                Comparison();
                _emitter.EmitLine(")");
                _emitter.EmitLine("{");

                Match(TokenType.Then);
                NewLine();

                while (!CheckToken(TokenType.Endif))
                {
                    Statement();
                }

                Match(TokenType.Endif);
                _emitter.EmitLine("}");
            }
            // "WHILE" comparison "REPEAT" nl {statement nl} "ENDWHILE" nl
            else if (CheckToken(TokenType.While))
            {
                NextToken();
                _emitter.Emit("while (");
                Comparison();
                _emitter.EmitLine(")");
                _emitter.EmitLine("{");

                Match(TokenType.Repeat);
                NewLine();

                while (!CheckToken(TokenType.EndWhile))
                {
                    Statement();
                }

                Match(TokenType.EndWhile);
                _emitter.EmitLine("}");
            }
            // "LABEL" ident nl
            else if (CheckToken(TokenType.Label))
            {
                NextToken();

                if (_labelsDeclared.Contains(_currentToken))
                {
                    Abort($"Label already exists: {_currentToken.Text}");
                }

                _labelsDeclared.Add(_currentToken);

                _emitter.EmitLine($"{_currentToken.Text}:");

                Match(TokenType.Ident);
            }
            // "GOTO" ident nl
            else if (CheckToken(TokenType.Goto))
            {
                NextToken();
                _labelesGotoed.Add(_currentToken);
                _emitter.EmitLine($"goto {_currentToken.Text};");
                Match(TokenType.Ident);
            }
            // "LET" ident "=" expression nl
            else if (CheckToken(TokenType.Let))
            {
                NextToken();

                if (!_symbols.Contains(_currentToken))
                {
                    _emitter.HeaderLine($"float {_currentToken.Text};");
                    _symbols.Add(_currentToken);
                }


                _emitter.Emit($"{_currentToken.Text} = ");
                Match(TokenType.Ident);
                Match(TokenType.EQ);
                Expression();
                _emitter.EmitLine(";");
            }
            // "Input" ident nl
            else if (CheckToken(TokenType.Input))
            {
                NextToken();

                if (!_symbols.Contains(_currentToken))
                {
                    _symbols.Add(_currentToken);
                    _emitter.HeaderLine($"float {_currentToken.Text};");
                }

                _emitter.EmitLine($"if (0 == scanf(\"%f\", &{_currentToken.Text}))");
                _emitter.EmitLine("{");
                _emitter.EmitLine($"{_currentToken.Text} = 0;");
                _emitter.Emit("scanf(\"%");
                _emitter.EmitLine("*s\");");
                _emitter.EmitLine("}");


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
            Term();
            while (CheckToken(TokenType.Plus) || CheckToken(TokenType.Minus))
            {
                _emitter.Emit(_currentToken.Text);
                NextToken();
                Term();
            }
        }

        // comparison ::= expression (("==" | "!=" | ">" | ">=" | "<" | "<=")) expression
        public void Comparison()
        {
            Expression();

            if (IsComparisonOperator())
            {
                _emitter.Emit(_currentToken.Text);
                NextToken();
                Expression();
            }
            else
            {
                Abort($"Expected comparison at: {_currentToken.Text}");
            }

            while (IsComparisonOperator())
            {
                _emitter.Emit(_currentToken.Text);
                NextToken();
                Expression();
            }
        }

        // term ::= unary {("/" | "*") unary}
        public void Term()
        {
            Unary();
            while (CheckToken(TokenType.Slash) || CheckToken(TokenType.Asterisk))
            {
                _emitter.Emit(_currentToken.Text);
                NextToken();
                Unary();
            }
        }

        // unary ::= ["+" | "-"] primary
        public void Unary()
        {
            if(CheckToken(TokenType.Plus) || CheckToken(TokenType.Minus))
            {
                _emitter.Emit(_currentToken.Text);
                NextToken();
            }
            Primary();
        }

        public void Primary()
        {
            if (CheckToken(TokenType.Number))
            {
                _emitter.Emit(_currentToken.Text);
                NextToken();
            }
            else if (CheckToken(TokenType.Ident))
            {
                if (!_symbols.Contains(_currentToken))
                {
                    Abort($"Referencing variable before assignment: {_currentToken.Text}");
                }

                _emitter.Emit(_currentToken.Text);
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
            Match(TokenType.Newline);
            while (CheckToken(TokenType.Newline))
            {
                NextToken();
            }
        }
    }
}
