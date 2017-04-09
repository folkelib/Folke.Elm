using System;
using System.Globalization;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface ILimitTarget<T, TMe> : IFluentBuilder
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
        public static ILimitResult<T, TMe> Limit<T, TMe>(this ILimitTarget<T, TMe> builder, int offset, int count)
        {
            builder.QueryBuilder.StringBuilder.DuringSkip();
            builder.QueryBuilder.StringBuilder.Append(offset.ToString(CultureInfo.InvariantCulture));
            builder.QueryBuilder.StringBuilder.DuringTake();
            builder.QueryBuilder.StringBuilder.Append(count.ToString(CultureInfo.InvariantCulture));
            builder.QueryBuilder.StringBuilder.AfterTake();
            return (ILimitResult<T, TMe>) builder;
        }

        public static ILimitTarget<T, TMe> Skip<T, TMe>(this ILimitTarget<T, TMe> builder, int offset)
        {
            builder.QueryBuilder.StringBuilder.DuringSkip();
            builder.QueryBuilder.StringBuilder.Append(offset.ToString(CultureInfo.InvariantCulture));
            return builder;
        }

        public static ILimitResult<T, TMe> Take<T, TMe>(this ILimitTarget<T, TMe> builder, int count)
        {
            builder.QueryBuilder.StringBuilder.DuringTake();
            builder.QueryBuilder.StringBuilder.Append(count.ToString(CultureInfo.InvariantCulture));
            builder.QueryBuilder.StringBuilder.AfterTake();
            return (ILimitResult<T, TMe>)builder;
        }

        public static ILimitResult<T, TMe> Limit<T, TMe>(this ILimitTarget<T, TMe> builder, Expression<Func<T, int>> offset, int count)
        {
            builder.QueryBuilder.StringBuilder.DuringSkip();
            builder.QueryBuilder.AddExpression(offset.Body);
            builder.QueryBuilder.StringBuilder.DuringTake();
            builder.QueryBuilder.StringBuilder.Append(count.ToString(CultureInfo.InvariantCulture));
            builder.QueryBuilder.StringBuilder.AfterTake();
            return (ILimitResult<T, TMe>)builder;
        }

        public static ILimitResult<T, TMe> Limit<T, TMe>(this ILimitTarget<T, TMe> builder, Expression<Func<T, TMe, int>> offset, int count)
        {
            builder.QueryBuilder.StringBuilder.DuringSkip();
            builder.QueryBuilder.AddExpression(offset.Body);
            builder.QueryBuilder.StringBuilder.DuringTake();
            builder.QueryBuilder.StringBuilder.Append(count.ToString(CultureInfo.InvariantCulture));
            builder.QueryBuilder.StringBuilder.AfterTake();
            return (ILimitResult<T, TMe>)builder;
        }
    }

    public interface ILimitResult<T, TMe> : IQueryableCommand<T>
    {
    }
}
