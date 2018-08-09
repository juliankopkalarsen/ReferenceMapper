using NUnit.Framework;
using System.Linq;

namespace ReferenceMapper.UnitTest
{
    public class ReferenceMapperTests
    {
        [Test]
        public void Get_AClass_Members()
        {
            var libMembers = SolutionScanner.GetMembers(@"..\..\..\TestClassLibrary\TestClassLibrary.csproj")
                .ToList();

            Assert.Contains(new Member
            {
                Name= "void TestClassLibrary.AClass.AMethod(int)"
            },
            libMembers);

            Assert.Contains(new Member
            {
                Name = "void TestClassLibrary.AClass.UnreferencedMethod()"
            },
            libMembers);

            Assert.Contains(new Member
            {
                Name = "void TestClassLibrary.AClass.IndirectlyReferencedMethod()"
            },
            libMembers);

            Assert.Contains(new Member
            {
                Name = "void TestClassLibrary.AClass.IndirectlyUnreferencedMethod()"
            },
            libMembers);
        }

        [Test]
        public void Get_Program_Members()
        {
            var appMembers = SolutionScanner.GetMembers(@"..\..\..\TestConsoleApp\TestConsoleApp.csproj")
                .ToList();

            Assert.Contains(new Member
            {
                Name = "static void TestConsoleApp.Program.Main(string[])"
            },
            appMembers);

        }

        [Test]
        public void FindUnreferencedMethods()
        {
            var libMembers = SolutionScanner.GetMembers(@"..\..\..\TestClassLibrary\TestClassLibrary.csproj");
            var appMembers = SolutionScanner.GetMembers(@"..\..\..\TestConsoleApp\TestConsoleApp.csproj");

            var unreferencedMembers = SolutionScanner.FindUnreferenced(libMembers.Concat(appMembers),
                isRoot: (Member m) => m.Name.Contains("static") && m.Name.Contains(".Main(") 
                ).ToList();

            Assert.IsFalse(unreferencedMembers.Contains(new Member
            {
                Name = "void TestClassLibrary.AClass.AMethod(int)"
            }
            )
            );

            Assert.IsFalse(unreferencedMembers.Contains(new Member
            {
                Name = "void TestClassLibrary.AClass.IndirectlyReferencedMethod()"
            }
            )
            );

            Assert.IsFalse(unreferencedMembers.Contains(new Member
            {
                Name = "static void TestConsoleApp.Program.Main(string[])"
            }
              )
              );

            Assert.Contains(new Member
            {
                Name = "void TestClassLibrary.AClass.UnreferencedMethod()"
            },
            unreferencedMembers);

            Assert.Contains(new Member
            {
                Name = "void TestClassLibrary.AClass.IndirectlyUnreferencedMethod()"
            },
            unreferencedMembers);
        }
    }
}
