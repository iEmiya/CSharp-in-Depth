using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    [TestFixture]
    public class Linq_LifeAfterYield
    {
        IEnumerable<string> Foo()
        {
            yield return "Foo";

            Console.Write("...");
            yield return "Bar";

            Console.WriteLine("Baz");
            Console.WriteLine("End");
        }

        [Test]
        public void Main()
        {
            foreach (var str in Foo())
                Console.Write(str);
        }

        // FooBarBaz
        // End
    }

    [TestFixture]
    public class Linq_LifeAfterYield_IL
    {
        private sealed class FooEnumerable :
            IEnumerable<string>, IEnumerator<string>
        {
            private int state;
            public string Current { get; private set; }
            object IEnumerator.Current
            {
                get { return Current; }
            }
            public FooEnumerable(int state)
            {
                this.state = state;
            }
            public IEnumerator<string> GetEnumerator()
            {
                FooEnumerable fooEnumerable;
                if (state == -2)
                {
                    state = 0;
                    fooEnumerable = this;
                }
                else
                    fooEnumerable = new FooEnumerable(0);
                return fooEnumerable;
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            bool IEnumerator.MoveNext()
            {
                switch (state)
                {
                    case 0:
                        Current = "Foo";                // yield return "Foo";
                        state = 1;
                        return true;
                    case 1:
                        Console.Write("...");
                        Current = "Bar";                // yield return "Bar";
                        state = 2;
                        return true;
                    case 2:
                        state = -1;
                        Console.WriteLine("Baz");
                        Console.WriteLine("End");
                        break;
                }
                return false;
            }
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }
            void IDisposable.Dispose()
            {
            }
        }
        IEnumerable<string> Foo()
        {
            return new FooEnumerable(-2);
        }

        [Test]
        public void Main()
        {
            var enumerator = Foo().GetEnumerator();
            while (enumerator.MoveNext())
                Console.Write(enumerator.Current);
        }
    }


}