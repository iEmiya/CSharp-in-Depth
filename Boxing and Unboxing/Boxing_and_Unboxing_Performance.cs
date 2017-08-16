using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    [TestFixture]
    public class Boxing_and_Unboxing_Performance
    {
        [Test]
        public void Main()
        {
            var len = 1000;
            Run00(len);
            Run01(len);
            Run02(len);
        }

        private static void Run00(int len)
        {
            string v = $"some string {DateTime.UtcNow.Ticks}";
            var t = Stopwatch.StartNew();
            for (int i = 0; i < len; i++)
            {
                // without cast
                var it = v as string;
                if (null == it) throw new ApplicationException();
            }
            t.Stop();
            Console.WriteLine($"{t.ElapsedTicks:D10}");
        }

        private static void Run01(int len)
        {
            object v = $"some string {DateTime.UtcNow.Ticks}";
            var t = Stopwatch.StartNew();
            for (int i = 0; i < len; i++)
            {
                // cast to type
                var it = v as string;
                if (null == it) throw new ApplicationException();
            }
            t.Stop();
            Console.WriteLine($"{t.ElapsedTicks:D10}");
        }

        [StructLayout(LayoutKind.Explicit)]
        struct Variant
        {
            [FieldOffset(0)]
            public object AsObject;

            [FieldOffset(0)]
            public string AsImmutable;
        }

        private static void Run02(int len)
        {
            object v = $"some string {DateTime.UtcNow.Ticks}";
            var t = Stopwatch.StartNew();
            for (int i = 0; i < len; i++)
            {
                // simple copy over memory
                Variant that;
                that.AsImmutable = null;
                that.AsObject = v;
                if (null == that.AsImmutable) throw new ApplicationException();
            }
            t.Stop();
            Console.WriteLine($"{t.ElapsedTicks:D10}");
        }
    }
}
