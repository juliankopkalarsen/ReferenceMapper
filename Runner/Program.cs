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

            var allMembers = args.SelectMany(a => SolutionScanner.GetMembers(a)).ToList();

            Console.WriteLine("Parsing is done");

            var unreferenced = SolutionScanner.FindUnreferenced(allMembers, Roots.MainMethods.And(Roots.TestRelated));

            Console.WriteLine("Unreferenced Members:");

            foreach (var member in unreferenced)
            {
                Console.WriteLine(member.Name);
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine(unreferenced.Count + " unreferenced members");

            Console.Read();
        }
    }
}
