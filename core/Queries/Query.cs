using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Glow.Core.Queries
{
   public enum Operation
    {
        StartsWith = 1,
        Equals,
        LessThan,
        GreaterThan,
        Contains
    }

    public class Where
    {
        public string Property { get; set; }
        public Operation Operation { get; set; }
        public string Value { get; set; }
    }

    public class OrderBy
    {
        public string Property { get; set; }
        public Direction Direction { get; set; }
    }

    public enum Direction
    {
        Asc, Desc
    }

    public class Query
    {
        public int? Take { get; set; }
        public int? Skip { get; set; }
        public Where Where { get; set; }
        public OrderBy Orderby { get; set; }
    }

    public static class QueryExtensions
    {
        public static IEnumerable<T> Apply<T>(this IEnumerable<T> self, Query query)
        {
            Func<T, bool> where = CreateWhereExpression<T>(query.Where);

            if (where != null)
            {
                self = self.Where(where);
            }

            Func<T, object> orderBy = CreateOrderByExpression<T>(query.Orderby.Property);

            if (orderBy != null)
            {
                self = query.Orderby.Direction == Direction.Asc
                    ? self.OrderBy(orderBy)
                    : self.OrderByDescending(orderBy);
            }

            return self
                .Skip(query.Skip ?? 0)
                .Take(query.Take ?? 10);
        }

        private static Func<T, bool> CreateWhereExpression<T>(Where where)
        {
            if (where == null) { return null; }
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            PropertyInfo propertyInfo = typeof(T).GetProperty(where.Property);

            MemberExpression property = Expression.Property(parameter, propertyInfo);

            BinaryExpression constant = propertyInfo.PropertyType == typeof(int)
                ? Expression.Equal(property, Expression.Constant(int.Parse(where.Value)))
                : Expression.Equal(property, Expression.Constant(where.Value));

            Expression StringInvocation(string methodName)
            {
                MemberExpression m = Expression.MakeMemberAccess(parameter, propertyInfo);
                ConstantExpression c = Expression.Constant(where.Value, typeof(string));
                MethodInfo mi = typeof(string).GetMethod(methodName, new Type[] { typeof(string) });
                Expression call = Expression.Call(m, mi, c);
                return call;
            }

            Expression comparision = where.Operation switch
            {
                Operation.StartsWith => StringInvocation("StartsWith"),
                Operation.Contains => StringInvocation("Contains"),
                Operation.Equals => Expression.Equal(property, constant),
                Operation.GreaterThan => Expression.GreaterThan(property, constant),
                Operation.LessThan => Expression.LessThan(property, constant),
                _ => null
            };

            Func<T, bool> result = comparision != null ? Expression.Lambda<Func<T, bool>>(comparision, parameter).Compile() : null;
            return result;
        }

        public static Func<T, object> CreateOrderByExpression<T>(string propertyName)
        {
            if (propertyName == null)
            {
                return null;
            }
            ParameterExpression param = Expression.Parameter(typeof(T), "v");
            Expression conversion = Expression.Convert(Expression.Property(param, propertyName), typeof(object));
            return Expression.Lambda<Func<T, object>>(conversion, param).Compile();
        }
    }
}
