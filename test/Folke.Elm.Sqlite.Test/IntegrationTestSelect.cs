using Xunit;

namespace Folke.Elm.Sqlite.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestSelect : IIntegrationTestSelect
    {
        private readonly BaseIntegrationTestSelect integrationTestSelect;

        public IntegrationTestSelect()
        {
            integrationTestSelect =  new BaseIntegrationTestSelect(new SqliteDriver(), TestHelpers.ConnectionString, false);
        }

        public void Dispose()
        {
            integrationTestSelect.Dispose();
        }

        [Fact]
        public void SelectAllFrom_TableType_WhereStringEqual_Single()
        {
            integrationTestSelect.SelectAllFrom_TableType_WhereStringEqual_Single();
        }

        [Fact]
        public void SelectAllFrom_TableType_WhereBooleanEqual_List()
        {
            integrationTestSelect.SelectAllFrom_TableType_WhereBooleanEqual_List();
        }

        [Fact]
        public void SelectAllFrom_TableType_WhereStringIsNull_Single()
        {
            integrationTestSelect.SelectAllFrom_TableType_WhereStringIsNull_Single();
        }

        [Fact]
        public void SelectAllFrom_TableType_WhereObject_ReturnedItemsHaveReferenceToCachedItem_List()
        {
            integrationTestSelect.SelectAllFrom_TableType_WhereObject_ReturnedItemsHaveReferenceToCachedItem_List();
        }

        [Fact]
        public void SelectAllFrom_TableType_WhereObject_ReturnedItemsHaveReferenceToNotCachedItem_List()
        {
            integrationTestSelect.SelectAllFrom_TableType_WhereObject_ReturnedItemsHaveReferenceToNotCachedItem_List();
        }

        [Fact]
        public void Select_TableType_LeftJoinOnIdWhereString_List()
        {
            integrationTestSelect.Select_TableType_LeftJoinOnIdWhereString_List();
        }

        [Fact]
        public void Select_TableType_ValuesTwoColumns_List()
        {
            integrationTestSelect.Select_TableType_ValuesTwoColumns_List();
        }

        [Fact]
        public void SelectAllFrom_TableTypeAndAutoJoin_WhereString_List()
        {
            integrationTestSelect.SelectAllFrom_TableTypeAndAutoJoin_WhereString_List();
        }

        [Fact]
        public void Select_TypeThatIsNotATable_AllFieldsFromProperties_List()
        {
            integrationTestSelect.Select_TypeThatIsNotATable_AllFieldsFromProperties_List();
        }

        [Fact]
        public void Select_TypeThatIsNotATable_AllFieldsFromProperties_JoinOnString_List()
        {
            integrationTestSelect.Select_TypeThatIsNotATable_AllFieldsFromProperties_JoinOnString_List();
        }

        [Fact]
        public void Select_TypeThatIsNotATable_AllFieldsFromProperties_JoinOnStringOrderByString_List()
        {
            integrationTestSelect.Select_TypeThatIsNotATable_AllFieldsFromProperties_JoinOnStringOrderByString_List();
        }

        [Fact]
        public void SelectFrom_TableType_OrderByStringDescLimit_List()
        {
            integrationTestSelect.SelectFrom_TableType_OrderByStringDescLimit_List();
        }

        [Fact]
        public void Select_Tuple_WhereExistsSubQuery_List()
        {
            integrationTestSelect.Select_Tuple_WhereExistsSubQuery_List();
        }

        [Fact]
        public void Select_Tuple_FromLeftJoinOnId_List()
        {
            integrationTestSelect.Select_Tuple_FromLeftJoinOnId_List();
        }

        [Fact(Skip = "Right Join not supported")]
        public void Select_Tuple_FromRightJoin_List()
        {
            integrationTestSelect.Select_Tuple_FromRightJoin_List();
        }

        [Fact]
        public void Select_TableType_InnerJoinOnId_List()
        {
            integrationTestSelect.Select_TableType_InnerJoinOnId_List();
        }

        [Fact]
        public void SelectAllFrom_TableType_WhereLike_List()
        {
            integrationTestSelect.SelectAllFrom_TableType_WhereLike_List();
        }

        [Fact]
        public void Select_OneColumnInTableWithForeignKeys_List()
        {
            integrationTestSelect.Select_OneColumnInTableWithForeignKeys_List();
        }

        [Fact]
        public void Select_OneColumnAndIdInTableWithForeignKeys_List()
        {
            integrationTestSelect.Select_OneColumnAndIdInTableWithForeignKeys_List();
        }

        [Fact]
        public void SelectAllFrom_TableType_OrderByAscLimitWithVariable_List()
        {
            integrationTestSelect.SelectAllFrom_TableType_OrderByAscLimitWithVariable_List();
        }

        [Fact]
        public void SelectAllFrom_LimitWithParameter_List()
        {
            integrationTestSelect.SelectAllFrom_LimitWithParameter_List();
        }

        [Fact]
        public void Select_CountAll_Scalar()
        {
            integrationTestSelect.Select_CountAll_Scalar();
        }

        [Fact]
        public void SelectAllFrom_Linq_WhereStringSelectAll_List()
        {
            integrationTestSelect.SelectAllFrom_Linq_WhereStringSelectAll_List();
        }

        [Fact]
        public void SelectAllFrom_TableType_WhereWithQuote_SingleOrDefault()
        {
            integrationTestSelect.SelectAllFrom_TableType_WhereWithQuote_SingleOrDefault();
        }

        [Fact]
        public void SelectAllFrom_TableType_WithDecimalValue_SingleOrDefault()
        {
            integrationTestSelect.SelectAllFrom_TableType_WithDecimalValue_SingleOrDefault();
        }

        [Fact]
        public void Select_TableWithGuid_WhereId_List()
        {
            integrationTestSelect.Select_TableWithGuid_WhereId_List();
        }

        [Fact]
        public void Select_TableWithGuid_WhereObject_List()
        {
            integrationTestSelect.Select_TableWithGuid_WhereObject_List();
        }

        [Fact]
        public void Select_TableWithGuid_WhereParameter_List()
        {
            integrationTestSelect.Select_TableWithGuid_WhereParameter_List();
        }

        [Fact]
        public void SelectAllFrom_TableType_WhereVariableIsNull_Single()
        {
            integrationTestSelect.SelectAllFrom_TableType_WhereVariableIsNull_Single();
        }
    }
}
