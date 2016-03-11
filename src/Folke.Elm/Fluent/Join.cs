using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IJoinTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class JoinTargetExtensions
    {
        public static IJoinResult<T,TMe> Join<T,TMe>(this IJoinTarget<T,TMe> joinTarget, Expression<Func<T, object>> tableExpression, JoinType type)
        {
            joinTarget.QueryBuilder.AppendJoin(type);
            joinTarget.QueryBuilder.AppendTable(tableExpression.Body);
            return (IJoinResult<T, TMe>)joinTarget;
        }

        public static IJoinResult<T, TMe> Join<T, TMe>(this IJoinTarget<T, TMe> joinTarget, Action<ISelectResult<T, TMe>> subQuery, Expression<Func<object>> tableAlias, JoinType type)
        {
            joinTarget.QueryBuilder.AppendJoin(type);
            joinTarget.SubQuery(subQuery);
            joinTarget.QueryBuilder.Append("AS");
            var table = joinTarget.QueryBuilder.RegisterTable(tableAlias.Body.Type, joinTarget.QueryBuilder.GetTableAlias(tableAlias.Body as MemberExpression));
            joinTarget.QueryBuilder.Append(table.Name);
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

        public static IOnResult<T, TMe> LeftJoinOnId<T, TMe>(this IJoinTarget<T, TMe> joinTarget, Expression<Func<T, object>> tableExpression)
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
                    queryBuilder.Append("LEFT JOIN");
                    break;
                case JoinType.RightOuter:
                    queryBuilder.Append("RIGHT JOIN");
                    break;
                case JoinType.Inner:
                    queryBuilder.Append("INNER JOIN");
                    break;
            }
        }
    }

    public interface IJoinResult<T, TMe> : IOnTarget<T, TMe>
    {
        
    }
}
