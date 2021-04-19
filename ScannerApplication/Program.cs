using AllanMilne.Ardkit;
using System;
using System.Collections.Generic;
using System.IO;

namespace PALLanguageCompiler
{
    class Program
    {
        static void Main(String[] args)
        {

            //if (args.Length != 1)
            //{
            //    Console.WriteLine("Invalid usage: Block1 <filename>");
            //    return;
            //}

            //String filePath = args[0];
            String filePath = "C:\\Users\\jacka\\Google Drive\\Uni work\\CMP409 -Languages and Compilers\\assessment\\testPrograms\\Errors\\6-5.txt";

            //prologue(filePath);

            StreamReader infile = null;
            try
            {
                infile = new StreamReader(filePath);
            }
            catch (IOException e)
            {
                Console.WriteLine("I.O error opening file '{0}'. ", filePath);
                Console.WriteLine(e);
                return;
            }

            List<ICompilerError> errors = new List<ICompilerError>();


            PALParser parser = new PALParser();
            parser.Parse(infile);

            foreach (CompilerError err in parser.Errors)
            {
                Console.WriteLine(err);
            }

            if (parser.Errors.Count != 0)
            {
                Console.WriteLine("{0} errors found.", parser.Errors.Count);
            }


            try
            {
                infile.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine("Error closing file '{0}'. ", filePath);
                Console.WriteLine(e);
                return;
            }


        }
    }
}
