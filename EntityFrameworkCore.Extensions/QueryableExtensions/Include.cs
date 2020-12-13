using System;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Deefault.EntityFrameworkCore.Extensions
{
    public class Include<T>
    {
        private readonly Expression<Func<T, object>> _underlyingExpression;

        public Expression<Func<T, object>> Expression => _underlyingExpression;
        
        public Include(Expression<Func<T, object>> expression)
        {
            _underlyingExpression = expression;
        }
    }
}