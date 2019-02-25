using lang.linker.pe;
using lang.program;

namespace lang.linker
{
    static class Linker {
        public enum Platform { Windows };

        public static void Link(Program prog, Platform p, string file) {
            switch (p) {
                case Platform.Windows:
                    new PE(prog).WriteFile(file);
                    break;
            }
        }
    }
}
