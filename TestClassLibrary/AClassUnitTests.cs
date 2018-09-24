using NUnit.Framework;

namespace TestClassLibrary
{
    public class AClassUnitTests
    {

        [Test, Ignore]
        public static void ATest()
        {
            var c = new AClass();
            c.OnlyReferencedByTestMethod();
        }
    }
}
