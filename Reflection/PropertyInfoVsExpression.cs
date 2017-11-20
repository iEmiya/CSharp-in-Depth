using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using CSharp_in_Depth.Utils;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    [TestFixture]
    public class PropertyInfoVsExpression
    {
        class TestClass
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
        }

        interface IWrapper<T> where T : class
        {
            int GetInt(T obj, string property);
            string GetString(T obj, string property);

            void SetInt(T obj, string property, int value);
            void SetString(T obj, string property, string value);
        }

        class Direct : IWrapper<TestClass>
        {
            public int GetInt(TestClass obj, string property) => obj.IntValue;
            public string GetString(TestClass obj, string property) => obj.StringValue;

            public void SetInt(TestClass obj, string property, int value) => obj.IntValue = value;
            public void SetString(TestClass obj, string property, string value) => obj.StringValue = value;
        }

        class WrapperWithExpression<T> : IWrapper<T>
            where T : class 
        {
            delegate int GetIntDelegate(T obj);
            delegate string GetStringDelegate(T obj);
            private readonly Dictionary<string, GetIntDelegate> _getInt;
            private readonly Dictionary<string, SetIntDelegate> _setInt;


            delegate void SetIntDelegate(T obj, int value);
            delegate void SetStringDelegate(T obj, string value);
            private readonly Dictionary<string, GetStringDelegate> _getString;
            private readonly Dictionary<string, SetStringDelegate> _setString;

            public WrapperWithExpression()
            {
                var type = typeof(T);

                _getInt = new Dictionary<string, GetIntDelegate>();
                _setInt = new Dictionary<string, SetIntDelegate>();

                _getString = new Dictionary<string, GetStringDelegate>();
                _setString = new Dictionary<string, SetStringDelegate>();


                var objParam = Expression.Parameter(type, "obj");
                foreach (var pi in type.GetProperties())
                {
                    var t = pi.PropertyType;
                    if (typeof(int) == t)
                    {
                        var valueParam = Expression.Parameter(typeof(int), "value");

                        var getCall = Expression.Call(objParam, pi.GetMethod);
                        var setCall = Expression.Call(objParam, pi.SetMethod, valueParam);

                        var getLambda = Expression.Lambda<GetIntDelegate>(getCall, objParam);
                        var setLambda = Expression.Lambda<SetIntDelegate>(setCall, objParam, valueParam);

                        var getFunc = getLambda.Compile();
                        var setFunc = setLambda.Compile();

                        _getInt.Add(pi.Name, getFunc);
                        _setInt.Add(pi.Name, setFunc);
                    }
                    else if (typeof(string) == t)
                    {
                        var valueParam = Expression.Parameter(typeof(string), "value");

                        var getCall = Expression.Call(objParam, pi.GetMethod);
                        var setCall = Expression.Call(objParam, pi.SetMethod, valueParam);

                        var getLambda = Expression.Lambda<GetStringDelegate>(getCall, objParam);
                        var setLambda = Expression.Lambda<SetStringDelegate>(setCall, objParam, valueParam);

                        var getFunc = getLambda.Compile();
                        var setFunc = setLambda.Compile();

                        _getString.Add(pi.Name, getFunc);
                        _setString.Add(pi.Name, setFunc);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            public int GetInt(T obj, string property) => _getInt[property].Invoke(obj);
            public string GetString(T obj, string property) => _getString[property].Invoke(obj);

            public void SetInt(T obj, string property, int value) => _setInt[property].Invoke(obj, value);
            public void SetString(T obj, string property, string value) => _setString[property].Invoke(obj, value);
        }

        class WrapperWithPropertyinfo<T> : IWrapper<T> where T: class
        {
            private readonly Dictionary<string, PropertyInfo> _propertyInfoDictionary;

            public WrapperWithPropertyinfo()
            {
                var type = typeof(T);
                _propertyInfoDictionary = new Dictionary<string, PropertyInfo>();
                foreach (var pi in type.GetProperties())
                {
                    _propertyInfoDictionary.Add(pi.Name, pi);
                }
            }

            public int GetInt(T obj, string property) => (int)_propertyInfoDictionary[property].GetValue(obj); // boxing
            public string GetString(T obj, string property) => (string)_propertyInfoDictionary[property].GetValue(obj);

            public void SetInt(T obj, string property, int value) => _propertyInfoDictionary[property].SetValue(obj, value); // boxing
            public void SetString(T obj, string property, string value) => _propertyInfoDictionary[property].SetValue(obj, value);
        }


        [Test]
        public void Main()
        {
            var iterationCount = 10000;
            Test(new Direct(), iterationCount);
            Test(new WrapperWithExpression<TestClass>(), iterationCount);
            Test(new WrapperWithPropertyinfo<TestClass>(), iterationCount);
        }
        
        private void Test(IWrapper<TestClass> wrapper, int iterationCount) 
        {
            var n = wrapper.GetType().ToString().Replace("CSharp_in_Depth.PropertyInfoVsExpression+", "");
            var pc = new PerfCounter();
            float finish = 0;
            for (int i = 0; i < iterationCount; i++)
            {
                var it = new TestClass() { IntValue = i, StringValue = $"{i}"};

                pc.Start();
                var ti = wrapper.GetInt(it, nameof(TestClass.IntValue));
                var ts = wrapper.GetString(it, nameof(TestClass.StringValue));
                finish += pc.Finish();

                Assert.That(ti, Is.EqualTo(i));
                Assert.That(ts, Is.EqualTo($"{i}"));
            }
            Console.WriteLine($"{n} get: {finish}");

            finish = 0;
            for (int i = 0; i < iterationCount; i++)
            {
                var it = new TestClass();

                pc.Start();
                wrapper.SetInt(it, nameof(TestClass.IntValue), i);
                wrapper.SetString(it, nameof(TestClass.StringValue), $"{i}");
                finish += pc.Finish();

                var ti = it.IntValue;
                var ts = it.StringValue;

                Assert.That(ti, Is.EqualTo(i));
                Assert.That(ts, Is.EqualTo($"{i}"));
            }
            Console.WriteLine($"{n} set: {finish}");
        }
    }
}