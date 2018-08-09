using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceMapper
{
    public static class Roots
    {
        public static Func<Member, bool> MainMethods => (Member m) => 
        m.Name.Contains("static") 
        && m.Name.Contains(".Main(");

        public static Func<Member, bool> TestRelated => (Member m) => 
            m.Attributes.Contains("NUnit.Framework.TestAttribute")
            || m.Attributes.Contains("Test")
            || m.Attributes.Contains("TestFixture")
            || m.Attributes.Contains("TestCaseSource")
            || m.Attributes.Contains("TestFixtureSetUp")
            || m.Attributes.Contains("TestFixtureTearDown")
            || m.Attributes.Contains("SetUp")
            || m.Attributes.Contains("TearDown");

        public static Func<Member, bool> Generated => (Member m) => m.IsGenerated;
    }

    public static class RootsExtensions
    {
        public static Func<Member, bool> And(this Func<Member, bool> a, Func<Member, bool> b) =>
            m => a(m) | b(m);

    }
}
