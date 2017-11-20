using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace CSharp_in_Depth
{
    // https://blogs.msdn.microsoft.com/meek/2008/05/02/linq-to-entities-combining-predicates/
    [TestFixture]
    public class Linq_CombiningPredicates
    {
        class Car
        {
            public string Color { get; set; }
            public float Price { get; set; }
        }

        private void Answer_1_Chaining_query_operators()
        {
            Expression<Func<Car, bool>> theCarIsRed = c => c.Color == "Red";

            Expression<Func<Car, bool>> theCarIsCheap = c => c.Price < 10.0;

            IQueryable<Car> carQuery = null;

            var query = carQuery.Where(theCarIsRed).Where(theCarIsCheap);
            var query2 = carQuery.Where(theCarIsRed).Union(carQuery.Where(theCarIsCheap));
        }

        private void Answer_2_Build_expressions_manually()
        {
            ParameterExpression c = Expression.Parameter(typeof(Car), "car");

            Expression theCarIsRed = Expression.Equal(Expression.Property(c, "Color"), Expression.Constant("Red"));

            Expression theCarIsCheap = Expression.LessThan(Expression.Property(c, "Price"), Expression.Constant(10.0));

            Expression<Func<Car, bool>> theCarIsRedOrCheap = Expression.Lambda<Func<Car, bool>>(
                Expression.Or(theCarIsRed, theCarIsCheap), c);

            IQueryable<Car> carQuery = null;

            var query = carQuery.Where(theCarIsRedOrCheap);
        }

        private void Answer_3_Composing_Lambda_Expresions()
        {
            Expression<Func<Car, bool>> theCarIsRed = c1 => c1.Color == "Red";

            Expression<Func<Car, bool>> theCarIsCheap = c2 => c2.Price < 10.0;

            ParameterExpression p = theCarIsRed.Parameters.Single();

            Expression<Func<Car, bool>> theCarIsRedOrCheap = Expression.Lambda<Func<Car, bool>>(
                Expression.Or(theCarIsRed.Body, theCarIsCheap.Body), p);

            IQueryable<Car> carQuery = null;

            var query = carQuery.Where(theCarIsRedOrCheap);
        }

        [Test]
        public void Main()
        {
            Expression<Func<Car, bool>> theCarIsRed = c => c.Color == "Red";

            Expression<Func<Car, bool>> theCarIsCheap = c => c.Price < 10.0;

            Expression<Func<Car, bool>> theCarIsRedOrCheap = theCarIsRed.Or(theCarIsCheap);

            IQueryable<Car> carQuery = null;

            var query = carQuery.Where(theCarIsRedOrCheap);
        }
    }

    public class ParameterRebinder : ExpressionVisitor
    {
        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this._map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;
            if (_map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }

            return base.VisitParameter(p);
        }
    }

    public static class Utility
    {
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);
            
            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }
        
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }
        
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }
    }
}
