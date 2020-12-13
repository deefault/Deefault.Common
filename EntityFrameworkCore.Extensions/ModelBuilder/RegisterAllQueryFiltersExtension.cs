using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace Deefault.EntityFrameworkCore.Extensions.ModelBuilderExtensions
{
    // https://github.com/dotnet/efcore/issues/10275
    public static class RegisterAllQueryFiltersExtension
    {
        public static ModelBuilder RegisterAllQueryFilters<TInterface>(this ModelBuilder builder,
            Expression<Func<TInterface, bool>> expression)
        {
            foreach (var entityType in builder.Model.GetEntityTypes()
                .Where(x => typeof(TInterface).IsAssignableFrom(x.ClrType)))
            {
                var parameterType = Expression.Parameter(entityType.ClrType);
                var expressionFilter = ReplacingExpressionVisitor.Replace(
                    expression.Parameters.Single(), parameterType, expression.Body);

                var currentQueryFilter = entityType.GetQueryFilter();
                if (currentQueryFilter != null)
                {
                    var currentExpressionFilter = ReplacingExpressionVisitor.Replace(
                        currentQueryFilter.Parameters.Single(), parameterType, currentQueryFilter.Body);
                    expressionFilter = Expression.AndAlso(currentExpressionFilter, expressionFilter);
                }

                var lambdaExpression = Expression.Lambda(expressionFilter, parameterType);
                entityType.SetQueryFilter(lambdaExpression);
            }

            return builder;
        }
    }
}