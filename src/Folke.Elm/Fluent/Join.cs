using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IJoinTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class JoinTargetExtensions
    {
        public static IJoinResult<T,TMe> Join<T,TMe, TKey>(this IJoinTarget<T,TMe> joinTarget, Expression<Func<T, TKey>> tableExpression, JoinType type)
        {
            joinTarget.QueryBuilder.AppendJoin(type);
            BaseQueryBuilder queryBuilder = joinTarget.QueryBuilder;
            var selectedTable = queryBuilder.GetTable(tableExpression.Body, true);
            queryBuilder.StringBuilder.AppendTable(selectedTable);
            return (IJoinResult<T, TMe>)joinTarget;
        }

        public static IJoinResult<T, TMe> Join<T, TMe>(this IJoinTarget<T, TMe> joinTarget, Action<ISelectResult<T, TMe>> subQuery, Expression<Func<object>> tableAliasExpression, JoinType type)
        {
            joinTarget.QueryBuilder.AppendJoin(type);
            joinTarget.SubQuery(subQuery);
            joinTarget.QueryBuilder.StringBuilder.Append("AS");
            var table = joinTarget.QueryBuilder.GetTable(tableAliasExpression.Body, true);
            joinTarget.QueryBuilder.StringBuilder.Append(table.Alias);
            return (IJoinResult<T, TMe>)joinTarget;
        }

        public static IJoinResult<T, TMe> InnerJoin<T, TMe>(this IJoinTarget<T, TMe> joinTarget,
            Action<ISelectResult<T, TMe>> subQuery, Expression<Func<object>> tableAlias)
        {
            return joinTarget.Join(subQuery, tableAlias, JoinType.Inner);
        }

        public static IJoinResult<T, TMe> InnerJoin<T, TMe>(this IJoinTarget<T, TMe> joinTarget, Expression<Func<T, object>> tableExpression)
        {
            return joinTarget.Join(tableExpression, JoinType.Inner);
        }

        public static IJoinResult<T, TMe> LeftJoin<T, TMe>(this IJoinTarget<T, TMe> joinTarget, Expression<Func<T, object>> tableExpression)
        {
            return joinTarget.Join(tableExpression, JoinType.LeftOuter);
        }

        public static IJoinResult<T, TMe> RightJoin<T, TMe>(this IJoinTarget<T, TMe> joinTarget, Expression<Func<T, object>> tableExpression)
        {
            return joinTarget.Join(tableExpression, JoinType.RightOuter);
        }

        public static IOnResult<T, TMe> LeftJoinOnId<T, TMe, TKey>(this IJoinTarget<T, TMe> joinTarget, Expression<Func<T, TKey>> tableExpression)
        {
            return joinTarget.Join(tableExpression, JoinType.LeftOuter).OnId(tableExpression);
        }

        public static IOnResult<T, TMe> InnerJoinOnId<T, TMe>(this IJoinTarget<T, TMe> joinTarget, Expression<Func<T, object>> tableExpression)
        {
            return joinTarget.Join(tableExpression, JoinType.Inner).OnId(tableExpression);
        }

        private static void AppendJoin(this BaseQueryBuilder queryBuilder, JoinType type)
        {
            switch (type)
            {
                case JoinType.LeftOuter:
                    queryBuilder.StringBuilder.AppendAfterSpace("LEFT JOIN");
                    break;
                case JoinType.RightOuter:
                    queryBuilder.StringBuilder.AppendAfterSpace("RIGHT JOIN");
                    break;
                case JoinType.Inner:
                    queryBuilder.StringBuilder.AppendAfterSpace("INNER JOIN");
                    break;
            }
        }
    }

    public interface IJoinResult<T, TMe> : IOnTarget<T, TMe>
    {
        
    }
}
