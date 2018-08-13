using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceMapper
{
    public static class Roots
    {
        public static Func<Method, bool> MainMethods => (Method m) => 
        m.Name.Contains("static") 
        && m.Name.Contains(".Main(");

        public static Func<Method, bool> TestRelated => (Method m) => 
            m.Attributes.Contains("NUnit.Framework.TestAttribute")
            || m.Attributes.Contains("Test")
            || m.Attributes.Contains("TestFixture")
            || m.Attributes.Contains("TestCaseSource")
            || m.Attributes.Contains("TestFixtureSetUp")
            || m.Attributes.Contains("TestFixtureTearDown")
            || m.Attributes.Contains("SetUp")
            || m.Attributes.Contains("TearDown");

        public static Func<Method, bool> Generated => (Method m) => m.IsGenerated;
    }

    public static class RootsExtensions
    {
        public static Func<Method, bool> And(this Func<Method, bool> a, Func<Method, bool> b) =>
            m => a(m) | b(m);

    }
}
