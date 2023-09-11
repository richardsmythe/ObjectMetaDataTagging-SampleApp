using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using System.Linq.Expressions;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDynamicQueryBuilder
    {
        IQueryable<T> BuildDynamicQuery<T>(
             List<BaseTag> sourceObject,
            List<DefaultFilterCriteria> filters,
            LogicalOperator logicalOperator = LogicalOperator.OR);
    }
}
