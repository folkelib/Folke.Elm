using System;
using System.Collections.Generic;
using System.Text;
using Folke.Elm.Mapping;
using Folke.Elm.Visitor;

namespace Folke.Elm.Fluent
{
    public static class FluentBuilderExtensions
    {
        public static void AppendFrom(this IFluentBuilder builder)
        {
            if (builder.CurrentContext == QueryContext.Select || builder.CurrentContext == QueryContext.Delete)
            {
                builder.QueryBuilder.StringBuilder.DuringSelect();
            }
            else
                throw new InvalidOperationException("Only one AppendFrom");
            builder.CurrentContext = QueryContext.From;
        }

        public static void AppendWhere(this IFluentBuilder builder)
        {
            if (builder.CurrentContext == QueryContext.Where)
            {
                builder.QueryBuilder.StringBuilder.DuringBinaryOperator(BinaryOperatorType.AndAlso);
            }
            else
            {
                builder.QueryBuilder.StringBuilder.BeforeWhere();
                builder.CurrentContext = QueryContext.Where;
            }
        }
        
        /// <summary>
        /// Begins a select command
        /// </summary>
        /// <returns>The query builder</returns>
        public static void AppendSelect(this IFluentBuilder builder)
        {
            if (builder.CurrentContext != QueryContext.Select)
            {
                builder.CurrentContext = QueryContext.Select;
                builder.QueryBuilder.StringBuilder.BeforeSelect();
            }
            else
            {
                builder.QueryBuilder.StringBuilder.DuringFields();
            }
        }

        public static void AppendSet(this IFluentBuilder builder)
        {
            if (builder.CurrentContext != QueryContext.Set)
            {
                builder.QueryBuilder.StringBuilder.BeforeSet();
                builder.CurrentContext = QueryContext.Set;
            }
            else
            {
                builder.QueryBuilder.StringBuilder.DuringSet();
            }
        }
    }
}
