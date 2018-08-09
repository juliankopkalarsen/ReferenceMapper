using ReferenceMapper;
using System;
using System.Linq;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Reading...");

            var allMembers = args.SelectMany(a => SolutionScanner.GetMembers(a));

            Console.WriteLine("Parsing is done");

            var unreferenced = SolutionScanner.FindUnreferenced(allMembers);

            Console.WriteLine("Unreferenced Members:");

            foreach (var member in unreferenced)
            {
                Console.WriteLine(member.Name);
            }

            Console.Read();
        }
    }
}
