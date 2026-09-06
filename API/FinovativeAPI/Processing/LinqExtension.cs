using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Project.Processing
{
    public static class LinqExtension
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> sourceList, string orderByMember, string SortDirection)
        {
            bool isAsc = false;
            if (SortDirection.Equals("Asc", StringComparison.InvariantCultureIgnoreCase))
            {
                isAsc = true;
            }

            var param = Expression.Parameter(typeof(T), "c");
            if (string.IsNullOrEmpty(orderByMember))
            {
                return sourceList;
            }

            var body = orderByMember.Split('.').Aggregate<string, Expression>(param, Expression.PropertyOrField);

            var queryable = isAsc == true ?
                (IOrderedQueryable<T>)Queryable.OrderBy(sourceList.AsQueryable(), (dynamic)Expression.Lambda(body, param)) :
                (IOrderedQueryable<T>)Queryable.OrderByDescending(sourceList.AsQueryable(), (dynamic)Expression.Lambda(body, param));

            return queryable;
        }

        public static IQueryable<T> SkipAndTakeCount<T>(this IQueryable<T> sourceList, int SkipCount, int TakeCount)
        {
            if (TakeCount <= 0)
            {
                return sourceList;
            }

            var queryable = sourceList.Skip(SkipCount).Take(TakeCount);

            return queryable;
        }

        public static IQueryable<T> WhereDynamic<T>(this IQueryable<T> sourceList, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return sourceList;
            }

            try
            {
                var properties = typeof(T).GetProperties()
                    .Where(x => x.CanRead && x.CanWrite && !x.GetGetMethod().IsVirtual);

                //Expression
                sourceList = sourceList.Where(c =>
                    properties.Any(p => p.GetValue(c) != null && p.GetValue(c).ToString().ToLower()
                        .Contains(query.ToLower())));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return sourceList;
        }

        public static IEnumerable<TSource> CustomDistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}