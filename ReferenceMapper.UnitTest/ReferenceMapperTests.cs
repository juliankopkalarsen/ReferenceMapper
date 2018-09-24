using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using NUnit.Framework;
using System.Linq;

namespace ReferenceMapper.UnitTest
{
    public class ReferenceMapperTests
    {
        [Test]
        public async void Get_AClass_Members()
        {
            var libMembers = (await SolutionScanner.GetMembers(@"..\..\..\TestClassLibrary\TestClassLibrary.csproj"))
                            .ToList();

            Assert.Contains(new Method
            {
                Name= "void TestClassLibrary.AClass.AMethod(int)"
            },
            libMembers);

            Assert.Contains(new Method
            {
                Name = "void TestClassLibrary.AClass.UnreferencedMethod()"
            },
            libMembers);

            Assert.Contains(new Method
            {
                Name = "void TestClassLibrary.AClass.IndirectlyReferencedMethod()"
            },
            libMembers);

            Assert.Contains(new Method
            {
                Name = "void TestClassLibrary.AClass.IndirectlyUnreferencedMethod()"
            },
            libMembers);

            Assert.Contains(new Method
            {
                Name = "void TestClassLibrary.AClass.OnlyReferencedByTestMethod()"
            },
libMembers);

            Assert.Contains(new Method
            {
                Name = "static void TestClassLibrary.AClassUnitTests.ATest()"
            },
libMembers);
        }

        [Test]
        public async void Get_Program_Members()
        {
            var appMembers = (await SolutionScanner.GetMembers(@"..\..\..\TestConsoleApp\TestConsoleApp.csproj"))
                            .ToList();

            Assert.Contains(new Method
            {
                Name = "static void TestConsoleApp.Program.Main(string[])"
            },
            appMembers);

        }

        [Test]
        public async void FindUnreferencedMethods()
        {
            var libMembers = await SolutionScanner.GetMembers(@"..\..\..\TestClassLibrary\TestClassLibrary.csproj");
            var appMembers = await SolutionScanner.GetMembers(@"..\..\..\TestConsoleApp\TestConsoleApp.csproj");

            var unreferencedMembers = SolutionScanner.FindUnreferenced(libMembers.Concat(appMembers),
                isRoot: Roots.MainMethods.And(Roots.TestRelated).And(Roots.Generated)
                ).ToList();

            AssertNotIncluded(unreferencedMembers, "void TestClassLibrary.AClass.AMethod(int)");
            AssertNotIncluded(unreferencedMembers, "void TestClassLibrary.AClass.IndirectlyReferencedMethod(");

            AssertNotIncluded(unreferencedMembers, "static void TestClassLibrary.AClassUnitTests.ATest()");
            AssertNotIncluded(unreferencedMembers, "void TestClassLibrary.AClass.OnlyReferencedByTestMethod(");

            AssertNotIncluded(unreferencedMembers, "static void TestConsoleApp.Program.Main(string[])");
            AssertNotIncluded(unreferencedMembers, "void TestClassLibrary.Properties.Resource1.Resource1()");

            

            AssertIncluded(unreferencedMembers, "void TestClassLibrary.AClass.UnreferencedMethod()");
            AssertIncluded(unreferencedMembers, "void TestClassLibrary.AClass.IndirectlyUnreferencedMethod()");
        }

        private static void AssertIncluded(System.Collections.Generic.List<Method> unreferencedMembers, string Name)
        {
            Assert.Contains(new Method
            {
                Name = Name
            },
            unreferencedMembers, Name + " was not included");
        }

        private static void AssertNotIncluded(System.Collections.Generic.List<Method> unreferencedMembers, string Name)
        {
            Assert.IsFalse(unreferencedMembers.Contains(new Method
            {
                Name = Name
            }
              )
              , Name + " was included in the collection but shouldn't."
              );
        }
    }
}
