using ReferenceMapper;
using System;
using System.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Runner
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            // Locate and register the default instance of MSBuild installed on this machine.
            MSBuildLocator.RegisterDefaults();

            // The test solution is copied to the output directory when you build this sample.
            var workspace = MSBuildWorkspace.Create();

            Console.WriteLine("Reading...");
            var allMembers = new List<Method>();

            foreach (var item in args)
            {
                var members = await SolutionScanner.GetMembers(item, workspace);
                allMembers.AddRange(members);
            }

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
