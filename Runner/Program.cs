using OpenSoftware.DgmlTools.Model;
using ReferenceMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {

            string lib = @"C:\Source\VMLib\src\VMLib.sln";
            string lab = @"C:\Source\VMLab\src\VMLab with Wix Installer.sln";


            //var methods = new SolutionScanner().GetUnreferencedMethods(solutionPath);

            var members = SolutionScanner.GetMembers(@"C:\Source\Projects\VMLine2\src\VMLine2.sln").ToArray();//.Concat(SolutionScanner.GetMembers(lab)).ToArray();


            Console.WriteLine("parsed solutions");
            //var members = new List<Member>
            //{
            //    new Member{Name = "first", References = {"second", "third"}},
            //    new Member{Name = "second"},
            //    new Member{Name = "third"},
            //    new Member{Name = "fourth"}
            //};

            var names = new HashSet<string>(members.Select(m => m.Name));

            Console.WriteLine("members found");

            foreach (var mem in members)
            {
                foreach (var reference in mem.References)
                {
                    names.Remove(reference);
                }
            }

            Console.WriteLine("writing");

            var filename = @"..\..\names.txt";
            File.Delete(filename);
            File.WriteAllLines(filename, names);

            foreach (var name in names)
            {
                Console.WriteLine(name);
            }

            Console.ReadKey();
            //var graph = ReferenceGraph.References2Dgml(members.Where(m => m != null).ToList());
            //graph.WriteToFile(@"../../class-diagram.dgml");

            
        }
    }
}
