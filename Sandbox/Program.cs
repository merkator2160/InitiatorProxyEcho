using Common;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var writer2 = new FileWriter("zxcvbn.txt");
            writer2.WriteLine("text2");
            writer2.WriteLine("asdfg2");
        }
    }
}
