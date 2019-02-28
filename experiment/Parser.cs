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
        public Parser(string fileName)
        {
            lexer = new Lexer(fileName);
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
            func.name = Consume(Token.Type.Identifier);
            Consume(Token.Type.ParentStart);
            Consume(Token.Type.ParentEnd);
            Consume(Token.Type.BraceStart);
            ParseFunctionBody(func);
            Consume(Token.Type.BraceEnd);
            return func;
        }

        public void ParseFunctionBody(ASTFunction fn)
        {
            var t = Consume();
            if(t.type == Token.Type.Identifier)
            {
                var t2 = Consume();
                if(t2.type == Token.Type.ParentStart)
                {
                    Consume(Token.Type.ParentEnd);
                    var fnCall = new ASTFunctionCall();
                    fnCall.name = t;
                    fn.body.Add(fnCall);
                }
            }
        }

        private Token Consume(Token.Type t)
        {
            Token tok = Consume();
            if (tok.type != t) throw new Exception("Expected something else");
            return tok;
        }
        private Token Consume() { return lexer.Lex(); }

    }

    class ASTFile
    {
        public List<ASTFunction> functions = new List<ASTFunction>();

        public override string ToString()
        {
            var fns = "";
            foreach (ASTFunction f in functions)
                fns += f.ToString(2) + "\n";

            return String.Format("ASTFile: {{\n{0}}}", fns);
        }
    }

    class ASTFunction
    {
        public Token name;
        public List<Expression> body = new List<Expression>();

        public string ToString(int indent)
        {
            var n = new String(' ', indent + 2) + name + "\n";
            var b = "";
            foreach(Expression exp in body)
                b += exp.ToString(indent+2) + "\n" + new String(' ', indent);

            return new string(' ', indent) + String.Format("ASTFunction: {{\n{0}{1}}}", n,b);
        }

    }

    interface Expression {
        string ToString(int indent);
    }

    class ASTFunctionCall : Expression
    {
        public Token name;
        public string ToString(int indent)
        {
            var n = new String(' ', indent + 2) + name + "\n" + new String(' ', indent);
            return new string(' ', indent) + String.Format("ASTFunctionCall: {{\n{0}}}", n);
        }
    }
}
