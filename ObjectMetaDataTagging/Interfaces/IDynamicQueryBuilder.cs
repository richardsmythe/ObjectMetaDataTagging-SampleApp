using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using System.Linq.Expressions;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDynamicQueryBuilder
    {
        IQueryable<T> BuildDynamicQuery<T>(
            List<T> sourceObject,
            List<FilterCriteria> filters,
            LogicalOperator logicalOperator = LogicalOperator.AND);
    }
}
