using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Deefault.EntityFrameworkCore.Extensions
{
    public static class IncludeExtensions
    {
        public static IQueryable<TSource> AddIncludes<TSource>(this IQueryable<TSource> source,
            params Expression<Func<TSource, object>>[] includes) where TSource : class
        {
            return includes.Aggregate(source, (current, include) => current.Include(include));
        }
        
        public static IQueryable<TSource> AddInclude<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, object>> include) where TSource : class
        {
            return source.Include(include);
        }
        
        public static IQueryable<TSource> AddIncludes<TSource>(this IQueryable<TSource> source,
            params Include<TSource>[] includes) where TSource : class
        {
            return includes.Aggregate(source, (current, include) => current.Include(include.Expression));
        }
        
        public static IQueryable<TSource> AddInclude<TSource>(this IQueryable<TSource> source,
            Include<TSource> include) where TSource : class
        {
            return source.Include(include.Expression);
        }
    }
}