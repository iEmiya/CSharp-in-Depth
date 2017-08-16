using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    [TestFixture]
    public class Linq_ClosureAndForeach
    {
        [Test]
        public void Main()
        {
            var actions = new List<Action>();
            foreach (var i in Enumerable.Range(1, 3))
                actions.Add(() => Console.WriteLine(i));
            foreach (var action in actions)
                action();

            // Mono compiler 2.4.4 : 3 3 3
            // Mono compiler 3.10.0 : 1 2 3
            // Mono compiler 3.10.0 langversion = 4 : 1 2 3
            // MS compiler 3.5.30729.7903 : 3 3 3
            // MS compiler 4.0.30319.1 : 3 3 3
            // MS compiler 4.0.30319.33440 : 1 2 3
            // MS compiler 4.0.30319.33440 langversion = 4 : 1 2 3

        }
    }

    [TestFixture]
    public class Linq_ClosureAndForeach_LegacyJit
    {
        [Test]
        public void Main()
        {
            var actions = new List<Action>();
            var c1 = new DisplayClass();
            foreach (var i in Enumerable.Range(1, 3))
            {
                c1.i = i;
                actions.Add(c1.Action);
            }
            foreach (var action in actions)
                action();

            // LegacyJit    : 3 3 3
        }

        private sealed class DisplayClass
        {
            public int i;
            public void Action()
            {
                Console.WriteLine(i);
            }
        }

    }

    [TestFixture]
    public class Linq_ClosureAndForeach_RyuJIT
    {
        public void Run()
        {
            var actions = new List<Action>();
            foreach (var i in Enumerable.Range(1, 3))
            {
                var c1 = new DisplayClass();
                c1.i = i;
                actions.Add(c1.Action);
            }
            foreach (Action action in actions)
                action();

            // RyuJIT       : 1 2 3
        }
        private sealed class DisplayClass
        {
            public int i;
            public void Action()
            {
                Console.WriteLine(i);
            }
        }

    }
}