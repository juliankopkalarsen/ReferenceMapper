using NUnit.Framework;

namespace TestClassLibrary
{
    public class AClassUnitTests
    {

        [Test]
        public static void ATest()
        {
            var c = new AClass();
            c.OnlyReferencedByTestMethod();
        }
    }
}
