using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Folke.Elm.Fluent
{
    public interface IWhereTarget<T, TParameters> : IFluentBuilder
    {
    }

    public static class WhereFluentBuilderExtensions
    {
        public static IWhereResult<T, TMe> Where<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, bool>> expression)
        {
            fluentBuilder.QueryBuilder.AppendWhere();
            fluentBuilder.QueryBuilder.AddExpression(expression.Body);
            return (IWhereResult<T, TMe>) fluentBuilder;
        }

        public static IWhereResult<T, TMe> Where<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, TMe, bool>> expression)
        {
            fluentBuilder.QueryBuilder.AppendWhere();
            fluentBuilder.QueryBuilder.AddExpression(expression.Body);
            return (IWhereResult<T, TMe>)fluentBuilder;
        }

        public static IWhereResult<T, TMe> Where<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Action<ISelectResult<T, TMe>> subQuery, SubQueryType type)
        {
            fluentBuilder.QueryBuilder.AppendWhere();
            switch (type)
            {
                case SubQueryType.Exists:
                    fluentBuilder.QueryBuilder.Append("EXISTS");
                    break;
                case SubQueryType.NotExists:
                    fluentBuilder.QueryBuilder.Append("NOT EXISTS");
                    break;
            }
            fluentBuilder.SubQuery(subQuery);
            return (IWhereResult<T, TMe>) fluentBuilder;
        }

        public static IWhereResult<T, TMe> WhereNotExists<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Action<ISelectResult<T, TMe>> subQuery)
        {
            return fluentBuilder.Where(subQuery, SubQueryType.NotExists);
        }

        public static IWhereResult<T, TMe> WhereExists<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Action<ISelectResult<T, TMe>> subQuery)
        {
            return fluentBuilder.Where(subQuery, SubQueryType.Exists);
        }

        public static IWhereResult<T, TMe> WhereSub<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Action<IAndWhereTarget<T, TMe>> expression)
        {
            fluentBuilder.QueryBuilder.AppendWhere();
            fluentBuilder.QueryBuilder.Append("(");
            expression((IAndWhereTarget<T, TMe>) fluentBuilder);
            fluentBuilder.QueryBuilder.Append(")");
            return (IWhereResult<T, TMe>)fluentBuilder;
        }

        /// <summary>
        /// A shortcut for Where(expression).Single();
        /// </summary>
        /// <typeparam name="T">The result table</typeparam>
        /// <typeparam name="TMe">The parameters</typeparam>
        /// <param name="fluentBuilder">A fluent query builder</param>
        /// <param name="expression">The expression that filters the results</param>
        /// <returns></returns>
        public static T Single<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, bool>> expression)
        {
            return fluentBuilder.Where(expression).Single();
        }

        public static Task<T> SingleAsync<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, bool>> expression)
        {
            return fluentBuilder.Where(expression).SingleAsync();
        }

        public static T First<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, bool>> expression)
        {
            return fluentBuilder.Where(expression).First();
        }

        public static Task<T> FirstAsync<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, bool>> expression)
        {
            return fluentBuilder.Where(expression).FirstAsync();
        }

        public static T SingleOrDefault<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, bool>> expression)
        {
            return fluentBuilder.Where(expression).SingleOrDefault();
        }

        public static Task<T> SingleOrDefaultAsync<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, bool>> expression)
        {
            return fluentBuilder.Where(expression).SingleOrDefaultAsync();
        }

        public static T FirstOrDefault<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, bool>> expression)
        {
            return fluentBuilder.Where(expression).FirstOrDefault();
        }

        public static Task<T> FirstOrDefaultAsync<T, TMe>(this IWhereTarget<T, TMe> fluentBuilder, Expression<Func<T, bool>> expression)
        {
            return fluentBuilder.Where(expression).FirstOrDefaultAsync();
        }
    }

    public interface IWhereResult<T, TMe> : IQueryableCommand<T>, IGroupByTarget<T, TMe>, IAndWhereTarget<T, TMe>, ILimitTarget<T, TMe>, IWhereTarget<T, TMe>
    {
    }
}
