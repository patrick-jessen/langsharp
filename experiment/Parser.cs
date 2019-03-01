using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace experiment
{
    class Parser
    {
        private Lexer lexer;
        private Token prevToken;
        private Token nextToken;

        public Parser(string fileName)
        {
            lexer = new Lexer(fileName);
            nextToken = lexer.Lex();
        }

        public ASTFile Parse()
        {
            ASTFile ast = new ASTFile();
            var t = Consume();

            if(t.type == Token.Type.Keyword)
            {
                if(t.value == "func")
                    ast.functions.Add(ParseFunction());
            }

            return ast;
        }

        public ASTFunction ParseFunction()
        {
            ASTFunction func = new ASTFunction();
            func.identifer = Consume(Token.Type.Identifier);
            Consume(Token.Type.ParentStart);
            Consume(Token.Type.ParentEnd);
            Consume(Token.Type.BraceStart);
            ParseFunctionBody(func);
            Consume(Token.Type.BraceEnd);
            return func;
        }

        public void ParseFunctionBody(ASTFunction fn)
        {
            while(Peek().type != Token.Type.BraceEnd)
            {
                var stmt = ParseStatement();
                fn.body.Add(stmt);
            }
        }

        private ASTStatement ParseStatement()
        {
            var t = Peek();
            if (t.type == Token.Type.Identifier)
            {
                var ident = Consume();
                var t2 = Consume(Token.Type.ParentStart);
                Consume(Token.Type.ParentEnd);
                var fnCall = new ASTFunctionCall();
                fnCall.name = t;
                return fnCall;
            }
            else if (t.type == Token.Type.Keyword)
            {
                switch (t.value)
                {
                    case "return":
                        Consume();
                        return new ASTReturn();
                }
            }
            throw new ParserException(t.start, "expected statement");
        }

        private Token Consume(Token.Type t)
        {
            Token tok = Peek();
            if (tok.type != t)
                throw new ParserException(prevToken.end, String.Format("expected '{0}'", t));
            return Consume();
        }
        private Token Consume()
        {
            prevToken = nextToken;
            nextToken = lexer.Lex();
            return prevToken;
        }
        private Token Peek() { return nextToken; }

        public static string Indent(int indent, string str)
        {
            string end = "";
            if (str.EndsWith("\n"))
            {
                end = "\n";
                str = str.TrimEnd('\n');
            }
            var indentStr = new String(' ', indent * 2);
            var separator = "\n" + indentStr;
            return indentStr + String.Join(separator, str.Split('\n')) + end;
        }
    }

    class ParserException : Exception
    {
        public Location location;
        public string message;

        public ParserException(Location location, string message)
        {
            this.location = location;
            this.message = message;
        }
        public override string Message => String.Format("{0} at {1}", message, location);
    }

    class ASTFile
    {
        public List<ASTFunction> functions = new List<ASTFunction>();

        public override string ToString()
        {
            var fns = "";
            foreach (ASTFunction f in functions)
                fns += f.ToString() + "\n";

            return String.Format("ASTFile: {{\n{0}}}", Parser.Indent(1, fns));
        }
    }

    class ASTFunction
    {
        public Token identifer;
        public List<ASTStatement> body = new List<ASTStatement>();

        public override string ToString()
        {
            var i = Parser.Indent(1, String.Format("identifier: {0}", identifer.value));

            var b = "";
            foreach (ASTStatement exp in body)
                b += exp.ToString() + "\n";

            b = Parser.Indent(1, String.Format("body: [\n{0}]", Parser.Indent(1, b)));
            return String.Format("ASTFunction: {{\n{0}\n{1}\n}}", i, b);
        }

    }

    interface ASTNode { }
    interface ASTStatement : ASTNode { }
    interface Expression : ASTNode { }

    class ASTFunctionCall : ASTStatement, Expression
    {
        public Token name;
        public override string ToString()
        {
            var n = Parser.Indent(1, String.Format("identifier: {0}", name.value));
            return String.Format("ASTFunctionCall: {{\n{0}\n}}", n);
        }
    }

    class ASTReturn : ASTStatement
    {
        public Expression expression;
        public override string ToString()
        {
            var e = "";
            if(expression != null)
                expression.ToString();
            return String.Format("ASTReturn: {{\n{0}}}", e);
        }
    }
}
