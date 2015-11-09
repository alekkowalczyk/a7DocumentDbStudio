using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace a7DocumentDbStudio.Utils
{
    public class XElementNames
    {
        public const string FilterAtomExpressionNode = "XFilterAtom";
        public const string FilterGroupExpressionNode = "XFilterGroup";
        public const string FilterDynamicExpressionNode = "XFilterDynamic";
        public const string FilterRelDynamicExpressionNode = "XFilterDynamicRelation";
        public const string FilterRelOnRelatedExpressionNode = "XFilterOnRelated";
        public const string FilterHasRelatedExpressionNode = "XFilterHasRelated";
        public const string FilterHasRelatedRecordExpressionNode = "XFilterHasRelatedRecord";
        public const string FilterValueAndEntityExpressionNode = "XFilterValueAndEntityExprData";
        public const string FilterValueAndEntityExpressionNode_Value = "XFilterValueAndEntityExprData_Value";
        public const string FilterValueAndEntityExpressionNode_EntityId = "XFilterValueAndEntityExprData_EntityId";
        public const string FilterConditionExpressionNode = "XFilterCondition";
        public const string FilterFlatExprNode = "XFilterFlatGroup";
        public const string FilterQuickExprNode = "XFilterQuickGroup";
        public const string FilterFalseExprNode = "XFilterFalse";
        public const string FilterTrueExprNode = "XFilterTrue";
    }
}
