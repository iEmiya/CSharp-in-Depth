using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    // https://metanit.com/sharp/tutorial/3.27.php
    [TestFixture]
    public class Covariance
    {
        class Account
        {
            private static readonly Random rnd = new Random();

            public void DoTransfer()
            {
                int sum = rnd.Next(10, 120);
                Console.WriteLine(this.GetType().ToString().Split('.').Last());
                Console.WriteLine("Customer put in an account {0} dollars", sum);
            }
        }
        class DepositAccount : Account
        {
        }

        /// <summary>
        /// Ковариантность: позволяет использовать более конкретный тип, чем заданный изначально
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <summary>
        /// При создании ковариантного интерфейса надо учитывать, что универсальный параметр 
        /// может использоваться только в качестве типа значения, возвращаемого методами интерфейса. 
        /// Но не может использоваться в качестве типа аргументов метода или ограничения методов интерфейса.
        /// </summary>
        interface IBank<out T> where T : Account
        {
            T DoOperation();

            // error CS1961: Invalid variance: The type parameter 'T' must be contravariantly valid on 'Covariance.IBank<T>.DoOperation(T)'. 'T' is covariant.
            //void DoOperation(T account);

            // error CS1961: Invalid variance: The type parameter 'T' must be contravariantly valid on 'Covariance.IBank<T>.DoSomething<R>()'. 'T' is covariant.
            //void DoSomething<R>() where R : T;
        }

        class Bank : IBank<DepositAccount>
        {
            public DepositAccount DoOperation()
            {
                DepositAccount acc = new DepositAccount();
                acc.DoTransfer();
                return acc;
            }
        }

        [Test]
        public void Main()
        {
            IBank<DepositAccount> depositBank = new Bank();
            depositBank.DoOperation();

            IBank<Account> ordinaryBank = depositBank;
            ordinaryBank.DoOperation();
        }
    }

    [TestFixture]
    public class Contravariance
    {
        class Account
        {
            private static readonly Random rnd = new Random();

            public void DoTransfer()
            {
                int sum = rnd.Next(10, 120);
                Console.WriteLine(this.GetType().ToString().Split('.').Last());
                Console.WriteLine("Customer put in an account {0} dollars", sum);
            }
        }
        class DepositAccount : Account
        {
        }

        /// <summary>
        /// Контравариантность: позволяет использовать более универсальный тип, чем заданный изначально
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <summary>
        /// При создании контрвариантного интерфейса надо учитывать, что универсальный параметр 
        /// контрвариантного типа может применяться только к аргументам метода, 
        /// но не может применяться к аргументам, используемым в качестве возвращаемых типов.
        /// </summary>
        interface IBank<in T> where T : Account
        {
            // error CS1961: Invalid variance: The type parameter 'T' must be covariantly valid on 'Contravariance.IBank<T>.DoOperation()'. 'T' is contravariant.
            //T DoOperation();

            void DoOperation(T account);

            void DoSomething<R>() where R : T;
        }

        class Bank<T> : IBank<T> where T : Account
        {
            public void DoOperation(T account)
            {
                Console.WriteLine(this.GetType().ToString().Split('.').Last());
                account.DoTransfer();
            }

            public void DoSomething<R>() where R : T
            {
                Console.WriteLine($"T is {this.GetType()}");
                Console.WriteLine($"R is {typeof(R)}");
            }
        }

        [Test]
        public void Main()
        {
            Account account = new Account();
            IBank<Account> ordinaryBank = new Bank<Account>();
            ordinaryBank.DoOperation(account);

            DepositAccount depositAcc = new DepositAccount();
            IBank<DepositAccount> depositBank = ordinaryBank;
            depositBank.DoOperation(depositAcc);


            ordinaryBank.DoOperation(depositAcc);
            // error CS1503: Argument 1: cannot convert from 'CSharp_in_Depth.Contravariance.Account' to 'CSharp_in_Depth.Contravariance.DepositAccount'
            //depositBank.DoOperation(account);


            ordinaryBank.DoSomething<Account>();
            ordinaryBank.DoSomething<DepositAccount>();
            // error CS0311: The type 'CSharp_in_Depth.Contravariance.Account' cannot be used as type parameter 'R' in the generic type or method 'Contravariance.IBank<Contravariance.DepositAccount>.DoSomething<R>()'. There is no implicit reference conversion from 'CSharp_in_Depth.Contravariance.Account' to 'CSharp_in_Depth.Contravariance.DepositAccount'.
            //depositBank.DoSomething<Account>();
            depositBank.DoSomething<DepositAccount>();
        }
    }

    public class Invariant
    {
        interface ICovariant<out T>
        {
            T DoOperation();
        }

        interface IContravariant<in T>
        {
            void DoOperation(T account);

            void DoSomething<R>() where R : T;
        }
        interface IInvariant<T> : ICovariant<T>, IContravariant<T> { }
    }

    [TestFixture]
    public class Introduces_ambiguity
    {
        // Simple class hierarchy.  
        class Animal { }
        class Cat : Animal { }
        class Dog : Animal { }

        // This class introduces ambiguity  
        // because IEnumerable<out T> is covariant.  
        class Pets : IEnumerable<Cat>, IEnumerable<Dog>
        {
            IEnumerator IEnumerable.GetEnumerator()
            {
                // Some code.  
                return null;
            }

            IEnumerator<Cat> IEnumerable<Cat>.GetEnumerator()
            {
                Console.WriteLine("Cat");
                // Some code.  
                return null;
            }

            IEnumerator<Dog> IEnumerable<Dog>.GetEnumerator()
            {
                Console.WriteLine("Dog");
                // Some code.  
                return null;
            }
        }

        [Test]
        public static void Main()
        {
            IEnumerable<Animal> pets = new Pets();
            // !!! Cat or Dog
            using (IEnumerator<Animal> _ = pets.GetEnumerator())
            {
            }
        }
    }

    [TestFixture]
    public class Explicit_interface
    {
        class Account
        {
            private static readonly Random rnd = new Random();

            public void DoTransfer()
            {
                int sum = rnd.Next(10, 120);
                Console.WriteLine(this.GetType().ToString().Split('.').Last());
                Console.WriteLine("Customer put in an account {0} dollars", sum);
            }
        }
        class DepositAccount : Account
        {
        }

        interface ICovariant<out T>
        {
            T DoOperation();
        }
        interface IContravariant<in T>
        {
            void DoOperation(T account);

            void DoSomething<R>() where R : T;
        }
        interface IBank<T> : ICovariant<T>, IContravariant<T> where T : Account { }


        class Bank<T> : IBank<T> where T : Account, new()
        {
            public T DoOperation()
            {
                T acc = new T();
                acc.DoTransfer();
                return acc;
            }

            public void DoOperation(T account)
            {
                Console.WriteLine(this.GetType().ToString().Split('.').Last());
                account.DoTransfer();
            }

            public void DoSomething<R>() where R : T
            {
                Console.WriteLine($"T is {this.GetType()}");
                Console.WriteLine($"R is {typeof(R)}");
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        struct CovarianceVariant
        {
            [FieldOffset(0)]
            public object AsObject;

            [FieldOffset(0)]
            public ICovariant<DepositAccount> AsImmutable;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct ContravariantVariant
        {
            [FieldOffset(0)]
            public object AsObject;

            [FieldOffset(0)]
            public IContravariant<Account> AsImmutable;
        }

        void Covariance()
        {
            IBank<DepositAccount> depositBank = new Bank<DepositAccount>();
            depositBank.DoOperation();

            // error CS0266: Cannot implicitly convert type 'CSharp_in_Depth.Explicit_interface.IBank<CSharp_in_Depth.Explicit_interface.DepositAccount>' to 'CSharp_in_Depth.Explicit_interface.IBank<CSharp_in_Depth.Explicit_interface.Account>'. An explicit conversion exists (are you missing a cast?)
            // IBank<Account> ordinaryBank = depositBank;
            {
                CovarianceVariant cov;
                cov.AsImmutable = null;
                cov.AsObject = depositBank;

                ICovariant<Account> ordinaryBank = cov.AsImmutable;
                ordinaryBank.DoOperation();
            }
            
        }

        void Contravariant()
        {
            Account account = new Account();
            IBank<Account> ordinaryBank = new Bank<Account>();
            ordinaryBank.DoOperation(account);

            DepositAccount depositAcc = new DepositAccount();
            IContravariant<DepositAccount> depositBank = ordinaryBank;
            depositBank.DoOperation(depositAcc);


            ordinaryBank.DoOperation(depositAcc);
            // error CS1503: Argument 1: cannot convert from 'CSharp_in_Depth.Contravariance.Account' to 'CSharp_in_Depth.Contravariance.DepositAccount'
            //depositBank.DoOperation(account);
            {
                ContravariantVariant cont;
                cont.AsImmutable = null;
                cont.AsObject = ordinaryBank;

                cont.AsImmutable.DoOperation(account);
            }


            ordinaryBank.DoSomething<Account>();
            ordinaryBank.DoSomething<DepositAccount>();
            // error CS0311: The type 'CSharp_in_Depth.Contravariance.Account' cannot be used as type parameter 'R' in the generic type or method 'Contravariance.IBank<Contravariance.DepositAccount>.DoSomething<R>()'. There is no implicit reference conversion from 'CSharp_in_Depth.Contravariance.Account' to 'CSharp_in_Depth.Contravariance.DepositAccount'.
            //depositBank.DoSomething<Account>();
            {
                ContravariantVariant cont;
                cont.AsImmutable = null;
                cont.AsObject = ordinaryBank;

                cont.AsImmutable.DoSomething<Account>();
            }
            depositBank.DoSomething<DepositAccount>();
        }

        [Test]
        public void Main()
        {
            Covariance();
            Contravariant();
        }

    }
}