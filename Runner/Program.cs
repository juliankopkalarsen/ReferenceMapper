using ReferenceMapper;
using System;
using System.Linq;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var libMembers = SolutionScanner.GetMembers(@"..\..\..\TestClassLibrary\TestClassLibrary.csproj");
            var appMembers = SolutionScanner.GetMembers(@"..\..\..\TestConsoleApp\TestConsoleApp.csproj");

            foreach (var member in libMembers.Concat(appMembers))
            {
                Console.WriteLine(member.Name);
            }

            Console.WriteLine("parsed solutions");
            Console.Read();
            //var members = new List<Member>
            //{
            //    new Member{Name = "first", References = {"second", "third"}},
            //    new Member{Name = "second"},
            //    new Member{Name = "third"},
            //    new Member{Name = "fourth"}
            //};

            //var names = new HashSet<string>(members.Select(m => m.Name));

            //Console.WriteLine("members found");

            //foreach (var mem in members)
            //{
            //    foreach (var reference in mem.References)
            //    {
            //        names.Remove(reference);
            //    }
            //}

            //Console.WriteLine("writing");

            //var filename = @"..\..\names.txt";
            //File.Delete(filename);
            //File.WriteAllLines(filename, names);

            //foreach (var name in names)
            //{
            //    Console.WriteLine(name);
            //}

            //Console.ReadKey();
            ////var graph = ReferenceGraph.References2Dgml(members.Where(m => m != null).ToList());
            //graph.WriteToFile(@"../../class-diagram.dgml");

            
        }
    }
}
