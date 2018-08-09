
namespace TestClassLibrary
{
    public class AClass
    {
        public void AMethod(int input) {
            IndirectlyReferencedMethod();
        }

        public void IndirectlyReferencedMethod() { }

        public void UnreferencedMethod() {
            IndirectlyUnreferencedMethod();
        }

        public void IndirectlyUnreferencedMethod() { }

        public void OnlyReferencedByTestMethod() { }

    }
}
