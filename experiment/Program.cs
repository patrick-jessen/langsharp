using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace experiment
{
    class Experiment
    {
        //static void Main(string[] args)
        //{
        //    var prog = new Program();

        //    // built in types are globally available
        //    var intType = new Type("int");            

        //    // Main package is the package in which compiler is run
        //    var mainPkg = new Package("main");
        //    prog.packages.Add(mainPkg);

        //    // mainPkg has imports at top (they are processed first)
        //    var libPkg = new Package("lib");
        //    prog.packages.Add(libPkg);

        //    // libPkg has a single function
        //    var addFn = new Function("add", new Function.Arg[] { new Function.Arg("arg1", intType), new Function.Arg("arg2", intType) }, intType);
        //    libPkg.functions.Add(addFn);
        //    // ... body of add is handled here

        //    // ... now we are done handling libPkg
        //    // ... lets return to mainPkg

        //    var mainFn = new Function("main", null, null);
        //    mainPkg.functions.Add(mainFn);

        //    // mainFn calls lib.add
        //    prog.GetPackage("lib").GetFunction("add");


        //    // Execute
        //    prog.GetPackage("main").GetFunction("main").Run();

        //    Console.ReadKey();
        //}
        
        static void Main()
        {
            Parser parser = new Parser("../../src/main.l");
            Console.WriteLine(parser.Parse());

            Console.ReadKey();
        }
    }

    class Program
    {
        public List<Package> packages = new List<Package>();

        public Package GetPackage(string name)
        {
            foreach(Package p in packages)
                if (p.name == name) return p;
            throw new Exception("No such package");
        }
    }

    // A package (defined by the folder structure)
    class Package
    {
        public bool undeclared;
        public string name;
        public List<string> imports = new List<string>();
        public List<Function> functions = new List<Function>();
        public List<Type> types = new List<Type>();

        public Package(string name, bool undeclared = false) { this.name = name; this.undeclared = undeclared; }

        public Function GetFunction(string name)
        {
            foreach (Function f in functions)
                if (f.name == name) return f;
            throw new Exception("no such function");
        }
    }

    class Function
    {
        public string name;
        Arg[] arguments;
        Type returns;

        public Function(string name, Arg[] args, Type ret)
        {
            this.name = name;
            this.arguments = args;
            this.returns = ret;
        }
        public void Run()
        {
            Console.Write("Running function " + name);
        }

        public class Arg
        {
            Type type;
            string name;
            public Arg(string name, Type t)
            {
                this.type = t;
                this.name = name;
            }
        }
    }
    class Type
    {
        public string name;

        public Type(string name)
        {
            this.name = name;
        }
    }
}
