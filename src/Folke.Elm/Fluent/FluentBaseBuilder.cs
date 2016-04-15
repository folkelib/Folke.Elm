using System;
using System.Collections.Generic;
using Folke.Elm.Mapping;

namespace Folke.Elm.Fluent
{
    public class FluentBaseBuilder<T, TMe> : IFluentBuilder, ILimitTarget<T, TMe>, ILimitResult<T, TMe>, IWhereTarget<T, TMe>, IWhereResult<T, TMe>,
        IOrderByTarget<T, TMe>, IOrderByResult<T, TMe>, IAscTarget<T, TMe>, IAscResult<T, TMe>, IFromTarget<T, TMe>, IFromResult<T, TMe>, IJoinTarget<T, TMe>,
        IJoinResult<T,TMe>, IOnTarget<T,TMe>, IOnResult<T,TMe>, IAndOnTarget<T,TMe>, IAndOnResult<T,TMe>,
        IGroupByTarget<T, TMe>, IGroupByResult<T, TMe>, IInsertedValuesTarget<T, TMe>, IInsertedValuesResult<T, TMe>,
        IAndWhereTarget<T,TMe>, IAndWhereResult<T, TMe>, ISetTarget<T, TMe>, ISetResult<T, TMe>,
        IDeleteResult<T, TMe>, IInsertIntoResult<T, TMe>, ISelectedValuesTarget<T, TMe>, ISelectedValuesResult<T, TMe>,
        ISelectResult<T, TMe>, IUpdateResult<T, TMe>
    {
        protected BaseQueryBuilder baseQueryBuilder;

        public BaseQueryBuilder QueryBuilder => baseQueryBuilder;
        public QueryContext CurrentContext { get; set; } = QueryContext.Unknown;

        protected FluentBaseBuilder(BaseQueryBuilder queryBuilder)
        {
            baseQueryBuilder = queryBuilder;
        }

        //protected void SubQuery(Action<FluentSelectBuilder<T, TMe>> subQuery)
        //{
        //    var queryBuilder = new BaseQueryBuilder(this.QueryBuilder);
        //    var builder = new FluentSelectBuilder<T, TMe>(queryBuilder);
        //    subQuery(builder);
        //    this.QueryBuilder.AppendInParenthesis(queryBuilder.Sql);
        //}

        public IFolkeConnection Connection => QueryBuilder.Connection;
        public string Sql => QueryBuilder.Sql;
        public object[] Parameters => QueryBuilder.Parameters;
        public IEnumerator<T> GetEnumerator()
        {
            return this.Enumerate().GetEnumerator();
        }

        public MappedClass MappedClass => QueryBuilder.MappedClass;

        public static IDeleteResult<T, TMe> Delete(BaseQueryBuilder queryBuilder)
        {
            var result = new FluentBaseBuilder<T, TMe>(queryBuilder);
            queryBuilder.StringBuilder.BeforeDelete();
            result.CurrentContext = QueryContext.Delete;
            return result;
        }

        public static IInsertIntoResult<T, TMe> InsertInto(BaseQueryBuilder baseQueryBuilder) 
        {
            baseQueryBuilder.StringBuilder.Append("INSERT INTO");
            var typeMapping = baseQueryBuilder.Mapper.GetTypeMapping(typeof(T));
            baseQueryBuilder.StringBuilder.DuringTable(typeMapping.TableSchema, typeMapping.TableName);
            return new FluentBaseBuilder<T, TMe>(baseQueryBuilder);
        }

        public static ISelectResult<T, TMe> Select(BaseQueryBuilder baseQueryBuilder)
        {
            return new FluentBaseBuilder<T, TMe>(baseQueryBuilder);
        }

        public static ISelectResult<T, TMe> Select(IDatabaseDriver driver, IMapper mapper)
        {
            return new FluentBaseBuilder<T, TMe>(new BaseQueryBuilder(driver, mapper, typeof(T), typeof(TMe)));
        }

        public static IUpdateResult<T, TMe> Update(BaseQueryBuilder queryBuilder)
        {
            queryBuilder.StringBuilder.BeforeUpdate();
            queryBuilder.StringBuilder.AppendTable(queryBuilder.RegisterRootTable());
            return new FluentBaseBuilder<T, TMe>(queryBuilder);
        }
    }

    public interface IDeleteResult<T, TMe> : IBaseCommand, IFromTarget<T, TMe>
    {
    }

    public interface IInsertIntoResult<T, TMe> : IBaseCommand, IInsertedValuesTarget<T, TMe>
    {
    }

    public interface ISelectResult<T, TMe> : ISelectedValuesTarget<T, TMe>
    {
    }

    public interface IUpdateResult<T, TMe> : ISetTarget<T, TMe>
    {
    }
}
