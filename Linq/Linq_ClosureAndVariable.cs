using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    [TestFixture]
    public class Linq_ClosureAndVariable
    {
        [Test]
        public void Main()
        {
            var list = new List<string> { "Foo", "Bar", "Baz" };
            var startLetter = "F";
            var query = list.Where(c => c.StartsWith(startLetter));
            startLetter = "B";
            query = query.Where(c => c.StartsWith(startLetter));
            Console.WriteLine(query.Count());
        }
    }

    [TestFixture]
    public class Linq_ClosureAndVariable_IL
    {
        [Test]
        public void Main()
        {
            var list = new List<string> { "Foo", "Bar", "Baz" };
            var c1 = new DisplayClass();
            c1.startLetter = "F";
            IEnumerable<string> source = list.Where(c1.Method1);
            c1.startLetter = "B";
            source = source.Where(c1.Method2);
            Console.WriteLine(source.Count());
        }

        class DisplayClass
        {
            public string startLetter;
            public bool Method1(string c)
            {
                return c.StartsWith(this.startLetter);
            }
            public bool Method2(string c)
            {
                return c.StartsWith(this.startLetter);
            }
        }

    }
}