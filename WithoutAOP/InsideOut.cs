using System;
using System.Collections;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using CSharp_in_Depth.InsideOut;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    [TestFixture]
    public class WithoutAOP
    {
        [Test]
        public static void Main()
        {
            var n = DateTime.UtcNow;

            IObject obj;
            Console.WriteLine($"{nameof(ObjectImpl)}...");
            obj = new ObjectImpl();
            var it = obj.Method(5, 7, "some", n);
            Console.WriteLine();
            var that = obj.Method(5, 7, "some", n);
            Assert.That(it, Is.LessThan(that));

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine($"{nameof(OutObjectImpl)}...");
            obj = new OutObjectImpl();
            it = obj.Method(5, 7, "some", n);
            Console.WriteLine();
            that = obj.Method(5, 7, "some", n);
            Assert.That(it, Is.EqualTo(that));
        }
    }

    namespace InsideOut
    {
        interface IObject
        {
            long Method(params object[] args);
        }

        class ObjectImpl : IObject
        {
            private static long Ticks = DateTime.UtcNow.Ticks;

            public long Method(params object[] args)
            {
                Console.WriteLine($"\t{nameof(ObjectImpl)}:{nameof(Method)}");
                Console.Write("\t\t");
                foreach (var it in args)
                {
                    Console.Write($"{it},");
                }
                Console.WriteLine();

                Console.WriteLine($"\t{nameof(ObjectImpl)}:{nameof(Method)}:Range");
                Console.Write("\t\t");
                foreach (var it in Enumerable.Range(0, 7))
                {
                    Console.Write($"{it},");
                }
                Console.WriteLine();
                return ++Ticks;
            }
        }

        class OutObjectImpl : IObject
        {
            private static long Ticks = DateTime.UtcNow.Ticks;

            public long Method(params object[] args)
            {
                return ((Func<object[], long>)(items =>
                    {
                        Console.WriteLine($"\t{nameof(OutObjectImpl)}:{nameof(Method)}");
                        Console.Write("\t\t");
                        foreach (var it in items)
                        {
                            Console.Write($"{it},");
                        }
                        Console.WriteLine();
                        return ++Ticks;
                    })).Cache(args).Trace()
                    .Then((Func<object[], long, long>)((items, that) =>
                    {
                        Console.WriteLine($"\t{nameof(OutObjectImpl)}:{nameof(Method)}:Range");
                        Console.Write("\t\t");
                        foreach (var it in Enumerable.Range(0, 7))
                        {
                            Console.Write($"{it},");
                        }
                        Console.WriteLine();
                        return that;
                    }));
            }
        }

        sealed class OutFunc<T, TResult>
        {
            public static implicit operator TResult(OutFunc<T, TResult> d) => d.Result;

            private bool _invoked;
            private TResult _result;

            public Func<T, TResult> Method { get; set; }
            public T Arg { get; set; }

            public TResult Result
            {
                get
                {
                    if (!_invoked)
                    {
                        _result = Method.Invoke(Arg);
                        _invoked = true;
                    }
                    return _result;
                }
            }
        }

        static class OutLogger
        {
            public static OutFunc<T, TResult> Trace<T, TResult>(this Func<T, TResult> method, T arg) => Trace(new OutFunc<T, TResult>() { Method = method, Arg = arg });
            public static OutFunc<T, TResult> Trace<T, TResult>(this OutFunc<T, TResult> func)
            {
                Method(func);
                return func;
            }

            private static void Method<T, TResult>(OutFunc<T, TResult> func)
            {
                var callback = func.Method;
                func.Method = (arg) =>
                {
                    Console.WriteLine($"\t\t{nameof(OutLogger)}:{nameof(Method)}");

                    BeforeMethod(arg);
                    var result = callback.Invoke(arg);
                    AfterMethod(arg);
                    return result;
                };
            }

            private static void BeforeMethod(params object[] args)
            {
                Console.WriteLine($"\t\t\t{nameof(OutLogger)}:{nameof(BeforeMethod)}");
                Console.Write("\t\t\t\t");
                foreach (var it in args)
                {
                    Console.Write($"{it},");
                }
                Console.WriteLine();
            }

            private static void AfterMethod(params object[] args)
            {
                Console.WriteLine($"\t\t\t{nameof(OutLogger)}:{nameof(AfterMethod)}");
                Console.Write("\t\t\t\t");
                foreach (var it in args)
                {
                    Console.Write($"{it},");
                }
                Console.WriteLine();
            }

        }

        static class OutCache
        {
            private static readonly MemoryCache m_cache = new MemoryCache(nameof(OutCache));

            public static OutFunc<T, TResult> Cache<T, TResult>(this Func<T, TResult> method, T arg, string key = null) => Cache(new OutFunc<T, TResult>() { Method = method, Arg = arg }, key);
            public static OutFunc<T, TResult> Cache<T, TResult>(this OutFunc<T, TResult> func, string key = null)
            {
                Method(func, key);
                return func;
            }

            private static void Method<T, TResult>(OutFunc<T, TResult> func, string key)
            {
                var callback = func.Method;
                func.Method = (arg) =>
                {
                    Console.WriteLine($"\t\t{nameof(OutCache)}:{nameof(Method)}");

                    if (null == key)
                    {
                        key = GetKey(func);
                    }
                    var obj = m_cache.Get(key);
                    TResult result;
                    if (null != obj)
                    {
                        Console.WriteLine("\t\t\tGet item from cache");
                        result = (TResult)obj;
                    }
                    else
                    {
                        Console.WriteLine("\t\t\tAdd item to cache");
                        result = callback.Invoke(arg);
                        m_cache.Add(key, result, DateTimeOffset.UtcNow.AddHours(1)); // 1 hour
                    }
                    return result;
                };
            }

            private static string GetKey<T, TResult>(OutFunc<T, TResult> func)
            {
                // TODO 2017-08-16 emiya: Не лучшая идея использовать ToString() для создания уникального ключа
                var sb = new StringBuilder();
                sb.Append($"{func.Method.Target}:{func.Method.Method.Name}");
                if (func.Arg is IEnumerable)
                {
                    foreach (var param in (IEnumerable)func.Arg)
                    {
                        sb.Append($"_{param.GetType()}@{param}");
                    }
                }
                else
                {
                    sb.Append($"_{func.Arg.GetType()}@{func.Arg}");
                }
                return sb.ToString();
            }
        }

        static class OutThen
        {
            // Can't be first
            public static OutFunc<T, TResult> Then<T, TResult>(this OutFunc<T, TResult> func, Func<T, TResult, TResult> other)
            {
                var callback = func.Method;
                func.Method = (arg) =>
                {
                    Console.WriteLine($"\t\t{nameof(OutThen)}:{nameof(Then)}");

                    TResult that = callback.Invoke(func.Arg);
                    return other.Invoke(func.Arg, that);
                };
                return func;
            }
        }
    }
}
