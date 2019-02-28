using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace experiment
{
    class Lexer
    {
        private string fileName;
        private string source;
        private string value;
        private int iter;
        private int lineNo = 1;
        private int lineStart;

        public Lexer(string fileName)
        {
            this.fileName = fileName;
            source = File.ReadAllText(fileName);
        }

        public Token Lex()
        {
            while (!EOF()) {
                value = "";

                var n = Peek();
                if (Char.IsLetter(n))
                {
                    while (Char.IsLetter(Peek())) Consume();
                    if (IsKeyword()) return new Token(Loc(), Token.Type.Keyword, value);
                    return new Token(Loc(), Token.Type.Identifier, value);
                }

                if (Char.IsWhiteSpace(n))
                {
                    Consume();
                    if (n == '\n')
                    {
                        lineNo++;
                        lineStart = iter;
                    }
                    continue;
                }

                switch(n)
                {
                    case '{': Consume(); return new Token(Loc(), Token.Type.BraceStart);
                    case '}': Consume(); return new Token(Loc(), Token.Type.BraceEnd);
                    case '(': Consume(); return new Token(Loc(), Token.Type.ParentStart);
                    case ')': Consume(); return new Token(Loc(), Token.Type.ParentEnd);
                }

                throw new Exception("Unknown token: " + n);
            }

            throw new Exception("End of file reached");
        }

        private char Peek()     { return source[iter]; }
        private void Consume()  { value += source[iter++]; }
        private Loc Loc()       { return new Loc(fileName, lineNo, iter-lineStart); }

        private bool IsKeyword()
        {
            switch(value)
            {
                case "func": return true;
            }
            return false;
        }
        public bool EOF()
        {
            return iter == source.Length;
        }
    }

    class Loc
    {
        public string file;
        public int lineNo;
        public int columnNo;

        public Loc(string file, int lineNo, int columnNo)
        {
            this.file = file;
            this.lineNo = lineNo;
            this.columnNo = columnNo;
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}:{2}", file, lineNo, columnNo);
        }
    }

    class Token
    {
        public enum Type { Keyword, Identifier, ParentStart, ParentEnd, BraceStart, BraceEnd };
        public Type type;
        public string value;
        public Loc loc;

        public Token(Loc loc, Type type, string value = "")
        {
            this.loc = loc;
            this.type = type;
            this.value = value;
        }

        public override string ToString()
        {
            var typeStr = Enum.GetName(typeof(Type), type);
            if (value.Length > 0)
                return String.Format("[{0}:{1}]", typeStr, value);
            return String.Format("[{0}]", typeStr);
        }
    }
}
