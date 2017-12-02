using System;
using CSharp_in_Depth.Utils;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    [TestFixture()]
    public class TaiCall
    {
        [Test]
        public void Main()
        {
            var pc = new PerfCounter();

            var lenght = 10000;
            for (int i = 0; i < lenght; i++)
            {
                var it = ds(i);
                var that = dsFast(i);
                Assert.AreEqual(it, that);
            }

            float finish = 0;
            for (int i = 0; i < lenght; i++)
            {
                pc.Start();
                ds(i);
                finish += pc.Finish();
            }
            Console.WriteLine($"Recursion get: {finish}");

            finish = 0;
            for (int i = 0; i < lenght; i++)
            {
                pc.Start();
                dsFast(i);
                finish += pc.Finish();
            }
            Console.WriteLine($"     Loop get: {finish}");
        }

        int ds(int n)
        {
            return (n < 0) ? ds(-n) : (n == 0) ? 0 : n % 10 + ds(n / 10);
        }

        int dsFast(int n)
        {
            var result = default(int);
            if (n < 0) n = -n;
            while (n != 0)
            {
                result += n % 10;
                n = n / 10;
            }
            return result;
        }
    }
}
