using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    [TestFixture]
    public class MagicJIT
    {
        class Request
        {
            public int x = 0;
            public int y = 0;
        }

        class Response
        {
            public int[] t1 = new[] { -1, -1 };
            public int[] t2 = new[] { -1, -1 };
        }

        // Поток 1: print x; x = 1; print y; => 00
        // Поток 2: print y; y = 1; print x; => 00
        /*
         Из этого следует, что первый поток вывел y до того, как второй поток присвоил ему единицу
         , что произошло перед тем, как второй поток вывел x, что произошло перед тем
         , как первый поток присвоил ему единицу, что произошло перед тем, как первый поток вывел y. 
         То есть первый поток вывел y прежде, чем он вывел y, что не имеет смысла. 
         Однако, такой сценарий возможен, потому что как компилятор, так и процессор могли поменять порядок чтения и записи x и y врутри потоков.
         */

        [Test]
        public static void Main()
        {
            for (int i = 0; i < 1000000; i++)
            {
                if (0 != i && 0 == i%1000) Console.WriteLine($"step {i} ...");
                if (Run())
                {
                    Console.WriteLine($".... into {i}");
                    break;
                }
            }
        }

        private static bool Run()
        {
            var req = new Request();
            var res = new Response();
            var t1 = Task.Factory.StartNew(() =>
            {
                res.t1[0] = req.x;
                req.x = 1;
                res.t1[1] = req.y;
            });
            var t2 = Task.Factory.StartNew(() =>
            {
                res.t2[0] = req.y;
                req.y = 1;
                res.t2[1] = req.x;
            });

            Task.WaitAll(t1, t2);

            if (res.t1[0] == 0 && res.t1[1] == 0 && res.t2[0] == 0 && res.t2[1] == 0)
            {
                Console.WriteLine("Found raise to write and read");
                return true;
            }

            return false;
        }
    }
}
