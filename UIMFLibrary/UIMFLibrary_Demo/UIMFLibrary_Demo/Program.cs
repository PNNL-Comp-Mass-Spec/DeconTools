using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIMFLibrary_Demo
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Please provide the path to a UIMF file.");
                Console.ReadKey();
                return;
            }

            DemoRunner runner = new DemoRunner(args[0]);
            runner.Execute();

            Console.ReadKey();

        }
    }
}
