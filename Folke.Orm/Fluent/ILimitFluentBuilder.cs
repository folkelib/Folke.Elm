using System;
using System.Linq.Expressions;

namespace Folke.Orm.Fluent
{
    public interface ILimitFluentBuilder<T, TMe> : IFluentBuilder
    {
    }

    public static class LimitFluentBuilderExtensions
    {
        /// <summary>
        /// Skips some rows and takes a given number of rows
        /// </summary>
        /// <typeparam name="T">The bean type</typeparam>
        /// <typeparam name="TMe">The parameters type</typeparam>
        /// <param name="builder">The query builder</param>
        /// <param name="offset">The number of rows to skip</param>
        /// <param name="count">The number of rows to take</param>
        /// <returns></returns>
        public static FluentLimitBuilder<T, TMe> Limit<T, TMe>(this ILimitFluentBuilder<T, TMe> builder, int offset, int count)
        {
            return new FluentLimitBuilder<T, TMe>(builder.QueryBuilder, offset, count);
        }
        
        public static FluentLimitBuilder<T, TMe> Limit<T, TMe>(this ILimitFluentBuilder<T, TMe> builder, Expression<Func<T, int>> offset, int count)
        {
            return new FluentLimitBuilder<T, TMe>(builder.QueryBuilder, offset, count);
        }

        public static FluentLimitBuilder<T, TMe> Limit<T, TMe>(this ILimitFluentBuilder<T, TMe> builder, Expression<Func<T, TMe, int>> offset, int count)
        {
            return new FluentLimitBuilder<T, TMe>(builder.QueryBuilder, offset, count);
        }
    }
}
