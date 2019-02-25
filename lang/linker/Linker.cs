using lang.assembler;
using lang.linker.pe;

namespace lang.linker
{
    static class Linker {
        public enum Platform { Windows };

        public static void Link(Assembler a, Platform p, string file) {
            switch (p) {
                case Platform.Windows:
                    new PE(a).WriteFile(file);
                    break;
            }
        }
    }
}
