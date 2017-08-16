using System;
using System.Diagnostics;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    // http://vscode.ru/prog-lessons/pomenyat-znacheniya-dvuh-peremennyih.html
    // https://proglib.io/puzzle/%D0%B8%D0%B7%D0%BC%D0%B5%D0%BD%D0%B8%D1%82%D1%8C-%D0%BC%D0%B5%D1%81%D1%82%D0%B0%D0%BC%D0%B8-%D0%B7%D0%BD%D0%B0%D1%87%D0%B5%D0%BD%D0%B8%D1%8F-%D0%B4%D0%B2%D1%83%D1%85-%D0%BF%D0%B5%D1%80%D0%B5%D0%BC/
    [TestFixture]
    public class Change_the_values_​​of_two_variables
    {
        [Test]
        public static void Main()
        {
            var len = 1000000;
            Run00(len);
            Run01(len);
            Run02(len);
        }

        private static void Run00(int len)
        {
            var t = Stopwatch.StartNew();
            for (int i = 0; i < len; i++)
            {
                int a = i + 2;
                int b = i - 2;
                int tmp = a;
                a = b;
                b = tmp;
                if (a + b != i << 1)
                    throw new ApplicationException();
            }
            t.Stop();
            Console.WriteLine($"{t.ElapsedTicks:D10}");
        }

        private static void Run01(int len)
        {
            var t = Stopwatch.StartNew();
            for (int i = 0; i < len; i++)
            {
                int a = i + 2;
                int b = i - 2;

                a = a + b;
                b = a - b;
                //b = b - a;      // b := b - (a + b) := -a
                //b = -b;         // b := -(-a) := a
                a = a - b;      // a := (a + b) - a := b

                if (a + b != i << 1)
                    throw new ApplicationException();
            }
            t.Stop();
            Console.WriteLine($"{t.ElapsedTicks:D10}");
        }

        private static void Run02(int len)
        {
            var t = Stopwatch.StartNew();
            for (int i = 0; i < len; i++)
            {
                int a = i + 2;
                int b = i - 2;

                a = a ^ b;
                b = b ^ a;
                a = a ^ b;

                if (a + b != i << 1)
                    throw new ApplicationException();
            }
            t.Stop();
            Console.WriteLine($"{t.ElapsedTicks:D10}");
        }
    }
}
