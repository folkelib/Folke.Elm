using System;

namespace Folke.Elm.Abstract.Test
{
    public interface IIntegrationTestSelect : IDisposable
    {
        void SelectAllFrom_TableType_WhereStringEqual_Single();
        void SelectAllFrom_TableType_WhereBooleanEqual_List();
        void SelectAllFrom_TableType_WhereStringIsNull_Single();
        void SelectAllFrom_TableType_WhereObject_ReturnedItemsHaveReferenceToCachedItem_List();
        void SelectAllFrom_TableType_WhereObject_ReturnedItemsHaveReferenceToNotCachedItem_List();
        void Select_TableType_LeftJoinOnIdWhereString_List();
        void Select_TableType_ValuesTwoColumns_List();
        void SelectAllFrom_TableTypeAndAutoJoin_WhereString_List();
        void Select_TypeThatIsNotATable_AllFieldsFromProperties_List();
        void Select_TypeThatIsNotATable_AllFieldsFromProperties_JoinOnString_List();
        void Select_TypeThatIsNotATable_AllFieldsFromProperties_JoinOnStringOrderByString_List();
        void SelectFrom_TableType_OrderByStringDescLimit_List();
        void Select_Tuple_WhereExistsSubQuery_List();
        void Select_Tuple_FromLeftJoinOnId_List();
        void Select_Tuple_FromRightJoin_List();
        void Select_TableType_InnerJoinOnId_List();
        void SelectAllFrom_TableType_WhereLike_List();
        void Select_OneColumnInTableWithForeignKeys_List();
        void Select_OneColumnAndIdInTableWithForeignKeys_List();
        void SelectAllFrom_TableType_OrderByAscLimitWithVariable_List();
        void SelectAllFrom_LimitWithParameter_List();
        void Select_CountAll_Scalar();
        void SelectAllFrom_Linq_WhereStringSelectAll_List();
        void SelectAllFrom_TableType_WhereWithQuote_SingleOrDefault();
        void SelectAllFrom_TableType_WithDecimalValue_SingleOrDefault();
        void Select_TableWithGuid_WhereId_List();
        void Select_TableWithGuid_WhereObject_List();
        void Select_TableWithGuid_WhereParameter_List();
        void SelectAllFrom_TableType_WhereVariableIsNull_Single();
        void Select_WithComplexType();
    }
}
