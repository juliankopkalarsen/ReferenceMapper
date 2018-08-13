using TestClassLibrary;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new AClass();
            c.AMethod(3);

            ISomething s = new ASomething();
            s.SomeBehavior();

        }
    }
}
