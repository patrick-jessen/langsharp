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
        private const char EOFChar = (char)0; // Character value which is used to represent End Of File.
        private string fileName;    // File which is being lexed
        private string source;      // Contents of the file
        private int index;          // Current index into source
        private int lineNo = 1;     // Current line number
        private int lineStartIndex; // The index at which the current line 

        private Location tokenStart;// The start location of the token currently being evaluated
        private string tokenValue;  // Value of the token currently being evaluated

        public Lexer(string fileName)
        {
            this.fileName = fileName;
            source = File.ReadAllText(fileName);
        }

        public Token Lex()
        {
            while (true) {
                tokenStart = GetLocation();
                tokenValue = "";

                var c = Consume();
                if (c == EOFChar) return MakeToken(Token.Type.EOF);
                if (Char.IsLetter(c))
                {
                    while (Char.IsLetter(Peek())) Consume();
                    if (IsKeyword()) return MakeToken(Token.Type.Keyword);
                    return MakeToken(Token.Type.Identifier);
                }

                if (Char.IsWhiteSpace(c))
                {
                    if (c == '\n')
                    {
                        lineNo++;
                        lineStartIndex = index;
                    }
                    continue;
                }

                switch(c)
                {
                    case '{': return MakeToken(Token.Type.BraceStart);
                    case '}': return MakeToken(Token.Type.BraceEnd);
                    case '(': return MakeToken(Token.Type.ParentStart);
                    case ')': return MakeToken(Token.Type.ParentEnd);
                }

                throw new LexerException(tokenStart, $"unexpected token '{tokenValue}'");
            }
        }

        private char Peek()
        {
            if (EOF()) return EOFChar;
            return source[index];
        }
        private char Consume()
        {
            var c = Peek();
            tokenValue += c;
            index++;
            return c;
        }
        private Location GetLocation()
        {
            return new Location(fileName, lineNo, index-lineStartIndex);
        }

        private bool IsKeyword()
        {
            switch(tokenValue)
            {
                case "func":    return true;
                case "return":  return true;
            }
            return false;
        }
        public bool EOF()
        {
            return index == source.Length;
        }

        private Token MakeToken(Token.Type t)
        {
            var end = GetLocation();
            end.columnNo += 1;
            return new Token(tokenStart, end, t, tokenValue);
        }
    }

    class Location
    {
        public string file;
        public int lineNo;
        public int columnNo;

        public Location(string file, int lineNo, int columnNo)
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
        public enum Type { Keyword, Identifier, ParentStart, ParentEnd, BraceStart, BraceEnd, EOF };
        public Type type;
        public string value;
        public Location start;
        public Location end;

        public Token(Location start, Location end, Type type, string value = "")
        {
            this.start = start;
            this.end = end;
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

    class LexerException : Exception
    {
        public Location location;
        public LexerException(Location location, string msg) : base(msg)
        {
            this.location = location;
        }
        public override string Message => String.Format("{0} at {1}", base.Message, location);
    }
}
